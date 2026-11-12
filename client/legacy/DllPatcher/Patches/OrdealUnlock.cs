using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchOrdealUnlock(ModuleDefinition module)
    {
        var result=OrdealBattleResultCallback(module);
        var win=result.Body.Instructions.SingleOrDefault(i=>i.Operand is FieldReference f && f.Name=="isWin");
        if(win!=null) {
            if(win.Previous.OpCode!=OpCodes.Ldarg_0 || win.Next.OpCode.Code is not (Code.Brfalse or Code.Brfalse_S))
                throw new InvalidOperationException("Unexpected Ordeals victory-only state application guard.");
            // A successful defeat response also contains authoritative dead heroes.
            // Keep both response success checks; only remove the victory guard.
            foreach(var instruction in new[]{win.Previous,win,win.Next}) {
                instruction.OpCode=OpCodes.Nop;instruction.Operand=null;
            }
        }
        var ordeal = module.GetType("NGame2.NOrdeal.OrdealHelper");
        var view=module.GetType("NGame2.NUI.NWindow.OrdealArenaBattlePartySetting");
        var portrait=module.GetType("NGame2.NUI.NComponent2.HeroPortraitComponent");
        var click=view.Methods.Single(m=>m.Name.EndsWith("HeroPortraitComponent.Listener.OnClick"));
        var sync=ordeal.Methods.SingleOrDefault(m=>m.Name=="SprkSyncDeadPortrait");
        if(sync==null) {
            sync=new MethodDefinition("SprkSyncDeadPortrait",MethodAttributes.Public|MethodAttributes.Static,module.TypeSystem.Boolean);
            sync.Parameters.Add(new ParameterDefinition("portrait",ParameterAttributes.None,portrait));
            ordeal.Methods.Add(sync);sync.Body.InitLocals=true;
            var dead=new VariableDefinition(module.TypeSystem.Boolean);sync.Body.Variables.Add(dead);
            var managerGetter=view.Methods.Single(m=>m.Name=="ResetDeadHeros").Body.Instructions
                .Select(i=>i.Operand).OfType<MethodReference>().First(m=>m.Name=="get_instance");
            var manager=module.GetType("NGame2.NOrdeal.OrdealArenaManager");
            var heroGetter=portrait.Methods.Single(m=>m.Name=="get_HeroInfo");
            var heroId=click.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().First(m=>m.Name=="get_HeroIndex");
            var il=sync.Body.GetILProcessor();
            il.Emit(OpCodes.Call,managerGetter);il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Callvirt,heroGetter);
            il.Emit(OpCodes.Callvirt,heroId);il.Emit(OpCodes.Callvirt,manager.Methods.Single(m=>m.Name=="IsDeadHero"));
            il.Emit(OpCodes.Stloc,dead);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldloc,dead);il.Emit(OpCodes.Callvirt,portrait.Methods.Single(m=>m.Name=="SetDead"));
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Call,ordeal.Methods.Single(m=>m.Name=="GetResurrectionHeroGemCount"));
            il.Emit(OpCodes.Callvirt,portrait.Methods.Single(m=>m.Name=="SetReviveGemCount"));
            il.Emit(OpCodes.Ldloc,dead);il.Emit(OpCodes.Ret);
        }
        foreach(var instruction in click.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.Name=="get_IsDead")) {
            instruction.OpCode=OpCodes.Call;instruction.Operand=sync;
        }
        // Portraits are created/rebound after SetUserHeroList's initial pass.
        // The original periodic update only refreshed countdown labels, leaving
        // those later portraits selectable without their revival state.
        var update=view.Methods.Single(m=>m.Name=="UpdateDeadHeros");
        if(!update.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ResetDeadHeros")) {
            var il=update.Body.GetILProcessor();var first=update.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,view.Methods.Single(m=>m.Name=="ResetDeadHeros")));
        }
        var user = module.GetType("NShared.UserInfo");
        var gem = user.Methods.Single(m=>m.Name=="get_Gem");
        var total = ordeal.Methods.SingleOrDefault(m=>m.Name=="SprkTotalGem");
        if(total==null) {
            total=new MethodDefinition("SprkTotalGem",MethodAttributes.Public|MethodAttributes.Static,gem.ReturnType);
            total.Parameters.Add(new ParameterDefinition("user",ParameterAttributes.None,user));
            ordeal.Methods.Add(total);
            var il=total.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Callvirt,gem);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Callvirt,user.Methods.Single(m=>m.Name=="get_PayGem"));
            il.Emit(OpCodes.Add);il.Emit(OpCodes.Ret);
        }
        foreach(var type in new[]{"NGame2.NUI.NWindow.OrdealArenaBattlePartySetting","NGame2.NUI.NManager.OrdealArenaManagement"})
            foreach(var method in module.GetType(type).Methods.Where(m=>m.HasBody))
                foreach(var i in method.Body.Instructions.Where(i=>i.Operand is MethodReference r && r.DeclaringType==user && r.Name=="get_Gem")) {
                    i.OpCode=OpCodes.Call;i.Operand=total;
                }
        var gate = module.GetType("NGame2.NOrdeal.OrdealHelper").Methods.Single(m => m.Name == "CheckOpenCondition");
        var helper = module.GetType("SprkAdventureUnlocks").Methods.Single(m => m.Name == "PortalTutorial");
        foreach (var instruction in gate.Body.Instructions.Where(i => i.Operand is MethodReference m
            && m.DeclaringType.FullName == "NShared.UserTutorialManager" && m.Name == "IsComplete"))
        {
            // Same stack arguments: the tutorial manager followed by the configured index.
            instruction.OpCode = OpCodes.Call;
            instruction.Operand = helper;
        }
        VerifyOrdealUnlock(module);
    }

    static void VerifyOrdealUnlock(ModuleDefinition module)
    {
        var result=OrdealBattleResultCallback(module);
        var callsResult=result.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        if(result.Body.Instructions.Any(i=>i.Operand is FieldReference f && f.Name=="isWin") ||
            !callsResult.Any(m=>m.Name=="get_BaseResult") || !callsResult.Any(m=>m.Name=="get_Result"))
            throw new InvalidOperationException("Ordeals must apply successful battle responses on defeat as well as victory.");
        VerifyArenaResultStack(result);
        var view=module.GetType("NGame2.NUI.NWindow.OrdealArenaBattlePartySetting");
        var click=view.Methods.Single(m=>m.Name.EndsWith("HeroPortraitComponent.Listener.OnClick"));
        var sync=module.GetType("NGame2.NOrdeal.OrdealHelper").Methods.Single(m=>m.Name=="SprkSyncDeadPortrait");
        bool RevivalCalls(MethodDefinition method,string name)=>method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name);
        if(!RevivalCalls(click,"SprkSyncDeadPortrait") || RevivalCalls(click,"get_IsDead") ||
            !RevivalCalls(sync,"IsDeadHero") || !RevivalCalls(sync,"SetDead") || !RevivalCalls(sync,"SetReviveGemCount") ||
            !RevivalCalls(view.Methods.Single(m=>m.Name=="UpdateDeadHeros"),"ResetDeadHeros"))
            throw new InvalidOperationException("Ordeals revival must use authoritative death state after portrait creation and before selection.");
        VerifyArenaResultStack(sync);VerifyArenaResultStack(click);
        VerifyArenaResultStack(view.Methods.Single(m=>m.Name=="UpdateDeadHeros"));
        foreach(var type in new[]{"NGame2.NUI.NWindow.OrdealArenaBattlePartySetting","NGame2.NUI.NManager.OrdealArenaManagement"}) {
            var calls2=module.GetType(type).Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>();
            if(calls2.Any(m=>m.DeclaringType.FullName=="NShared.UserInfo" && m.Name=="get_Gem") || !calls2.Any(m=>m.Name=="SprkTotalGem"))
                throw new InvalidOperationException("Ordeals ruby checks must include paid rubies.");
        }
        var gate = module.GetType("NGame2.NOrdeal.OrdealHelper").Methods.Single(m => m.Name == "CheckOpenCondition");
        var calls = gate.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().ToArray();
        if (calls.Count(m => m.DeclaringType.Name == "SprkAdventureUnlocks" && m.Name == "PortalTutorial") != 1
            || calls.Any(m => m.DeclaringType.FullName == "NShared.UserTutorialManager" && m.Name == "IsComplete")
            || !calls.Any(m => m.Name == "get_isValidInstance")
            || !gate.Body.Instructions.Any(i => Equals(i.Operand, "OrdealArenaOpenCondition")))
            throw new InvalidOperationException("Ordeals must retain account guards and configured tutorial requirement with one skip-aware hook.");
        var helper = module.GetType("SprkAdventureUnlocks");
        bool Calls(string method, string name) => helper.Methods.Single(m => m.Name == method).Body.Instructions
            .Any(i => i.Operand is MethodReference m && m.Name == name);
        if (!helper.Methods.Single(m => m.Name == "PortalTutorial").Body.Instructions
                .Any(i => i.Operand is FieldReference f && f.Name == "IsTutorialSkip")
            || !Calls("PortalTutorial", "TutorialSatisfied")
            || !Calls("TutorialSatisfied", "IsComplete"))
            throw new InvalidOperationException("Missing native completion / tutorial skip policy.");
        VerifyArenaResultStack(gate);
    }

    static MethodDefinition OrdealBattleResultCallback(ModuleDefinition module)
        => module.GetType("NGame2.NOrdeal.OrdealArenaManager").NestedTypes
            .Where(t=>t.Fields.Any(f=>f.Name=="isWin"))
            .SelectMany(t=>t.Methods).Single(m=>m.HasBody && m.Body.Instructions.Any(i=>
                i.Operand is MethodReference r && r.Name=="Apply" &&
                r.DeclaringType.FullName=="NGame2.NOrdeal.UserOrdealArenaInfoManager" &&
                r.Parameters.Count==1 && r.Parameters[0].ParameterType.FullName=="NShared.EndOrdealArena.Response"));

    static void VerifyOrdealUnlockFile(string root, bool staged)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(root, "King's Raid_Data", "Managed",
            "Assembly-CSharp.dll" + (staged ? ".ordeal-unlock-staged" : "")));
        VerifyOrdealUnlock(assembly.MainModule);
        Console.WriteLine("Verified Ordeals skip-aware tutorial gate, native prerequisites and IL stack.");
    }

    static void InstallOrdealUnlock(string root, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".ordeal-unlock-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchOrdealUnlock(assembly.MainModule);
            assembly.Write(staged);
        }
        VerifyOrdealUnlockFile(root, true);
        if (!stageOnly)
        {
            var backup = Path.Combine("target", "ordeal-unlock-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
            VerifyOrdealUnlockFile(root, false);
        }
        Console.WriteLine(stageOnly ? "Staged Ordeals tutorial unlock." : "Installed Ordeals tutorial unlock. Restart the client.");
    }
}
