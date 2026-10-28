using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace DllPatcher;
partial class Program {
    static MethodDefinition TechnomagicTierCallback(ModuleDefinition module) => module.GetTypes()
        .Where(t=>t.FullName.Contains("ValanceWebManager/"))
        .SelectMany(t=>t.Methods).Single(m=>m.HasBody && m.Name.Contains("<RequestValanceTierUpgrade>") &&
            m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="NShared.UpgradeEquipTier.Response");

    static void PatchTechnomagicRefresh(ModuleDefinition module) {
        var callback=TechnomagicTierCallback(module);
        var code=callback.Body.Instructions;
        var notify=code.Single(i=>i.Operand is MethodReference m && m.Name=="ForEach");
        var lastApply=code.Last(i=>i.Operand is MethodReference m && m.Name=="Apply");
        if(code.IndexOf(notify)<code.IndexOf(lastApply)) {
            // Keep native error exits and closure payloads; notify only after all
            // currency, inventory, equipped-hero and preset updates are applied.
            var first=notify;for(int i=0;i<6;i++)first=first.Previous;
            var block=code.Skip(code.IndexOf(first)).Take(7).ToArray();
            if(first.OpCode!=OpCodes.Ldarg_0 || !block.Any(i=>i.Operand is FieldReference f && f.Name=="_listListener") ||
               !block.Any(i=>i.OpCode==OpCodes.Ldftn))throw new InvalidOperationException("Unknown TM tier response notification block");
            var il=callback.Body.GetILProcessor();var ret=code.Last();
            if(ret.OpCode!=OpCodes.Ret)throw new InvalidOperationException("Unknown TM tier response return");
            foreach(var instruction in block) {
                var copy=Instruction.Create(OpCodes.Nop);copy.OpCode=instruction.OpCode;copy.Operand=instruction.Operand;
                il.InsertBefore(ret,copy);instruction.OpCode=OpCodes.Nop;instruction.Operand=null;
            }
        }
        // Widen existing branches before serializing the reordered callback.
        foreach(var i in code.Where(i=>i.OpCode.OperandType==OperandType.ShortInlineBrTarget))
            i.OpCode=typeof(OpCodes).GetFields().Select(f=>f.GetValue(null)).OfType<OpCode>().Single(c=>c.Name==i.OpCode.Name[..^2]);

        var management=module.GetType("NGame2.NUI.NManager.ValanceManagement");
        var listener=management.Methods.Single(m=>m.Name.EndsWith("IListener.OnValanceTierUpgrade"));
        var window=module.GetType("NGame2.NUI.NWindow.ValanceTierUpgradeWindow");
        var get=management.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand)
            .OfType<GenericInstanceMethod>().First(m=>m.Name=="GetInstance" && m.GenericArguments.Single().FullName==window.FullName);
        var isOpen=management.Methods.Single(m=>m.Name=="OpenTierUpgradeResult").Body.Instructions
            .Select(i=>i.Operand).OfType<MethodReference>().Single(m=>m.Name=="IsOpen");
        listener.Body=new MethodBody(listener){InitLocals=true};
        listener.Body.Variables.Add(new VariableDefinition(window));
        var body=listener.Body.GetILProcessor();var result=Instruction.Create(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldarg_0);body.Emit(OpCodes.Ldc_I4_0);body.Emit(OpCodes.Call,get);body.Emit(OpCodes.Stloc_0);
        body.Emit(OpCodes.Ldloc_0);body.Emit(OpCodes.Brfalse,result);
        body.Emit(OpCodes.Ldloc_0);body.Emit(OpCodes.Callvirt,isOpen);body.Emit(OpCodes.Brfalse,result);
        // Refresh rebuilds hero/inventory icons and clears the old 5-star target.
        body.Emit(OpCodes.Ldloc_0);body.Emit(OpCodes.Callvirt,window.Methods.Single(m=>m.Name=="Refresh"));
        body.Append(result);body.Emit(OpCodes.Ldarg_1);body.Emit(OpCodes.Ldarg_2);
        body.Emit(OpCodes.Call,management.Methods.Single(m=>m.Name=="OpenTierUpgradeResult"));body.Emit(OpCodes.Ret);
        VerifyTechnomagicRefresh(module);
    }

    static void VerifyTechnomagicRefresh(ModuleDefinition module) {
        var callback=TechnomagicTierCallback(module);var code=callback.Body.Instructions;
        var notify=code.Single(i=>i.Operand is MethodReference m && m.Name=="ForEach");
        if(code.IndexOf(notify)<=code.IndexOf(code.Last(i=>i.Operand is MethodReference m && m.Name=="Apply")) || notify.Next.OpCode!=OpCodes.Ret)
            throw new InvalidOperationException("TM tier UI notified before account state was applied");
        foreach(var name in new[]{"get_CurrencyResults","get_ItemResults","SetEquipItemInfo","ApplyEquippedHeroIndex","get_EquipPresetInfos"})
            if(!code.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Missing native TM result update: "+name);
        if(code.Count(i=>i.Operand is MethodReference m && m.Name=="OpenError")!=2)
            throw new InvalidOperationException("TM response error guards missing");
        var listener=module.GetType("NGame2.NUI.NManager.ValanceManagement").Methods.Single(m=>m.Name.EndsWith("IListener.OnValanceTierUpgrade"));
        var calls=listener.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        if(calls.Count(m=>m.Name=="Refresh" && m.DeclaringType.Name=="ValanceTierUpgradeWindow")!=1 ||
           calls.Count(m=>m.Name=="IsOpen")!=1 || calls.Last().Name!="OpenTierUpgradeResult")
            throw new InvalidOperationException("TM tier window refresh/result sequence missing");
        var refresh=module.GetType("NGame2.NUI.NWindow.ValanceTierUpgradeWindow").Methods.Single(m=>m.Name=="Refresh");
        if(!refresh.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ClearSeletedTarget") ||
           !refresh.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="Refresh" && m.DeclaringType.Name=="ValanceEnhanceBase"))
            throw new InvalidOperationException("Native TM target/list refresh missing");
        VerifyArenaResultStack(callback);VerifyArenaResultStack(listener);
        Console.WriteLine("Verified TM tier account updates before UI notification, open-window icon rebuild, stale-target clearing and native error/result paths.");
    }

    static void InstallTechnomagicRefresh(string root,bool stageOnly,bool verifyOnly=false) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        if(verifyOnly){VerifyTechnomagicRefresh(game.MainModule);return;}
        PatchTechnomagicRefresh(game.MainModule);
        int count=TechnomagicTierCallback(game.MainModule).Body.Instructions.Count;
        PatchTechnomagicRefresh(game.MainModule);
        if(count!=TechnomagicTierCallback(game.MainModule).Body.Instructions.Count)throw new InvalidOperationException("TM refresh patch is not idempotent");
        TestTechnomagicTierCallback(game,managed);
        var staged=dll+".technomagic-refresh-staged";game.Write(staged);
        using(var check=AssemblyDefinition.ReadAssembly(staged))VerifyTechnomagicRefresh(check.MainModule);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-technomagic-refresh-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
        File.Move(staged,dll,true);
        Console.WriteLine("Installed TM tier-upgrade UI refresh. Restart the client.");
    }

    static void TestTechnomagicTierCallback(AssemblyDefinition game,string managed) {
        // Run the actual response callback and its native delegate/ForEach. Replace
        // external account/UI services in a private in-memory copy with trace sinks.
        using var bytes=new MemoryStream();game.Write(bytes);bytes.Position=0;
        using var fixture=AssemblyDefinition.ReadAssembly(bytes,new ReaderParameters{AssemblyResolver=game.MainModule.AssemblyResolver});
        var module=fixture.MainModule;var callback=TechnomagicTierCallback(module);
        var account=module.GetType("NGame2.NAccount.AccountManager");
        var trace=new FieldDefinition("SPRKFixtureTrace",Mono.Cecil.FieldAttributes.Public|Mono.Cecil.FieldAttributes.Static,module.TypeSystem.String);
        account.Fields.Add(trace);
        void Sink(MethodDefinition method,string tag) {
            method.Body=new MethodBody(method);var il=method.Body.GetILProcessor();
            var concat=new MethodReference("Concat",module.TypeSystem.String,module.TypeSystem.String);
            concat.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));concat.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
            il.Emit(OpCodes.Ldsfld,trace);il.Emit(OpCodes.Ldstr,tag);il.Emit(OpCodes.Call,concat);il.Emit(OpCodes.Stsfld,trace);il.Emit(OpCodes.Ret);
        }
        var create=module.GetType("NVespa.NSingleton.GlobalAutoStatic`1").Methods.Single(m=>m.Name=="CreateInstance");
        create.Body=new MethodBody(create);create.Body.GetILProcessor().Emit(OpCodes.Ret);
        foreach(var name in new[]{"UserManager","ItemManager","EquipItemManager","EquipPresetSotrageManger"}) {
            var getter=account.Methods.Single(m=>m.Name=="get_"+name);
            var field=new FieldDefinition("SPRKFixture"+name,Mono.Cecil.FieldAttributes.Public|Mono.Cecil.FieldAttributes.Static,getter.ReturnType);
            account.Fields.Add(field);getter.Body=new MethodBody(getter);var il=getter.Body.GetILProcessor();il.Emit(OpCodes.Ldsfld,field);il.Emit(OpCodes.Ret);
        }
        var owner=account.Methods.Single(m=>m.Name=="GetHeroOfEquipItem" && m.Parameters.Count==1 && m.Parameters[0].ParameterType.MetadataType==MetadataType.Int32);
        owner.Body=new MethodBody(owner);owner.Body.GetILProcessor().Emit(OpCodes.Ldnull);owner.Body.GetILProcessor().Emit(OpCodes.Ret);
        foreach(var reference in callback.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Where(m=>m.Name is "Apply" or "OpenError")) {
            var method=module.GetType(reference.DeclaringType.FullName).Methods.Single(m=>m.FullName==reference.FullName);
            string tag=reference.DeclaringType.Name switch {
                "UserManager"=>"Currency;","UserItemManager"=>"Items;","UserEquipItemManager"=>reference.Parameters.Count==2?"Remove;":"Equip;",
                "UserEquipPresetStorageManager"=>"Presets;",_=>"Error;"};
            Sink(method,tag);
        }
        using var output=new MemoryStream();fixture.Write(output);
        ResolveEventHandler resolve=(_,e)=>{var path=Path.Combine(managed,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
        AppDomain.CurrentDomain.AssemblyResolve+=resolve;
        try {
            var assembly=Assembly.Load(output.ToArray());var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var accountType=assembly.GetType(account.FullName)!;var log=accountType.GetField(trace.Name)!;
            var singleton=assembly.GetType("NVespa.NSingleton.GlobalAutoStatic`1")!.MakeGenericType(accountType);
            singleton.GetField("_instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic)!
                .SetValue(null,System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(accountType));
            foreach(var field in accountType.GetFields().Where(f=>f.Name.StartsWith("SPRKFixture") && f!=log))
                field.SetValue(null,System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(field.FieldType));
            var closureType=assembly.GetType(callback.DeclaringType.FullName.Replace('/','+'))!;
            var closure=Activator.CreateInstance(closureType)!;
            var webType=assembly.GetType("NGame2.NUI.NManager.ValanceWebManager")!;
            var web=System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(webType);
            closureType.GetField("<>4__this",flags)!.SetValue(closure,web);
            var infoType=assembly.GetType("NShared.EquipItemInfo")!;
            var old=Activator.CreateInstance(infoType)!;var updated=Activator.CreateInstance(infoType)!;
            infoType.GetProperty("Star")!.SetValue(old,5);infoType.GetProperty("Star")!.SetValue(updated,0);
            closureType.GetField("oldEquipInfo",flags)!.SetValue(closure,old);
            var listenersField=webType.GetField("_listListener",flags)!;
            var listeners=Activator.CreateInstance(listenersField.FieldType)!;listenersField.SetValue(web,listeners);
            var proxy=DispatchProxy.Create(assembly.GetType("NGame2.NUI.NManager.ValanceWebManager+IListener")!,typeof(TechnomagicTierProbe));
            int notifications=0;
            ((TechnomagicTierProbe)proxy).Notify=args=>{
                if((string?)log.GetValue(null)!="Currency;Items;Remove;Equip;Presets;" || args==null || !ReferenceEquals(args[0],old) || !ReferenceEquals(args[1],updated))
                    throw new InvalidOperationException("TM UI observed stale account state or incorrect before/after gear");
                notifications++;
            };
            listeners.GetType().GetMethod("Add")!.Invoke(listeners,new[]{proxy});
            var responseType=assembly.GetType("NShared.UpgradeEquipTier.Response")!;var response=Activator.CreateInstance(responseType)!;
            responseType.GetProperty("ResultEquipItem")!.SetValue(response,updated);
            var methodInfo=closureType.GetMethod(callback.Name,flags)!;
            foreach(var (baseResult,result) in new[]{(0,1),(1,0),(1,1)}) {
                log.SetValue(null,"");int before=notifications;
                foreach(var (name,value) in new[]{("BaseResult",baseResult),("Result",result)}) {
                    var property=responseType.GetProperty(name)!;property.SetValue(response,Enum.ToObject(property.PropertyType,value));
                }
                methodInfo.Invoke(closure,new[]{response});
                if(baseResult==0 || result==0) {
                    if(notifications!=before || (string?)log.GetValue(null)!="Error;")throw new InvalidOperationException("Failed TM response changed state or notified success");
                } else if(notifications!=before+1)throw new InvalidOperationException("TM result notification missing/duplicated");
            }
            Console.WriteLine("PASS: native TM response callback applies all account updates before one listener notification; preserved old/new gear payload and both failure exits.");
        } finally {AppDomain.CurrentDomain.AssemblyResolve-=resolve;}
    }
}

public class TechnomagicTierProbe : DispatchProxy {
    public Action<object?[]?>? Notify;
    protected override object? Invoke(MethodInfo? method,object?[]? args) {if(method?.Name=="OnValanceTierUpgrade")Notify!(args);return null;}
}
