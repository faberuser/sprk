using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static void PrepareLoadingBackgroundProbe(string managed)
    {
        // Test runner passes its copied Managed directory; never install this
        // opt-in probe hook into the player's automation assembly.
        var path = Path.Combine(Path.GetFullPath(managed), "SprkAutomation.dll");
        using var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { InMemory = true });
        using var helper = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "SprkImageRecovery.dll"));
        var method = assembly.MainModule.GetType("SprkAutomation").Methods.Single(m => m.Name == "Update");
        var hook = assembly.MainModule.ImportReference(helper.MainModule.GetType("SprkImageRecovery").Methods.Single(m => m.Name == "ProbeLoadingBackground"));
        method.Body.GetILProcessor().InsertBefore(method.Body.Instructions[0], Instruction.Create(OpCodes.Call, hook));
        assembly.Write(path);
    }

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
        var loadTexture = loader.Methods.Single(m => m.Name == "LoadAssetTexture");
        if (!loadTexture.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkImageRecovery" && m.Name == "TryTpIcon")) {
            var il = loadTexture.Body.GetILProcessor(); var first = loadTexture.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, Hook("TryTpIcon")));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var update = loader.Methods.SingleOrDefault(m => m.Name == "Update");
        if (update == null) {
            update = new MethodDefinition("Update", MethodAttributes.Private | MethodAttributes.HideBySig, module.TypeSystem.Void);
            update.Body.Instructions.Add(Instruction.Create(OpCodes.Ret)); loader.Methods.Add(update);
        }
        Prefix(update, "Tick", OpCodes.Ldarg_0);
        foreach (var name in new[] { "OnDisable", "OnDestroy" }) Prefix(loader.Methods.Single(m => m.Name == name), "Reset", OpCodes.Ldarg_0);
        Prefix(module.GetTypes().Single(t => t.Name == "GlobalUpdateBehaviour").Methods.Single(m => m.Name == "Update"), "TickGlobal");
        foreach (var entry in new[] {
            ("LoadingGamePlayTipComponent", "InitBackgroundImage", "WatchLoadingTip"),
            ("LoadingHeroIntroComponent", "InitHeroBackground", "WatchLoadingHero") }) {
            var method = module.GetTypes().Single(t => t.Name == entry.Item1).Methods.Single(m => m.Name == entry.Item2);
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkImageRecovery" && m.Name == entry.Item3)) continue;
            // Rewrite each return so existing branches also pass through the hook.
            var il = method.Body.GetILProcessor();
            foreach (var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray()) {
                ret.OpCode = OpCodes.Ldarg_0;
                var call = Instruction.Create(OpCodes.Call, Hook(entry.Item3));
                il.InsertAfter(ret, call); il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        VerifyImageRecovery(module);
    }

    static void VerifyImageRecovery(ModuleDefinition module)
    {
        var calls = module.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).SelectMany(m => m.Body.Instructions)
            .Select(i => i.Operand).OfType<MethodReference>().Where(m => m.DeclaringType.Name == "SprkImageRecovery").ToArray();
        foreach (var name in new[] { "BeforeCachedBundle", "BeforeCachedAsset", "BeforeCachedResource", "TryTpIcon", "Tick", "TickGlobal", "Reset", "WatchLoadingTip", "WatchLoadingHero" })
            if (calls.Count(m => m.Name == name) != (name == "Reset" || name == "WatchLoadingHero" ? 2 : 1)) throw new InvalidOperationException("Missing or duplicate image recovery hook: " + name);
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
        var images = new[] { 10, 15 }.Select(value => new ResourceDescription("SprkTpIcons." + value + ".png",
            () => typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.TpIcons." + value + ".png")!, true));
        using var compiled = new MemoryStream(); var result = compilation.Emit(compiled, manifestResources: images);
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
