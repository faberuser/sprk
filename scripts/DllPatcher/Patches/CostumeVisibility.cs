using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchHeroAndCostumeVisibility(ModuleDefinition module)
    {
        RestoreCostumeOwnership(module);
        PatchCreatureOpenTypeGetter(module);
        // Preserve the original ownership checks so the shop can offer unowned costumes.
        PatchCostumeBuying(module);
        PatchHeroCostumeViewMotionButton(module);

        // Costume IsOpen flags
        PatchBooleanGetter(module, "NShared.CostumeData", "get_IsOpen", true);
        PatchBooleanGetter(module, "NShared.HairCostumeData", "get_IsOpen", true);
        PatchBooleanGetter(module, "NShared.WeaponCostumeData", "get_IsOpen", true);
        PatchBooleanGetter(module, "NShared.AccessoryCostumeData", "get_IsOpen", true);

        // Costume IsShow flags (used by HeroDetailView costume list / motion-entry visibility)
        PatchBooleanGetter(module, "NShared.CostumeData", "get_IsShow", true);
        PatchBooleanGetter(module, "NShared.HairCostumeData", "get_IsShow", true);
        PatchBooleanGetter(module, "NShared.WeaponCostumeData", "get_IsShow", true);
        PatchBooleanGetter(module, "NShared.AccessoryCostumeData", "get_IsShow", true);

        // Preview ownership gates (force false so UI won't require ownership to preview/list)
        PatchBooleanGetter(module, "NShared.CostumeData", "get_PreviewableWhenOwned", false);
        PatchBooleanGetter(module, "NShared.HairCostumeData", "get_PreviewableWhenNeedCostumeOwned", false);
        PatchBooleanGetter(module, "NShared.HairCostumeData", "get_PreviewableWhenNeedHairOwned", false);
        PatchBooleanGetter(module, "NShared.WeaponCostumeData", "get_PreviewableWhenNeedCostumeOwned", false);
        PatchBooleanGetter(module, "NShared.WeaponCostumeData", "get_PreviewableWhenNeedWeaponOwned", false);
        PatchBooleanGetter(module, "NShared.AccessoryCostumeData", "get_PreviewableWhenOwned", false);
    }

    static void PatchHeroCostumeViewMotionButton(ModuleDefinition module)
    {
        var type = module.Types.FirstOrDefault(t => t.FullName == "NGame2.NUI.NWindow.HeroCostumeView");
        if (type == null)
        {
            Console.WriteLine("WARNING: HeroCostumeView type not found (View Motion button patch skipped)");
            return;
        }

        var method = type.Methods.FirstOrDefault(m => m.Name == "OnClickClosePhoto" && !m.HasParameters);
        if (method == null)
        {
            Console.WriteLine("WARNING: HeroCostumeView.OnClickClosePhoto not found (View Motion button patch skipped)");
            return;
        }

        var il = method.Body.GetILProcessor();
        var instructions = method.Body.Instructions;
        bool patched = false;

        for (int i = 0; i < instructions.Count - 1; i++)
        {
            if (instructions[i].OpCode == OpCodes.Ldfld &&
                instructions[i].Operand is FieldReference fieldRef &&
                fieldRef.Name == "WidgetViewMotion")
            {
                for (int j = i + 1; j < Math.Min(i + 6, instructions.Count); j++)
                {
                    if (instructions[j].OpCode == OpCodes.Ldc_I4_0 ||
                        (instructions[j].OpCode == OpCodes.Ldc_I4_S && (sbyte)instructions[j].Operand == 0) ||
                        (instructions[j].OpCode == OpCodes.Ldc_I4 && (int)instructions[j].Operand == 0))
                    {
                        il.Replace(instructions[j], il.Create(OpCodes.Ldc_I4_1));
                        patched = true;
                        break;
                    }
                }

                if (patched)
                {
                    break;
                }
            }
        }

        if (patched)
        {
            Console.WriteLine("Patched HeroCostumeView.OnClickClosePhoto to keep WidgetViewMotion visible");
        }
        else
        {
            Console.WriteLine("WARNING: Could not locate WidgetViewMotion disable flag in OnClickClosePhoto");
        }
    }
}
