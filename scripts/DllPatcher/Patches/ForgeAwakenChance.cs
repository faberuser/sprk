using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    const string ForgeChanceHelper = "SprkForgeAwakenChance";
    static IEnumerable<MethodDefinition> ForgeChanceHooks(ModuleDefinition module) =>
        module.GetType("NGame2.NUI.NWindow.HeroEquipAwaken").Methods.Where(m => m.Name is "UpdateAwakeUITask" or "ClearIcon");

    static void VerifyForgeAwakenChanceFile(string root, bool staged = false) {
        var managed = Path.Combine(root, "King's Raid_Data", "Managed");
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll" + (staged ? ".forge-chance-staged" : "")), new ReaderParameters { AssemblyResolver = resolver });
        foreach(var method in ForgeChanceHooks(game.MainModule)) {
            var exits = method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray();
            if(exits.Length == 0 || exits.Any(i => i.Previous?.Operand is not MethodReference { Name: "Sync" } m || m.DeclaringType.Name != ForgeChanceHelper))
                throw new InvalidOperationException("Missing Forge chance hook: " + method.Name);
            VerifyArenaResultStack(method);
        }
        using var helper = AssemblyDefinition.ReadAssembly(Path.Combine(managed, ForgeChanceHelper + ".dll" + (staged ? ".staged" : "")));
        var type = helper.MainModule.GetType(ForgeChanceHelper);
        foreach(var member in new[] { "_ListAwakeInvenItems", "_ListPublicAwakenItemComponents", "ClearMaterialEquipIcon", "SetupMaterialEquipIcon", "CanUseAwakenStone", "GetTotalAwakenSuccessRate", "UpdateAwakeUITask", "UpdateAwakeEquipItemListTask", "UpdatePublicAwakeItemListTask" }) {
            var native = game.MainModule.GetType("NGame2.NUI.NWindow.HeroEquipAwaken");
            if(!native.Fields.Any(f => f.Name == member) && !native.Methods.Any(m => m.Name == member)) throw new InvalidOperationException("Missing native Forge member: " + member);
        }
        var calls = type.Methods.Where(m => m.HasBody).SelectMany(m => m.Body.Instructions).Select(i => i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[] { "CLI_CanUse_as_AwakenMaterial", "CLI_CanAwaken", "GetMaxAwakenPoint", "GetData", "Assign", "Plan" })
            if(!calls.Any(m => m.Name == name)) throw new InvalidOperationException("Incomplete Forge chance helper: " + name);
        Console.WriteLine("PASS: Forge chance exit hooks, native eligibility/table calculations, reflection members and IL stacks.");
    }

    static void InstallForgeAwakenChance(string root, bool stageOnly) {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, ForgeChanceHelper + ".dll");
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ForgeAwakenChance.cs.txt")!;
        using var reader = new StreamReader(stream);
        string source = reader.ReadToEnd();
        using var probeStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ForgeAwakenChanceProbe.cs.txt")!;
        using var probeReader = new StreamReader(probeStream);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => p != helperPath).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create(ForgeChanceHelper, new[] { CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(probeReader.ReadToEnd()) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var compiled = new MemoryStream();
        var result = compilation.Emit(compiled);
        if(!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        TestForgeChancePlanner(source);
        compiled.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(compiled);
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        var hook = game.MainModule.ImportReference(helper.MainModule.GetType(ForgeChanceHelper).Methods.Single(m => m.Name == "Sync"));
        foreach(var method in ForgeChanceHooks(game.MainModule)) {
            if(method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == ForgeChanceHelper)) continue;
            var il = method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray()) {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = il.Create(OpCodes.Call, hook);
                il.InsertAfter(ret, call); il.InsertAfter(call, il.Create(OpCodes.Ret));
            }
            foreach(var i in method.Body.Instructions.Where(i => i.OpCode.OperandType == OperandType.ShortInlineBrTarget))
                i.OpCode = typeof(OpCodes).GetFields().Select(f => f.GetValue(null)).OfType<OpCode>().Single(c => c.Name == i.OpCode.Name[..^2]);
        }
        game.Write(dll + ".forge-chance-staged"); File.WriteAllBytes(helperPath + ".staged", compiled.ToArray());
        VerifyForgeAwakenChanceFile(root, true);
        if(stageOnly) return;
        if(System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root))) throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        var backup = Path.Combine("target", "forge-chance-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup); File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        File.Move(dll + ".forge-chance-staged", dll, true); File.Move(helperPath + ".staged", helperPath, true);
        VerifyForgeAwakenChanceFile(root);
        Console.WriteLine("Installed Forge awakening success-rate slider. Restart the client.");
    }

    static void TestForgeChancePlanner(string source) {
        // Compile the actual pure planner without Unity, then compare it against
        // exhaustive subset enumeration, including nonlinear rates and fail bonus.
        var tree = CSharpSyntaxTree.ParseText(source);
        var type = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();
        var members = type.Members.Where(m => m is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax || m is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax { Identifier.ValueText: "Plan" });
        var pure = "using System; using System.Collections.Generic; public class Planner {" + string.Join("\n", members) + "}";
        var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("ForgePlannerTests", new[] { CSharpSyntaxTree.ParseText(pure) }, refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output = new MemoryStream();
        var emitted = compilation.Emit(output);
        if(!emitted.Success) throw new InvalidOperationException(string.Join("\n", emitted.Diagnostics));
        var context = new System.Runtime.Loader.AssemblyLoadContext("ForgePlannerTests", true);
        try {
            output.Position = 0;
            var plan = context.LoadFromStream(output).GetType("Planner")!.GetMethod("Plan")!;
            int[][] Run(int[] points, int[] rates, int bonus) => (int[][])plan.Invoke(null, new object[] { points, rates, bonus })!;
            var linear = Enumerable.Range(0, 101).ToArray();
            var twoCopies = Run(new[]{50,50}, linear, 0);
            if(twoCopies.Length != 2 || twoCopies[0].Length != 1 || twoCopies[1].Length != 2)
                throw new InvalidOperationException("Two 50% materials must produce exactly 50% and 100% stops.");
            if(Run(new[]{100,100}, linear, 0).Length != 1 || Run(Array.Empty<int>(), linear, 10).Length != 0)
                throw new InvalidOperationException("Single-rate / empty inventory stops are incorrect.");
            var random = new Random(873);
            for(int trial = 0; trial < 350; trial++) {
                int[] points = Enumerable.Range(0, random.Next(0, 11)).Select(_ => random.Next(1, 90)).ToArray();
                var rates = Enumerable.Range(0, 101).Select(p => p * p / 100).ToArray();
                int bonus = random.Next(0, 90);
                var plans = Run(points, rates, bonus);
                var expected = new SortedDictionary<int, (int total, int count)>();
                for(int mask = 1; mask < (1 << points.Length); mask++) {
                    int total = 0, count = 0;
                    for(int i = 0; i < points.Length; i++) if((mask & (1 << i)) != 0) { total += points[i]; count++; }
                    int rate = Math.Min(100, rates[Math.Min(100, total)] + bonus);
                    if(rate <= 0) continue;
                    var score = (total, count);
                    if(!expected.TryGetValue(rate, out var old) || score.CompareTo(old) < 0) expected[rate] = score;
                }
                if(plans.Length != expected.Count) throw new InvalidOperationException("Forge planner omitted or duplicated an attainable chance.");
                int step = 0;
                foreach(var (rate, score) in expected) {
                    var chosen = plans[step++];
                    if(chosen.Length == 0 || chosen.Distinct().Count() != chosen.Length || chosen.Any(i => i < 0 || i >= points.Length)) throw new InvalidOperationException("Planner reused or invented a material.");
                    int total = chosen.Sum(i => points[i]);
                    if(Math.Min(100, rates[Math.Min(100, total)] + bonus) != rate || (total, chosen.Length) != score)
                        throw new InvalidOperationException("Forge planner stop has the wrong rate or a suboptimal subset.");
                }
            }
            Console.WriteLine("PASS: dynamic Forge stops versus every subset of 350 inventories; exact 50/100 example, single/empty inventories, nonlinear rates, fail bonuses, overflow and no reuse.");
        } finally { context.Unload(); }
    }
}
