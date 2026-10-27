using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void TestMissionWorker() {
        string Resource(string name) {using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name)!;using var reader=new StreamReader(input);return reader.ReadToEnd();}
        var syntax=CSharpSyntaxTree.ParseText(Resource("Conquest.cs.txt")).GetRoot();
        var method=syntax.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().Single(m=>m.Identifier.Text=="CaptureDefeats");
        var fixture=Resource("MissionWorkerTests.cs.txt").Replace("/* CAPTURE */",method.ToFullString());
        var compilation=CSharpCompilation.Create("MissionWorkerTests-"+Guid.NewGuid().ToString("N"),
            new[]{CSharpSyntaxTree.ParseText(fixture)},
            ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        System.Reflection.Assembly.Load(output.ToArray()).GetType("MissionWorkerFixture")!.GetMethod("Test")!.Invoke(null,null);
        Console.WriteLine("PASS: actual worker capture excludes live/friendly/arena actors and counts each enemy once per wave.");
    }
    static void VerifyMissionWorker(string clientRoot, bool staged=false) {
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkConquest.dll"+(staged?".staged":"")));
        var worker=helper.MainModule.GetType("SprkConquest");
        var run=worker.NestedTypes.Single(t=>t.Name=="Run");
        var capture=run.Methods.Single(m=>m.Name=="CaptureDefeats");
        var ops=capture.Body.Instructions;
        foreach(var name in new[]{"get_TeamId","get_Hp","get_MaxHp","get_Key","Add"})
            if(!ops.Any(i=>i.Operand is MethodReference m && m.Name==name))
                throw new InvalidOperationException("Worker defeat capture missing enemy/death/deduplication check: "+name);
        foreach(var method in new[]{run.Methods.Single(m=>m.Name=="OnProcessBattleLog"),worker.Methods.Single(m=>m.Name=="Finish")})
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==capture.Name))
                throw new InvalidOperationException("Worker must capture defeats at wave and battle completion.");
        if(!worker.Methods.Single(m=>m.Name=="Finish").Body.Instructions.Any(i=>i.OpCode==OpCodes.Ldstr && (string?)i.Operand=="DefeatedCreatures"))
            throw new InvalidOperationException("Worker mission defeat result missing.");
        if (!run.Fields.Any(f=>f.Name=="dispatch") ||
            !run.Methods.Single(m=>m.Name=="OnProcessBattleLog").Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="CreatureSetAiBattleLog"))
            throw new InvalidOperationException("Offline dispatch worker must enable native hero AI.");
        var prepare=worker.Methods.Single(m=>m.Name=="Prepare").Body.Instructions;
        foreach(var getter in new[]{"get_IsAi","get_IsSmartAi"})
            if(!prepare.Any(i=>i.Operand is MethodReference m && m.Name==getter))
                throw new InvalidOperationException("Dispatch creation must validate native simulation AI: "+getter);
        Console.WriteLine("Verified worker enemy defeats, wave capture, duplicate suppression and mission result.");
    }
    static void VerifyConquestSessionCache(string clientRoot,bool staged) {
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".conquest-staged":"")));
        foreach(var name in new[]{"JM_NShared_BeginCampaign_Response","JM_NShared_EndCampaign_Response"}) {
            var parse=game.MainModule.GetType(name).Methods.Single(m=>m.Name=="Parse" && m.Parameters.Count==2 && m.Parameters[0].ParameterType.FullName=="System.Collections.IDictionary");
            var calls=parse.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="RestoredPortal" && m.Name=="AcceptServerConquestSessions").ToArray();
            if(calls.Length!=1 || calls[0]!=parse.Body.Instructions[1] || parse.Body.Instructions[0].OpCode!=OpCodes.Ldarg_0)
                throw new InvalidOperationException("Battle response must cache server Conquest definitions once before parsing: "+name);
        }
        Console.WriteLine("Verified Conquest entry and result response session cache for invited guests.");
    }
    static void VerifyConquestPartyCleanup(string clientRoot,bool staged) {
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".conquest-staged":"")));
        var end=game.MainModule.GetType("NGame2.NUI.NManager.NBattle.EndCampaign");
        var lobby=end.Methods.Single(m=>m.Name=="ReturnToLobby" && m.Parameters.Count==0);
        bool Cleanup(Instruction i)=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkConquest" && m.Name=="LeaveSoloConquestOnLobbyReturn";
        if(lobby.Body.Instructions.Count(Cleanup)!=1 || !Cleanup(lobby.Body.Instructions[0]))
            throw new InvalidOperationException("Solo Conquest must leave exactly once before explicit result-to-lobby transition.");
        if(end.Methods.Where(m=>m.Name.Contains("Repeat") || m.Name.Contains("ExecReturnToLobbyAnd"))
            .Where(m=>m.HasBody).Any(m=>m.Body.Instructions.Any(Cleanup)))
            throw new InvalidOperationException("Repeat and formation/inventory return paths must preserve their party.");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkConquest.dll"+(staged?".staged":"")));
        var leave=helper.MainModule.GetType("SprkConquest").Methods.Single(m=>m.Name=="LeaveSoloConquestOnLobbyReturn");
        if(!leave.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="IsSoloConquestParty") ||
            !leave.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="SendLeaveParty"))
            throw new InvalidOperationException("Cleanup must use the native leave request and reset, scoped to solo Conquest.");
        Console.WriteLine("Verified solo Conquest result-to-lobby cleanup; repeat and cooperative paths preserved.");
    }
    static void VerifyArcdimWorker(string clientRoot) {
        var managed=Path.Combine(clientRoot,"King's Raid_Data","Managed");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkConquest.dll"));
        var prepare=helper.MainModule.GetType("SprkConquest").Methods.Single(m=>m.Name=="Prepare");
        if(!prepare.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="SetExtraData")
            || !prepare.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="set_AffixIndices"))
            throw new InvalidOperationException("Challenge Raid worker must apply server-validated affixes before building the battle.");
        if(!helper.MainModule.GetType("SprkConquest").Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions)
            .Any(i=>i.OpCode==OpCodes.Ldstr && (string?)i.Operand=="ChallengeRaidVersion"))
            throw new InvalidOperationException("Challenge Raid worker capability handshake missing.");
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
        TestMissionWorker();
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
        // Invitations do not open the board, so guests also seed the server
        // session cache from battle responses before native DTO handling.
        var acceptSessions=module.GetType("RestoredPortal").Methods.Single(m=>m.Name=="AcceptServerConquestSessions");
        foreach(var name in new[]{"JM_NShared_BeginCampaign_Response","JM_NShared_EndCampaign_Response"}) {
            var parse=module.GetType(name).Methods.Single(m=>m.Name=="Parse" && m.Parameters.Count==2 && m.Parameters[0].ParameterType.FullName=="System.Collections.IDictionary");
            if(!parse.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="AcceptServerConquestSessions")) {
                var il=parse.Body.GetILProcessor();var parseStart=parse.Body.Instructions[0];
                il.InsertBefore(parseStart,Instruction.Create(OpCodes.Ldarg_0));il.InsertBefore(parseStart,Instruction.Create(OpCodes.Call,acceptSessions));
            }
        }
        var returnToLobby=module.GetType("NGame2.NUI.NManager.NBattle.EndCampaign").Methods.Single(m=>m.Name=="ReturnToLobby" && m.Parameters.Count==0);
        if(!Hooked(returnToLobby,"LeaveSoloConquestOnLobbyReturn"))
            returnToLobby.Body.GetILProcessor().InsertBefore(returnToLobby.Body.Instructions[0],Instruction.Create(OpCodes.Call,Method("LeaveSoloConquestOnLobbyReturn")));
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
        VerifyMissionWorker(clientRoot,true);
        if(!stageOnly) {
            if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,clientRoot)))
                throw new InvalidOperationException("Conquest patch staged. Close game clients and shared-asset workers before installation.");
            File.Copy(dll,dll+".before-conquest-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));assembly.Dispose();
            File.Move(staged,dll,true);File.Move(Path.Combine(managed,"SprkConquest.dll.staged"),Path.Combine(managed,"SprkConquest.dll"),true);
            VerifyMissionWorker(clientRoot);
        }
        Console.WriteLine(stageOnly?"Staged Conquest native combat worker and multiplayer hooks":"Installed Conquest native combat worker and multiplayer hooks");
    }
}
