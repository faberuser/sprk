using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

// EXP Potion window: a quick tap on a hero slot used one potion. It now opens
// the native Max confirmation with level steppers (SprkHeroExpLevel), so the
// player chooses the target level; holding the slot still pours continuously.
partial class Program
{
    static MethodDefinition HeroExpRelease(ModuleDefinition module) => module
        .GetType("NGame2.NUI.NComponent2.HeroGetExpComponent").Methods.Single(m => m.Name == "OnReleaseComponent");

    static void PatchHeroExpLevel(ModuleDefinition module, ModuleDefinition helper)
    {
        var release = HeroExpRelease(module);
        if (release.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkHeroExpLevel")) return;
        var type = release.DeclaringType;
        // Native: if (_UsedPotion == 0) TryUseExpPotionAni(); FinalizeReleaseComponent();
        var tap = release.Body.Instructions.Where(i => i.Operand is MethodReference m && m.Name == "TryUseExpPotionAni").ToArray();
        if (tap.Length != 1 || tap[0].Previous.OpCode != OpCodes.Ldarg_0 || tap[0].Next.OpCode != OpCodes.Ldarg_0
            || tap[0].Next.Next.Operand is not MethodReference { Name: "FinalizeReleaseComponent" })
            throw new InvalidOperationException("Unexpected hero EXP slot release handler.");
        var finalize = type.Methods.Single(m => m.Name == "FinalizeReleaseComponent");
        var max = type.Methods.Single(m => m.Name == "OnClickMax");
        var perPotion = type.Fields.Single(f => f.Name == "_AddHeroExp");
        var attach = module.ImportReference(helper.GetType("SprkHeroExpLevel").Methods.Single(m => m.Name == "Attach"));
        var il = release.Body.GetILProcessor();
        var call = tap[0];
        // Reset the press (no potion was used), open the native confirmation
        // with the maximum, then let the helper add level steppers to it.
        il.InsertBefore(call, il.Create(OpCodes.Call, finalize));
        il.InsertBefore(call, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(call, il.Create(OpCodes.Call, max));
        il.InsertBefore(call, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(call, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(call, il.Create(OpCodes.Ldfld, perPotion));
        il.InsertBefore(call, il.Create(OpCodes.Call, attach));
        call.OpCode = OpCodes.Ret; call.Operand = null;
        // The inserted instructions push the held-release branch target further.
        foreach (var i in release.Body.Instructions)
            if (i.OpCode == OpCodes.Brtrue_S) i.OpCode = OpCodes.Brtrue;
            else if (i.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                throw new InvalidOperationException("Unexpected short branch in hero EXP slot release handler.");
        VerifyHeroExpLevel(module);
    }

    static void VerifyHeroExpLevel(ModuleDefinition module)
    {
        var code = HeroExpRelease(module).Body.Instructions;
        string Name(Instruction i) => i.Operand is MethodReference m ? m.Name : "";
        if (code.Any(i => Name(i) == "TryUseExpPotionAni"))
            throw new InvalidOperationException("A quick tap must no longer spend a single EXP potion.");
        var attach = code.Single(i => Name(i) == "Attach" && ((MethodReference)i.Operand).DeclaringType.Name == "SprkHeroExpLevel");
        // Tap branch: Finalize (reset), OnClickMax, Attach(this, _AddHeroExp), ret.
        var tap = new[] { attach.Previous.Previous.Previous.Previous, attach.Previous.Previous, attach.Previous, attach.Next };
        if (Name(attach.Previous.Previous.Previous.Previous.Previous.Previous) != "FinalizeReleaseComponent" || Name(tap[0]) != "OnClickMax"
            || tap[2].Operand is not FieldReference { Name: "_AddHeroExp" } || tap[3].OpCode != OpCodes.Ret)
            throw new InvalidOperationException("Hero EXP slot tap must open the native confirmation with level steppers.");
        // The held-release path still sends the poured potions.
        if (code.Count(i => Name(i) == "FinalizeReleaseComponent") != 2 || Name(code[^2]) != "FinalizeReleaseComponent"
            || !code.Any(i => i.OpCode == OpCodes.Brtrue && i.Operand == code[^3]))
            throw new InvalidOperationException("Holding a hero slot must still use EXP potions continuously.");
        if (code.Any(i => i.OpCode.OperandType == OperandType.ShortInlineBrTarget && i.OpCode != OpCodes.Brtrue_S))
            throw new InvalidOperationException("Unexpected short branch after hero EXP level patch.");
        VerifyArenaResultStack(HeroExpRelease(module));
    }

    static void TestHeroExpLevel(byte[] helperBytes, string managed)
    {
        ResolveEventHandler resolve = (_, e) => {
            var path = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        AppDomain.CurrentDomain.AssemblyResolve += resolve;
        try
        {
            var planType = Assembly.Load(helperBytes).GetType("SprkHeroExpLevelPlan")!;
            // Cumulative EXP: level L needs 1000 * (L - 1)^2; max level 100.
            Func<int, long> total = l => 1000L * (l - 1) * (l - 1);
            object Plan(long current, long per, int potions, int from) =>
                Activator.CreateInstance(planType, current, total(100), per, potions, from, 100, total)!;
            int Field(object plan, string name) => (int)planType.GetField(name)!.GetValue(plan)!;
            int Call(object plan, string name, params object[] args) => Convert.ToInt32(planType.GetMethod(name)!.Invoke(plan, args));

            // Level 1, 5000 EXP potions, 200 owned: 1,000,000 EXP reaches level 32.
            var plan = Plan(0, 5000, 200, 1);
            if (Field(plan, "MinLevel") != 3 || Field(plan, "TopLevel") != 32)
                throw new InvalidOperationException("Hero EXP level range must start at the first reachable level and stop at what owned potions reach.");
            // Fewest potions per level, and the level they reach.
            if (Call(plan, "Potions", 32) != 193 || Call(plan, "LevelAfter", 193) != 32 || Call(plan, "LevelAfter", 192) != 31)
                throw new InvalidOperationException("Hero EXP level must use the fewest potions that reach it.");
            // Steps move to the next reachable level and stay within range.
            if (Call(plan, "Step", 32, 1) != 32 || Call(plan, "Step", 32, -1) != 31 || Call(plan, "Step", 31, -10) != 21
                || Call(plan, "Step", 5, -10) != 3 || Call(plan, "Step", 3, 10) != 13)
                throw new InvalidOperationException("Incorrect hero EXP level stepping.");

            // A potion that crosses several levels: only reachable levels are offered.
            var big = Plan(0, 250000, 3, 1);
            int top = Field(big, "TopLevel"), min = Field(big, "MinLevel");
            if (min != 16 || top != 28 || Call(big, "Step", min, 1) != 23 || Call(big, "Step", top, -1) != 23 || Call(big, "Step", 23, -1) != 16)
                throw new InvalidOperationException("Hero EXP level stepping must skip unreachable levels.");

            // Enough potions for the hero's maximum: the range ends there.
            var capped = Plan(total(95), 125000, 2829, 95);
            if (Field(capped, "TopLevel") != 100 || Call(capped, "Potions", 100) != 8)
                throw new InvalidOperationException("Hero EXP level must stop at the hero's maximum level.");

            // Not enough for one level: no choice, the native confirmation stays as-is.
            var short_ = Plan(total(60), 1000, 2, 60);
            if ((bool)planType.GetProperty("CanChoose")!.GetValue(short_)! || !(bool)planType.GetProperty("CanChoose")!.GetValue(plan)!)
                throw new InvalidOperationException("Hero EXP level choice must require at least one reachable level.");
            Console.WriteLine("PASS: hero EXP level range, fewest-potion counts, reachable-level steps and max-level cap.");
        }
        finally { AppDomain.CurrentDomain.AssemblyResolve -= resolve; }
    }

    static void InstallHeroExpLevel(string root, bool stageOnly, bool verify = false)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data/Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, "SprkHeroExpLevel.dll");
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        if (verify)
        {
            VerifyHeroExpLevel(game.MainModule);
            using var installedHelper = AssemblyDefinition.ReadAssembly(helperPath);
            if (installedHelper.MainModule.GetType("SprkHeroExpLevel")?.Methods.Any(m => m.Name == "Attach") != true)
                throw new InvalidOperationException("Hero EXP level helper is missing.");
            TestHeroExpLevel(File.ReadAllBytes(helperPath), managed);
            Console.WriteLine("Verified hero EXP slot tap opens the level picker; holding still pours potions.");
            return;
        }
        using var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.HeroExpLevel.cs.txt")!);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => p != helperPath).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkHeroExpLevel", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var compiled = new MemoryStream(); var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        TestHeroExpLevel(compiled.ToArray(), managed);
        compiled.Position = 0; using var helper = AssemblyDefinition.ReadAssembly(compiled);
        PatchHeroExpLevel(game.MainModule, helper.MainModule);
        var count = HeroExpRelease(game.MainModule).Body.Instructions.Count;
        PatchHeroExpLevel(game.MainModule, helper.MainModule);
        if (count != HeroExpRelease(game.MainModule).Body.Instructions.Count) throw new InvalidOperationException("Hero EXP level patch is not idempotent.");
        var staged = dll + ".hero-exp-level-staged"; game.Write(staged);
        File.WriteAllBytes(helperPath + ".staged", compiled.ToArray());
        using (var check = AssemblyDefinition.ReadAssembly(staged, new ReaderParameters { AssemblyResolver = resolver })) VerifyHeroExpLevel(check.MainModule);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root))) throw new InvalidOperationException("Close game clients before installing the hero EXP level picker.");
        var backup = Path.Combine("target/hero-exp-level-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")); Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkHeroExpLevel.dll"));
        File.Move(staged, dll, true); File.Move(helperPath + ".staged", helperPath, true);
        Console.WriteLine("Installed hero EXP level picker. Restart the game.");
    }
}
