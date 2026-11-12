using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

// Inventory > Filter & Sort Gear > Detailed Options lists every OptionType in
// EquipOptionTable (identical to the Pre-Doomsday archive: 1,453 rows, 39
// types) except those rejected by Helper.IsExceptionOperationType. That
// function matched enum *names* by substring, which also hid real gear stats:
// Recovery (ModRecoverPower), Tough / P./M.Resistance (*Toughness), DMG
// Reduction upon Block / P./M.Block (*BlockPower) and Mana Recovery/DMG
// (ModMpOnDamageR). All have native labels (OPERATION_TYPE.*) and the matching
// code is generic, so only non-stat types stay hidden.
partial class Program
{
    const string GearFilterHelperType = "NGame2.NUI.NWindow.EquipmentItemFilterManager/Helper";
    // Non-stat OperationTypes: ModMaxMp, ModMpOnDamage (not on gear), UseSkill,
    // skill level of all/1st-4th skill, Soul Weapon usage limit.
    static readonly int[] GearFilterHiddenTypes = { 32, 121, 203, 238, 239, 240, 241, 242, 243 };
    static readonly int[] GearFilterRestoredTypes = { 49, 53, 54, 55, 108, 109, 110, 122 };

    static MethodDefinition GearFilterRule(ModuleDefinition module) => module.GetTypes()
        .Single(t => t.FullName == GearFilterHelperType).Methods.Single(m => m.Name == "IsExceptionOperationType");

    static void PatchGearFilterOptions(ModuleDefinition module)
    {
        var rule = GearFilterRule(module);
        if (!rule.Body.Instructions.Any(i => i.Operand is string s && s == "Mod")) { VerifyGearFilterOptions(module); return; }
        // Returns true when the type is listed. Hidden: the explicit non-stat set.
        rule.Body = new Mono.Cecil.Cil.MethodBody(rule);
        var il = rule.Body.GetILProcessor();
        var hidden = il.Create(OpCodes.Ldc_I4_0);
        foreach (var type in GearFilterHiddenTypes)
        {
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldc_I4, type); il.Emit(OpCodes.Beq, hidden);
        }
        il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Ret);
        il.Append(hidden); il.Emit(OpCodes.Ret);
        VerifyGearFilterOptions(module);
    }

    static void VerifyGearFilterOptions(ModuleDefinition module)
    {
        var code = GearFilterRule(module).Body.Instructions;
        var constants = code.Where(i => i.OpCode == OpCodes.Ldc_I4).Select(i => (int)i.Operand).ToArray();
        if (code.Any(i => i.Operand is string) || !constants.SequenceEqual(GearFilterHiddenTypes)
            || code.Count(i => i.OpCode == OpCodes.Beq) != GearFilterHiddenTypes.Length)
            throw new InvalidOperationException("Gear Detailed Options must hide only non-stat option types.");
        VerifyArenaResultStack(GearFilterRule(module));
    }

    // Executes the patched rule for every EquipOptionTable type.
    static void TestGearFilterOptions(byte[] assemblyBytes, string managed)
    {
        ResolveEventHandler resolve = (_, e) => {
            var path = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        AppDomain.CurrentDomain.AssemblyResolve += resolve;
        try
        {
            var game = Assembly.Load(assemblyBytes);
            var helper = game.GetType("NGame2.NUI.NWindow.EquipmentItemFilterManager+Helper")!;
            var rule = helper.GetMethod("IsExceptionOperationType")!;
            var enumType = rule.GetParameters()[0].ParameterType;
            bool Listed(int type) => (bool)rule.Invoke(null, new[] { Enum.ToObject(enumType, type) })!;
            // EquipOptionTable OptionTypes (identical in the Pre-Doomsday archive).
            int[] table = { 31, 39, 40, 41, 45, 49, 50, 51, 52, 53, 54, 55, 57, 61, 62, 64, 67, 70, 71, 72, 73, 74, 75, 76,
                105, 107, 108, 109, 110, 120, 122, 126, 203, 238, 239, 240, 241, 242, 243 };
            var listed = table.Where(Listed).ToArray();
            if (listed.Length != 32 || !GearFilterRestoredTypes.All(listed.Contains) || GearFilterHiddenTypes.Any(Listed))
                throw new InvalidOperationException($"Unexpected gear Detailed Options: {listed.Length} listed.");
            Console.WriteLine("PASS: gear Detailed Options list 32 stat types, including Recovery, Tough/Resistance, Block DMG Reduction and Mana Recovery/DMG.");
        }
        finally { AppDomain.CurrentDomain.AssemblyResolve -= resolve; }
    }

    static void InstallGearFilterOptions(string root, bool verifyOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            if (verifyOnly)
            {
                VerifyGearFilterOptions(assembly.MainModule);
                resolver.Dispose();
                TestGearFilterOptions(File.ReadAllBytes(dll), managed);
                return;
            }
            if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
                throw new InvalidOperationException("Close game clients before installing gear Detailed Options.");
            PatchGearFilterOptions(assembly.MainModule);
            var count = GearFilterRule(assembly.MainModule).Body.Instructions.Count;
            PatchGearFilterOptions(assembly.MainModule);
            if (count != GearFilterRule(assembly.MainModule).Body.Instructions.Count)
                throw new InvalidOperationException("Gear Detailed Options patch is not idempotent.");
            var backup = Path.Combine("target", "gear-filter-options-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            assembly.Write(dll + ".gear-filter-staged");
        }
        resolver.Dispose();
        TestGearFilterOptions(File.ReadAllBytes(dll + ".gear-filter-staged"), managed);
        File.Move(dll + ".gear-filter-staged", dll, true);
        InstallGearFilterOptions(root, true);
    }
}
