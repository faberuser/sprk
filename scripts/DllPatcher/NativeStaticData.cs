using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

public static class NativeStaticData
{
    public const string ManifestName = "SPRK-NativeData.json";
    public static bool ValidateInstalled(string managedDirectory)
    {
        var root=Path.GetFullPath(Path.Combine(managedDirectory,"../Documents/Patch/StandaloneWindows"));
        var path=Path.Combine(root,ManifestName);
        if(!File.Exists(path))return false;
        using var manifest=JsonDocument.Parse(File.ReadAllText(path));
        if(manifest.RootElement.GetProperty("version").GetInt32()!=1)throw new InvalidOperationException("Unsupported native data manifest");
        var files=manifest.RootElement.GetProperty("files");
        foreach(var name in new[]{"Raid","WeaponUniqueSelectItem","SelectItem","HeroSelectItem","CraftItem","NPC","CampaignChapter","CampaignNode","Portal","GodkingTrialGroup",
            "PortalRenewal","ShopItem","GuildSuppressDungeon","InventoryExtend","OptionSell","MainStory","Creature","Costume","HairCostume","WeaponCostume","AccessoryCostume"})
            if(!files.TryGetProperty($"TableJit/{name}Table.jit",out _))throw new InvalidOperationException("Native data manifest is missing "+name+"Table.jit");
        foreach(var name in new[]{"LocalizationJit/LocalizationAll_English.jit","ContentsDefine/SoulWeaponLimitBreak.json"})
            if(!files.TryGetProperty(name,out _))throw new InvalidOperationException("Native data manifest is missing "+name);
        foreach(var entry in files.EnumerateObject()) {
            var target=Path.GetFullPath(Path.Combine(root,entry.Name));
            if(!target.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Invalid native data path");
            if(!File.Exists(target)||!Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(target))).Equals(entry.Value.GetProperty("sha256").GetString(),StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Baked native data changed: "+entry.Name);
        }
        return true;
    }

    public static void Apply(ModuleDefinition module)
    {
        var portal=module.Types.Single(t=>t.Name=="RestoredPortal");
        var methods=module.GetTypes().Where(t=>t!=portal).SelectMany(t=>t.Methods).Where(m=>m.HasBody).ToArray();
        void Nop(Instruction i){i.OpCode=OpCodes.Nop;i.Operand=null;}
        void DropCall(Instruction instruction,int arguments) {
            var prior=instruction.Previous;
            for(int i=0;i<arguments;i++) {
                if(prior==null||!(prior.OpCode.Code is Code.Ldarg or Code.Ldarg_S or Code.Ldarg_0 or Code.Ldarg_1 or Code.Ldarg_2 or Code.Ldarg_3 or Code.Ldstr))
                    throw new InvalidOperationException("Unexpected argument sequence before "+instruction.Operand);
                var next=prior.Previous;Nop(prior);prior=next;
            }
            Nop(instruction);
        }
        var passThrough=new HashSet<string>{"Raid","RaidDungeon","RaidIndices","RaidLevels","RaidList","MultiRaid","Categories","Contents",
            "RestoreGuildShopItem","RestoreGuildShopList","RestoreGuildShopGroup","OrvelNode","OrvelNodes","MainChapters",
            "RestoreInventoryFilterOptions","EquipmentBagMaxExtensions","CombatGradeStat","RestoreMissingCombatOperation"};
        var voidHooks=new HashSet<string>{"RestoreOrvelNpcMenus","RestoreGodkingClassRules","RestoreInventoryGradeLabel","FinalizeCombatContainer","RestorePunishmentOperation","InitializeCombatConstants"};
        var direct=new HashSet<string>{"PunishmentSkill","CombatCreature","CombatState","CombatProjectile","ConquestDungeon"};
        int removed=0;
        foreach(var method in methods)foreach(var instruction in method.Body.Instructions.ToArray()) {
            if(instruction.Operand is not MethodReference call)continue;
            if(call.DeclaringType.Name=="SprkCrafting") {
                MethodReference lookup;
                if(call.Name=="Category")lookup=module.GetType("NShared.CraftItemDataContainer").Methods.Single(m=>m.Name=="GetCategoryData");
                else {
                    var type=module.GetType(call.Name=="Get"?"NShared.NJit.JitDataContainerDefault`2":"NShared.NJit.JitDataContainer`1");
                    var source=type.Methods.Single(m=>m.Name==(call.Name=="Get"?"GetData":"GetAllData"));
                    var closed=new GenericInstanceType(type);
                    if(call.Name=="Get")closed.GenericArguments.Add(module.TypeSystem.Int32);
                    closed.GenericArguments.Add(module.GetType("NShared.CraftItemData"));
                    lookup=new MethodReference(source.Name,source.ReturnType,closed){HasThis=true};
                    foreach(var parameter in source.Parameters)lookup.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
                }
                instruction.OpCode=OpCodes.Callvirt;instruction.Operand=module.ImportReference(lookup);removed++;continue;
            }
            if(call.DeclaringType.Name=="SprkDispatch"&&call.Name=="HelpText"){Nop(instruction);removed++;continue;}
            if(call.DeclaringType.Name!="RestoredPortal")continue;
            if(direct.Contains(call.Name)) {
                var source=portal.Methods.Single(m=>m.Name==call.Name);
                var lookup=(MethodReference)source.Body.Instructions.First(i=>i.Operand is MethodReference m&&m.Name=="GetData").Operand;
                instruction.OpCode=OpCodes.Callvirt;instruction.Operand=lookup;removed++;
            } else if(passThrough.Contains(call.Name)){DropCall(instruction,call.Parameters.Count-1);removed++;}
            else if(voidHooks.Contains(call.Name)){DropCall(instruction,call.Parameters.Count);removed++;}
            else if(call.Name=="IsRemovedPunishmentOperation") {
                // The baked operation dictionary already omits removed rows.
                var branch=instruction.Next;
                if(branch.OpCode.Code is not (Code.Brfalse or Code.Brfalse_S))throw new InvalidOperationException("Unexpected operation removal branch");
                var loadNull=branch.Next;
                if(loadNull.OpCode!=OpCodes.Ldnull)throw new InvalidOperationException("Unexpected operation removal return");
                var ret=loadNull.Next;while(ret.OpCode!=OpCodes.Ret)ret=ret.Next;
                DropCall(instruction,2);Nop(branch);Nop(loadNull);Nop(ret);removed++;
            }
        }
        foreach(var method in portal.Methods.Where(m=>m.HasBody))foreach(var instruction in method.Body.Instructions.ToArray())
            if(instruction.Operand is MethodReference call&&call.DeclaringType.Name=="RestoredPortal"&&voidHooks.Contains(call.Name)){DropCall(instruction,call.Parameters.Count);removed++;}
        void Getter(string typeName,string property) {
            var type=module.GetType("NShared."+typeName);
            var getter=type.Methods.Single(m=>m.Name=="get_"+property);
            var field=type.Fields.Single(f=>f.Name=="<"+property+">k__BackingField");
            getter.Body=new MethodBody(getter);var il=getter.Body.GetILProcessor();il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,field);il.Emit(OpCodes.Ret);
        }
        foreach(var (type,property) in new[]{("SkillLevelFactorData","Factor5"),("SoulWeaponOptionData","OptionRatioPerValue1"),("SoulWeaponOptionData","OptionRatioPerValue2"),
            ("BattleDefineData","MaxTime"),("BattleDefineData","ExtraTime"),("ConstantData","Value"),("ItemBaseData","BuyGuildPoint"),("ItemBaseData","BuyGuildArenaPoint"),
            ("CampaignChapterData","IsOpen"),("CampaignNodeData","Hidden"),("PortalData","Hidden"),("MainStoryData","Condition"),
            ("WeaponUniqueSelectItemData","ItemIndices"),("SelectItemData","ItemIndices"),("HeroSelectItemData","HeroIndices"),("CreatureData","OpenType"),("NewPayShopGroupData","IsEnable")})Getter(type,property);
        foreach(var property in new[]{"PhysicalAttack","MagicalAttack","PhysicalDefense","MagicalDefense","MaxHp"})Getter("MonsterTierData",property);
        foreach(var name in new[]{"CostumeData","HairCostumeData","WeaponCostumeData","AccessoryCostumeData"}) {
            var type=module.GetType("NShared."+name);
            foreach(var property in new[]{"IsOpen","IsShow","IsBuy","ReqBuyGem","BuyType","PreviewableWhenOwned","PreviewableWhenNeedCostumeOwned","PreviewableWhenNeedHairOwned","PreviewableWhenNeedWeaponOwned"})
                if(type.Fields.Any(f=>f.Name=="<"+property+">k__BackingField"))Getter(name,property);
        }
        // Cache network DTOs once, before GuildSuppressManagement receives the response.
        foreach(var name in new[]{"JM_NShared_GetGuildSuppressSessionInfo_Response","JM_NShared_GetGuildSuppressStatusBoard_Response"}) {
            var parser=module.GetType(name)?.Methods.SingleOrDefault(m=>m.Name=="Parse"&&m.Parameters.Count==2&&m.Parameters[0].ParameterType.FullName=="System.Collections.IDictionary");
            if(parser==null)continue;
            if(parser.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="AcceptServerConquestSessions"))continue;
            var il=parser.Body.GetILProcessor();var first=parser.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));il.InsertBefore(first,il.Create(OpCodes.Call,portal.Methods.Single(m=>m.Name=="AcceptServerConquestSessions")));
        }
        var checker=module.GetType("NGame2.NUtil.ConditionChecker").Methods.Single(m=>m.Name=="Check"&&m.Parameters.Count==1);
        if(!checker.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.Name=="CheckNativeStoryCondition")) {
            var il=checker.Body.GetILProcessor();var first=checker.Body.Instructions[0];
            var equality=new MethodReference("op_Equality",module.TypeSystem.Boolean,module.TypeSystem.String);
            equality.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));equality.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
            foreach(var instruction in new[]{il.Create(OpCodes.Ldarg_1),il.Create(OpCodes.Brfalse,first),il.Create(OpCodes.Ldarg_1),il.Create(OpCodes.Ldlen),il.Create(OpCodes.Conv_I4),
                il.Create(OpCodes.Ldc_I4_5),il.Create(OpCodes.Bne_Un,first),il.Create(OpCodes.Ldarg_1),il.Create(OpCodes.Ldc_I4_0),il.Create(OpCodes.Ldelem_Ref),
                il.Create(OpCodes.Ldstr,"CompleteTutorialOrSkippedDungeon"),il.Create(OpCodes.Call,equality),il.Create(OpCodes.Brfalse,first),
                il.Create(OpCodes.Ldarg_1),il.Create(OpCodes.Call,portal.Methods.Single(m=>m.Name=="CheckNativeStoryCondition")),il.Create(OpCodes.Ret)})il.InsertBefore(first,instruction);
        }
        RestoreLimitBreak(module);
        // Keep callable native lookups for the optional diagnostic tool, but no
        // gameplay path calls them. No JSON initialization or correction caches.
        var ctor=portal.Methods.Single(m=>m.IsConstructor&&m.IsStatic);
        var constructor=(MethodReference)ctor.Body.Instructions.Single(i=>i.Operand is MethodReference m&&m.DeclaringType.FullName=="NShared.NUtil.ExponentialTable"&&m.Name==".ctor").Operand;
        ctor.Body=new MethodBody(ctor);var ctorIL=ctor.Body.GetILProcessor();ctorIL.Emit(OpCodes.Ldc_I4,1000000);ctorIL.Emit(OpCodes.Newobj,constructor);
        ctorIL.Emit(OpCodes.Stsfld,portal.Fields.Single(f=>f.Name=="PunishmentDefenseRatios"));ctorIL.Emit(OpCodes.Ret);
        var live=new HashSet<MethodDefinition>(portal.Methods.Where(m=>m.IsConstructor||m.Name is "PunishmentSkill" or "CombatCreature" or "CombatState" or "CombatProjectile" or "MenuShortcutKeyUp"));
        void Follow(IEnumerable<MethodDefinition> roots) {
            foreach(var method in roots)foreach(var call in method.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Where(m=>m.DeclaringType.Name=="RestoredPortal"))
                live.Add(portal.Methods.Single(m=>m.Name==call.Name));
        }
        Follow(methods);int before;
        do{before=live.Count;Follow(live.ToArray());}while(before!=live.Count);
        foreach(var method in portal.Methods.Where(m=>!live.Contains(m)).ToArray())portal.Methods.Remove(method);
        var liveFields=live.SelectMany(m=>m.Body.Instructions.Select(i=>i.Operand).OfType<FieldReference>()).Where(f=>f.DeclaringType.Name=="RestoredPortal").Select(f=>f.Name).ToHashSet();
        foreach(var field in portal.Fields.Where(f=>!liveFields.Contains(f.Name)).ToArray())portal.Fields.Remove(field);
        foreach(var reference in module.AssemblyReferences.Where(r=>r.Name=="SprkCrafting").ToArray())module.AssemblyReferences.Remove(reference);
        Console.WriteLine($"Native static data: removed {removed} lookup/mutation calls and unused restoration payloads; Conquest calendar comes from the server.");
    }

    static void RestoreLimitBreak(ModuleDefinition module)
    {
        var type=module.GetType("NShared.NContentsDefine.SoulWeaponLimitBreak");
        var inner=type.NestedTypes.Single(t=>t.Name=="InnerData");var innerField=type.Fields.Single(f=>f.Name=="_inner");
        var init=type.Methods.Single(m=>m.Name=="InitInner");
        MethodReference TryGet(FieldDefinition field) {
            var generic=(GenericInstanceType)field.FieldType;
            var method=new MethodReference("TryGetValue",module.TypeSystem.Boolean,generic){HasThis=true};
            if(generic.ElementType.GenericParameters.Count==0){generic.ElementType.GenericParameters.Add(new GenericParameter("TKey",generic.ElementType));generic.ElementType.GenericParameters.Add(new GenericParameter("TValue",generic.ElementType));}
            method.Parameters.Add(new ParameterDefinition(generic.ElementType.GenericParameters[0]));
            method.Parameters.Add(new ParameterDefinition(new ByReferenceType(generic.ElementType.GenericParameters[1])));
            return method;
        }
        var max=type.Methods.Single(m=>m.Name=="TryGetMaxStar");max.Body=new MethodBody(max);var il=max.Body.GetILProcessor();
        var maxField=inner.Fields.Single(f=>f.Name=="<MaxStarByDetailIndex>k__BackingField");
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Call,init);il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,innerField);il.Emit(OpCodes.Callvirt,inner.Methods.Single(m=>m.Name=="get_MaxStarByDetailIndex"));
        il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Ldarg_2);il.Emit(OpCodes.Callvirt,TryGet(maxField));il.Emit(OpCodes.Ret);
        var get=type.Methods.Single(m=>m.Name=="GetDataByResultStar");get.Body=new MethodBody(get){InitLocals=true};
        var result=new VariableDefinition(get.ReturnType);get.Body.Variables.Add(result);il=get.Body.GetILProcessor();
        var map=inner.Fields.Single(f=>f.Name=="<DataMap>k__BackingField");var key=type.NestedTypes.Single(t=>t.Name=="MapKey");
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Call,init);il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,innerField);il.Emit(OpCodes.Callvirt,inner.Methods.Single(m=>m.Name=="get_DataMap"));
        il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Ldarg_2);il.Emit(OpCodes.Newobj,key.Methods.Single(m=>m.IsConstructor&&m.Parameters.Count==2));
        il.Emit(OpCodes.Ldloca,result);il.Emit(OpCodes.Callvirt,TryGet(map));il.Emit(OpCodes.Pop);il.Emit(OpCodes.Ldloc,result);il.Emit(OpCodes.Ret);
        var after=type.Methods.Single(m=>m.Name=="GetAfterDataByCurrent");after.Body=new MethodBody(after);il=after.Body.GetILProcessor();var none=il.Create(OpCodes.Ldnull);
        var info=module.GetType("NShared.EquipItemInfo");var equip=info.Methods.Single(m=>m.Name=="get_EquipItemData");
        il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Brfalse,none);il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Callvirt,equip);il.Emit(OpCodes.Brfalse,none);
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Callvirt,equip);il.Emit(OpCodes.Callvirt,module.GetType("NShared.EquipItemData").Methods.Single(m=>m.Name=="get_DetailIndex"));
        il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Callvirt,info.Methods.Single(m=>m.Name=="get_Star"));il.Emit(OpCodes.Ldc_I4_1);il.Emit(OpCodes.Add);il.Emit(OpCodes.Call,get);il.Emit(OpCodes.Ret);il.Append(none);il.Emit(OpCodes.Ret);
    }
}
