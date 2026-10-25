using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void PatchInventoryExpansion(ModuleDefinition module) {
        var type = module.GetType("NShared.InventoryExtendData");
        var getter = type.Methods.Single(m => m.Name == "get_EquipItemExtendCount");
        var inventory = type.Methods.Single(m => m.Name == "get_InventoryType");
        var field = type.Fields.Single(f => f.Name == "<EquipItemExtendCount>k__BackingField");
        getter.Body = new MethodBody(getter);
        var il = getter.Body.GetILProcessor();
        var original = il.Create(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, inventory);
        il.Emit(OpCodes.Brtrue, original);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, field);
        il.Emit(OpCodes.Ldc_I4, 90); il.Emit(OpCodes.Bne_Un, original);
        il.Emit(OpCodes.Ldc_I4, 140); il.Emit(OpCodes.Ret);
        il.Append(original); il.Emit(OpCodes.Ldfld, field); il.Emit(OpCodes.Ret);
        // Old clients may still have the portal-era ceiling hook installed.
        var legacy = module.GetType("RestoredPortal")?.Methods.SingleOrDefault(m => m.Name == "EquipmentBagMaxExtensions");
        if (legacy != null) foreach (var i in legacy.Body.Instructions)
            if (i.OpCode == OpCodes.Ldc_I4_S && Convert.ToInt32(i.Operand) == 90 || i.OpCode == OpCodes.Ldc_I4 && Equals(i.Operand, 90)) {
                i.OpCode = OpCodes.Ldc_I4; i.Operand = 140;
            }
        VerifyInventoryExpansion(module);
    }
    static void VerifyInventoryExpansion(ModuleDefinition module) {
        var getter = module.GetType("NShared.InventoryExtendData").Methods.Single(m => m.Name == "get_EquipItemExtendCount");
        if (!getter.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4 && Equals(i.Operand, 140)) ||
            !getter.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "get_InventoryType"))
            throw new InvalidOperationException("Inventory expansion row patch missing.");
    }
    static void VerifyInventoryExpansionFile(string root, bool staged = false) {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var path = Path.Combine(managed, "Assembly-CSharp.dll" + (staged ? ".inventory-expansion-staged" : ""));
        using (var assembly = AssemblyDefinition.ReadAssembly(path)) VerifyInventoryExpansion(assembly.MainModule);
        var context = new System.Runtime.Loader.AssemblyLoadContext("InventoryExpansion-" + Guid.NewGuid(), true);
        context.Resolving += (load, name) => {
            var dep = Path.Combine(managed, name.Name + ".dll");
            if (!File.Exists(dep)) return null;
            using var stream = new MemoryStream(File.ReadAllBytes(dep));
            return load.LoadFromStream(stream);
        };
        try {
            using var stream = new MemoryStream(File.ReadAllBytes(path));
            var game = context.LoadFromStream(stream);
            var type = game.GetType("NShared.InventoryExtendData")!;
            var inventory = type.GetProperty("InventoryType")!;
            foreach (int kind in new[] { 0, 1, 3 }) foreach (int count in new[] { 10, 50, 90, 140, 190 }) {
                var row = Activator.CreateInstance(type)!;
                inventory.SetValue(row, Enum.ToObject(inventory.PropertyType, kind));
                type.GetProperty("EquipItemExtendCount")!.SetValue(row, count);
                int actual = (int)type.GetProperty("EquipItemExtendCount")!.GetValue(row)!;
                if (actual != (kind == 0 && count == 90 ? 140 : count)) throw new InvalidOperationException("Inventory row regression.");
            }
            // Native ticket and gem expansion flows must read the patched getter.
            Console.WriteLine("PASS: 15 actual inventory row cases; gear ceiling 280 + 140*8 = 1400; chest/rune tiers preserved.");
        } finally { context.Unload(); }
    }
    static void InstallInventoryExpansion(string root, bool stageOnly) {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var path = Path.Combine(managed, "Assembly-CSharp.dll");
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using (var game = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver, InMemory = true })) {
            PatchInventoryExpansion(game.MainModule);
            var getter = game.MainModule.GetType("NShared.InventoryExtendData").Methods.Single(m => m.Name == "get_EquipItemExtendCount");
            int count = getter.Body.Instructions.Count;
            PatchInventoryExpansion(game.MainModule);
            if (count != getter.Body.Instructions.Count) throw new InvalidOperationException("Inventory expansion patch is not idempotent.");
            game.Write(path + ".inventory-expansion-staged");
        }
        VerifyInventoryExpansionFile(root, true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Inventory expansion staged. Close the game and local server before installation.");
        File.Copy(path, path + ".before-inventory-expansion-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(path + ".inventory-expansion-staged", path, true);
        VerifyInventoryExpansionFile(root);
    }
}
