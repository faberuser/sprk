using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchPortal(ModuleDefinition module)
    {
        bool nativeCombatTables = NativeCombatTables.ValidateInstalled(Path.GetDirectoryName(module.FileName)!);
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Portal.cs.txt")!;
        using var reader = new StreamReader(stream);
        using var raidStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ReconstructedRaids.json")!;
        using var raidReader = new StreamReader(raidStream);
        var sourceText = reader.ReadToEnd().Replace("\"__RECONSTRUCTED_RAIDS__\"", System.Text.Json.JsonSerializer.Serialize(raidReader.ReadToEnd()));
        using var punishmentSkillStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RestoredCombat.json")!;
        using var punishmentSkillReader = new StreamReader(punishmentSkillStream);
        sourceText = sourceText.Replace("\"__RESTORED_PUNISHMENT_SKILLS__\"", System.Text.Json.JsonSerializer.Serialize(punishmentSkillReader.ReadToEnd()));
        using var orvelStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RestoredOrvelMenus.json")!;
        using var orvelReader = new StreamReader(orvelStream);
        sourceText = sourceText.Replace("\"__RESTORED_ORVEL_MENUS__\"", System.Text.Json.JsonSerializer.Serialize(orvelReader.ReadToEnd()));
        using var guildStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RestoredGuildMenus.json")!;
        using var guildReader = new StreamReader(guildStream);
        sourceText = sourceText.Replace("\"__RESTORED_GUILD_MENUS__\"", System.Text.Json.JsonSerializer.Serialize(guildReader.ReadToEnd()));
        using var contentStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RestoredGuildContent.json")!;
        using var contentReader = new StreamReader(contentStream);
        sourceText = sourceText.Replace("\"__RESTORED_GUILD_CONTENT__\"", System.Text.Json.JsonSerializer.Serialize(contentReader.ReadToEnd()));
        using var conquestStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RestoredGuildConquest.json")!;
        using var conquestReader = new StreamReader(conquestStream);
        sourceText = sourceText.Replace("\"__RESTORED_GUILD_CONQUEST__\"", System.Text.Json.JsonSerializer.Serialize(conquestReader.ReadToEnd()));
        var references = Directory.GetFiles(Path.GetDirectoryName(module.FileName)!, "*.dll")
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("PortalRestore",
            new[] { CSharpSyntaxTree.ParseText(sourceText) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var compiled = new MemoryStream();
        var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics));
        compiled.Position = 0;
        using var sourceAssembly = AssemblyDefinition.ReadAssembly(compiled);
        var sourceType = sourceAssembly.MainModule.Types.Single(t => t.Name == "RestoredPortal");
        var targetType = module.Types.SingleOrDefault(t => t.Name == sourceType.Name);
        if (targetType == null)
        {
            targetType = new TypeDefinition("", sourceType.Name, sourceType.Attributes, module.TypeSystem.Object);
            module.Types.Add(targetType);
        }
        foreach(var field in sourceType.Fields)
            if(!targetType.Fields.Any(f=>f.Name==field.Name))
                targetType.Fields.Add(new FieldDefinition(field.Name,field.Attributes,module.ImportReference(field.FieldType)));
        foreach (var source in sourceType.Methods)
        {
            if (targetType.Methods.Any(m => m.Name == source.Name)) continue;
            var target = new MethodDefinition(source.Name, source.Attributes, module.ImportReference(source.ReturnType));
            foreach (var parameter in source.Parameters)
                target.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, module.ImportReference(parameter.ParameterType)));
            targetType.Methods.Add(target);
        }
        foreach (var source in sourceType.Methods)
        {
            var target = targetType.Methods.Single(m => m.Name == source.Name);
            var body = new MethodBody(target) { InitLocals = source.Body.InitLocals, MaxStackSize = source.Body.MaxStackSize };
            target.Body = body;
            foreach (var variable in source.Body.Variables)
                body.Variables.Add(new VariableDefinition(module.ImportReference(variable.VariableType)));
            var instructions = source.Body.Instructions.ToDictionary(i => i, i => Instruction.Create(OpCodes.Nop));
            foreach (var instruction in source.Body.Instructions)
            {
                var copy = instructions[instruction];
                copy.OpCode = instruction.OpCode;
                copy.Operand = instruction.Operand switch
                {
                    Instruction branch => instructions[branch],
                    Instruction[] branches => branches.Select(b => instructions[b]).ToArray(),
                    VariableDefinition variable => body.Variables[variable.Index],
                    ParameterDefinition parameter => target.Parameters[parameter.Index],
                    MethodReference method when method.DeclaringType.FullName == sourceType.FullName => targetType.Methods.Single(m => m.Name == method.Name),
                    MethodReference method => module.ImportReference(method),
                    FieldReference field when field.DeclaringType.FullName == sourceType.FullName => targetType.Fields.Single(f=>f.Name==field.Name),
                    FieldReference field => module.ImportReference(field),
                    TypeReference type => module.ImportReference(type),
                    var operand => operand
                };
                body.Instructions.Add(copy);
            }
            foreach (var handler in source.Body.ExceptionHandlers)
                body.ExceptionHandlers.Add(new ExceptionHandler(handler.HandlerType) {
                    TryStart = instructions[handler.TryStart], TryEnd = handler.TryEnd == null ? null : instructions[handler.TryEnd],
                    HandlerStart = instructions[handler.HandlerStart], HandlerEnd = handler.HandlerEnd == null ? null : instructions[handler.HandlerEnd],
                    CatchType = handler.CatchType == null ? null : module.ImportReference(handler.CatchType),
                    FilterStart = handler.FilterStart == null ? null : instructions[handler.FilterStart]
                });
        }
        var loginInit=module.Types.Single(t=>t.FullName=="NGame2.NUI.NWindow.LoginBackground").Methods.Single(m=>m.Name=="Init");
        if(!loginInit.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="FinishStartupLogo")) {
            var il=loginInit.Body.GetILProcessor();
            il.InsertBefore(loginInit.Body.Instructions.First(),il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="FinishStartupLogo")));
        }
        if(loginInit.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.Name=="FinishStartupLogo")!=1)
            throw new InvalidOperationException("Expected one startup logo cleanup hook");
        var afterLogin=module.Types.Single(t=>t.FullName=="NGame2.NAccount.AccountManager").Methods.Single(m=>m.Name=="AfterTableLoad_OnLoginResponse");
        if(!afterLogin.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="EnsureNativeShaderVariables")) {
            var il=afterLogin.Body.GetILProcessor();
            foreach(var ret in afterLogin.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Call;
                ret.Operand=targetType.Methods.Single(m=>m.Name=="EnsureNativeShaderVariables");
                il.InsertAfter(ret,il.Create(OpCodes.Ret));
            }
        }
        var hardUnlock = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.RaidHelper")
            .Methods.Single(m => m.Name == "IsOpenedHardMode");
        hardUnlock.Body = new MethodBody(hardUnlock) { MaxStackSize = 1 };
        var unlockIl = hardUnlock.Body.GetILProcessor();
        unlockIl.Append(unlockIl.Create(OpCodes.Ldarg_0));
        unlockIl.Append(unlockIl.Create(OpCodes.Call, targetType.Methods.Single(m => m.Name == "IsOpenedDragonHard")));
        unlockIl.Append(unlockIl.Create(OpCodes.Ret));

        // Block locked Hard dragons before opening a party window. Cover direct
        // links/return routes as well as the raid list, and recheck at battle start.
        var raidManagement = module.GetType("NGame2.NUI.NManager.NLobby.RaidManagement");
        var raidParty = module.GetType("NGame2.NUI.NWindow.RaidPartySetting");
        var raidGetter = raidParty.Methods.Single(m => m.Name == "get_RaidData");
        void GuardDragonEntry(MethodDefinition method, string helperName, params Instruction[] arguments)
        {
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m
                && m.DeclaringType.Name == "RestoredPortal" && m.Name == helperName)) return;
            var il = method.Body.GetILProcessor();
            var original = method.Body.Instructions[0];
            foreach (var argument in arguments) il.InsertBefore(original, argument);
            il.InsertBefore(original, Instruction.Create(OpCodes.Call, targetType.Methods.Single(m => m.Name == helperName)));
            il.InsertBefore(original, Instruction.Create(OpCodes.Brtrue, original));
            il.InsertBefore(original, Instruction.Create(OpCodes.Ret));
            method.Body.MaxStackSize = System.Math.Max(method.Body.MaxStackSize, 2);
        }
        foreach (var method in raidManagement.Methods.Where(m => m.Name == "EnterRaidInternal"
            || (m.Name == "OpenSingleRaidPartySetting" && m.Parameters[0].ParameterType.Name == "RaidData")))
            GuardDragonEntry(method, "CheckDragonHardEntry", Instruction.Create(OpCodes.Ldarg_1));
        foreach (var method in raidManagement.Methods.Where(m => (m.Name == "OpenSingleRaidPartySetting"
            || m.Name == "OpenRaidPartySetting") && m.Parameters[0].ParameterType.MetadataType == MetadataType.Int32))
            GuardDragonEntry(method, "CheckDragonHardEntryByIndex", Instruction.Create(OpCodes.Ldarg_1), Instruction.Create(OpCodes.Ldarg_2));
        GuardDragonEntry(raidParty.Methods.Single(m => m.Name == "OnClickStartBattleButton"), "CheckDragonHardEntry",
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Call, raidGetter));
        GuardDragonEntry(module.GetType("NGame2.NUI.NWindow.RaidSinglePartySetting").Methods.Single(m => m.Name == "RequestStartBattle"),
            "CheckDragonHardEntry", Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Call, raidGetter));

        // One immutable defense policy for every battle, including detached
        // dispatch/Conquest simulations and callers of the public calculator.
        var statController = module.Types.Single(t => t.FullName == "NShared.StatController");
        foreach(var calculator in statController.Methods.Where(m=>m.Name is "CalculateDamage" or "CalculateDamage_Season1")) {
            calculator.Body = new MethodBody(calculator) { MaxStackSize = 4 };
            var il=calculator.Body.GetILProcessor();
            for(int argument=0;argument<calculator.Parameters.Count;argument++)il.Append(il.Create(OpCodes.Ldarg,calculator.Parameters[argument]));
            il.Append(il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="LegacyPunishmentDamage")));
            il.Append(il.Create(OpCodes.Ret));
        }
        foreach (var method in statController.Methods.Where(m => m.HasBody && !m.IsStatic))
        {
            foreach (var call in method.Body.Instructions.Where(i => i.Operand is MethodReference m
                && m.DeclaringType.FullName == "NShared.StatController" && m.Name == "CalculateDamage").ToArray())
            {
                method.Body.GetILProcessor().InsertBefore(call, Instruction.Create(OpCodes.Ldarg_0));
                call.OpCode = OpCodes.Call;
                call.Operand = targetType.Methods.Single(m => m.Name == "PunishmentDamage");
                method.Body.MaxStackSize = System.Math.Max(method.Body.MaxStackSize, 8);
            }
        }
        foreach(var type in module.GetTypes().Where(t=>t!=targetType))
        foreach(var method in type.Methods.Where(m=>m.HasBody))
        foreach(var instruction in method.Body.Instructions) {
            if(instruction.Operand is not MethodReference call || call.Name!="GetData" || call.Parameters.Count!=1)continue;
            var name=call.DeclaringType.FullName;
            string? helper=null;
            if(name.Contains("<System.Int32,NShared.SkillData>"))helper="PunishmentSkill";
            else if(name.Contains("<System.Int32,NShared.CreatureData>"))helper="CombatCreature";
            else if(name.Contains("<System.String,NShared.StateData>"))helper="CombatState";
            else if(name.Contains("<System.Int32,NShared.SkillProjectileData>"))helper="CombatProjectile";
            else if(name.Contains("NShared.GuildSuppressSessionData"))helper="ConquestSession";
            else if(name.Contains("NShared.GuildSuppressDungeonData"))helper="ConquestDungeon";
            if(helper!=null) {instruction.OpCode=OpCodes.Call;instruction.Operand=targetType.Methods.Single(m=>m.Name==helper);}
        }
        var tableLoad=module.Types.Single(t=>t.FullName=="NShared.NJit.JitDataContainer`1").Methods.Single(m=>m.Name=="Load");
        if(!tableLoad.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="FinalizeCombatContainer")) {
            var loaded=tableLoad.Body.Instructions.Single(i=>i.Operand is MethodReference m&&m.Name=="set_IsLoaded");
            var il=tableLoad.Body.GetILProcessor();var self=il.Create(OpCodes.Ldarg_0);
            il.InsertAfter(loaded,self);il.InsertAfter(self,il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="FinalizeCombatContainer")));
        }
        var operationLoad = module.Types.Single(t => t.FullName == "NShared.OperationData").Methods.Single(m => m.IsConstructor && m.Parameters.Count == 1);
        if (!operationLoad.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "RestorePunishmentOperation")) {
            var il = operationLoad.Body.GetILProcessor();var first = operationLoad.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="RestorePunishmentOperation")));
        }
        var operationLookup = module.Types.Single(t=>t.FullName=="NShared.SkillDataContainer").Methods.Single(m=>m.Name=="GetOperationData");
        if (!operationLookup.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="IsRemovedPunishmentOperation")) {
            var il=operationLookup.Body.GetILProcessor();var first=operationLookup.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_1));il.InsertBefore(first,il.Create(OpCodes.Ldarg_2));
            il.InsertBefore(first,il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="IsRemovedPunishmentOperation")));
            il.InsertBefore(first,il.Create(OpCodes.Brfalse,first));il.InsertBefore(first,il.Create(OpCodes.Ldnull));il.InsertBefore(first,il.Create(OpCodes.Ret));
        }
        if(!operationLookup.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="RestoreMissingCombatOperation")) {
            var il=operationLookup.Body.GetILProcessor();
            foreach(var ret in operationLookup.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_1;var arg=il.Create(OpCodes.Ldarg_2);il.InsertAfter(ret,arg);
                var call=il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="RestoreMissingCombatOperation"));
                il.InsertAfter(arg,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        // Getter hooks also cover tables accessed by separate managed workers;
        // no mutation of shared stat lookup dictionaries or account records.
        void CombatGetter(string typeName,string property,string table) {
            var getter=module.Types.Single(t=>t.FullName=="NShared."+typeName).Properties.Single(p=>p.Name==property).GetMethod;
            if(getter.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="CombatRowNumber"))return;
            var il=getter.Body.GetILProcessor();
            foreach(var ret in getter.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Conv_I8;var last=ret;
                foreach(var next in new[]{il.Create(OpCodes.Ldarg_0),il.Create(OpCodes.Ldstr,table),il.Create(OpCodes.Ldstr,property),
                    il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="CombatRowNumber")),
                    il.Create(getter.ReturnType.FullName=="System.Int64"?OpCodes.Nop:OpCodes.Conv_I4),il.Create(OpCodes.Ret)}) {
                    il.InsertAfter(last,next);last=next;
                }
            }
            getter.Body.MaxStackSize=5;
        }
        CombatGetter("SkillLevelFactorData","Factor5","SkillLevelFactor");
        foreach(var property in new[]{"OptionRatioPerValue1","OptionRatioPerValue2"})CombatGetter("SoulWeaponOptionData",property,"SoulWeaponOption");
        foreach(var property in new[]{"PhysicalAttack","MagicalAttack","PhysicalDefense","MagicalDefense","MaxHp"})CombatGetter("MonsterTierData",property,"MonsterTier");
        foreach(var property in new[]{"MaxTime","ExtraTime"})CombatGetter("BattleDefineData",property,"BattleDefine");
        var grade=module.Types.Single(t=>t.FullName=="NShared.CreatureStarGradeStatData").Methods.Single(m=>m.Name=="GetStat");
        if(!grade.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="CombatGradeStat")) {
            var il=grade.Body.GetILProcessor();
            foreach(var ret in grade.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;var arg=il.Create(OpCodes.Ldarg_1);il.InsertAfter(ret,arg);
                var call=il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="CombatGradeStat"));
                il.InsertAfter(arg,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
            grade.Body.MaxStackSize=5;
        }
        var constant=module.Types.Single(t=>t.FullName=="NShared.ConstantData").Properties.Single(p=>p.Name=="Value").GetMethod;
        if(!constant.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="CombatConstant")) {
            var il=constant.Body.GetILProcessor();var ret=constant.Body.Instructions.Single(i=>i.OpCode==OpCodes.Ret);
            ret.OpCode=OpCodes.Ldarg_0;var call=il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="CombatConstant"));
            il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));constant.Body.MaxStackSize=3;
        }
        var constants=module.Types.Single(t=>t.FullName=="NShared.BattleConstant").Methods.Single(m=>m.Name=="StaticInitialize");
        if(!constants.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="InitializeCombatConstants")) {
            var il=constants.Body.GetILProcessor();
            foreach(var ret in constants.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Call;ret.Operand=targetType.Methods.Single(m=>m.Name=="InitializeCombatConstants");
                il.InsertAfter(ret,il.Create(OpCodes.Ret));
            }
        }
        var sessionList=module.Types.Single(t=>t.FullName=="NShared.GuildSuppressSessionDataContainer")
            .Methods.Single(m=>m.Name=="GetSessionList");
        if(!sessionList.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="ConquestSessions")) {
            var il=sessionList.Body.GetILProcessor();foreach(var ret in sessionList.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_1;var call=il.Create(OpCodes.Call,targetType.Methods.Single(m=>m.Name=="ConquestSessions"));
                il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        foreach (var spec in new[]{("GetData","RestoreGuildShopItem",2), ("GetShopItemList","RestoreGuildShopList",1),
            ("GetShopItemListByShopItemGroup","RestoreGuildShopGroup",2)})
        {
            var method=module.Types.Single(t=>t.FullName=="NShared.ShopItemDataContainer").Methods
                .Single(m=>m.Name==spec.Item1 && m.Parameters.Count==spec.Item3);
            var helper=targetType.Methods.Single(m=>m.Name==spec.Item2);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==helper.Name)) continue;
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_1;var last=ret;
                if(spec.Item3==2) {var arg=il.Create(OpCodes.Ldarg_2);il.InsertAfter(last,arg);last=arg;}
                var call=il.Create(OpCodes.Call,helper);il.InsertAfter(last,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        foreach(var name in new[]{"BuyGuildPoint","BuyGuildArenaPoint"}) {
            var getter=module.Types.Single(t=>t.FullName=="NShared.ItemBaseData").Methods.Single(m=>m.Name=="get_"+name);
            var helper=targetType.Methods.Single(m=>m.Name=="GuildItemPrice");
            if(getter.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name==helper.Name))continue;
            var il=getter.Body.GetILProcessor();foreach(var ret in getter.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;var key=il.Create(OpCodes.Ldstr,name);il.InsertAfter(ret,key);
                var call=il.Create(OpCodes.Call,helper);il.InsertAfter(key,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        var attendance=module.Types.Single(t=>t.FullName=="NGame2.NUI.NManager.NLobby.LobbyManagement")
            .Methods.Single(m=>m.Name=="OpenGuildAttendancePopup");
        var attendanceHelper=targetType.Methods.Single(m=>m.Name=="AllowAutoGuildAttendance");
        if(!attendance.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name==attendanceHelper.Name)) {
            var il=attendance.Body.GetILProcessor();var first=attendance.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Call,attendanceHelper));
            il.InsertBefore(first,il.Create(OpCodes.Brtrue,first));il.InsertBefore(first,il.Create(OpCodes.Ret));
        }
        var guildResult=module.Types.Single(t=>t.FullName=="NGame2.NAccount.AccountManager").Methods.Single(m=>m.Name=="ApplyGuildRaidResult");
        var scoreHelper=targetType.Methods.Single(m=>m.Name=="SeedGuildRaidScore");
        if(!guildResult.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name==scoreHelper.Name)) {
            var il=guildResult.Body.GetILProcessor();var first=guildResult.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_1));il.InsertBefore(first,il.Create(OpCodes.Call,scoreHelper));
        }
        var filterMethod = module.Types.Single(t => t.FullName == "NShared.OptionSellDataContainer")
            .Methods.Single(m => m.Name == "GetCategoryData");
        var filterHelper = targetType.Methods.Single(m => m.Name == "RestoreInventoryFilterOptions");
        if (!filterMethod.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == filterHelper.Name))
        {
            var il = filterMethod.Body.GetILProcessor();
            foreach (var ret in filterMethod.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_1;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, filterHelper));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        var gradeInit = module.Types.Single(t => t.FullName == "NGame2.NUI.NComponent2.OptionGradeComponent")
            .Methods.Single(m => m.Name == "Init");
        var gradeHelper = targetType.Methods.Single(m => m.Name == "RestoreInventoryGradeLabel");
        if (!gradeInit.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == gradeHelper.Name))
        {
            var il = gradeInit.Body.GetILProcessor();
            foreach (var ret in gradeInit.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, gradeHelper));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        var rankingAction = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.NEntryMenu.NAction.OpenGuildRankingBoard")
            .Methods.Single(m => m.Parameters.Count == 3);
        var rankingHelper = targetType.Methods.Single(m => m.Name == "OpenPortalGuildRankings");
        if (!rankingAction.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == rankingHelper.Name))
        {
            var il = rankingAction.Body.GetILProcessor(); var first = rankingAction.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_2));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, rankingHelper));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        // The stripped single-building setter never applies its argument.
        var buildingSetter = module.Types.Single(t => t.FullName == "NShared.MyGuildInfo").Methods
            .Single(m => m.Name == "SetGuildBuildingInfo" && m.Parameters[0].ParameterType.FullName == "NShared.GuildBuildingInfo");
        buildingSetter.Body = new MethodBody(buildingSetter);
        var buildingIl = buildingSetter.Body.GetILProcessor();
        buildingIl.Emit(OpCodes.Ldarg_0);
        buildingIl.Emit(OpCodes.Ldarg_1);
        buildingIl.Emit(OpCodes.Call, targetType.Methods.Single(m => m.Name == "UpdateGuildBuilding"));
        buildingIl.Emit(OpCodes.Ret);
        var resourcesSetter = module.Types.Single(t => t.FullName == "NGame2.NGuild.UserGuildManager").Methods
            .Single(m => m.Name == "SetConstructionResource");
        var refreshConstruction = targetType.Methods.Single(m => m.Name == "RefreshGuildConstruction");
        if (!resourcesSetter.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == refreshConstruction.Name))
        {
            var il = resourcesSetter.Body.GetILProcessor();
            foreach (var ret in resourcesSetter.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Call; ret.Operand = refreshConstruction;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Ret));
            }
        }
        var initNpcMenus = module.Types.Single(t => t.FullName == "NShared.NPCData").Methods.Single(m => m.Name == "InitEntryMenu");
        var restoreNpcMenus = targetType.Methods.Single(m => m.Name == "RestoreOrvelNpcMenus");
        if (!initNpcMenus.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == restoreNpcMenus.Name))
        {
            var il = initNpcMenus.Body.GetILProcessor();
            var first = initNpcMenus.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, restoreNpcMenus));
        }
        var nodes = module.Types.Single(t => t.FullName == "NShared.CampaignNodeDataContainer");
        var canEnhanceSkill = module.Types.Single(t => t.FullName == "NGame2.NAccount.Helper")
            .Methods.Single(m => m.Name == "CanSkillExtend");
        var hasSkill = targetType.Methods.Single(m => m.Name == "HasEnhanceableSkill");
        if (!canEnhanceSkill.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == hasSkill.Name))
        {
            var il = canEnhanceSkill.Body.GetILProcessor();
            var first = canEnhanceSkill.Body.Instructions[0];
            foreach (var instruction in new[] { Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, hasSkill), Instruction.Create(OpCodes.Brtrue, first),
                Instruction.Create(OpCodes.Ldc_I4_0), Instruction.Create(OpCodes.Ret) }) il.InsertBefore(first, instruction);
        }
        // Both the shared multiplayer insertion path and the single-raid override
        // must reject under-level heroes before changing the party or sending it.
        foreach (var typeName in new[] { "BattlePartySetting", "RaidSinglePartySetting" })
        {
            var method = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow." + typeName)
                .Methods.Single(m => m.Name == "EnableInsertSquard");
            var helper = targetType.Methods.Single(m => m.Name == "CheckRaidHeroLevel");
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == helper.Name)) continue;
            var il = method.Body.GetILProcessor();
            var first = method.Body.Instructions[0];
            foreach (var instruction in new[] { Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call, helper), Instruction.Create(OpCodes.Brtrue, first),
                Instruction.Create(OpCodes.Ldc_I4_0), Instruction.Create(OpCodes.Ret) }) il.InsertBefore(first, instruction);
        }
        var savedHeroes = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.BattlePartySetting")
            .Methods.Single(m => m.Name == "GetHeroInfoList");
        var eligibleSaved = targetType.Methods.Single(m => m.Name == "EligibleRaidSavedHeroes");
        if (!savedHeroes.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == eligibleSaved.Name))
        {
            var il = savedHeroes.Body.GetILProcessor();
            foreach (var ret in savedHeroes.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, eligibleSaved);
                il.InsertAfter(ret, call); il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var punishmentPartyInit = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.NPunish.PunishmentRaidPartySetting")
            .Methods.Single(m => m.Name == "Init" && m.Parameters.Count == 0);
        var partyFrames = targetType.Methods.Single(m => m.Name == "RestorePunishmentPartyFrames");
        if (!punishmentPartyInit.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == partyFrames.Name))
        {
            var il = punishmentPartyInit.Body.GetILProcessor();
            foreach (var ret in punishmentPartyInit.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, partyFrames);
                il.InsertAfter(ret, call); il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        foreach (var name in new[] { "GetData", "GetAllChapterData" })
        {
            var method = nodes.Methods.Single(m => m.Name == name);
            var helper = targetType.Methods.Single(m => m.Name == (name == "GetData" ? "OrvelNode" : "OrvelNodes"));
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == helper.Name)) continue;
            var il = method.Body.GetILProcessor();
            foreach (var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_1; ret.Operand = null;
                var tail = ret;
                if (name == "GetData") { var arg = Instruction.Create(OpCodes.Ldarg_2); il.InsertAfter(tail, arg); tail = arg; }
                var call = Instruction.Create(OpCodes.Call, helper); il.InsertAfter(tail, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var archiveCondition = module.Types.Single(t => t.FullName == "NShared.MainStoryData")
            .Methods.Single(m => m.Name == "get_Condition");
        var archiveHelper = targetType.Methods.Single(m => m.Name == "ArchiveReplayCondition");
        if (!archiveCondition.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == archiveHelper.Name))
        {
            var il = archiveCondition.Body.GetILProcessor();
            foreach (var ret in archiveCondition.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, archiveHelper);
                il.InsertAfter(ret, call); il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        // Preserve the required Orvel map change when story tutorials are skipped.
        var canStartTutorial = module.Types.Single(t => t.FullName == "NGame2.NEventTrigger.EventTriggerManager")
            .Methods.Single(m => m.Name == "CanStartTutorial");
        var orvelAvailable = targetType.Methods.Single(m => m.Name == "OrvelTutorialAvailable");
        if (!canStartTutorial.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == orvelAvailable.Name))
        {
            var il = canStartTutorial.Body.GetILProcessor();
            foreach (var ret in canStartTutorial.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_1; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, orvelAvailable);
                il.InsertAfter(ret, call); il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var startTutorial = module.Types.Single(t => t.FullName == "NGame2.NTutorial.TutorialManager")
            .Methods.Single(m => m.Name == "StartByEventTrigger");
        var orvelTransition = targetType.Methods.Single(m => m.Name == "TrySkippedOrvelTransition");
        if (!startTutorial.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == orvelTransition.Name))
        {
            var il = startTutorial.Body.GetILProcessor(); var first = startTutorial.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_2));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_3));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, orvelTransition));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        // Carry the server's authoritative Raider balance alongside every stamina
        // result, and apply it on the same main-thread path as the stamina balance.
        var staminaType = module.Types.Single(t => t.FullName == "NShared.StaminaResultInfo");
        var expType = module.Types.Single(t => t.FullName == "NShared.ExpResultInfo");
        var expField = staminaType.Fields.SingleOrDefault(f => f.Name == "SprkRaiderExp");
        if (expField == null)
        {
            expField = new FieldDefinition("SprkRaiderExp", FieldAttributes.Public, expType);
            staminaType.Fields.Add(expField);
        }
        var parseStamina = module.Types.Single(t => t.Name == "JM_NShared_StaminaResultInfo").Methods
            .Single(m => m.Name == "Parse" && m.Parameters.Count == 2 && m.Parameters[0].ParameterType.FullName == "System.Collections.IDictionary");
        var parseExp = targetType.Methods.Single(m => m.Name == "ParseStaminaExp");
        if (!parseStamina.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == parseExp.Name))
        {
            var il = parseStamina.Body.GetILProcessor(); var first = parseStamina.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, parseExp));
            il.InsertBefore(first, Instruction.Create(OpCodes.Stfld, expField));
        }
        var userManager = module.Types.Single(t => t.FullName == "NGame2.NAccount.UserManager");
        var applyStamina = userManager.Methods.Single(m => m.Name == "Apply" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == staminaType.FullName);
        var applyExp = userManager.Methods.Single(m => m.Name == "Apply" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == expType.FullName);
        if (!applyStamina.Body.Instructions.Any(i => i.Operand is FieldReference f && f.Name == expField.Name))
        {
            var il = applyStamina.Body.GetILProcessor(); var first = applyStamina.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldfld, expField));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, applyExp));
        }
        foreach (var type in module.GetTypes().Where(t => t.FullName.Contains("MailManagement")))
        foreach (var method in type.Methods.Where(m => m.HasBody && m.ReturnType.FullName == "System.Void"))
        {
            var responseParameter = method.Parameters.FirstOrDefault(p => p.ParameterType.FullName is "NShared.ReceiveMail.Response" or "NShared.ReceiveAllMail.Response");
            if (responseParameter == null || method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "ShowMailCapacityError")) continue;
            var il = method.Body.GetILProcessor(); var first = method.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg, responseParameter));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, targetType.Methods.Single(m => m.Name == "ShowMailCapacityError")));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var unlockNotice = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.NLobby.LobbyPopupQueue")
            .Methods.Single(m => m.Name == "ReservedContentsOpenConditionPopup" && m.Parameters.Count == 1);
        var unlockHelper = targetType.Methods.Single(m => m.Name == "ReserveContentsUnlockNotice");
        if (!unlockNotice.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == unlockHelper.Name))
        {
            var il = unlockNotice.Body.GetILProcessor();
            var first = unlockNotice.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg, unlockNotice.Parameters[0]));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, unlockHelper));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var emptyScene = module.Types.Single(t => t.FullName == "NGame2.NCutScene.CutSceneManager")
            .Methods.Single(m => m.Name == "LoadEmptyScript");
        var missingSceneHelper = targetType.Methods.Single(m => m.Name == "TempleMissingScene");
        if (!emptyScene.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == missingSceneHelper.Name))
        {
            var il = emptyScene.Body.GetILProcessor();
            foreach (var ret in emptyScene.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, missingSceneHelper);
                il.InsertAfter(ret, call); il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var shakmehManager = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.ShakmehDungeonManagement");
        var shakmehActivity = module.Types.Single(t => t.FullName == "NGame2.NLobby.NActivity.OpenShakmehDungeon")
            .Methods.Single(m => m.Name == "Create");
        var branchHelper = targetType.Methods.Single(m => m.Name == "OpenShakmehBranch");
        if (!shakmehActivity.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == branchHelper.Name))
        {
            var il = shakmehActivity.Body.GetILProcessor(); var first = shakmehActivity.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, branchHelper));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, first));
            // The native activity does its work synchronously in its constructor.
            // A handled explicit branch needs no coroutine; ActivityManager accepts null.
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldnull));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        foreach (var method in shakmehManager.Methods.Where(m => m.Name == "OpenShakmehDungeonWindow" && m.IsPublic))
        {
            var helper = targetType.Methods.Single(m => m.Name == "ShakmehAccess");
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == helper.Name)) continue;
            var il = method.Body.GetILProcessor(); var first = method.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, helper));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var shakmehNode = shakmehManager.Methods.Single(m => m.Name == "OpenShakmehDungeonWindow"
            && m.Parameters.Count == 2 && m.Parameters[0].ParameterType.FullName == "System.Int32");
        var bossAccess = targetType.Methods.Single(m => m.Name == "ShakmehBossAccess");
        if (!shakmehNode.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == bossAccess.Name))
        {
            var il = shakmehNode.Body.GetILProcessor(); var first = shakmehNode.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_2));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, bossAccess));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var soulManager = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.NLobby.SoulWeaponManagement");
        foreach (var method in soulManager.Methods.Where(m => m.Name is "OpenGodkingTrialDungeonList" or "OpenGodkingTrialDungeonInfo" or "OpenEclipseMainWithReqeust"))
        {
            var helper = targetType.Methods.Single(m => m.Name == "GodkingAccess");
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == helper.Name)) continue;
            var il = method.Body.GetILProcessor(); var first = method.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, helper));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var moveActivity = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.PortalRenewal")
            .Methods.Single(m => m.Name == "IsMoveActivityType");
        var closePortalHelper = targetType.Methods.Single(m => m.Name == "ClosePortalForActivity");
        if (!moveActivity.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == closePortalHelper.Name))
        {
            var il = moveActivity.Body.GetILProcessor();
            foreach (var ret in moveActivity.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg; ret.Operand = moveActivity.Parameters[0];
                var call = Instruction.Create(OpCodes.Call, closePortalHelper);
                il.InsertAfter(ret, call); il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var subStoryHidden = module.Types.Single(t => t.FullName == "NShared.CampaignNodeData")
            .Methods.Single(m => m.Name == "get_Hidden");
        var subStoryHelper = targetType.Methods.Single(m => m.Name == "ChapterFiveSubStoryHidden");
        if (!subStoryHidden.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == subStoryHelper.Name))
        {
            var il = subStoryHidden.Body.GetILProcessor();
            foreach (var ret in subStoryHidden.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, subStoryHelper);
                il.InsertAfter(ret, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var portalHidden = module.Types.Single(t => t.FullName == "NShared.PortalData")
            .Methods.Single(m => m.Name == "get_Hidden");
        var worldMapHelper = targetType.Methods.Single(m => m.Name == "WorldMapHidden");
        if (!portalHidden.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == worldMapHelper.Name))
        {
            var il = portalHidden.Body.GetILProcessor();
            foreach (var ret in portalHidden.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, worldMapHelper);
                il.InsertAfter(ret, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var mainChapters = module.Types.Single(t => t.FullName == "NShared.CampaignChapterDataContainer")
            .Methods.Single(m => m.Name == "GetMainChapterDataDic");
        var mainHelper = targetType.Methods.Single(m => m.Name == "MainChapters");
        if (!mainChapters.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == mainHelper.Name))
        {
            var il = mainChapters.Body.GetILProcessor();
            foreach (var ret in mainChapters.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Call; ret.Operand = mainHelper;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Ret));
            }
        }
        var chapterOpen = module.Types.Single(t => t.FullName == "NShared.CampaignChapterData")
            .Methods.Single(m => m.Name == "get_IsOpen");
        var chapterHelper = targetType.Methods.Single(m => m.Name == "RestoredCampaignChapterOpen");
        if (!chapterOpen.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == chapterHelper.Name))
        {
            var il = chapterOpen.Body.GetILProcessor();
            foreach (var ret in chapterOpen.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, chapterHelper);
                il.InsertAfter(ret, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var bagMax = module.Types.Single(t => t.FullName == "NShared.InventoryExtendDataContainer")
            .Methods.Single(m => m.Name == "GetExtendMaxCount");
        var bagHelper = targetType.Methods.Single(m => m.Name == "EquipmentBagMaxExtensions");
        if (!bagMax.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == bagHelper.Name))
        {
            var il = bagMax.Body.GetILProcessor();
            foreach (var ret in bagMax.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg;
                ret.Operand = bagMax.Parameters[0];
                var call = Instruction.Create(OpCodes.Call, bagHelper);
                il.InsertAfter(ret, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var storyGetter = module.Types.Single(t => t.FullName == "NGame2.NTutorial.TutorialManager")
            .Methods.Single(m => m.Name == "get_IsTutorialEnabled");
        var storyHelper = targetType.Methods.Single(m => m.Name == "RequiredBattleStoryEnabled");
        if (!storyGetter.Body.Instructions.Any(i => i.Operand is MethodReference m && m.FullName == storyHelper.FullName))
        {
            var il = storyGetter.Body.GetILProcessor();
            foreach (var ret in storyGetter.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Call;
                ret.Operand = storyHelper;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Ret));
            }
        }
        var punishOpen = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.PunishManagment")
            .Methods.Single(m => m.Name == "OpenPunishMainWindow");
        // Shipped factory reads argument zero for both coordinates. The
        // chapter-map destination is 11/700, so read the dungeon from slot one.
        var punishCreate = module.Types.Single(t => t.FullName == "NGame2.NLobby.NActivity.OpenPunishmentRaid")
            .Methods.Single(m => m.Name == "Create");
        var punishArgReads = punishCreate.Body.Instructions.Where(i =>
            i.Operand is MethodReference m && m.Name == "GetValueInt").ToArray();
        if (punishArgReads.Length != 3) throw new InvalidOperationException("Unexpected Apocalypsion activity arguments");
        var dungeonArgSlot = punishArgReads[1].Previous.Previous;
        if (dungeonArgSlot.OpCode != OpCodes.Ldc_I4_0 && dungeonArgSlot.OpCode != OpCodes.Ldc_I4_1)
            throw new InvalidOperationException("Unexpected Apocalypsion dungeon argument slot");
        dungeonArgSlot.OpCode = OpCodes.Ldc_I4_1;
        dungeonArgSlot.Operand = null;
        var punishAccess = targetType.Methods.Single(m => m.Name == "ApocalypsionAccess");
        if (!punishOpen.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == punishAccess.Name))
        {
            var il = punishOpen.Body.GetILProcessor(); var first = punishOpen.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, punishAccess));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var trialWindow = module.Types.Single(t => t.Name == "ContentUIGodkingTrialDungeonList");
        foreach (var pair in new[] { ("Open", "GodkingWindowBackground"), ("OnClosing", "GodkingReturnToPortal") })
        {
            var method = trialWindow.Methods.Single(m => m.Name == pair.Item1);
            var helper = targetType.Methods.Single(m => m.Name == pair.Item2);
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == helper.Name)) continue;
            var il = method.Body.GetILProcessor();
            foreach (var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = pair.Item1 == "Open" ? OpCodes.Ldarg_0 : OpCodes.Nop;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, helper));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        var dispatchMenu = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.LobbyTopmostMenu")
            .Methods.Single(m => m.Name == "ShowDispatchBattleMenu");
        var dispatchVisible = targetType.Methods.Single(m => m.Name == "DispatchShortcutVisible");
        if (!dispatchMenu.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == dispatchVisible.Name))
        {
            var il = dispatchMenu.Body.GetILProcessor(); var first = dispatchMenu.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, dispatchVisible));
            il.InsertBefore(first, Instruction.Create(OpCodes.Starg, dispatchMenu.Parameters[0]));
        }
        var dispatchClick = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.LobbyTopmostMenu")
            .Methods.Single(m => m.Name == "OnClickDispatchBattleMenu");
        var dispatchEntries = targetType.Methods.Single(m => m.Name == "DispatchListHasEntries");
        if (!dispatchClick.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == dispatchEntries.Name))
        {
            var il = dispatchClick.Body.GetILProcessor(); var first = dispatchClick.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, dispatchEntries));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var heroDeck = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.HeroDeck2")
            .Methods.Single(m => m.Name == "CreateHeroItem" && m.Parameters.Count == 0);
        var dispatchHeroes = targetType.Methods.Single(m => m.Name == "RefreshHeroDeckDispatch");
        if (!heroDeck.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == dispatchHeroes.Name))
        {
            var il = heroDeck.Body.GetILProcessor();
            foreach (var ret in heroDeck.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, dispatchHeroes));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        var cardType = module.Types.Single(t => t.FullName == "NGame2.NUI.NComponent2.PortalContentsComponent");
        var cardBackground = cardType.Methods.Single(m => m.Name == "SetBackgroundTexture");
        var curveHelper = targetType.Methods.Single(m => m.Name == "SetupCardCurve");
        if (!cardBackground.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == curveHelper.Name))
        {
            var il = cardBackground.Body.GetILProcessor();
            var first = cardBackground.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, cardType.Methods.Single(m => m.Name == "get__portalRenewalData")));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, curveHelper));
        }
        var container = module.Types.Single(t => t.FullName == "NShared.PortalRenewalDataContainer");
        // The native Shakmeh category has one Window panel. Reconstructed Small
        // cards must use the ordinary loop instead of looking for that missing panel.
        var refreshPortal = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.PortalRenewal")
            .Methods.Single(m => m.Name == "RefreshContentsUI");
        var windowHelper = targetType.Methods.Single(m => m.Name == "UseShakmehWindow");
        if (!refreshPortal.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == windowHelper.Name))
        {
            var categoryCheck = refreshPortal.Body.Instructions.Single(i =>
                i.Operand is MethodReference m && m.Name == "get_Category"
                && i.Next?.OpCode == OpCodes.Ldc_I4_S && Convert.ToInt32(i.Next.Operand) == 12);
            var branch = categoryCheck.Next.Next;
            if (branch.OpCode != OpCodes.Bne_Un && branch.OpCode != OpCodes.Bne_Un_S)
                throw new InvalidOperationException("Unexpected Shakmeh panel branch");
            categoryCheck.OpCode = OpCodes.Call; categoryCheck.Operand = windowHelper;
            categoryCheck.Next.OpCode = OpCodes.Nop; categoryCheck.Next.Operand = null;
            branch.OpCode = OpCodes.Brfalse;
        }
        foreach (var pair in new[] { ("GetCategoryDatas", "Categories"), ("GetData", "Contents") })
        {
            var target = container.Methods.Single(m => m.Name == pair.Item1);
            var helper = targetType.Methods.Single(m => m.Name == pair.Item2);
            if (target.Body.Instructions.Any(i => i.Operand is MethodReference m && m.FullName == helper.FullName)) continue;
            // Mutate the return itself so existing branches to it also pass through the hook.
            foreach (var ret in target.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                var il = target.Body.GetILProcessor();
                ret.OpCode = pair.Item1 == "GetData" ? OpCodes.Ldarg_1 : OpCodes.Nop;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, helper));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        var raids = module.Types.Single(t => t.FullName == "NShared.RaidDataContainer");
        foreach (var pair in new[] { ("GetData", "Raid"), ("GetRaidIndices", "RaidIndices"), ("GetRaidLevels", "RaidLevels"), ("GetRaidDataListByType", "RaidList"), ("GetRaidDataByDungeon", "RaidDungeon"), ("GetMultiRaidIndexBySingleRaidIndex", "MultiRaid") })
        {
            var target = raids.Methods.Single(m => m.Name == pair.Item1);
            var helper = targetType.Methods.Single(m => m.Name == pair.Item2);
            if (target.Body.Instructions.Any(i => i.Operand is MethodReference m && m.FullName == helper.FullName)) continue;
            foreach (var ret in target.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Nop;
                var il = target.Body.GetILProcessor();
                var cursor = ret;
                foreach (var parameter in target.Parameters)
                {
                    var load = Instruction.Create(OpCodes.Ldarg, parameter);
                    il.InsertAfter(cursor, load); cursor = load;
                }
                var call = Instruction.Create(OpCodes.Call, helper);
                il.InsertAfter(cursor, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        foreach (var pair in new[] { ("OpenValanceCraft", "OpenValanceCraft"), ("OpenValanceIndentified", "OpenValanceIdentified"), ("OpenValanceManage", "OpenValanceManage") })
        {
            var action = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.NEntryMenu.NAction." + pair.Item1);
            var run = action.Methods.Single(m => m.Name.EndsWith(".Run") && m.Parameters.Count == 3);
            run.Body = new MethodBody(run);
            var il = run.Body.GetILProcessor();
            il.Append(Instruction.Create(OpCodes.Ldarg_1));
            il.Append(Instruction.Create(OpCodes.Call, targetType.Methods.Single(m => m.Name == pair.Item2)));
            il.Append(Instruction.Create(OpCodes.Ret));
        }
        var matchSelect = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.MatchManagement")
            .Methods.Single(m => m.Name == "OpenMatchSelect" && m.Parameters.Count == 3);
        if (!matchSelect.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "RestoredPortal" && m.Name == "ArenaMenu"))
        {
            var il = matchSelect.Body.GetILProcessor();
            var first = matchSelect.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_3));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, targetType.Methods.Single(m => m.Name == "ArenaMenu")));
            il.InsertBefore(first, Instruction.Create(OpCodes.Starg, matchSelect.Parameters[2]));
        }
        var lobbyUpdate = module.Types.Single(t => t.FullName == "NGame2.NScene.LobbyScene2")
            .Methods.Single(m => m.Name == "Update" && m.Parameters.Count == 0);
        var shortcutHelper = targetType.Methods.Single(m => m.Name == "MenuShortcutKeyUp");
        foreach (var call in lobbyUpdate.Body.Instructions.Where(i => i.Operand is MethodReference m
            && m.DeclaringType.FullName == "UnityEngine.Input" && m.Name == "GetKeyUp").ToArray())
        {
            int? key = call.Previous.OpCode == OpCodes.Ldc_I4 ? (int)call.Previous.Operand
                : call.Previous.OpCode == OpCodes.Ldc_I4_S ? (sbyte)call.Previous.Operand : null;
            // B = inventory, H = heroes, P = special shop. Keep Escape and the
            // rest of the lobby update running normally during text entry.
            if (key is 98 or 104 or 112) call.Operand = shortcutHelper;
        }
        if (lobbyUpdate.Body.Instructions.Count(i => i.Operand is MethodReference m
            && m.DeclaringType.Name == "RestoredPortal" && m.Name == "MenuShortcutKeyUp") != 3)
            throw new InvalidOperationException("Expected all three lobby menu shortcuts to check text focus");
        var gaugeInfo = module.Types.Single(t => t.FullName == "NGame2.NUI.NComponent2.NStat.StatGaugeInfo");
        var dragHelper = targetType.Methods.Single(m => m.Name == "WorkshopOptionDrag");
        foreach (var method in gaugeInfo.Methods.Where(m =>
            (m.Name == "Open" && m.Parameters.Count == 6) || m.Name == "OpenExtraSkill"))
        {
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == dragHelper.Name)) continue;
            foreach (var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0;
                var il = method.Body.GetILProcessor();
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, dragHelper));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        if (gaugeInfo.Methods.Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count(i =>
            i.Operand is MethodReference call && call.Name == "WorkshopOptionDrag")) != 2)
            throw new InvalidOperationException("Expected two Technomagic option drag hooks");
        var perkPopup = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.LearnTranscendSkillMessagePopup");
        var openPerk = perkPopup.Methods.Single(m => m.Name == "OpenInternal" && m.Parameters.Count == 3);
        var perkHelper = targetType.Methods.Single(m => m.Name == "ReadablePerkDescription");
        if (!openPerk.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == perkHelper.Name))
        {
            // Only the successful final return. Invalid skill indices close
            // the popup and return earlier without touching its layout.
            var ret = openPerk.Body.Instructions.Last(i => i.OpCode == OpCodes.Ret);
            ret.OpCode = OpCodes.Ldarg_0;
            var il = openPerk.Body.GetILProcessor();
            il.InsertAfter(ret, Instruction.Create(OpCodes.Call, perkHelper));
            il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
        }
        if (openPerk.Body.Instructions.Count(i => i.Operand is MethodReference call && call.Name == "ReadablePerkDescription") != 1)
            throw new InvalidOperationException("Expected one readable perk popup hook");
        if (module.AssemblyReferences.Any(r => r.Name == "PortalRestore"))
            throw new InvalidOperationException("Portal patch leaked a helper assembly reference");
        if (nativeCombatTables) NativeCombatTables.Apply(module);
        if (NativeStaticData.ValidateInstalled(Path.GetDirectoryName(module.FileName)!)) NativeStaticData.Apply(module);
        Console.WriteLine("Restored missing Portal categories and shared reconstructed raid definitions.");
    }
}

