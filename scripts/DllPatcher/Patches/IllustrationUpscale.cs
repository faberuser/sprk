using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static string IllustrationResource(string name) {
        using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name)!;
        using var reader=new StreamReader(input);return reader.ReadToEnd();
    }
    static void PatchIllustrationUpscale(ModuleDefinition game,ModuleDefinition helper) {
        var type=game.GetType("NGame2.NUI.NWindow.HeroIllustBook");
        var bridge=helper.GetType("SprkIllustrationUpscale");
        var awake=game.GetType("UICamera").Methods.Single(m=>m.Name=="Awake");
        if(!awake.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==bridge.Name && m.Name=="EnsureProbe"))
            awake.Body.GetILProcessor().InsertBefore(awake.Body.Instructions[0],Instruction.Create(OpCodes.Call,game.ImportReference(bridge.Methods.Single(m=>m.Name=="EnsureProbe"))));
        foreach(var pair in new[]{("SetupIllust","BeforeChange"),("Close","Closing")}) {
            var method=type.Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==bridge.Name && m.Name==pair.Item2))continue;
            var first=method.Body.Instructions[0];var il=method.Body.GetILProcessor();
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,game.ImportReference(bridge.Methods.Single(m=>m.Name==pair.Item2))));
        }
        var setup=type.Methods.Single(m=>m.Name=="SetupIllust");
        foreach(var instruction in setup.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.Name=="ResetPlaneAssetTextureAsync"))
            instruction.Operand=game.ImportReference(bridge.Methods.Single(m=>m.Name=="Assign"));
        VerifyIllustrationUpscale(game);
    }
    static void VerifyIllustrationUpscale(ModuleDefinition game) {
        var type=game.GetType("NGame2.NUI.NWindow.HeroIllustBook");
        foreach(var pair in new[]{("SetupIllust","BeforeChange"),("Close","Closing")}) {
            var code=type.Methods.Single(m=>m.Name==pair.Item1).Body.Instructions;
            if(code[0].OpCode!=OpCodes.Ldarg_0 || code[1].Operand is not MethodReference hook
                || hook.DeclaringType.Name!="SprkIllustrationUpscale" || hook.Name!=pair.Item2
                || code.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name==hook.DeclaringType.Name && m.Name==hook.Name)!=1
                || !code.Any(i=>i.Operand is MethodReference m && m.Name=="ReleaseTextureInfo"))
                throw new InvalidOperationException("Illustration AI lifecycle/ownership hook missing: "+pair.Item1);
        }
        var setup=type.Methods.Single(m=>m.Name=="SetupIllust");
        if(setup.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.Name=="Assign" && m.DeclaringType.Name=="SprkIllustrationUpscale")!=2
            || setup.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ResetPlaneAssetTextureAsync"))
            throw new InvalidOperationException("Both normal and season-reward assignments must use the async source callback.");
        VerifyIllustrationZoom(game);
        foreach(var name in new[]{"SetupIllust","Close"})VerifyArenaResultStack(type.Methods.Single(m=>m.Name==name));
    }
    static void VerifyIllustrationUpscaleFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".illustration-ai-staged":"")));
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkIllustrationUpscale.dll"+(staged?".staged":"")));
        VerifyIllustrationUpscale(game.MainModule);
        var before=game.MainModule.GetType("NGame2.NUI.NWindow.HeroIllustBook").Methods.Sum(m=>m.HasBody?m.Body.Instructions.Count:0);
        PatchIllustrationUpscale(game.MainModule,helper.MainModule);
        if(before!=game.MainModule.GetType("NGame2.NUI.NWindow.HeroIllustBook").Methods.Sum(m=>m.HasBody?m.Body.Instructions.Count:0))
            throw new InvalidOperationException("Illustration AI patch is not idempotent");
        foreach(var name in new[]{"SprkIllustrationUpscale","SprkIllustrationUpscaleCore"})
            if(helper.MainModule.GetType(name)==null)throw new InvalidOperationException("Missing illustration helper: "+name);
        var core=helper.MainModule.GetType("SprkIllustrationUpscaleCore");
        if(!core.Methods.Any(m=>m.Name=="ModelUrlFromProfile") || !core.Methods.Any(m=>m.Name=="ModelUrlFromManifest")
            || !core.Methods.Single(m=>m.Name=="Run").Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="Provision" && m.Parameters.Count==3)
            || !helper.MainModule.GetType("SprkIllustrationUpscale").Methods.Single(m=>m.Name=="ModelDownloadUrl").Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ModelUrlFromProfile"))
            throw new InvalidOperationException("Illustration model provisioning must use the active server update profile");
        var resolve=helper.MainModule.GetType("SprkIllustrationUpscale").Methods.Single(m=>m.Name=="ModelDownloadUrl");
        if(!resolve.Body.Instructions.Any(i=>i.OpCode==OpCodes.Ldstr && Equals(i.Operand,"SPRK_UPDATE_MANIFEST_URL"))
            || !resolve.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ModelUrlFromManifest"))
            throw new InvalidOperationException("Illustration model provisioning must honor the launcher's captured update URL");
        var runtime=Path.Combine(managed,"SprkIllustrationRuntime"+(staged?"-staged":""));
        foreach(var name in new[]{"realesrgan-ncnn-vulkan.exe","vcomp140.dll","default-model.json","NOTICE.txt"})
            if(!File.Exists(Path.Combine(runtime,name)))throw new InvalidOperationException("Missing illustration runtime: "+name);
        foreach(var pair in new[]{("realesrgan-ncnn-vulkan.exe","07e49f7cbb4ede01ae4dd4c399d3a7e5846e3d2085c3128eff881e55cb7b1a0c"),("vcomp140.dll","8f72ef2e483465444b2059fc6744d6cb22cd8d8a27f6fa56befd2a42dcd0f78b")})
            if(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(Path.Combine(runtime,pair.Item1)))).ToLowerInvariant()!=pair.Item2)
                throw new InvalidOperationException("Illustration runtime integrity mismatch: "+pair.Item1);
        var descriptor=System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(runtime,"default-model.json"))).RootElement;
        if(descriptor.GetProperty("sha256").GetString()!="f18ef1a86726f26288739b107f2bb6d4b815d04bc14f6bb7b3d0dacbc1a1fdba"
            || descriptor.GetProperty("size").GetInt32()!=33564831 || descriptor.GetProperty("interface").GetString()!="sprk-ncnn-rrdb-rgb-x4-v1")
            throw new InvalidOperationException("Default model descriptor differs from authenticated helper metadata");
        Console.WriteLine("Verified illustration AI lifecycle, async assignment for both viewer modes, zoom preservation, runtime and model descriptor.");
    }
    static void TestIllustrationUpscale() {
        var compilation=CSharpCompilation.Create("IllustrationUpscaleTests-"+Guid.NewGuid().ToString("N"),
            new[]{CSharpSyntaxTree.ParseText(IllustrationResource("IllustrationUpscaleCore.cs.txt")),CSharpSyntaxTree.ParseText(IllustrationResource("IllustrationUpscaleTests.cs.txt"))},
            ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        System.Reflection.Assembly.Load(output.ToArray()).GetType("IllustrationUpscaleTests")!.GetMethod("Run")!.Invoke(null,null);
    }
    static void InstallIllustrationUpscale(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkIllustrationUpscale.dll");
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkIllustrationUpscale",
            new[]{"IllustrationUpscale.cs.txt","IllustrationUpscaleCore.cs.txt","IllustrationUpscaleProbe.cs.txt"}.Select(n=>CSharpSyntaxTree.ParseText(IllustrationResource(n))),refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        string runtime=Path.Combine(managed,"SprkIllustrationRuntime-staged");Directory.CreateDirectory(runtime);
        string script=Path.Combine(runtime,"prepare.py");File.WriteAllText(script,IllustrationResource("prepare_illustration_runtime.py"));
        try {
            var start=new System.Diagnostics.ProcessStartInfo("python"){UseShellExecute=false,CreateNoWindow=true};
            start.ArgumentList.Add(script);start.ArgumentList.Add("--client");start.ArgumentList.Add(runtime);
            using var process=System.Diagnostics.Process.Start(start)!;process.WaitForExit();
            if(process.ExitCode!=0)throw new InvalidOperationException("Pinned illustration runtime preparation failed");
        }finally{File.Delete(script);}
        File.WriteAllText(Path.Combine(runtime,"NOTICE.txt"),IllustrationResource("IllustrationUpscaleNOTICE.txt"));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        PatchIllustrationUpscale(game.MainModule,helper.MainModule);
        game.Write(dll+".illustration-ai-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyIllustrationUpscaleFile(root,true);TestIllustrationUpscale();
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Illustration AI staged. Close the game and shared-asset workers before installation.");
        string stamp=".before-illustration-ai-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff");File.Copy(dll,dll+stamp);
        if(File.Exists(helperPath))File.Copy(helperPath,helperPath+stamp);
        var installedRuntime=Path.Combine(managed,"SprkIllustrationRuntime");Directory.CreateDirectory(installedRuntime);
        foreach(var path in Directory.GetFiles(runtime))File.Move(path,Path.Combine(installedRuntime,Path.GetFileName(path)),true);
        Directory.Delete(runtime);
        File.Move(helperPath+".staged",helperPath,true);File.Move(dll+".illustration-ai-staged",dll,true);
        VerifyIllustrationUpscaleFile(root);Console.WriteLine("Installed illustration AI. Active writable model and cache were not touched. Restart the client.");
    }
}
