using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void PatchLobbyAvatarStability(ModuleDefinition game,ModuleDefinition helper) {
        var map=game.GetType("NGame2.NUI.NWorldMap.MapLayout");var hooks=helper.GetType("SprkLobbyAvatar");
        MethodReference Hook(string name)=>game.ImportReference(hooks.Methods.Single(m=>m.Name==name));
        var builder=map.Methods.Single(m=>m.Name=="CreateAvatarHeroRenderFacadeBuilder");
        var clone=builder.Body.Instructions.Single(i=>i.Operand is MethodReference r && ((r.DeclaringType.Name=="SprkAccounts" && r.Name=="CreateLobbyAvatarHeroInfo") || (r.DeclaringType.Name=="SprkLobbyAvatar" && r.Name=="Create") || (r.DeclaringType.Name=="Customizing_Extension_Method" && r.Name=="CreatePlayerAvatarHeroInfo")));
        clone.Operand=Hook("Create");clone.OpCode=OpCodes.Call;
        var create=map.Methods.Single(m=>m.Name=="CreatePlayerAvatar");
        if(!create.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkLobbyAvatar" && r.Name=="Bind")) {
            var il=create.Body.GetILProcessor();var first=create.Body.Instructions[0];il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));il.InsertBefore(first,il.Create(OpCodes.Call,Hook("Bind")));
        }
        var observer=map.Methods.Single(m=>m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="NGame2.NAccount.UserHeroManager/ChangeHeroInfo");
        foreach(var i in observer.Body.Instructions.Where(i=>i.Operand is MethodReference r && r.Name=="ChangePlayerAvatarRenderFacadeOnly").ToArray()){i.Operand=Hook("Refresh");i.OpCode=OpCodes.Call;}
        // An asset completion can remain queued after Close released its OpenData.
        var load=game.GetType("NGameAsset.NCreature.CreatureLoad");
        var complete=load.Methods.Single(m=>m.Name=="OnCompleteAsset");
        if(complete.Body.Instructions[0].OpCode!=OpCodes.Ldarg_0 || complete.Body.Instructions[1].Operand is not FieldReference f || f.Name!="_isClosed") {
            var il=complete.Body.GetILProcessor();var first=complete.Body.Instructions[0];
            foreach(var i in new[]{il.Create(OpCodes.Ldarg_0),il.Create(OpCodes.Ldfld,load.Fields.Single(f=>f.Name=="_isClosed")),il.Create(OpCodes.Brfalse,first),il.Create(OpCodes.Ret)})il.InsertBefore(first,i);
        }
        VerifyLobbyAvatarStability(game);
    }
    static void VerifyLobbyAvatarStability(ModuleDefinition game) {
        var map=game.GetType("NGame2.NUI.NWorldMap.MapLayout");
        foreach(var pair in new[]{("CreateAvatarHeroRenderFacadeBuilder","Create"),("CreatePlayerAvatar","Bind")}) {
            var m=map.Methods.Single(m=>m.Name==pair.Item1);
            if(m.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkLobbyAvatar" && r.Name==pair.Item2)!=1)throw new Exception("Missing/duplicated lobby hook "+pair);
            VerifyArenaResultStack(m);
        }
        var observer=map.Methods.Single(m=>m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="NGame2.NAccount.UserHeroManager/ChangeHeroInfo");
        if(observer.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkLobbyAvatar" && r.Name=="Refresh")!=1 || observer.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="ChangePlayerAvatarRenderFacadeOnly"))throw new Exception("Accessory notification still replaces the whole lobby hero");
        foreach(var name in new[]{"ShouldRefreshLobbyAccessories","IsForcedHeroAvatar"})if(!observer.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name==name))throw new Exception("Lobby accessory scope missing "+name);
        VerifyArenaResultStack(observer);
        var complete=game.GetType("NGameAsset.NCreature.CreatureLoad").Methods.Single(m=>m.Name=="OnCompleteAsset");
        var code=complete.Body.Instructions;
        if(code[0].OpCode!=OpCodes.Ldarg_0 || code[1].OpCode!=OpCodes.Ldfld || code[1].Operand is not FieldReference f || f.Name!="_isClosed" || (code[2].OpCode!=OpCodes.Brfalse && code[2].OpCode!=OpCodes.Brfalse_S) || code[3].OpCode!=OpCodes.Ret || code[2].Operand!=code[4])throw new Exception("Closed creature load completion is not rejected: "+string.Join(";",code.Take(8)));
        VerifyArenaResultStack(complete);
    }
    static void VerifyLobbyAvatarStabilityFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".lobby-avatar-stability-staged":"")));
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkLobbyAvatarStability.dll"+(staged?".staged":"")));
        VerifyLobbyAvatarStability(game.MainModule);
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"get_IsDone","get_IsValid","ChangeCustomizeStock","Clone","TryAssignCustomizeStock","get_AccountId"})if(!calls.Any(r=>r.Name==name))throw new Exception("Missing lobby safeguard "+name);
        var matches=helper.MainModule.GetType("SprkLobbyAccessoryLease").Methods.Single(m=>m.Name=="Matches");
        if(!matches.Body.Instructions.Any(i=>i.OpCode==OpCodes.Ceq || i.Operand is MethodReference r && r.Name=="ReferenceEquals"))throw new Exception("Lobby refresh must compare owner identity");
        if(calls.Any(r=>r.Name=="ReleaseRenderFacade" || r.Name=="CreateRenderFacade"))throw new Exception("Accessory refresh must retain the live hero body");
        Console.WriteLine("Verified account-bound lobby accessory updates, ready-load checks, cloned transforms, retained hero body and rejected closed asset callbacks.");
    }
    static void TestLobbyAvatarStability(string root,bool staged=false) {
        var path=Path.Combine(root,"King's Raid_Data","Managed","SprkLobbyAvatarStability.dll"+(staged?".staged":""));
        var asm=System.Reflection.Assembly.Load(File.ReadAllBytes(path));var t=asm.GetType("SprkLobbyAccessoryLease")!;var lease=Activator.CreateInstance(t)!;
        void Bind(object? owner)=>t.GetMethod("Bind")!.Invoke(lease,new[]{owner});
        bool Queue(object owner)=>(bool)t.GetMethod("Queue")!.Invoke(lease,new[]{owner})!;
        bool Take(object owner,bool ready)=>(bool)t.GetMethod("Take")!.Invoke(lease,new object[]{owner,ready})!;
        var a=new object();var b=new object();var aAgain=new object();
        Bind(a);if(!Queue(a)||Take(a,false))throw new Exception("Accessory update was not deferred until model readiness");
        if(!Queue(a)||!Take(a,true)||Take(a,true))throw new Exception("Accessory updates were not coalesced");
        Queue(a);Bind(b);if(Queue(a)||Take(a,true)||Take(b,true))throw new Exception("Previous account update survived switching");
        if(!Queue(b)||!Take(b,true))throw new Exception("New account update was rejected");
        Queue(b);Bind(aAgain);if(Queue(a)||Queue(b)||Take(aAgain,true))throw new Exception("A-B-A switch reused a prior session");
        Queue(aAgain);t.GetMethod("Cancel")!.Invoke(lease,null);if(Take(aAgain,true)||!Queue(aAgain))throw new Exception("Disable cancellation broke same-account reactivation");
        Queue(aAgain);Bind(null);if(Take(aAgain,true)||Queue(aAgain))throw new Exception("Disabled lobby retained a pending update");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".lobby-avatar-stability-staged":"")));
        using var helper=AssemblyDefinition.ReadAssembly(path);
        int Count()=>game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
        var before=Count();PatchLobbyAvatarStability(game.MainModule,helper.MainModule);if(before!=Count())throw new Exception("Lobby patch is not idempotent");
        Console.WriteLine("PASS: readiness deferral, coalescing, A-B-A account isolation, stale event rejection, disable cancellation and idempotence.");
    }
    static void InstallLobbyAvatarStability(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkLobbyAvatarStability.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.LobbyAvatarStability.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkLobbyAvatarStability",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        PatchLobbyAvatarStability(game.MainModule,helper.MainModule);
        var staged=dll+".lobby-avatar-stability-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyLobbyAvatarStabilityFile(root,true);TestLobbyAvatarStability(root,true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        var backup=dll+".before-lobby-avatar-stability-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        File.Copy(dll,backup);
        if (File.Exists(helperPath)) File.Copy(helperPath,helperPath+".before-lobby-avatar-stability-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyLobbyAvatarStabilityFile(root);
        Console.WriteLine("Installed lobby avatar account-switch safeguards. Restart the client.");
    }
}
