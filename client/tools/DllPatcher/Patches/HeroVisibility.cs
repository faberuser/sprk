using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchCreatureOpenTypeGetter(ModuleDefinition module)
    {
        var creatureDataType = module.Types.FirstOrDefault(t => t.FullName == "NShared.CreatureData");
        if (creatureDataType == null)
        {
            Console.WriteLine("WARNING: NShared.CreatureData type not found (OpenType patch skipped)");
            return;
        }

        var getter = creatureDataType.Methods.FirstOrDefault(m => m.Name == "get_OpenType" && !m.HasParameters);
        var backingField = creatureDataType.Fields.FirstOrDefault(f => f.Name == "<OpenType>k__BackingField");
        var indexField = creatureDataType.Fields.FirstOrDefault(f => f.Name == "<Index>k__BackingField");
        if (getter == null || backingField == null || indexField == null)
        {
            Console.WriteLine("WARNING: CreatureData OpenType/Index backing fields not found (OpenType patch skipped)");
            return;
        }

        getter.Body.Instructions.Clear();
        getter.Body.ExceptionHandlers.Clear();
        getter.Body.Variables.Clear();
        getter.Body.InitLocals = false;

        var il = getter.Body.GetILProcessor();
        var returnNone = il.Create(OpCodes.Ldc_I4_0); // CreatureOpenType.None
        var returnOpened = il.Create(OpCodes.Ldc_I4_3); // CreatureOpenType.Opened
        var returnExisting = il.Create(OpCodes.Ret);

        // value = this.<OpenType>k__BackingField;
        // if (value != CreatureOpenType.None) return value;
        // Only open known hero indices when source OpenType is None.
        // if (index < 1) return None;
        // if (index <= 102 || index == 111) return Opened;
        // return None;
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldfld, backingField));
        il.Append(il.Create(OpCodes.Dup));
        il.Append(il.Create(OpCodes.Ldc_I4_0)); // CreatureOpenType.None
        il.Append(il.Create(OpCodes.Bne_Un_S, returnExisting));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldfld, indexField));
        il.Append(il.Create(OpCodes.Ldc_I4_1));
        il.Append(il.Create(OpCodes.Blt_S, returnNone));
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldfld, indexField));
        il.Append(il.Create(OpCodes.Ldc_I4, 102));
        il.Append(il.Create(OpCodes.Ble_S, returnOpened));
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldfld, indexField));
        il.Append(il.Create(OpCodes.Ldc_I4, 111));
        il.Append(il.Create(OpCodes.Beq_S, returnOpened));
        il.Append(il.Create(OpCodes.Ldc_I4_0)); // CreatureOpenType.None
        il.Append(returnExisting);
        il.Append(returnOpened);
        il.Append(il.Create(OpCodes.Ret));
        il.Append(returnNone);
        il.Append(il.Create(OpCodes.Ret));

        Console.WriteLine("Patched NShared.CreatureData.get_OpenType (None -> Opened only for index 1..102 and 111)");
    }
}
