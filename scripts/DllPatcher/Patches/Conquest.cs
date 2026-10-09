using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void InstallConquest(string clientRoot,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(clientRoot,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Conquest.cs.txt")!;
        using var reader=new StreamReader(input);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>!Path.GetFileName(p).StartsWith("SprkConquest"))
            .Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkConquest",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release));
        using var compiled=new MemoryStream();var emitted=compilation.Emit(compiled);
        if(!emitted.Success)throw new InvalidOperationException(string.Join("\n",emitted.Diagnostics));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var assembly=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver});
        var module=assembly.MainModule;var worker=helper.MainModule.Types.Single(t=>t.Name=="SprkConquest");
        MethodReference Method(string name)=>module.ImportReference(worker.Methods.Single(m=>m.Name==name));
        bool Hooked(MethodDefinition method,string hook)=>method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkConquest" && m.Name==hook);
        var awake=module.Types.Single(t=>t.Name=="UICamera").Methods.Single(m=>m.Name=="Awake");
        if(!awake.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkConquest"))
            awake.Body.GetILProcessor().InsertBefore(awake.Body.Instructions[0],Instruction.Create(OpCodes.Call,Method("Tick")));
        var update=module.Types.Single(t=>t.Name=="GlobalUpdateBehaviour").Methods.Single(m=>m.Name=="Update");
        if(!update.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkConquest"))
            update.Body.GetILProcessor().InsertBefore(update.Body.Instructions[0],Instruction.Create(OpCodes.Call,Method("Tick")));
        var login=module.Types.Single(t=>t.Name=="JM_NShared_BattleLoginReq").Methods.Single(m=>m.Name=="EncodeJsonObject");
        if(!Hooked(login,"Login"))foreach(var ret in login.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
            ret.OpCode=OpCodes.Call;ret.Operand=Method("Login");login.Body.GetILProcessor().InsertAfter(ret,Instruction.Create(OpCodes.Ret));
        }
        foreach(var name in new[]{"JM_NShared_PingReq","JM_NShared_PingIdleReq"}) {
            var encoder=module.Types.Single(t=>t.Name==name).Methods.Single(m=>m.Name=="EncodeJsonObject");
            if(!Hooked(encoder,"ReplayStatus"))foreach(var ret in encoder.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Call;ret.Operand=Method("ReplayStatus");encoder.Body.GetILProcessor().InsertAfter(ret,Instruction.Create(OpCodes.Ret));
            }
        }
        var parser=module.Types.Single(t=>t.Name=="JM_NShared_WaveReadyBattleLog").Methods.Single(m=>m.Name=="Parse" && m.Parameters.Count==2 && m.Parameters[0].ParameterType.Name=="IDictionary");
        if(!Hooked(parser,"ReadSeed"))foreach(var ret in parser.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
            ret.OpCode=OpCodes.Ldarg_0;var il=parser.Body.GetILProcessor();var call=Instruction.Create(OpCodes.Call,Method("ReadSeed"));
            il.InsertAfter(ret,call);il.InsertAfter(call,Instruction.Create(OpCodes.Ret));
        }
        var ready=module.Types.Single(t=>t.FullName=="NShared.WaveReadyBattleLog").Methods.Single(m=>m.Name.EndsWith(".Run"));
        var readyIl=ready.Body.GetILProcessor();var first=ready.Body.Instructions[0];
        if(!Hooked(ready,"ApplySeed")) {
            readyIl.InsertBefore(first,Instruction.Create(OpCodes.Ldarg_0));readyIl.InsertBefore(first,Instruction.Create(OpCodes.Ldarg_1));readyIl.InsertBefore(first,Instruction.Create(OpCodes.Call,Method("ApplySeed")));
        }
        var results=module.Types.Single(t=>t.FullName=="NShared.BattleInstanceIntegrityChecker").Methods.Single(m=>m.Name=="GetCreatureInfoList");
        if(!Hooked(results,"ScoreBoss"))foreach(var ret in results.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
            ret.OpCode=OpCodes.Ldarg_0;var il=results.Body.GetILProcessor();var call=Instruction.Create(OpCodes.Call,Method("ScoreBoss"));
            il.InsertAfter(ret,call);il.InsertAfter(call,Instruction.Create(OpCodes.Ret));
        }
        var staged=dll+".conquest-staged";File.WriteAllBytes(Path.Combine(managed,"SprkConquest.dll.staged"),compiled.ToArray());assembly.Write(staged);
        if(!stageOnly) {
            File.Copy(dll,dll+".before-conquest-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));assembly.Dispose();
            File.Move(staged,dll,true);File.Move(Path.Combine(managed,"SprkConquest.dll.staged"),Path.Combine(managed,"SprkConquest.dll"),true);
        }
        Console.WriteLine(stageOnly?"Staged Conquest native combat worker and multiplayer hooks":"Installed Conquest native combat worker and multiplayer hooks");
    }
}
