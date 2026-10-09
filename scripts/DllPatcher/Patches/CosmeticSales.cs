using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchCostumeBuying(ModuleDefinition module)
    {
        foreach (var name in new[] { "CostumeData", "HairCostumeData", "WeaponCostumeData", "AccessoryCostumeData" })
        {
            var type = module.Types.Single(t => t.FullName == "NShared." + name);
            PatchBooleanGetter(module, type.FullName, "get_IsOpen", true);
            PatchBooleanGetter(module, type.FullName, "get_IsShow", true);
            var getter = type.Methods.Single(m => m.Name == "get_ReqBuyGem");
            getter.Body = new MethodBody(getter);
            var il = getter.Body.GetILProcessor();
            var originalPrice = il.Create(OpCodes.Ldarg_0);
            var gem = type.Fields.Single(f => f.Name == "<ReqBuyGem>k__BackingField");
            foreach (var price in name == "CostumeData" ? new[] { "ReqBuyGem", "ReqBuyGold", "ReqBuyMileage" } : new[] { "ReqBuyGem" })
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, type.Fields.Single(f => f.Name == $"<{price}>k__BackingField"));
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Bgt, originalPrice);
            }
            if (name != "AccessoryCostumeData")
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, type.Methods.Single(m => m.Name == "get_IsDefault"));
                il.Emit(OpCodes.Brtrue, originalPrice);
            }
            if (name == "HairCostumeData" || name == "WeaponCostumeData")
            {
                var need = type.Methods.Single(m => m.Name == "get_NeedCostume");
                var fallback = il.Create(OpCodes.Ldc_I4, 3000);
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, need); il.Emit(OpCodes.Brfalse, fallback);
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, need); il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Brtrue, originalPrice);
                il.Append(fallback);
            }
            else il.Emit(OpCodes.Ldc_I4, name == "AccessoryCostumeData" ? 500 : 3000);
            il.Emit(OpCodes.Ret);
            il.Append(originalPrice); il.Emit(OpCodes.Ldfld, gem); il.Emit(OpCodes.Ret);

            if (name == "CostumeData")
            {
                var buy = type.Methods.Single(m => m.Name == "get_IsBuy");
                buy.Body = new MethodBody(buy);
                var code = buy.Body.GetILProcessor();
                code.Emit(OpCodes.Ldarg_0);
                code.Emit(OpCodes.Call, type.Methods.Single(m => m.Name == "get_IsDefault"));
                code.Emit(OpCodes.Ldc_I4_0); code.Emit(OpCodes.Ceq); code.Emit(OpCodes.Ret);
            }
            if (name == "AccessoryCostumeData") PatchBooleanGetter(module, type.FullName, "get_IsBuy", true);
        }
        // Ungrouped outfits are purchasable in the Dressing Room. The shop's
        // grouped hero tiles require group metadata and must still skip them.
        var helper = module.Types.Single(t => t.FullName == "NGame2.NUI.NHelper.NPayShop.PayShopHelper");
        var group = module.Types.Single(t => t.FullName == "NShared.CostumeData").Methods.Single(m => m.Name == "get_Group");
        var empty = new MethodReference("IsNullOrEmpty", module.TypeSystem.Boolean, module.TypeSystem.String);
        empty.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        foreach (var predicate in helper.NestedTypes.SelectMany(t => t.Methods)
                     .Where(m => m.Name.StartsWith("<GetBuyableCostumeByHero") && m.HasBody))
        {
            if (predicate.Body.Instructions.Any(i => i.Operand is MethodReference mr && mr.Name == "get_Group")) continue;
            var il = predicate.Body.GetILProcessor();
            var first = predicate.Body.Instructions[0];
            foreach (var instruction in new[] {
                il.Create(OpCodes.Ldarg_1), il.Create(OpCodes.Callvirt, group), il.Create(OpCodes.Call, empty),
                il.Create(OpCodes.Brfalse, first), il.Create(OpCodes.Ldc_I4_0), il.Create(OpCodes.Ret) })
                il.InsertBefore(first, instruction);
        }
        Console.WriteLine("Enabled cosmetic sales: existing prices preserved; unpriced outfits 3000 rubies, accessories 500; bundled parts stay included");
    }
}
