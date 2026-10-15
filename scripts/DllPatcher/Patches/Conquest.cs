using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void VerifyArcdimWorker(string clientRoot) {
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkConquest.dll"));
        var prepare=helper.MainModule.GetType("SprkConquest").Methods.Single(m=>m.Name=="Prepare");
        if(!prepare.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="get_GroupHeroInfos")
            || !prepare.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ContainsKey")
            || !prepare.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="AddEachWaveEachTeamBattleCreatureCreateInfo")
            || !prepare.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="SetWavePauseCallback"))
            throw new InvalidOperationException("Arcdim worker must register both wave teams and their transition callback.");
        var methods=helper.MainModule.GetType("SprkConquest").NestedTypes.SelectMany(t=>t.Methods)
            .Concat(helper.MainModule.GetType("SprkConquest").Methods).Where(m=>m.HasBody).ToArray();
        if(!methods.SelectMany(m=>m.Body.Instructions).Any(i=>i.OpCode==OpCodes.Ldstr && (string?)i.Operand=="SetNextTeamNot")
            || !methods.SelectMany(m=>m.Body.Instructions).Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="CreatureCreateBattleLog" && m.Name=="set_PlayerId"))
            throw new InvalidOperationException("Arcdim transition must notify clients and retain replacement hero ownership.");
        VerifyArenaWorker(clientRoot);
        Console.WriteLine("Verified Arcdim native wave teams, team-change notification and hero ownership.");
    }
    static void VerifyArenaWorker(string clientRoot) {
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkConquest.dll"));
        var worker=helper.MainModule.GetType("SprkConquest");
        var prepare=worker.Methods.Single(m=>m.Name=="Prepare");
        if(!prepare.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="get_TeamType") ||
           !prepare.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="get_LeaderHeroIndex"))
            throw new InvalidOperationException("Arena worker must preserve each side and registered leader.");
        var finish=worker.Methods.Single(m=>m.Name=="Finish");
        if(!finish.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="get_PlayerId"))
            throw new InvalidOperationException("Arena results must resolve creature ownership, including identical heroes on both teams.");
        if(!worker.NestedTypes.SelectMany(t=>t.Methods).Concat(worker.Methods).Where(m=>m.HasBody)
            .SelectMany(m=>m.Body.Instructions).Any(i=>i.OpCode==OpCodes.Ldstr && (string?)i.Operand=="ArenaVersion"))
            throw new InvalidOperationException("Arena worker capability handshake missing.");
        VerifyConquestReplay(clientRoot);
        Console.WriteLine("Verified native arena worker teams, leader, ownership and capability handshake.");
    }
    static void VerifyConquestReplay(string clientRoot) {
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkConquest.dll"));
        var method=helper.MainModule.GetType("SprkConquest").Methods.Single(m=>m.Name=="ReplayStatus");
        if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="GetStoppedBattleInstance") ||
            !method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="GetBattleCache") ||
            !method.Body.Instructions.Any(i=>i.Operand is FieldReference f && f.Name=="IsBattleEnded"))
            throw new InvalidOperationException("Battle heartbeat must observe the cache without stopping the replay.");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"));
        foreach(var name in new[]{"JM_NShared_PingReq","JM_NShared_PingIdleReq"}) {
            var encode=game.MainModule.GetType(name).Methods.Single(m=>m.Name=="EncodeJsonObject");
            if(encode.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).Any(i=>
                i.Previous?.Operand is not MethodReference m || m.DeclaringType.Name!="SprkConquest" || m.Name!="ReplayStatus"))
                throw new InvalidOperationException("Missing or duplicate battle heartbeat hook: "+name);
        }
        var clock=helper.MainModule.GetType("SprkConquest").Methods.Single(m=>m.Name=="ReadClock");
        if(!clock.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="SetServerTimeMs") ||
           clock.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="GetStoppedBattleInstance"))
            throw new InvalidOperationException("Battle log clocks must synchronize the running replay without stopping it.");
        var parse=game.MainModule.GetType("JM_NShared_BattleLogNot").Methods.Single(m=>m.Name=="Parse" &&
            m.Parameters.Count==2 && m.Parameters[0].ParameterType.Name=="IDictionary");
        if(parse.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkConquest" && m.Name=="ReadClock")!=1)
            throw new InvalidOperationException("Missing or duplicate continuous battle clock hook.");
        Console.WriteLine("Verified battle heartbeat observes replay completion without stopping combat.");
        Console.WriteLine("Verified continuous worker clock synchronizes native battle playback.");
    }
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
        var clockParser=module.GetType("JM_NShared_BattleLogNot").Methods.Single(m=>m.Name=="Parse" &&
            m.Parameters.Count==2 && m.Parameters[0].ParameterType.Name=="IDictionary");
        if(!Hooked(clockParser,"ReadClock"))foreach(var ret in clockParser.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
            ret.OpCode=OpCodes.Ldarg_0;var il=clockParser.Body.GetILProcessor();var call=Instruction.Create(OpCodes.Call,Method("ReadClock"));
            il.InsertAfter(ret,call);il.InsertAfter(call,Instruction.Create(OpCodes.Ret));
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
