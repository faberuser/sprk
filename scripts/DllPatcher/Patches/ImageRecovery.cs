using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static void PatchImageRecovery(ModuleDefinition module, ModuleDefinition helper)
    {
        var worker = helper.GetType("SprkImageRecovery");
        MethodReference Hook(string name) => module.ImportReference(worker.Methods.Single(m => m.Name == name));
        void Prefix(MethodDefinition method, string hook, params OpCode[] loads) {
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkImageRecovery" && m.Name == hook)) return;
            var il = method.Body.GetILProcessor(); var first = method.Body.Instructions[0];
            foreach (var code in loads) il.InsertBefore(first, Instruction.Create(code));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, Hook(hook)));
        }
        var cache = module.GetType("NGameAsset.AssetCacheService");
        Prefix(cache.Methods.Single(m => m.Name == "GetCachedAssetBundle"), "BeforeCachedBundle", OpCodes.Ldarg_0, OpCodes.Ldarg_1);
        Prefix(cache.Methods.Single(m => m.Name == "GetCachedAsset"), "BeforeCachedAsset", OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2);
        Prefix(cache.Methods.Single(m => m.Name == "GetCachedResource"), "BeforeCachedResource", OpCodes.Ldarg_0, OpCodes.Ldarg_1);
        var loader = module.GetType("NGame2.NUI.UIBundleLoader");
        var update = loader.Methods.SingleOrDefault(m => m.Name == "Update");
        if (update == null) {
            update = new MethodDefinition("Update", MethodAttributes.Private | MethodAttributes.HideBySig, module.TypeSystem.Void);
            update.Body.Instructions.Add(Instruction.Create(OpCodes.Ret)); loader.Methods.Add(update);
        }
        Prefix(update, "Tick", OpCodes.Ldarg_0);
        foreach (var name in new[] { "OnDisable", "OnDestroy" }) Prefix(loader.Methods.Single(m => m.Name == name), "Reset", OpCodes.Ldarg_0);
        Prefix(module.GetTypes().Single(t => t.Name == "GlobalUpdateBehaviour").Methods.Single(m => m.Name == "Update"), "TickGlobal");
        VerifyImageRecovery(module);
    }

    static void VerifyImageRecovery(ModuleDefinition module)
    {
        var calls = module.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).SelectMany(m => m.Body.Instructions)
            .Select(i => i.Operand).OfType<MethodReference>().Where(m => m.DeclaringType.Name == "SprkImageRecovery").ToArray();
        foreach (var name in new[] { "BeforeCachedBundle", "BeforeCachedAsset", "BeforeCachedResource", "Tick", "TickGlobal", "Reset" })
            if (calls.Count(m => m.Name == name) != (name == "Reset" ? 2 : 1)) throw new InvalidOperationException("Missing or duplicate image recovery hook: " + name);
        Console.WriteLine("Verified image cache guards, bounded widget recovery, atlas retry scheduling and lifecycle cleanup.");
    }

    static void VerifyImageRecoveryFile(string clientRoot)
    {
        var managed = Path.Combine(Path.GetFullPath(clientRoot), "King's Raid_Data", "Managed");
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll"));
        VerifyImageRecovery(assembly.MainModule);
        if (!File.Exists(Path.Combine(managed, "SprkImageRecovery.dll"))) throw new InvalidOperationException("Missing image recovery helper.");
    }

    static void InstallImageRecovery(string clientRoot, bool stageOnly, string? sourcePath = null)
    {
        var managed = Path.Combine(Path.GetFullPath(clientRoot), "King's Raid_Data", "Managed");
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ImageRecovery.cs.txt")!;
        using var reader = new StreamReader(resource);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => Path.GetFileName(p) != "SprkImageRecovery.dll").Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkImageRecovery", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var compiled = new MemoryStream(); var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        compiled.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(compiled);
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(sourcePath ?? dll, new ReaderParameters { InMemory = true, AssemblyResolver = resolver });
        PatchImageRecovery(assembly.MainModule, helper.MainModule);
        var staged = dll + ".image-recovery-staged";
        assembly.Write(staged);
        File.WriteAllBytes(Path.Combine(managed, "SprkImageRecovery.dll.staged"), compiled.ToArray());
        using (var verified = AssemblyDefinition.ReadAssembly(staged)) VerifyImageRecovery(verified.MainModule);
        if (!stageOnly) {
            var backup = Path.Combine("target", "image-recovery-backups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")); Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            var helperPath = Path.Combine(managed, "SprkImageRecovery.dll");
            if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkImageRecovery.dll"));
            File.Move(staged, dll, true); File.Move(helperPath + ".staged", helperPath, true);
        }
        Console.WriteLine(stageOnly ? "Staged image recovery patch." : "Installed image recovery patch. Restart the client.");
    }
}
