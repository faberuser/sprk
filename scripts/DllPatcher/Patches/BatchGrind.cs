using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    const string BatchGrindGuard = "SprkSkipBreakSelectionWarning";
    static MethodDefinition GrindSelection(ModuleDefinition module) => module.GetType("NGame2.NUI.NWindow.NInventory.ItemListListener")
        .Methods.Single(m => m.Name.EndsWith("IEquipListListener.OnClick"));

    static void PatchBatchGrind(ModuleDefinition module, ModuleDefinition helper)
    {
        var click = GrindSelection(module);
        var type = click.DeclaringType;
        var guard = type.Methods.FirstOrDefault(m => m.Name == BatchGrindGuard);
        if (guard == null)
        {
            guard = new MethodDefinition(BatchGrindGuard, MethodAttributes.Private | MethodAttributes.HideBySig, module.TypeSystem.Boolean);
            type.Methods.Add(guard);
            var il = guard.Body.GetILProcessor();
            var yes = Instruction.Create(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, type.Fields.Single(f => f.Name == "functionType"));
            il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(type.NestedTypes.Single(t => t.Name == "EFunctionType").Fields.Single(f => f.Name == "Break").Constant));
            il.Emit(OpCodes.Beq, yes);
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, type.Methods.Single(m => m.Name == "get_isFiltering"));
            il.Emit(OpCodes.Ret); il.Append(yes); il.Emit(OpCodes.Ret);
        }
        foreach (var call in click.Body.Instructions.Where(i => i.Operand is MethodReference m && m.Name == "get_isFiltering")) call.Operand = guard;
        var submit = module.GetType("NGame2.NUI.NManager.InventoryManagement").Methods.Single(m => m.Name == "BreakEquipItems");
        if (!submit.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkBatchGrind"))
        {
            var popup = submit.Body.Instructions.Single(i => i.Operand is MethodReference m && m.DeclaringType.Name == "MessagePopup" && m.Name == "Open");
            // Native code leaves the title on the stack, then stores the localized
            // question in local 1. Decorate that question before the same popup.
            var store = submit.Body.Instructions.Last(i => i.Offset < popup.Offset && i.OpCode == OpCodes.Stloc_1);
            var il = submit.Body.GetILProcessor();
            il.InsertBefore(store, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(store, Instruction.Create(OpCodes.Call, module.ImportReference(helper.GetType("SprkBatchGrind").Methods.Single(m => m.Name == "Summary"))));
        }
        VerifyBatchGrind(module);
    }

    static void VerifyBatchGrind(ModuleDefinition module)
    {
        var click = GrindSelection(module);
        var code = click.Body.Instructions;
        var gate = code.Single(i => i.Operand is MethodReference m && m.Name == BatchGrindGuard);
        if (gate.Next.OpCode.FlowControl != FlowControl.Cond_Branch || gate.Next.Operand is not Instruction target
            || !code.Skip(code.IndexOf(target)).Take(8).Any(i => i.Operand is MethodReference m && m.Name == "Add")
            || !code.Take(code.IndexOf(gate)).Any(i => i.Operand is MethodReference m && m.Name == "get_IsEquipped")
            || !code.Take(code.IndexOf(gate)).Any(i => i.Operand is MethodReference m && m.Name == "get_SelectableFunction"))
            throw new InvalidOperationException("Batch selection bypass must follow native eligibility guards and enter normal selection.");
        var submit = module.GetType("NGame2.NUI.NManager.InventoryManagement").Methods.Single(m => m.Name == "BreakEquipItems");
        if (submit.Body.Instructions.Count(i => i.Operand is MethodReference m && m.Name == "Summary" && m.DeclaringType.Name == "SprkBatchGrind") != 1
            || submit.Body.Instructions.Count(i => i.Operand is MethodReference m && m.Name == "Open" && m.DeclaringType.Name == "MessagePopup") != 1)
            throw new InvalidOperationException("Missing single final grind confirmation and summary.");
        VerifyArenaResultStack(click);
        VerifyArenaResultStack(click.DeclaringType.Methods.Single(m => m.Name == BatchGrindGuard));
        VerifyArenaResultStack(submit);
        Console.WriteLine("PASS: mass-grind selection keeps eligibility guards, skips per-item warnings and retains one final confirmation.");
    }

    static void InstallBatchGrind(string root, bool stageOnly, bool verify = false)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data/Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, "SprkBatchGrind.dll");
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        if (verify)
        {
            VerifyBatchGrind(game.MainModule);
            using var installedHelper = AssemblyDefinition.ReadAssembly(helperPath);
            var summary = installedHelper.MainModule.GetType("SprkBatchGrind").Methods.Single(m => m.Name == "Summary");
            if (!summary.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "GetData" && m.DeclaringType.Name == "EquipItemBreakDataContainer"))
                throw new InvalidOperationException("Batch grind reward preview is missing native table lookup.");
            TestBatchGrind(game, File.ReadAllBytes(helperPath), managed);
            return;
        }
        using var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.BatchGrind.cs.txt")!);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => p != helperPath).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkBatchGrind", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var compiled = new MemoryStream(); var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        compiled.Position = 0; using var helper = AssemblyDefinition.ReadAssembly(compiled);
        PatchBatchGrind(game.MainModule, helper.MainModule);
        var count = GrindSelection(game.MainModule).Body.Instructions.Count;
        PatchBatchGrind(game.MainModule, helper.MainModule);
        if (count != GrindSelection(game.MainModule).Body.Instructions.Count) throw new InvalidOperationException("Batch grind patch is not idempotent.");
        var staged = dll + ".batch-grind-staged"; game.Write(staged);
        File.WriteAllBytes(helperPath + ".staged", compiled.ToArray());
        using (var check = AssemblyDefinition.ReadAssembly(staged, new ReaderParameters { AssemblyResolver = resolver })) VerifyBatchGrind(check.MainModule);
        TestBatchGrind(game, compiled.ToArray(), managed);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root))) throw new InvalidOperationException("Close game clients before installing batch grinding.");
        var backup = Path.Combine("target/batch-grind-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")); Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkBatchGrind.dll"));
        File.Move(staged, dll, true); File.Move(helperPath + ".staged", helperPath, true);
        Console.WriteLine("Installed batch grind confirmation. Restart the game.");
    }
}
