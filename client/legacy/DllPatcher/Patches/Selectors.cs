using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;

namespace DllPatcher;

partial class Program
{
    static void TestSelectorPools()
    {
        using var module = ModuleDefinition.CreateModule("SelectorTest", ModuleKind.Dll);
        foreach (var (name, field) in new[] { ("WeaponUniqueSelectItemData", "ItemIndices"), ("SelectItemData", "ItemIndices"), ("HeroSelectItemData", "HeroIndices") })
        {
            var type = new TypeDefinition("NShared", name, TypeAttributes.Public, module.TypeSystem.Object);
            module.Types.Add(type);
            type.Fields.Add(new FieldDefinition("<ItemCode>k__BackingField", FieldAttributes.Public, module.TypeSystem.String));
            type.Fields.Add(new FieldDefinition("<" + field + ">k__BackingField", FieldAttributes.Public, new ArrayType(module.TypeSystem.Int32)));
            type.Methods.Add(new MethodDefinition("get_" + field, MethodAttributes.Public, new ArrayType(module.TypeSystem.Int32)));
        }
        PatchSelectorPools(module);
        using var bytes = new MemoryStream(); module.Write(bytes);
        var asm = System.Reflection.Assembly.Load(bytes.ToArray());
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RestoredSelectors.json")!;
        using var doc = JsonDocument.Parse(stream);
        foreach (var row in doc.RootElement.EnumerateArray())
        {
            var name = row.GetProperty("Family").GetString() switch { "EquipmentSelectors" => "WeaponUniqueSelectItemData", "Selectors" => "SelectItemData", _ => "HeroSelectItemData" };
            var type = asm.GetType("NShared." + name)!;
            var instance = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
            type.GetField("<ItemCode>k__BackingField")!.SetValue(instance, row.GetProperty("Code").GetString());
            var method = type.GetMethod("get_" + row.GetProperty("Field").GetString())!;
            var expected = row.GetProperty("Choices").EnumerateArray().Select(v => v.GetInt32());
            if (!((int[])method.Invoke(instance, null)!).SequenceEqual(expected)) throw new Exception("Selector IL mismatch");
            type.GetField("<ItemCode>k__BackingField")!.SetValue(instance, "RESTRICTED_SELECTOR");
            type.GetField("<" + row.GetProperty("Field").GetString() + ">k__BackingField")!.SetValue(instance, new[] { 7, 9 });
            if (!((int[])method.Invoke(instance, null)!).SequenceEqual(new[] { 7, 9 })) throw new Exception("Restricted selector changed");
        }
        Console.WriteLine("Selector IL tests passed for every override and restricted fallback");
    }

    static void PatchSelectorPools(ModuleDefinition module)
    {
        if(!string.IsNullOrEmpty(module.FileName) && NativeStaticData.ValidateInstalled(Path.GetDirectoryName(module.FileName)!)) {
            Console.WriteLine("Selectors use baked native pools."); return;
        }
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RestoredSelectors.json")!;
        using var doc = JsonDocument.Parse(stream);
        foreach (var (family, name, field) in new[] {
            ("EquipmentSelectors", "WeaponUniqueSelectItemData", "ItemIndices"),
            ("Selectors", "SelectItemData", "ItemIndices"),
            ("HeroSelectors", "HeroSelectItemData", "HeroIndices") })
        {
            var type = module.Types.Single(t => t.FullName == "NShared." + name);
            var getter = type.Methods.Single(m => m.Name == "get_" + field);
            var backing = type.Fields.Single(f => f.Name == "<" + field + ">k__BackingField");
            var code = type.Fields.Single(f => f.Name == "<ItemCode>k__BackingField");
            getter.Body = new MethodBody(getter);
            var il = getter.Body.GetILProcessor();
            // Reuse the client's string equality reference (avoid importing a
            // System.Private.CoreLib reference from the patcher's .NET runtime).
            var equal = new MethodReference("op_Equality", module.TypeSystem.Boolean, module.TypeSystem.String);
            equal.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
            equal.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
            int count = 0;
            foreach (var row in doc.RootElement.EnumerateArray().Where(r => r.GetProperty("Family").GetString() == family))
            {
                var next = il.Create(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, code);
                il.Emit(OpCodes.Ldstr, row.GetProperty("Code").GetString()!);
                il.Emit(OpCodes.Call, equal); il.Emit(OpCodes.Brfalse, next);
                var choices = row.GetProperty("Choices").EnumerateArray().Select(v => v.GetInt32()).ToArray();
                il.Emit(OpCodes.Ldc_I4, choices.Length); il.Emit(OpCodes.Newarr, module.TypeSystem.Int32);
                for (int i = 0; i < choices.Length; i++)
                {
                    il.Emit(OpCodes.Dup); il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldc_I4, choices[i]); il.Emit(OpCodes.Stelem_I4);
                }
                il.Emit(OpCodes.Ret); il.Append(next); count++;
            }
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, backing); il.Emit(OpCodes.Ret);
            Console.WriteLine($"Restored {count} {name} pools; restricted pools retain their native data");
        }
    }
}
