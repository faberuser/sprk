using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static MethodDefinition PunishmentLevelSelectionMethod(ModuleDefinition module) =>
        module.GetType("NGame2.NUI.NWindow.NPunish.PunishmentLevelSelectPopup").Methods.Single(m => m.Name == "Init");

    static Instruction PunishmentLevelAssignment(MethodDefinition method, string field) =>
        method.Body.Instructions.Single(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == field);

    static void PatchPunishmentLevelSelection(ModuleDefinition module)
    {
        var method = PunishmentLevelSelectionMethod(module);
        var operation = PunishmentLevelAssignment(method, "_currentLevel").Previous;
        if (operation.OpCode == OpCodes.Add) {
            // Keep the unlocked maximum independent from the initial selection.
            // The native ClearLevel belongs to the selected raid group.
            var max = new MethodReference("Max", module.TypeSystem.Int32,
                new TypeReference("System", "Math", module, module.TypeSystem.CoreLibrary));
            max.Parameters.Add(new ParameterDefinition(module.TypeSystem.Int32));
            max.Parameters.Add(new ParameterDefinition(module.TypeSystem.Int32));
            operation.OpCode = OpCodes.Call;
            operation.Operand = max;
        }
        VerifyPunishmentLevelSelection(module);
    }

    static void VerifyPunishmentLevelSelection(ModuleDefinition module)
    {
        var method = PunishmentLevelSelectionMethod(module);
        var selected = PunishmentLevelAssignment(method, "_currentLevel");
        var maximum = PunishmentLevelAssignment(method, "_maxLevel");
        foreach (var store in new[] { selected, maximum }) {
            if (store.Previous.Previous.OpCode != OpCodes.Ldc_I4_1
                || store.Previous.Previous.Previous.OpCode != OpCodes.Ldfld
                || store.Previous.Previous.Previous.Operand is not FieldReference { Name: "_lastCleardLevel" })
                throw new InvalidOperationException("Unexpected punishment level initialization.");
        }
        if (selected.Previous.OpCode != OpCodes.Call
            || selected.Previous.Operand is not MethodReference { Name: "Max", DeclaringType.FullName: "System.Math" } max
            || max.ReturnType.FullName != "System.Int32" || max.Parameters.Count != 2
            || max.Parameters.Any(p => p.ParameterType.FullName != "System.Int32")
            || maximum.Previous.OpCode != OpCodes.Add)
            throw new InvalidOperationException("Punishment selection must default to max(1, cleared level) and preserve the unlocked maximum.");
        VerifyArenaResultStack(method);
    }

    static void VerifyPunishmentLevelSelectionFile(string root, bool staged)
    {
        using var game = AssemblyDefinition.ReadAssembly(Path.Combine(root, "King's Raid_Data/Managed/Assembly-CSharp.dll")
            + (staged ? ".punishment-level-staged" : ""));
        VerifyPunishmentLevelSelection(game.MainModule);
        Console.WriteLine("Verified Apocalypsion/Solenis cleared-level default and unchanged unlocked maximum.");
    }

    static void InstallPunishmentLevelSelection(string root, bool stageOnly)
    {
        root = Path.GetFullPath(root);
        var dll = Path.Combine(root, "King's Raid_Data/Managed/Assembly-CSharp.dll");
        var staged = dll + ".punishment-level-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            PatchPunishmentLevelSelection(game.MainModule);
            game.Write(staged);
        }
        VerifyPunishmentLevelSelectionFile(root, true);
        if (stageOnly) return;
        foreach (var process in System.Diagnostics.Process.GetProcesses())
            using (process)
                if (UsesClientFiles(process, root))
                    throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installing.");
        var backup = Path.Combine("target/punishment-level-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        File.Move(staged, dll, true);
        VerifyPunishmentLevelSelectionFile(root, false);
    }

    static void TestPunishmentLevelSelection(string root)
    {
        using var game = AssemblyDefinition.ReadAssembly(Path.Combine(root, "King's Raid_Data/Managed/Assembly-CSharp.dll"));
        PatchPunishmentLevelSelection(game.MainModule);
        var method = PunishmentLevelSelectionMethod(game.MainModule);
        var before = string.Join("\n", method.Body.Instructions);
        PatchPunishmentLevelSelection(game.MainModule);
        if (before != string.Join("\n", method.Body.Instructions)) throw new Exception("Punishment level patch is not idempotent.");
        // Execute the shipping selection expression without loading Unity.
        var test = new System.Reflection.Emit.DynamicMethod("PunishmentInitialLevel", typeof(int), new[] { typeof(int) });
        var il = test.GetILGenerator();
        var store = PunishmentLevelAssignment(method, "_currentLevel");
        for (var i = store.Previous.Previous.Previous; i != store; i = i.Next) {
            if (i.OpCode == OpCodes.Ldfld) il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            else if (i.OpCode == OpCodes.Ldc_I4_1) il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_1);
            else if (i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: "Max" })
                il.Emit(System.Reflection.Emit.OpCodes.Call, typeof(Math).GetMethod("Max", new[] { typeof(int), typeof(int) })!);
            else throw new Exception("Unexpected selected-level instruction: " + i);
        }
        il.Emit(System.Reflection.Emit.OpCodes.Ret);
        var select = (Func<int, int>)test.CreateDelegate(typeof(Func<int, int>));
        foreach (var cleared in new[] { 0, 1, 2, 5, 50, 1000, int.MaxValue })
            if (select(cleared) != (cleared == 0 ? 1 : cleared))
                throw new Exception("Incorrect initial punishment level for clear " + cleared);
        Console.WriteLine("PASS: production IL selects level 1 before first clear and the cleared level thereafter; patch is idempotent.");
    }
}
