using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static bool PatchLegacyHeroStats(ModuleDefinition module)
    {
        // Build-1558 SoulWeaponOption coefficients are 7/8/9 and 250.
        // Native 4.86.6 GetSoulWeaponBaseStat (ARM64 0x30d6b38) divides
        // ratio * coefficient * factor by 1000, after calculating factor.
        // Later clients use coefficients 1000 times larger and divide again.
        var method = module.GetType("NShared.StatCalculator").Methods.Single(m => m.Name == "GetSoulWeaponBaseStat");
        var original = new[] {
            Code.Ldc_I4, Code.Ldarg_2, Code.Add, Code.Conv_I8,
            Code.Ldc_I4, Code.Ldarg_3, Code.Add, Code.Conv_I8, Code.Mul,
            Code.Ldc_I4, Code.Conv_I8, Code.Div, Code.Stloc_0,
            Code.Ldarg_0, Code.Ldarg_1, Code.Mul, Code.Conv_I8, Code.Ldloc_0, Code.Mul,
            Code.Ldc_I4, Code.Conv_I8, Code.Div,
            Code.Ldc_I4, Code.Conv_I8, Code.Div, Code.Conv_I4, Code.Ret
        };
        var restored = original.Take(22).Concat(original.Skip(25)).ToArray();
        var instructions = method.Body.Instructions;
        if (!method.IsStatic || method.ReturnType.FullName != "System.Int32" ||
            method.Parameters.Count != 4 || method.Parameters.Any(p => p.ParameterType.FullName != "System.Int32") ||
            method.Body.Variables.Count != 1 || method.Body.Variables[0].VariableType.FullName != "System.Int64" ||
            instructions.Where(i => i.OpCode == OpCodes.Ldc_I4).Any(i => (int)i.Operand != 1000))
            throw new InvalidOperationException("Unrecognized Soul Weapon stat calculator; refusing to patch.");
        var codes = instructions.Select(i => i.OpCode.Code).ToArray();
        if (codes.SequenceEqual(restored)) return false;
        if (!codes.SequenceEqual(original))
            throw new InvalidOperationException("Unrecognized Soul Weapon stat arithmetic; refusing to patch.");
        for (int i = 0; i < 3; i++) instructions.RemoveAt(22);
        Console.WriteLine("Restored legacy Soul Weapon ATK/HP units (removed extra /1000).");
        return true;
    }

    static void InstallHeroStats(string clientRoot, bool stageOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        // Do not pair the legacy arithmetic with an unknown/unbaked table set.
        if (!NativeCombatTables.ValidateInstalled(Path.GetDirectoryName(dll)!))
            throw new InvalidOperationException("Hero stat restoration requires verified baked native combat tables.");
        var staged = dll + ".hero-stats-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchLegacyHeroStats(assembly.MainModule);
            if (PatchLegacyHeroStats(assembly.MainModule)) throw new Exception("Hero stat patch is not idempotent.");
            assembly.Write(staged);
        }
        if (!stageOnly)
        {
            File.Copy(dll, dll + ".before-hero-stats-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? $"Staged: {staged}" : "Installed legacy hero stat scaling.");
    }

    static void TestHeroStats(string dll)
    {
        dll = Path.GetFullPath(dll);
        AssemblyLoadContext.Default.Resolving += (context, name) => {
            var dependency = Path.Combine(Path.GetDirectoryName(dll)!, name.Name + ".dll");
            return File.Exists(dependency) ? context.LoadFromAssemblyPath(dependency) : null;
        };
        using var definition = AssemblyDefinition.ReadAssembly(dll);
        if (PatchLegacyHeroStats(definition.MainModule)) throw new Exception("Hero stat scaling is not restored.");
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
        var calculator = assembly.GetType("NShared.StatCalculator", true)!;
        var basic = calculator.GetMethod("GetSoulWeaponBaseStat")!;
        var final = calculator.GetMethod("GetSoulWeaponFinalStat", BindingFlags.Static | BindingFlags.NonPublic)!;
        // Fixed outputs independently executed from the archived ARM64 routine.
        foreach (var (ratio, coefficient, reinforce, grade, expected) in new[] {
            (500, 8, 0, 0, 4000), (500, 250, 0, 0, 125000),
            (699, 8, 6130, 3000, 159483), (301, 250, 6130, 3000, 2146130),
            (700, 9, 6130, 3000, 179676), (300, 7, 0, 0, 2100)
        })
            if ((int)basic.Invoke(null, new object[] { ratio, coefficient, reinforce, grade })! != expected)
                throw new Exception($"Archived Soul Weapon stat mismatch: {ratio}/{coefficient}/{reinforce}/{grade}.");
        int cases = 0;
        foreach (int reinforce in new[] { 0,30,60,90,120,160,240,320,420,520,620,820,1040,1280,1550,1860,2430,3120,3950,4940,6130 })
        foreach (int grade in new[] { 0,1000,3000 })
        foreach (int coefficient in new[] { 7,8,9,250 })
        foreach (int ratio in new[] { 300,500,699,700 })
        foreach (int bonus in new[] { 0,1,171,200 })
        {
            long factor = (1000L + reinforce) * (1000L + grade) / 1000;
            int baseStat = (int)(ratio * coefficient * factor / 1000);
            int expected = baseStat + (int)((long)baseStat * bonus / 1000);
            int actual = (int)final.Invoke(null, new object[] { ratio, coefficient, bonus, reinforce, grade })!;
            if (actual != expected) throw new Exception("Soul Weapon final stat/bonus mismatch.");
            cases++;
        }
        Console.WriteLine($"PASS: 6 archived native outputs and {cases} installed Soul Weapon ATK/HP cases; patch is idempotent.");
    }
}
