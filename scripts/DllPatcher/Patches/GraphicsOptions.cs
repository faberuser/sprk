using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void VerifyGraphicsOptions(ModuleDefinition module) {
        foreach(var pair in new[]{("UICamera","Awake","Ensure"),("NGame2.NUI.NWindow.GameOptionWindow","SetupOptionGrids","Setup"),("NShader.FilterBase","CreateRenderTarget","CreateTarget")}) {
            var method=module.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions" && m.Name==pair.Item3))throw new InvalidOperationException("Missing graphics hook: "+pair.Item3);
            if(!method.Body.HasExceptionHandlers)VerifyArenaResultStack(method);
        }
    }
    static void VerifyGraphicsOptionsFile(string root,bool staged=false) {
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".graphics-options-staged":"")));
        VerifyGraphicsOptions(game.MainModule);
        using var post=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Unity.Postprocessing.Runtime.dll"));
        var modes=post.MainModule.GetType("UnityEngine.Rendering.PostProcessing.PostProcessLayer").NestedTypes.Single(t=>t.Name=="Antialiasing");
        foreach(var pair in new[]{("None",0),("FastApproximateAntialiasing",1),("SubpixelMorphologicalAntialiasing",2),("TemporalAntialiasing",3)})
            if(Convert.ToInt32(modes.Fields.Single(f=>f.Name==pair.Item1).Constant)!=pair.Item2)throw new InvalidOperationException("Unexpected native AA enum mapping.");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","SprkGraphicsOptions.dll"+(staged?".staged":"")));
        var bridge=helper.MainModule.GetType("SprkGraphicsOptions");
        var setup=bridge.Methods.Single(m=>m.Name=="Setup");
        if(setup.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.Name=="Add" && m.DeclaringType.Name=="SprkGraphicsOptions")!=2)
            throw new InvalidOperationException("Graphics settings must expose only preset and render scale.");
        foreach(var required in new[]{"Performance","Balanced","Quality","Preset","Scale"})
            if(!setup.Body.Instructions.Any(i=>Equals(i.Operand,required)))throw new InvalidOperationException("Missing graphics preset control: "+required);
        foreach(var call in new[]{"GetRenderTextureSupportedMSAASampleCount","Init","ResetHistory","Blit","get_supportsMotionVectors"})
            if(!bridge.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Any(i=>i.Operand is MethodReference m && m.Name==call))throw new InvalidOperationException("Missing graphics integration: "+call);
        if(bridge.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Any(i=>i.Operand is MethodReference m && m.DeclaringType.FullName=="UnityEngine.PlayerPrefs"))throw new InvalidOperationException("Graphics preferences bypass test isolation.");
        Console.WriteLine("Verified graphics hooks, native AA modes, runtime integration and isolated preferences.");
    }
    static void InstallGraphicsOptions(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkGraphicsOptions.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.GraphicsOptions.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkGraphicsOptions",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var module=game.MainModule;
        var bridge=helper.MainModule.GetType("SprkGraphicsOptions");
        foreach(var pair in new[]{("UICamera","Awake","Ensure"),("NGame2.NUI.NWindow.GameOptionWindow","SetupOptionGrids","Setup")}) {
            var method=module.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions"))continue;
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=pair.Item3=="Setup"?OpCodes.Ldarg_0:OpCodes.Nop;ret.Operand=null;
                var call=il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name==pair.Item3)));
                il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        var factory=module.GetType("NShader.FilterBase").Methods.Single(m=>m.Name=="CreateRenderTarget");
        factory.Body.Instructions.Clear();factory.Body.Variables.Clear();factory.Body.ExceptionHandlers.Clear();
        var factoryIl=factory.Body.GetILProcessor();factoryIl.Emit(OpCodes.Ldarg_0);factoryIl.Emit(OpCodes.Ldarg_1);factoryIl.Emit(OpCodes.Ldarg_2);
        factoryIl.Emit(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="CreateTarget")));factoryIl.Emit(OpCodes.Ret);
        VerifyGraphicsOptions(game.MainModule);
        var staged=dll+".graphics-options-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        // Reapplying to the emitted assembly must not duplicate hooks.
        using(var repeated=AssemblyDefinition.ReadAssembly(staged)) {
            VerifyGraphicsOptions(repeated.MainModule);
            foreach(var method in repeated.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody))
                foreach(var name in new[]{"Ensure","Setup","CreateTarget"})
                    if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions" && m.Name==name)>method.Body.Instructions.Count(i=>i.OpCode==OpCodes.Ret))throw new InvalidOperationException("Duplicated graphics hook.");
        }
        VerifyGraphicsOptionsFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>p.ProcessName=="King's Raid" || p.ProcessName.StartsWith("SprkTest-")))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-graphics-options-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyGraphicsOptionsFile(root);
    }
}
