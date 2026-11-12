using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchBooleanGetter(ModuleDefinition module, string typeName, string getterName, bool value)
    {
        var type = module.Types.FirstOrDefault(t => t.FullName == typeName);
        if (type == null)
        {
            Console.WriteLine($"WARNING: {typeName} not found ({getterName} patch skipped)");
            return;
        }

        var getter = type.Methods.FirstOrDefault(m => m.Name == getterName && !m.HasParameters);
        if (getter == null)
        {
            Console.WriteLine($"WARNING: {typeName}.{getterName} not found");
            return;
        }

        getter.Body.Instructions.Clear();
        getter.Body.ExceptionHandlers.Clear();
        getter.Body.Variables.Clear();
        getter.Body.InitLocals = false;

        var il = getter.Body.GetILProcessor();
        il.Append(il.Create(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
        il.Append(il.Create(OpCodes.Ret));

        Console.WriteLine($"Patched {typeName}.{getterName} => {value}");
    }

}
