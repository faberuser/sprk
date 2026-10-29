using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static string SoloRetrySource(string name)
    {
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher." + name + ".cs.txt")!;
        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }

    static MethodDefinition SoloRetryGate(ModuleDefinition game) => game.GetType(
        "ExtensionMethod_Game2_BattleField_Manager_Play_IDataProvider").Methods.Single(m => m.Name == "CanRetryBattle");
    static MethodDefinition SoloRetryPause(ModuleDefinition game) => game.GetType(
        "NGame2.NUI.NManager.NBattle.BattleManagement").Methods.Single(m => m.Name.EndsWith(".PausedMenu.Listener.OnRetry"));

    static void PatchSoloRetry(ModuleDefinition game, ModuleDefinition helper)
    {
        var hooks = helper.GetType("SprkSoloRetry");
        MethodReference Hook(string name) => game.ImportReference(hooks.Methods.Single(m => m.Name == name));
        var gate = SoloRetryGate(game);
        if (!gate.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkSoloRetry"))
        {
            var il = gate.Body.GetILProcessor();
            var original = gate.Body.Instructions[0];
            foreach (var instruction in new[] {
                il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Ldarg_1), il.Create(OpCodes.Call, Hook("Allow")),
                il.Create(OpCodes.Brfalse, original), il.Create(OpCodes.Ldc_I4_1), il.Create(OpCodes.Ret) })
                il.InsertBefore(original, instruction);
        }
        var pause = SoloRetryPause(game);
        var restart = pause.Body.Instructions.Single(i => i.Operand is MethodReference m &&
            (m.Name == "RepeatCampaign" || m.DeclaringType.Name == "SprkSoloRetry" && m.Name == "Retry"));
        restart.OpCode = OpCodes.Call;
        restart.Operand = Hook("Retry");

        var content = game.GetType("NGame2.NBattleContext.BattleContext").Methods.Single(m => m.Name == "RepeatContent");
        if (!content.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkSoloRetry"))
        {
            var request = content.Body.Variables.Single(v => v.VariableType.FullName == "NShared.BeginContent.Request");
            var instance = content.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>()
                .First(m => m.Name == "get_instance" && m.DeclaringType.FullName.Contains("PunishmentRaidContext"));
            var last = content.Body.Instructions.Single(i => i.Operand is MethodReference m &&
                m.Name == "set_Level" && m.DeclaringType.FullName == "NShared.BeginContent.Request");
            var il = content.Body.GetILProcessor();
            foreach (var instruction in new[] {
                il.Create(OpCodes.Ldloc, request), il.Create(OpCodes.Call, instance), il.Create(OpCodes.Call, Hook("PreserveShards")) })
            { il.InsertAfter(last, instruction); last = instruction; }
        }
        VerifySoloRetry(game);
    }

    static void VerifySoloRetry(ModuleDefinition game)
    {
        var gate = SoloRetryGate(game);
        var code = gate.Body.Instructions;
        bool IsHook(Instruction i, string name) => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkSoloRetry" && m.Name == name;
        if (code.Count < 7 || code[0].OpCode != OpCodes.Ldarg_0 || code[1].OpCode != OpCodes.Ldarg_1
            || !IsHook(code[2], "Allow") || code[3].OpCode != OpCodes.Brfalse || code[3].Operand != code[6]
            || code[4].OpCode != OpCodes.Ldc_I4_1 || code[5].OpCode != OpCodes.Ret
            || code.Count(i => IsHook(i, "Allow")) != 1)
            throw new InvalidOperationException("Solo Retry gate must fall back to the original policy exactly once.");
        foreach (var name in new[] { "GetBattleMenuType", "GetBattleDefine", "get_RetryCondition", "Contains" })
            if (!code.Any(i => i.Operand is MethodReference m && m.Name == name))
                throw new InvalidOperationException("Original retry policy missing " + name);
        var pause = SoloRetryPause(game);
        if (pause.Body.Instructions.Count(i => IsHook(i, "Retry")) != 1
            || pause.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "RepeatCampaign")
            || !pause.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "RaiseEvent"))
            throw new InvalidOperationException("Pause Retry routing or native event missing.");
        var content = game.GetType("NGame2.NBattleContext.BattleContext").Methods.Single(m => m.Name == "RepeatContent");
        var preserve = content.Body.Instructions.Single(i => IsHook(i, "PreserveShards"));
        if (preserve.Previous.Previous.Previous.Operand is not MethodReference level || level.Name != "set_Level"
            || !content.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "RequestBeginContent"))
            throw new InvalidOperationException("Boss shard selection must be preserved on the native content restart path.");
        foreach (var pair in new[] {
            ("NGame2.NUI.NManager.NBattle.BattleManagement", "OpenPausedMenu"),
            ("NGame2.NUI.NWindow.NBattle.EndBattleLose", "UpdateRepeatable") })
            if (!game.GetType(pair.Item1).Methods.Single(m => m.Name == pair.Item2).Body.Instructions
                .Any(i => i.Operand is MethodReference m && m.Name == "CanRetryBattle"))
                throw new InvalidOperationException("Retry button no longer uses the verified policy: " + pair.Item2);
        VerifyArenaResultStack(gate);
        VerifyArenaResultStack(pause);
        VerifyArenaResultStack(content);
        Console.WriteLine("Verified solo pause/defeat Retry hooks, original victory policy, content restart routing and Boss shard preservation.");
    }

    static void TestSoloRetry()
    {
        var trees = new[] { "SoloRetry", "SoloRetryTests" }.Select(name => CSharpSyntaxTree.ParseText(SoloRetrySource(name)));
        var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SoloRetryTests", trees, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var bytes = new MemoryStream();
        var result = compilation.Emit(bytes);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        System.Reflection.Assembly.Load(bytes.ToArray()).GetType("SoloRetryTests")!.GetMethod("Run")!.Invoke(null, null);
        Console.WriteLine("Solo Retry policy, online/Guild Conquest/replay exclusions, restart contracts and shard selection tests passed.");
    }

    static void InstallSoloRetry(string root, bool stageOnly, bool verifyOnly = false)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, "SprkSoloRetry.dll");
        var staged = dll + ".solo-retry-staged";
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(verifyOnly && stageOnly ? staged : dll,
            new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        TestSoloRetry();
        if (verifyOnly) { VerifySoloRetry(game.MainModule); return; }
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => p != helperPath).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkSoloRetry", new[] { CSharpSyntaxTree.ParseText(SoloRetrySource("SoloRetry")) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var compiled = new MemoryStream(); var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        compiled.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(compiled);
        PatchSoloRetry(game.MainModule, helper.MainModule);
        var counts = game.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count);
        PatchSoloRetry(game.MainModule, helper.MainModule);
        if (counts != game.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count))
            throw new InvalidOperationException("Solo Retry patch is not idempotent.");
        game.Write(staged); File.WriteAllBytes(helperPath + ".staged", compiled.ToArray());
        using (var check = AssemblyDefinition.ReadAssembly(staged)) VerifySoloRetry(check.MainModule);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll, dll + ".before-solo-retry-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
        File.Move(helperPath + ".staged", helperPath, true);
        File.Move(staged, dll, true);
        Console.WriteLine("Installed solo Retry. Restart the client.");
    }
}
