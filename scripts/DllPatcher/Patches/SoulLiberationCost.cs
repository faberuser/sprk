using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static void VerifySoulLiberationCost(ModuleDefinition module)
    {
        var method = module.GetType("NGame2.NUI.NWindow.NSoulWeapon.SoulWeaponLiberationWindow")
            .Methods.Single(m => m.Name == "SetSelectSoulWeaponInfo");
        var calls = method.Body.Instructions.Where(i => i.Operand is MethodReference m &&
            m.DeclaringType.Name == "SprkSoulLiberationCost" && m.Name == "Show").ToArray();
        if (calls.Length != 1 || calls[0].Previous.OpCode != OpCodes.Ldarg_0 || calls[0].Next.OpCode != OpCodes.Ret)
            throw new InvalidOperationException("Missing or duplicate Soul Weapon liberation price display.");
    }

    static void VerifySoulLiberationCostFile(string clientRoot)
    {
        var managed = Path.Combine(clientRoot, "King's Raid_Data", "Managed");
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll"));
        VerifySoulLiberationCost(assembly.MainModule);
        using var helper = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "SprkSoulLiberationCost.dll"));
        var view = helper.MainModule.GetType("SprkSoulLiberationCost");
        var calls = view.Methods.Where(m => m.HasBody).SelectMany(m => m.Body.Instructions)
            .Select(i => i.Operand).OfType<MethodReference>().Select(m => m.Name).ToArray();
        foreach (var required in new[] { "GetSoulWeaponUpgradePriceData", "get_ReqGold", "get_Gold", "Open", "SetPrice", "Close" })
            if (!calls.Contains(required)) throw new InvalidOperationException("Incomplete gold display: " + required);
        Console.WriteLine("Verified liberation price hook, table-driven gold requirement, balance refresh and icon cleanup.");
    }

    static void InstallSoulLiberationCost(string clientRoot, bool stageOnly, string? sourcePath = null)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, "SprkSoulLiberationCost.dll");
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.SoulLiberationCost.cs.txt")!;
        using var reader = new StreamReader(resource);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => p != helperPath).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkSoulLiberationCost", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var compiled = new MemoryStream();
        var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        compiled.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(compiled);
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(sourcePath ?? dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        var method = assembly.MainModule.GetType("NGame2.NUI.NWindow.NSoulWeapon.SoulWeaponLiberationWindow")
            .Methods.Single(m => m.Name == "SetSelectSoulWeaponInfo");
        if (!method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkSoulLiberationCost")) {
            var ret = method.Body.Instructions.Single(i => i.OpCode == OpCodes.Ret);
            var il = method.Body.GetILProcessor();
            il.InsertBefore(ret, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(ret, il.Create(OpCodes.Call, assembly.MainModule.ImportReference(helper.MainModule.GetType("SprkSoulLiberationCost").Methods.Single(m => m.Name == "Show"))));
        }
        VerifySoulLiberationCost(assembly.MainModule);
        VerifySoulWeaponCinematic(assembly.MainModule);
        var staged = dll + ".soul-cost-staged";
        assembly.Write(staged);
        File.WriteAllBytes(helperPath + ".staged", compiled.ToArray());
        using (var verified = AssemblyDefinition.ReadAssembly(staged)) VerifySoulLiberationCost(verified.MainModule);
        if (stageOnly) { Console.WriteLine("Staged Soul Weapon liberation gold display."); return; }
        if (System.Diagnostics.Process.GetProcessesByName("King's Raid").Length != 0)
            throw new InvalidOperationException("Gold display staged. Close King's Raid before installing.");
        var backup = Path.Combine("target", "soul-cost-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkSoulLiberationCost.dll"));
        File.Move(staged, dll, true);
        File.Move(helperPath + ".staged", helperPath, true);
        Console.WriteLine("Installed and verified Soul Weapon liberation gold display.");
    }
}
