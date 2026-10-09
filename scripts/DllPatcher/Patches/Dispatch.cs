using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void InstallDispatch(string clientRoot,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(clientRoot,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Dispatch.cs.txt")!;
        using var reader=new StreamReader(input);
        var references=Directory.GetFiles(managed,"*.dll").Where(p=>Path.GetFileName(p)!="SprkDispatch.dll")
            .Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkDispatch",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release));
        using var compiled=new MemoryStream();var emitted=compilation.Emit(compiled);
        if(!emitted.Success)throw new InvalidOperationException(string.Join("\n",emitted.Diagnostics));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var assembly=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver});
        var module=assembly.MainModule;var worker=helper.MainModule.Types.Single(t=>t.Name=="SprkDispatch");
        var rejected=module.ImportReference(worker.Methods.Single(m=>m.Name=="StartRejected"));
        foreach(var method in module.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody && m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="NShared.StartDispatch.Response")) {
            if(!method.Body.Instructions.Any(i=>i.OpCode==OpCodes.Ldstr && (i.Operand as string)?.Contains("Failed to StartDispatch")==true)
                || method.Body.Instructions.Any(i=>i.Operand is MethodReference call && call.Name=="StartRejected"))continue;
            var failure=method.Body.Instructions.Single(i=>i.OpCode==OpCodes.Ldstr && (i.Operand as string)?.Contains("Failed to StartDispatch")==true);
            var log=method.Body.Instructions.SkipWhile(i=>i!=failure).First(i=>i.Operand is MethodReference call && call.Name=="Invoke");
            var il=method.Body.GetILProcessor();var response=Instruction.Create(OpCodes.Ldarg,method.Parameters[0]);
            il.InsertAfter(log,response);il.InsertAfter(response,Instruction.Create(OpCodes.Call,rejected));
        }
        if(module.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions)
            .Count(i=>i.Operand is MethodReference call && call.DeclaringType.Name=="SprkDispatch" && call.Name=="StartRejected")!=1)
            throw new InvalidOperationException("Dispatch start failure callback was not patched exactly once");
        var awake=module.Types.Single(t=>t.Name=="UICamera").Methods.Single(m=>m.Name=="Awake");
        if(!awake.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkDispatch" && m.Name=="Ensure"))
            awake.Body.GetILProcessor().InsertBefore(awake.Body.Instructions[0],Instruction.Create(OpCodes.Call,module.ImportReference(worker.Methods.Single(m=>m.Name=="Ensure"))));
        var cameraUpdate=module.Types.Single(t=>t.Name=="UICamera").Methods.Single(m=>m.Name=="ChainUpdate");
        foreach(var old in cameraUpdate.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkDispatch" && m.Name=="Tick").ToArray())cameraUpdate.Body.GetILProcessor().Remove(old);
        var update=module.Types.Single(t=>t.Name=="GlobalUpdateBehaviour").Methods.Single(m=>m.Name=="Update");
        if(!update.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkDispatch" && m.Name=="Tick"))
            update.Body.GetILProcessor().InsertBefore(update.Body.Instructions[0],Instruction.Create(OpCodes.Call,module.ImportReference(worker.Methods.Single(m=>m.Name=="Tick"))));
        var text=module.Types.Single(t=>t.FullName=="NVespa.NGlobal.LocalizationManager").Methods.Single(m=>m.Name=="GetStringDirect" && m.Parameters.Count==1);
        if(!NativeStaticData.ValidateInstalled(managed) && !text.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkDispatch" && m.Name=="HelpText")) {
            foreach(var ret in text.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Call;ret.Operand=module.ImportReference(worker.Methods.Single(m=>m.Name=="HelpText"));
                text.Body.GetILProcessor().InsertAfter(ret,Instruction.Create(OpCodes.Ret));
            }
        }
        var simulator=module.Types.Single(t=>t.FullName=="NShared.BattleSimulationInstance");
        var setting=simulator.Methods.Single(m=>m.Name=="SettingBattleStart");
        if(!setting.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkDispatch" && m.Name=="Seed")) {
            var random=setting.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.Name=="GetRandomByMonth");
            var il=setting.Body.GetILProcessor();var getThis=Instruction.Create(OpCodes.Ldarg_0);
            var getUid=Instruction.Create(OpCodes.Call,module.ImportReference(simulator.Methods.Single(m=>m.Name=="get_UID")));
            var seed=Instruction.Create(OpCodes.Call,module.ImportReference(worker.Methods.Single(m=>m.Name=="Seed")));
            il.InsertAfter(random,getThis);il.InsertAfter(getThis,getUid);il.InsertAfter(getUid,seed);
        }
        var staged=Path.Combine(managed,"Assembly-CSharp.dll.dispatch-staged");
        File.WriteAllBytes(Path.Combine(managed,"SprkDispatch.dll.staged"),compiled.ToArray());assembly.Write(staged);
        if(!stageOnly) {
            string stamp=DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            File.Copy(dll,dll+".before-dispatch-"+stamp);
            var helperPath=Path.Combine(managed,"SprkDispatch.dll");
            if(File.Exists(helperPath))File.Copy(helperPath,helperPath+".backup-"+stamp);
            assembly.Dispose();File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        }
        Console.WriteLine(stageOnly?"Staged native dispatch simulator helper and rejection popup":"Installed native dispatch simulator helper and rejection popup");
    }
}
