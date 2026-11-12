using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace DllPatcher;
partial class Program {
    const string TmHeroRefresh = "SprkRefreshTechnomagicGear";
    const string TmHeroBridge = "SprkRefreshTechnomagicHero";

    static void PatchTechnomagicHeroRefresh(ModuleDefinition module) {
        var hero=module.GetType("NGame2.NUI.NManager.MyHeroManagement");
        var web=module.GetType("NGame2.NUI.NManager.ValanceWebManager");
        var refresh=hero.Methods.SingleOrDefault(m=>m.Name==TmHeroRefresh);
        if(refresh==null) {refresh=new MethodDefinition(TmHeroRefresh,MethodAttributes.Public|MethodAttributes.HideBySig,module.TypeSystem.Void);hero.Methods.Add(refresh);}
        refresh.Body=new MethodBody(refresh){InitLocals=true};
        var il=refresh.Body.GetILProcessor();var end=Instruction.Create(OpCodes.Ret);
        var current=hero.Fields.Single(f=>f.Name=="_currentHero");
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,current);il.Emit(OpCodes.Brfalse,end);
        // Use existing native getters and rebuilding methods. Rebuild, rather
        // than repainting old ItemIconComponents that retain the removed item.
        void Rebuild(MethodReference getter,Action loadGetterArgs,MethodReference update,Action loadUpdateArgs) {
            var local=new VariableDefinition(getter is GenericInstanceMethod generic ? generic.GenericArguments.Single() : getter.ReturnType);refresh.Body.Variables.Add(local);
            var skip=Instruction.Create(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);loadGetterArgs();il.Emit(OpCodes.Call,getter);il.Emit(OpCodes.Stloc,local);
            il.Emit(OpCodes.Ldloc,local);il.Emit(OpCodes.Brfalse,skip);
            il.Emit(OpCodes.Ldloc,local);loadUpdateArgs();il.Emit(OpCodes.Callvirt,update);il.Append(skip);
        }
        var panel=module.GetType("NGame2.NUI.NComponent2.Hero3DViewPanel");
        Rebuild(hero.Methods.Single(m=>m.Name=="GetHero3DView"),()=>il.Emit(OpCodes.Ldc_I4_1),panel.Methods.Single(m=>m.Name=="SetupEquipItemIcon"),()=>{});
        var list=module.GetType("NGame2.NUI.NComponent2.HeroEquipListPanel");
        Rebuild(hero.Methods.Single(m=>m.Name=="GetHeroEquipList"),()=>il.Emit(OpCodes.Ldc_I4_1),
            list.Methods.Single(m=>m.Name=="Refresh" && m.Parameters.Count==2),()=>{il.Emit(OpCodes.Ldc_I4_1);il.Emit(OpCodes.Ldc_I4_1);});
        // The native change-hero path discards the old equipped-item description
        // and workshop menu, so neither can continue to use the replaced hood.
        var container=module.GetType("NGame2.NUI.NWindow.NInventory.InvenEquipInfoUIContainer");
        var getContainer=hero.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand)
            .OfType<GenericInstanceMethod>().First(m=>m.Name=="GetInstance" && m.GenericArguments.Single().FullName==container.FullName);
        Rebuild(getContainer,()=>il.Emit(OpCodes.Ldc_I4_1),container.Methods.Single(m=>m.Name=="ChangeHero"),()=>{il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,current);});
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,current);
        il.Emit(OpCodes.Call,hero.Methods.Single(m=>m.Name=="RefreshHeroEquipTreasure"));il.Append(end);

        var bridge=web.Methods.SingleOrDefault(m=>m.Name==TmHeroBridge);
        if(bridge==null) {bridge=new MethodDefinition(TmHeroBridge,MethodAttributes.Private|MethodAttributes.Static|MethodAttributes.HideBySig,module.TypeSystem.Void);web.Methods.Add(bridge);}
        var getHero=web.Methods.Concat(web.NestedTypes.SelectMany(t=>t.Methods)).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand)
            .OfType<MethodReference>().First(m=>m.Name=="get_instance" && m.DeclaringType is GenericInstanceType g && g.GenericArguments.Single().FullName==hero.FullName);
        bridge.Body=new MethodBody(bridge){InitLocals=true};bridge.Body.Variables.Add(new VariableDefinition(hero));
        var b=bridge.Body.GetILProcessor();var done=Instruction.Create(OpCodes.Ret);
        b.Emit(OpCodes.Call,getHero);b.Emit(OpCodes.Stloc_0);b.Emit(OpCodes.Ldloc_0);b.Emit(OpCodes.Brfalse,done);
        b.Emit(OpCodes.Ldloc_0);b.Emit(OpCodes.Call,refresh);b.Append(done);
        var callback=TechnomagicTierCallback(module);
        if(!callback.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==TmHeroBridge)) {
            var lastApply=callback.Body.Instructions.Last(i=>i.Operand is MethodReference m && m.Name=="Apply");
            callback.Body.GetILProcessor().InsertAfter(lastApply,Instruction.Create(OpCodes.Call,bridge));
        }
    }

    static void VerifyTechnomagicHeroRefresh(ModuleDefinition module) {
        var callback=TechnomagicTierCallback(module);var code=callback.Body.Instructions;
        var refresh=code.Single(i=>i.Operand is MethodReference m && m.Name==TmHeroBridge);
        var notify=code.Single(i=>i.Operand is MethodReference m && m.Name=="ForEach");
        if(code.IndexOf(refresh)<=code.IndexOf(code.Last(i=>i.Operand is MethodReference m && m.Name=="Apply")) || code.IndexOf(refresh)>=code.IndexOf(notify))
            throw new InvalidOperationException("Hero gear must refresh after replacement and before TM result notification.");
        var method=module.GetType("NGame2.NUI.NManager.MyHeroManagement").Methods.Single(m=>m.Name==TmHeroRefresh);
        var calls=method.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"SetupEquipItemIcon","Refresh","ChangeHero","RefreshHeroEquipTreasure"})
            if(calls.Count(m=>m.Name==name)!=1)throw new InvalidOperationException("Missing Hero Info TM refresh: "+name);
        VerifyArenaResultStack(method);
        VerifyArenaResultStack(module.GetType("NGame2.NUI.NManager.ValanceWebManager").Methods.Single(m=>m.Name==TmHeroBridge));
        Console.WriteLine("Verified restoration rebuilds Hero Info gear icons, inventory and stale item details after account replacement.");
    }

    static void TestTechnomagicHeroRebuild(AssemblyDefinition game,string managed) {
        using var bytes=new MemoryStream();game.Write(bytes);bytes.Position=0;
        using var fixture=AssemblyDefinition.ReadAssembly(bytes,new ReaderParameters{AssemblyResolver=game.MainModule.AssemblyResolver});
        var module=fixture.MainModule;var hero=module.GetType("NGame2.NUI.NManager.MyHeroManagement");
        var refresh=hero.Methods.Single(m=>m.Name==TmHeroRefresh);
        var trace=new FieldDefinition("SPRKFixtureTrace",Mono.Cecil.FieldAttributes.Public|Mono.Cecil.FieldAttributes.Static,module.TypeSystem.String);hero.Fields.Add(trace);
        void Sink(MethodDefinition method,string tag) {
            method.Body=new MethodBody(method);var il=method.Body.GetILProcessor();
            var concat=new MethodReference("Concat",module.TypeSystem.String,module.TypeSystem.String);
            concat.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));concat.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
            il.Emit(OpCodes.Ldsfld,trace);il.Emit(OpCodes.Ldstr,tag);il.Emit(OpCodes.Call,concat);il.Emit(OpCodes.Stsfld,trace);il.Emit(OpCodes.Ret);
        }
        var dependencies=new List<FieldDefinition>();
        foreach(var name in new[]{"GetHero3DView","GetHeroEquipList","GetInstance"}) {
            var instruction=refresh.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.Name==name);
            var reference=(MethodReference)instruction.Operand;
            var type=reference is GenericInstanceMethod generic?generic.GenericArguments.Single():reference.ReturnType;
            var field=new FieldDefinition("SPRKFixture"+name,Mono.Cecil.FieldAttributes.Public|Mono.Cecil.FieldAttributes.Static,type);hero.Fields.Add(field);dependencies.Add(field);
            // Substitute service lookup only, retaining the actual installed
            // null guards, opened-only arguments, rebuild calls and their order.
            var getter=new MethodDefinition("SPRKFixtureLookup"+name,MethodAttributes.Private,module.ImportReference(type));
            getter.Parameters.Add(new ParameterDefinition(module.TypeSystem.Boolean));hero.Methods.Add(getter);
            getter.Body.GetILProcessor().Emit(OpCodes.Ldsfld,field);getter.Body.GetILProcessor().Emit(OpCodes.Ret);
            instruction.Operand=getter;
            var updateName=name switch {"GetHero3DView"=>"SetupEquipItemIcon","GetHeroEquipList"=>"Refresh",_=>"ChangeHero"};
            var update=module.GetType(type.FullName).Methods.Single(m=>m.Name==updateName && (updateName!="Refresh" || m.Parameters.Count==2));
            Sink(update,name switch {"GetHero3DView"=>"Icons;","GetHeroEquipList"=>"Inventory;",_=>"Details;"});
        }
        Sink(hero.Methods.Single(m=>m.Name=="RefreshHeroEquipTreasure"),"Treasure;");
        using var output=new MemoryStream();fixture.Write(output);
        ResolveEventHandler resolve=(_,e)=>{var path=Path.Combine(managed,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
        AppDomain.CurrentDomain.AssemblyResolve+=resolve;
        try {
            var assembly=Assembly.Load(output.ToArray());var type=assembly.GetType(hero.FullName)!;
            var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var instance=System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
            var current=type.GetField("_currentHero",flags)!;var log=type.GetField(trace.Name)!;
            var method=type.GetMethod(TmHeroRefresh)!;int cases=0;
            foreach(bool selected in new[]{false,true})foreach(int mask in Enumerable.Range(0,8)) {
                current.SetValue(instance,selected?System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(current.FieldType):null);
                string expected="";
                for(int i=0;i<dependencies.Count;i++) {
                    var field=type.GetField(dependencies[i].Name)!;bool present=(mask&(1<<i))!=0;
                    field.SetValue(null,present?System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(field.FieldType):null);
                    if(selected && present)expected+=new[]{"Icons;","Inventory;","Details;"}[i];
                }
                if(selected)expected+="Treasure;";
                log.SetValue(null,"");method.Invoke(instance,null);
                if((string?)log.GetValue(null)!=expected)throw new Exception("Native Hero Info restoration rebuild failed: "+selected+"/"+mask);
                cases++;
            }
            Console.WriteLine($"PASS: actual Hero Info refresh method, {cases} selected/no-hero and present/absent panel cases; icons, inventory, stale details and treasure rebuilding.");
        } finally {AppDomain.CurrentDomain.AssemblyResolve-=resolve;}
    }
}
