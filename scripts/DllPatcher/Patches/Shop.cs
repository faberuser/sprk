using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchShopShortcut(ModuleDefinition module)
    {
        PatchShopOpening(module);
        var method = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.LobbyRightMenu")
            .Methods.Single(m => m.Name == "SetupButtons");
        var comparisons = method.Body.Instructions.Where(i =>
            i.Previous?.Operand is MethodReference getter && getter.FullName == "NShared.ERightMenuButton NShared.BottomButtonData::get_Type()"
            && (i.Next?.OpCode == OpCodes.Bne_Un_S || i.Next?.OpCode == OpCodes.Bne_Un)).ToArray();
        var shop = comparisons.SingleOrDefault(i => i.OpCode == OpCodes.Ldc_I4_7);
        if (shop == null)
        {
            if (comparisons.Any(i => i.OpCode == OpCodes.Ldc_I4_S && Convert.ToInt32(i.Operand) == 10))
            { Console.WriteLine("Shop shortcut already restored"); return; }
            throw new InvalidOperationException("Expected shop suppression check was not found");
        }
        // Hide only HeroStat (10); let PayShop (7) follow the normal tutorial/
        // dungeon visibility check, native click handler, and grid sizing.
        shop.OpCode = OpCodes.Ldc_I4_S;
        shop.Operand = (sbyte)10;
        Console.WriteLine("Restored old Shop shortcut with native unlock rules");
    }

    static void PatchShopOpening(ModuleDefinition module)
    {
        PatchShopCostumeCategory(module);
        var manager = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.PayShopManagement");
        var window = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.NPayShop.PayShop");
        var get = manager.Methods.Where(m => m.HasBody).SelectMany(m => m.Body.Instructions)
            .Select(i => i.Operand).OfType<GenericInstanceMethod>().First(m => m.Name == "GetInstance"
                && m.GenericArguments[0].FullName == window.FullName);
        var open = window.Methods.Single(m => m.Name == "Open" && m.Parameters.Count == 2);
        var select = window.Methods.Single(m => m.Name == "SelectCategory");
        var animate = window.Methods.Single(m => m.Name == "PlayOpenAnimation");
        var detailed = manager.Methods.Single(m => m.Name == "OpenNewPayShop" && m.Parameters.Count == 3);
        foreach (var method in manager.Methods.Where(m => m.Name == "OpenNewPayShop" && (m.Parameters.Count == 0 || m.Parameters.Count == 3)))
        {
            method.Body = new MethodBody(method) { InitLocals = true };
            var il = method.Body.GetILProcessor();
            if (method.Parameters.Count == 0)
            {
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldstr, "Hero");
                il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Call, detailed); il.Emit(OpCodes.Ret);
                continue;
            }
            var local = new VariableDefinition(window); method.Body.Variables.Add(local);
            var done = il.Create(OpCodes.Ret);
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Call, get); il.Emit(OpCodes.Stloc, local);
            il.Emit(OpCodes.Ldloc, local); il.Emit(OpCodes.Brfalse, done);
            il.Emit(OpCodes.Ldloc, local); il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldarg_3); il.Emit(OpCodes.Callvirt, open);
            il.Emit(OpCodes.Ldloc, local); il.Emit(OpCodes.Ldarg_1); il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Callvirt, select);
            il.Emit(OpCodes.Ldarg_2); il.Emit(OpCodes.Brfalse, done);
            il.Emit(OpCodes.Ldloc, local); il.Emit(OpCodes.Callvirt, animate); il.Append(done);
        }
        Console.WriteLine("Restored native shop window opening in place of PTS popup stubs");
    }

    static void PatchShopCostumeCategory(ModuleDefinition module)
    {
        RestoreCostumeOwnership(module);
        // The PTS costume table also disables buying. The shop filters all
        // heroes out unless they have a priced, buyable costume.
        PatchCostumeBuying(module);
        var type = module.Types.Single(t => t.FullName == "NShared.NewPayShopGroupData");
        var getter = type.Methods.Single(m => m.Name == "get_IsEnable");
        var category = type.Methods.Single(m => m.Name == "get_PayShopCategoryType");
        var backing = type.Fields.Single(f => f.Name == "<IsEnable>k__BackingField");
        var equals = module.Types.SelectMany(t => t.Methods).Where(m => m.HasBody)
            .SelectMany(m => m.Body.Instructions).Select(i => i.Operand).OfType<MethodReference>()
            .First(m => m.DeclaringType.FullName == "System.String" && m.Name == "op_Equality");
        getter.Body = new MethodBody(getter);
        var il = getter.Body.GetILProcessor();
        var enabled = il.Create(OpCodes.Ldc_I4_1);
        foreach (var key in new[] { "Custom", "Costume" })
        {
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, category);
            il.Emit(OpCodes.Ldstr, key); il.Emit(OpCodes.Call, equals); il.Emit(OpCodes.Brtrue, enabled);
        }
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, backing); il.Emit(OpCodes.Ret);
        il.Append(enabled); il.Emit(OpCodes.Ret);
        Console.WriteLine("Restored Custom / Costume shop categories; other category flags preserved");
    }
}
