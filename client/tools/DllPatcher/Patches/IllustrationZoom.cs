using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void PatchIllustrationZoom(ModuleDefinition game, ModuleDefinition helper) {
        var viewer=game.GetType("NGame2.NUI.NWindow.HeroIllustBook");
        foreach(var name in new[]{"Init","InitSeasonReward"}) {
            var init=viewer.Methods.Single(m=>m.Name==name);
            var store=init.Body.Instructions.Single(i=>i.OpCode==OpCodes.Stfld && i.Operand is FieldReference f && f.Name=="_scaleLimitMax");
            if(store.Previous.OpCode==OpCodes.Ldc_I4_8 && store.Previous.Previous.OpCode==OpCodes.Pop)continue;
            var il=init.Body.GetILProcessor();il.InsertBefore(store,il.Create(OpCodes.Pop));il.InsertBefore(store,il.Create(OpCodes.Ldc_I4_8));
        }
        var method=viewer.Methods.Single(m=>m.Name=="UpdateDerived");
        if (!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkIllustrationZoom")) {
            var first=method.Body.Instructions[0];var il=method.Body.GetILProcessor();
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,game.ImportReference(helper.GetType("SprkIllustrationZoom").Methods.Single(m=>m.Name=="Update"))));
        }
        VerifyIllustrationZoom(game);
    }
    static void VerifyIllustrationZoom(ModuleDefinition game) {
        var type=game.GetType("NGame2.NUI.NWindow.HeroIllustBook");
        foreach(var name in new[]{"Init","InitSeasonReward"}) {
            var init=type.Methods.Single(m=>m.Name==name);
            var store=init.Body.Instructions.Single(i=>i.OpCode==OpCodes.Stfld && i.Operand is FieldReference f && f.Name=="_scaleLimitMax");
            if(store.Previous.OpCode!=OpCodes.Ldc_I4_8 || store.Previous.Previous.OpCode!=OpCodes.Pop)
                throw new InvalidOperationException("Native illustration touch zoom must support 800%: "+name);
            VerifyArenaResultStack(init);
        }
        var method=type.Methods.Single(m=>m.Name=="UpdateDerived");
        var code=method.Body.Instructions;
        if (code[0].OpCode!=OpCodes.Ldarg_0 || code[1].Operand is not MethodReference hook
            || hook.DeclaringType.Name!="SprkIllustrationZoom" || hook.Name!="Update"
            || code.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkIllustrationZoom")!=1)
            throw new InvalidOperationException("Illustration wheel zoom hook is missing or duplicated.");
        foreach (var name in new[]{"DragIllust","ZoomIllust","UpdateDerived"})
            if (!code.Any(i=>i.Operand is MethodReference m && m.Name==name))
                throw new InvalidOperationException("Native illustration input was removed: "+name);
        var setup=type.Methods.Single(m=>m.Name=="SetupIllust");
        if (!setup.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ResetLocalTM"))
            throw new InvalidOperationException("Switching illustration must reset its transform.");
        foreach (var name in new[]{"OnClickPrev","OnClickNext","OnClickHideUI","OnClickClose","OnDoubleClick"})
            if (!type.Methods.Single(m=>m.Name==name).HasBody) throw new InvalidOperationException("Native illustration control missing: "+name);
        VerifyArenaResultStack(method);
    }
    static void VerifyIllustrationZoomFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".illustration-zoom-staged":"")));
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkIllustrationZoom.dll"+(staged?".staged":"")));
        VerifyIllustrationZoom(game.MainModule);
        var update=helper.MainModule.GetType("SprkIllustrationZoom").Methods.Single(m=>m.Name=="Update");
        var calls=update.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Select(m=>m.Name).ToArray();
        foreach (var name in new[]{"IsOpen","get_isFocused","GetAxis","GetMouseButtonDown","IsChildOf","GetComponentInParent","OnDoubleClick","NextScale","ClampOffset"})
            if (!calls.Contains(name)) throw new InvalidOperationException("Incomplete illustration input guard/control: "+name);
        if (!update.Body.Instructions.Any(i=>i.OpCode==OpCodes.Ldstr && Equals(i.Operand,"Mouse ScrollWheel")))
            throw new InvalidOperationException("Illustration mouse-wheel axis missing.");
        Console.WriteLine("Verified illustration wheel zoom, focus/modal guards, native drag/reset/hide/navigation and transform reset on switching artwork.");
    }
    static void TestIllustrationZoom(string root,bool staged=false) {
        var path=Path.Combine(root,"King's Raid_Data","Managed","SprkIllustrationZoom.dll"+(staged?".staged":""));
        var assembly=System.Reflection.Assembly.Load(File.ReadAllBytes(path));
        var math=assembly.GetType("SprkIllustrationZoomMath")!;
        float Zoom(float current,float wheel)=>(float)math.GetMethod("NextScale")!.Invoke(null,new object[]{current,wheel})!;
        float Offset(float p,float s,float image,float viewport)=>(float)math.GetMethod("ClampOffset")!.Invoke(null,new object[]{p,s,image,viewport})!;
        if (Zoom(1,0)!=1 || Zoom(1,0.1f)<=1 || Zoom(1,-0.1f)>=1 || Zoom(1,20)!=8 || Zoom(1,-20)!=0.5f)
            throw new Exception("Illustration zoom direction or bounds failed.");
        if (Math.Abs(Zoom(Zoom(1,0.1f),-0.1f)-1)>0.00001f || Zoom(float.NaN,0)!=1 || Zoom(1,float.PositiveInfinity)!=1)
            throw new Exception("Illustration zoom reversibility/invalid-input checks failed.");
        foreach (var current in new[]{0.5f,0.75f,1,2,4,6,8}) foreach (var wheel in new[]{-100f,-1,-0.1f,0,0.1f,1,100}) {
            float result=Zoom(current,wheel);
            if (result<0.5f || result>8 || float.IsNaN(result)) throw new Exception("Illustration zoom escaped bounds.");
        }
        if (Offset(5000,2,1000,1000)!=500 || Offset(-5000,2,1000,1000)!=-500 || Offset(10,1,1000,1000)!=0)
            throw new Exception("Illustration pan clamp failed.");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".illustration-zoom-staged":"")));
        using var helper=AssemblyDefinition.ReadAssembly(path);
        int total=game.MainModule.GetType("NGame2.NUI.NWindow.HeroIllustBook").Methods.Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
        var before=game.MainModule.GetType("NGame2.NUI.NWindow.HeroIllustBook").Methods.Single(m=>m.Name=="UpdateDerived").Body.Instructions.Count;
        PatchIllustrationZoom(game.MainModule,helper.MainModule);
        if (before!=game.MainModule.GetType("NGame2.NUI.NWindow.HeroIllustBook").Methods.Single(m=>m.Name=="UpdateDerived").Body.Instructions.Count)
            throw new Exception("Illustration zoom patch is not idempotent.");
        if(total!=game.MainModule.GetType("NGame2.NUI.NWindow.HeroIllustBook").Methods.Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count))
            throw new Exception("Native touch zoom limit patch is not idempotent.");
        Console.WriteLine("PASS: installed zoom math, limits, inverse wheel steps, invalid inputs, pan bounds and patch idempotence.");
    }
    static void InstallIllustrationZoom(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkIllustrationZoom.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.IllustrationZoom.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkIllustrationZoom",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        PatchIllustrationZoom(game.MainModule,helper.MainModule);
        var staged=dll+".illustration-zoom-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyIllustrationZoomFile(root,true);TestIllustrationZoom(root,true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        var backup=dll+".before-illustration-zoom-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        File.Copy(dll,backup);
        if (File.Exists(helperPath)) File.Copy(helperPath,helperPath+".before-illustration-zoom-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyIllustrationZoomFile(root);
        Console.WriteLine("Installed illustration zoom. Restart the client.");
    }
}
