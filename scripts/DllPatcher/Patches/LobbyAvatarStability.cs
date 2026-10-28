using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
namespace DllPatcher;
partial class Program {
    // The full build tests staged and installed copies of the same helper. Reuse
    // their assembly identity so DispatchProxy cannot bind a second client copy.
    static readonly Dictionary<string,Assembly> LobbyTestAssemblies=new();
    static readonly Dictionary<string,Assembly> LobbyReconciliationAssemblies=new();
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
        foreach(var method in new[]{create,map.Methods.Single(m=>m.Name=="ChangePlayerAvatar"),map.Methods.Single(m=>m.Name=="ChangePlayerAvatarRenderFacadeOnly")}) {
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkLobbyAvatar" && r.Name=="Track"))continue;
            var il=method.Body.GetILProcessor();var ret=method.Body.Instructions.Last();
            if(ret.OpCode!=OpCodes.Ret)throw new Exception("Unknown lobby avatar completion");
            il.InsertBefore(ret,il.Create(OpCodes.Ldarg_0));il.InsertBefore(ret,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(ret,il.Create(OpCodes.Call,map.Methods.Single(m=>m.Name=="IsForcedHeroAvatar")));
            il.InsertBefore(ret,il.Create(OpCodes.Call,Hook("Track")));
        }
        var observer=map.Methods.Single(m=>m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="NGame2.NAccount.UserHeroManager/ChangeHeroInfo");
        observer.Body=new Mono.Cecil.Cil.MethodBody(observer);
        var notify=observer.Body.GetILProcessor();
        notify.Emit(OpCodes.Ldarg_0);notify.Emit(OpCodes.Ldarg_1);notify.Emit(OpCodes.Ldarg_0);
        notify.Emit(OpCodes.Call,map.Methods.Single(m=>m.Name=="IsForcedHeroAvatar"));
        notify.Emit(OpCodes.Call,Hook("Notify"));notify.Emit(OpCodes.Ret);
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
        foreach(var name in new[]{"CreatePlayerAvatar","ChangePlayerAvatar","ChangePlayerAvatarRenderFacadeOnly"}) {
            var method=map.Methods.Single(m=>m.Name==name);
            var track=method.Body.Instructions.Single(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkLobbyAvatar" && r.Name=="Track");
            if(track.Previous.Operand is not MethodReference forced || forced.Name!="IsForcedHeroAvatar" || track.Next.OpCode!=OpCodes.Ret)
                throw new Exception("Initial/replaced lobby avatar reconciliation or forced-avatar guard missing");
            VerifyArenaResultStack(method);
        }
        var observer=map.Methods.Single(m=>m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="NGame2.NAccount.UserHeroManager/ChangeHeroInfo");
        if(observer.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkLobbyAvatar" && r.Name=="Notify")!=1 || observer.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="ChangePlayerAvatarRenderFacadeOnly"))throw new Exception("Lobby appearance notification must defer and coalesce refreshes");
        if(!observer.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="IsForcedHeroAvatar"))throw new Exception("Lobby appearance forced-avatar guard missing");
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
        foreach(var name in new[]{"get_IsDone","get_IsValid","ChangeCustomizeStock","Clone","TryAssignCustomizeStock","GetCreatureOpenData","GetStock","GetCutomizeItemInstance","Observe","IsAppearanceChange","Invoke"})if(!calls.Any(r=>r.Name==name))throw new Exception("Missing lobby safeguard "+name);
        var create=helper.MainModule.GetType("SprkLobbyAvatar").Methods.Single(m=>m.Name=="Create");
        if(!create.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="CreatePlayerAvatarHeroInfo" && i.Previous.OpCode==OpCodes.Ldnull))throw new Exception("Lobby appearance still uses a saved costume snapshot");
        var matches=helper.MainModule.GetType("SprkLobbyAccessoryLease").Methods.Single(m=>m.Name=="Matches");
        if(!matches.Body.Instructions.Any(i=>i.OpCode==OpCodes.Ceq || i.Operand is MethodReference r && r.Name=="ReferenceEquals"))throw new Exception("Lobby refresh must compare owner identity");
        if(calls.Any(r=>r.Name=="ReleaseRenderFacade" || r.Name=="CreateRenderFacade"))throw new Exception("Accessory refresh must retain the live hero body");
        Console.WriteLine("Verified initial/replaced lobby avatar reconciliation, live accessory stocks/instances, account-bound updates, ready-load checks, cloned transforms and retained hero body.");
    }
    static void TestLobbyAvatarStability(string root,bool staged=false) {
        var path=Path.Combine(root,"King's Raid_Data","Managed","SprkLobbyAvatarStability.dll"+(staged?".staged":""));
        var bytes=File.ReadAllBytes(path);var key=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
        if(!LobbyTestAssemblies.TryGetValue(key,out var asm))LobbyTestAssemblies[key]=asm=Assembly.Load(bytes);
        var t=asm.GetType("SprkLobbyAccessoryLease")!;var lease=Activator.CreateInstance(t)!;
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
        TestLobbyAccessoryReconciliation(root,bytes);
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".lobby-avatar-stability-staged":"")));
        using var helper=AssemblyDefinition.ReadAssembly(path);
        int Count()=>game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
        var before=Count();PatchLobbyAvatarStability(game.MainModule,helper.MainModule);if(before!=Count())throw new Exception("Lobby patch is not idempotent");
        Console.WriteLine("PASS: readiness deferral, coalescing, A-B-A account isolation, stale event rejection, disable cancellation and idempotence.");
    }
    static void TestLobbyAccessoryReconciliation(string root,byte[] helperBytes) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var key=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(helperBytes));
        Assembly? nativeFixture=null;
        var nativeName="SPRK.LobbyAccessory.Native."+key;
        ResolveEventHandler resolve=(_,e)=>{
            var name=new AssemblyName(e.Name).Name;
            if(name==nativeName)return nativeFixture;
            var p=Path.Combine(managed,name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;
        };
        AppDomain.CurrentDomain.AssemblyResolve+=resolve;
        try {
            if(!LobbyReconciliationAssemblies.TryGetValue(key,out var helper)) {
                // Give this private client copy a unique identity. Other full-build
                // fixtures also use DispatchProxy against their own client copies;
                // identical assembly names cause Reflection.Emit to bind the wrong one.
                using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
                using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"),new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
                using var input=new MemoryStream(helperBytes);using var bridge=AssemblyDefinition.ReadAssembly(input);
                var helperName="SPRK.LobbyAccessory.Helper."+key;
                game.Name.Name=nativeName;bridge.Name.Name=helperName;
                foreach(var module in new[]{game.MainModule,bridge.MainModule})foreach(var reference in module.AssemblyReferences) {
                    if(reference.Name=="Assembly-CSharp")reference.Name=nativeName;
                    if(reference.Name=="SprkLobbyAvatarStability")reference.Name=helperName;
                }
                using var gameBytes=new MemoryStream();game.Write(gameBytes);nativeFixture=Assembly.Load(gameBytes.ToArray());
                using var bridgeBytes=new MemoryStream();bridge.Write(bridgeBytes);helper=Assembly.Load(bridgeBytes.ToArray());
                LobbyReconciliationAssemblies[key]=helper;
            }
            var reconcile=helper.GetType("SprkLobbyAvatar")!.GetMethod("Reconcile")!;
            var native=reconcile.GetParameters()[0].ParameterType.Assembly;
            TestLobbyAppearance(helper,native);
            var stockType=native.GetType("NGameAsset.CustomizeStock")!;
            var creatureStockType=native.GetType("NGameAsset.CreatureStock")!;
            object Proxy(string name,Func<MethodInfo,object?[]?,object?> handler) {
                var p=DispatchProxy.Create(native.GetType(name)!,typeof(LobbyAccessoryProbe));((LobbyAccessoryProbe)p).Handler=handler;return p;
            }
            object Accessory(int index) {
                var stock=Activator.CreateInstance(stockType)!;
                stockType.GetField("Part")!.SetValue(stock,Enum.ToObject(stockType.GetField("Part")!.FieldType,1));
                stockType.GetField("AccessoryIndex")!.SetValue(stock,index);
                stockType.GetField("Code")!.SetValue(stock,index==0?"":"Accessory_"+index);
                stockType.GetField("Scale")!.SetValue(stock,1f);return stock;
            }
            foreach(var hero in new[]{"Arch","Esker"})foreach(var weaponPrefix in new[]{"","CostumeA","CostumeC"}) {
                object Stock() {
                    var s=Activator.CreateInstance(creatureStockType)!;
                    foreach(var (name,value) in new[]{("Code",hero),("BodyPrefix","CostumeA"),("HairPrefix","HairB")})creatureStockType.GetField(name)!.SetValue(s,value);
                    var weapon=creatureStockType.GetField("WeaponStock")!.GetValue(s)!;
                    weapon.GetType().GetField("WeaponPrefix")!.SetValue(weapon,weaponPrefix);creatureStockType.GetField("WeaponStock")!.SetValue(s,weapon);return s;
                }
                var requested=Stock();var live=Stock();
                var original=Accessory(101);creatureStockType.GetMethod("SetCustomizeStock")!.Invoke(requested,new[]{original});
                bool ready=false,valid=true,instancePresent=false,allowCreation=true;int changes=0;
                var instance=Proxy("NGameAsset.NCreature.ITransformInstnace",(_,_)=>throw new Exception("Unexpected accessory transform mutation"));
                var open=Proxy("NGameAsset.ICreatureOpenData",(m,_)=>m.Name=="GetStock"?live:throw new Exception("Unexpected creature open-data operation "+m.Name));
                var creature=Proxy("NGameAsset.ICreature",(m,_)=>m.Name switch {
                    "GetCreatureOpenData"=>open,"GetCutomizeItemInstance"=>instancePresent?instance:null,_=>throw new Exception("Unexpected body/weapon mutation "+m.Name)});
                var load=Proxy("NGameAsset.ICreatureLoad",(m,args)=>m.Name switch {
                    "get_IsDone"=>ready,"get_IsValid"=>valid,"get_Value"=>creature,"get_Stock"=>requested,
                    "ChangeCustomizeStock"=>Change(args![0]!),_=>throw new Exception("Unexpected creature load mutation "+m.Name)});
                object? Change(object stock) {
                    changes++;creatureStockType.GetMethod("SetCustomizeStock")!.Invoke(live,new[]{stock});
                    instancePresent=allowCreation && (int)stockType.GetField("AccessoryIndex")!.GetValue(stock)! !=0;return null;
                }
                bool Apply(object stock) {
                    var desired=Array.CreateInstance(stockType,1);desired.SetValue(stock,0);
                    return (bool)reconcile.Invoke(null,new[]{load,desired})!;
                }
                if(Apply(original)||changes!=0)throw new Exception("Lobby accessory applied before the model was ready");
                ready=true;valid=false;if(Apply(original)||changes!=0)throw new Exception("Lobby accessory applied to an invalid model");valid=true;
                allowCreation=false;if(Apply(original)||changes!=1)throw new Exception("Missing delayed accessory instance was treated as complete");
                allowCreation=true;changes=0;live=Stock();
                if(!Apply(original)||changes!=1||!instancePresent)throw new Exception("Initial lobby accessory was lost: "+hero+"/"+weaponPrefix);
                if(!Apply(original)||changes!=1)throw new Exception("Stable lobby accessory was reapplied");
                instancePresent=false;if(!Apply(original)||changes!=2)throw new Exception("Missing rendered accessory instance was not repaired");
                var replacement=Accessory(102);if(!Apply(replacement)||changes!=3||!Apply(replacement)||changes!=3)
                    throw new Exception("Live accessory stock was confused with the original load request");
                var empty=Accessory(0);if(!Apply(empty)||changes!=4||instancePresent||!Apply(empty)||changes!=4)throw new Exception("Accessory unequip did not converge");
                foreach(var s in new[]{requested,live}) {
                    var weapon=creatureStockType.GetField("WeaponStock")!.GetValue(s)!;
                    if((string)creatureStockType.GetField("BodyPrefix")!.GetValue(s)! !="CostumeA" ||
                       (string)creatureStockType.GetField("HairPrefix")!.GetValue(s)! !="HairB" ||
                       (string)weapon.GetType().GetField("WeaponPrefix")!.GetValue(weapon)! !=weaponPrefix)
                        throw new Exception("Lobby repair changed body, hair or weapon appearance");
                }
            }
            Console.WriteLine("PASS: native accessory reconciliation for Arch/Esker, basic and two costume weapons; readiness, missing live instances, stale request stock, replacement, unequip and stable no-op.");
        } finally {AppDomain.CurrentDomain.AssemblyResolve-=resolve;}
    }
    static void TestLobbyAppearance(Assembly helper,Assembly native) {
        var heroType=native.GetType("NShared.CachedHeroInfo")!;
        var hero=Activator.CreateInstance(heroType)!;
        void Set(string name,object value)=>heroType.GetProperty(name)!.SetValue(hero,value);
        Set("HeroIndex",1);Set("CostumeIndex",101);Set("HairCostumeIndex",201);Set("WeaponCostumeIndex",301);
        var appearanceType=helper.GetType("SprkLobbyAppearance")!;
        var appearance=Activator.CreateInstance(appearanceType)!;
        int Observe()=>(int)appearanceType.GetMethod("Observe")!.Invoke(appearance,new[]{hero})!;
        if(Observe()!=0 || Observe()!=0)throw new Exception("Initial/stable lobby appearance must not rebuild the model");
        foreach(var (field,value) in new[]{("CostumeIndex",102),("HairCostumeIndex",202),("WeaponCostumeIndex",0),("WeaponCostumeIndex",302)}) {
            Set(field,value);if(Observe()!=1 || Observe()!=0)throw new Exception("Live lobby appearance change not detected/coalesced: "+field);
        }
        Set("HideUniqueWeapon",(byte)1);if(Observe()!=1 || Observe()!=0)throw new Exception("Basic/unique weapon visibility not synchronized");
        var partType=native.GetType("NShared.AccessoryPartType")!;
        var positionType=native.GetType("NShared.PositionInfo")!;
        var position=positionType.GetMethod("Create")!.Invoke(null,null)!;
        var setter=heroType.GetMethod("SetAccessoryCostume")!;
        setter.Invoke(hero,new[]{Enum.ToObject(partType,1),(object)101,position});
        if(Observe()!=2 || Observe()!=0)throw new Exception("Accessory equip must update parts without rebuilding the body");
        foreach(var field in new[]{"PositionX","PositionY","PositionZ","RotationX","RotationY","RotationZ","Scale"}) {
            positionType.GetProperty(field)!.SetValue(position,2f);
            if(Observe()!=2 || Observe()!=0)throw new Exception("In-place accessory transform edits not detected: "+field);
        }
        setter.Invoke(hero,new[]{Enum.ToObject(partType,1),(object)0,null});
        if(Observe()!=2 || Observe()!=0)throw new Exception("Accessory removal did not synchronize");
        Set("Level",(short)10);if(Observe()!=0)throw new Exception("Nonvisual hero updates rebuilt the lobby");
        var changeType=native.GetType("NGame2.NAccount.UserHeroManager+ChangeHeroInfo")!;
        var change=Activator.CreateInstance(changeType)!;
        var heroIndex=changeType.GetField("HeroIndex")!;
        var kind=changeType.GetField("ChangeType")!;
        var filter=helper.GetType("SprkLobbyAvatar")!.GetMethod("IsAppearanceChange")!;
        bool Matches()=>(bool)filter.Invoke(null,new[]{change,(object)1})!;
        foreach(var name in new[]{"Costume","HairCostume","WeaponCostume","AccessoryCostume","HideUniqueWeapon"}) {
            heroIndex.SetValue(change,1);kind.SetValue(change,Enum.Parse(kind.FieldType,name));
            if(!Matches())throw new Exception("Missing lobby notification for "+name);
            heroIndex.SetValue(change,2);if(Matches())throw new Exception("Other hero's appearance affected the lobby");
        }
        Console.WriteLine("PASS: live body/hair/basic and costume weapon changes, weapon visibility, accessory equip/unequip and all seven transform edits; snapshot isolation, stable no-op and selected-hero notification scope.");
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
public class LobbyAccessoryProbe : DispatchProxy {
    public Func<MethodInfo,object?[]?,object?>? Handler;
    protected override object? Invoke(MethodInfo? method,object?[]? args)=>Handler!(method!,args);
}
