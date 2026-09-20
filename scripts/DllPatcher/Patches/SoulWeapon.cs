using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchSoulWeaponLimitBreak(ModuleDefinition module)
    {
        // ================================================================
        // PATCH SoulWeaponLimitBreak.TryGetMaxStar & GetAfterDataByCurrent
        // Disables Limit Break (stars 6-15) completely
        // ================================================================
        Console.WriteLine("\n=== Patching SoulWeaponLimitBreak (Disable Stars 6-15) ===");
        var swlbType = module.Types.FirstOrDefault(t => t.FullName == "NShared.NContentsDefine.SoulWeaponLimitBreak");
        if (swlbType != null)
        {
            // 1. Patch TryGetMaxStar
            var tryGetMaxStar = swlbType.Methods.FirstOrDefault(m => m.Name == "TryGetMaxStar");
            if (tryGetMaxStar != null)
            {
                tryGetMaxStar.Body.Instructions.Clear();
                tryGetMaxStar.Body.ExceptionHandlers.Clear();
                tryGetMaxStar.Body.Variables.Clear();

                var ilProc = tryGetMaxStar.Body.GetILProcessor();
                ilProc.Append(ilProc.Create(OpCodes.Ldarg_2));
                ilProc.Append(ilProc.Create(OpCodes.Ldc_I4_0));
                ilProc.Append(ilProc.Create(OpCodes.Stind_I4));
                ilProc.Append(ilProc.Create(OpCodes.Ldc_I4_0));
                ilProc.Append(ilProc.Create(OpCodes.Ret));
                Console.WriteLine("Successfully patched SoulWeaponLimitBreak.TryGetMaxStar");
            }

            // 2. Patch GetAfterDataByCurrent
            var getAfterData = swlbType.Methods.FirstOrDefault(m => m.Name == "GetAfterDataByCurrent");
            if (getAfterData != null)
            {
                getAfterData.Body.Instructions.Clear();
                getAfterData.Body.ExceptionHandlers.Clear();
                getAfterData.Body.Variables.Clear();

                var ilProc = getAfterData.Body.GetILProcessor();
                ilProc.Append(ilProc.Create(OpCodes.Ldnull));
                ilProc.Append(ilProc.Create(OpCodes.Ret));
                Console.WriteLine("Successfully patched SoulWeaponLimitBreak.GetAfterDataByCurrent");
            }

            // 3. Patch GetDataByResultStar
            var getDataByResult = swlbType.Methods.FirstOrDefault(m => m.Name == "GetDataByResultStar");
            if (getDataByResult != null)
            {
                getDataByResult.Body.Instructions.Clear();
                getDataByResult.Body.ExceptionHandlers.Clear();
                getDataByResult.Body.Variables.Clear();

                var ilProc = getDataByResult.Body.GetILProcessor();
                ilProc.Append(ilProc.Create(OpCodes.Ldnull));
                ilProc.Append(ilProc.Create(OpCodes.Ret));
                Console.WriteLine("Successfully patched SoulWeaponLimitBreak.GetDataByResultStar");
            }
        }
        else
        {
            Console.WriteLine("ERROR: SoulWeaponLimitBreak type not found!");
        }
    }
}
