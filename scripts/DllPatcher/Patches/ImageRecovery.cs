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
        var battlePortrait = module.GetType("NGame2.NUI.UIHelper").Methods.Single(m => m.Name == "GetBattleHeroPortraitPath"
            && m.Parameters.Count == 3 && m.Parameters[0].ParameterType.FullName == "System.String");
        if (!battlePortrait.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkImageRecovery" && m.Name == "BattlePortraitFallback")) {
            var il = battlePortrait.Body.GetILProcessor(); var first = battlePortrait.Body.Instructions[0];
            var fallback = Instruction.Create(OpCodes.Pop);
            foreach (var instruction in new[] {
                il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Ldarg_1), il.Create(OpCodes.Ldarg_2),
                il.Create(OpCodes.Call, Hook("BattlePortraitFallback")), il.Create(OpCodes.Dup),
                il.Create(OpCodes.Brfalse, fallback), il.Create(OpCodes.Ret), fallback })
                il.InsertBefore(first, instruction);
        }
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
        Prefix(loader.Methods.Single(m => m.Name == "SetAssetPath"), "BeforePortraitChange", OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2);
        Prefix(loader.Methods.Single(m => m.Name == "set_AssetPath"), "BeforePortraitPathChange", OpCodes.Ldarg_0, OpCodes.Ldarg_1);
        var portraitComplete = loader.Methods.Single(m => m.Name == "OnComplete");
        if (!portraitComplete.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkImageRecovery" && m.Name == "FitHeroPortrait")) {
            var assign = portraitComplete.Body.Instructions.Single(i => i.Operand is MethodReference m && m.DeclaringType.Name == "UIHelper" && m.Name == "SetTextureInfo");
            var info = portraitComplete.Body.Variables.Single(v => v.VariableType.FullName == "NGameAsset.AssetTexture/TextureInfo");
            var il = portraitComplete.Body.GetILProcessor();
            var self = Instruction.Create(OpCodes.Ldarg_0); var data = Instruction.Create(OpCodes.Ldloc, info);
            il.InsertAfter(assign, self); il.InsertAfter(self, data); il.InsertAfter(data, Instruction.Create(OpCodes.Call, Hook("FitHeroPortrait")));
        }
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
        var lateUpdate = loader.Methods.SingleOrDefault(m => m.Name == "LateUpdate");
        if (lateUpdate == null) {
            lateUpdate = new MethodDefinition("LateUpdate", MethodAttributes.Private | MethodAttributes.HideBySig, module.TypeSystem.Void);
            lateUpdate.Body.Instructions.Add(Instruction.Create(OpCodes.Ret)); loader.Methods.Add(lateUpdate);
        }
        Prefix(lateUpdate, "TickPortrait", OpCodes.Ldarg_0);
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
        foreach (var name in new[] { "BattlePortraitFallback", "BeforeCachedBundle", "BeforeCachedAsset", "BeforeCachedResource", "TryTpIcon", "Tick", "TickPortrait", "TickGlobal", "Reset", "WatchLoadingTip", "WatchLoadingHero", "BeforePortraitChange", "BeforePortraitPathChange", "FitHeroPortrait" })
            if (calls.Count(m => m.Name == name) != (name == "Reset" || name == "WatchLoadingHero" ? 2 : 1)) throw new InvalidOperationException("Missing or duplicate image recovery hook: " + name);
        var portrait = module.GetType("NGame2.NUI.UIHelper").Methods.Single(m => m.Name == "GetBattleHeroPortraitPath" && m.Parameters.Count == 3);
        var code = portrait.Body.Instructions;
        if (code[0].OpCode != OpCodes.Ldarg_0 || code[1].OpCode != OpCodes.Ldarg_1 || code[2].OpCode != OpCodes.Ldarg_2
            || code[3].Operand is not MethodReference hook || hook.Name != "BattlePortraitFallback"
            || code[4].OpCode != OpCodes.Dup || code[5].OpCode != OpCodes.Brfalse || code[5].Operand != code[7]
            || code[6].OpCode != OpCodes.Ret || code[7].OpCode != OpCodes.Pop
            || !code.Any(i => i.Operand is MethodReference m && m.Name == "get_BattlePortraitPath"))
            throw new InvalidOperationException("Light battle portrait fallback must preserve the native costume/basic portrait path.");
        VerifyArenaResultStack(portrait);
        Console.WriteLine("Verified image cache guards, bounded widget recovery, atlas retry scheduling and lifecycle cleanup.");
    }

    static void VerifyImageRecoveryFile(string clientRoot)
    {
        var managed = Path.Combine(Path.GetFullPath(clientRoot), "King's Raid_Data", "Managed");
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll"));
        VerifyImageRecovery(assembly.MainModule);
        if (!File.Exists(Path.Combine(managed, "SprkImageRecovery.dll"))) throw new InvalidOperationException("Missing image recovery helper.");
        TestLightBattlePortraits(File.ReadAllBytes(Path.Combine(managed, "SprkImageRecovery.dll")));
    }

    static void TestLightBattlePortraits(byte[] bytes)
    {
        using var input = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.CostumeTicketRules.json")!;
        using var doc = System.Text.Json.JsonDocument.Parse(input);
        var rules = doc.RootElement;
        var light = rules.GetProperty("LightCostumeIndices").EnumerateArray().Select(x => x.GetInt32()).ToHashSet();
        var fallback = rules.GetProperty("NewCostumes").EnumerateArray().Concat(rules.GetProperty("CostumeUpdates").EnumerateArray())
            .Where(row => light.Contains(row.GetProperty("CostumeIndex").GetInt32()) && row.TryGetProperty("BattlePortraitPath", out var path)
                && path.GetString()!.StartsWith("Icon/Creature/Monster/", StringComparison.OrdinalIgnoreCase)).ToArray();
        var method = System.Reflection.Assembly.Load(bytes).GetType("SprkLightBattlePortrait")!.GetMethod("Resolve")!;
        var ids = fallback.Select(row => row.GetProperty("CostumeIndex").GetInt32()).ToHashSet();
        if (ids.Count == 0 || ids.Count != fallback.Length) throw new InvalidOperationException("Invalid Light battle fallback selection.");
        foreach (var row in fallback) {
            var id = row.GetProperty("CostumeIndex").GetInt32();
            var hero = row.GetProperty("HeroIndex").GetInt32();
            var bundle = rules.GetProperty("AssetEvidence").GetProperty("Models").GetProperty(hero.ToString())[0].GetProperty("BundleFileName").GetString()!;
            var code = System.Text.RegularExpressions.Regex.Match(bundle, @"^Creature_Hero_(.+)_Star\d+\.unity3d$").Groups[1].Value;
            if (code.Length == 0) throw new InvalidOperationException("Missing basic hero code for Light costume " + id);
            foreach (var star in new[] { 0, 1, 3, 5 })
                if ((string?)method.Invoke(null, new object[] { "Hero." + code, star, id }) != "Battle/Portrait/Hero/" + code + "_5")
                    throw new InvalidOperationException("Light costume must use highest-star basic battle art: " + id);
            foreach (var codeValue in new string?[] { null, "" })
                if (method.Invoke(null, new object?[] { codeValue, 5, id }) != null) throw new InvalidOperationException("Empty hero code changed.");
        }
        foreach (var id in light.Concat(rules.GetProperty("DarkCostumeIndices").EnumerateArray().Select(x => x.GetInt32())).Append(0).Append(999999).Where(id => !ids.Contains(id)))
            if (method.Invoke(null, new object[] { "Hero.Kasel", 1, id }) != null)
                throw new InvalidOperationException("Dedicated Light/Dark or ordinary costume portrait changed: " + id);
        Console.WriteLine($"PASS: {ids.Count} Light monster-icon fallbacks use highest-star basic battle portraits at every star; dedicated Light/Dark, ordinary and missing-code paths retain native behavior.");
    }

    static void InstallImageRecovery(string clientRoot, bool stageOnly, string? sourcePath = null)
    {
        var managed = Path.Combine(Path.GetFullPath(clientRoot), "King's Raid_Data", "Managed");
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ImageRecovery.cs.txt")!;
        using var reader = new StreamReader(resource);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => Path.GetFileName(p) != "SprkImageRecovery.dll").Select(p => MetadataReference.CreateFromFile(p));
        using var rules = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.CostumeTicketRules.json")!;
        using var doc = System.Text.Json.JsonDocument.Parse(rules);
        var rows = doc.RootElement.GetProperty("NewCostumes").EnumerateArray()
            .Concat(doc.RootElement.GetProperty("CostumeUpdates").EnumerateArray()
                .Where(row => row.TryGetProperty("PortraitPath", out var path) &&
                    path.GetString()!.StartsWith("Portrait/Monster/Ordeal/Light", StringComparison.OrdinalIgnoreCase)))
            .Select(row => (Id:row.GetProperty("CostumeIndex").GetInt32(), Path:row.GetProperty("PortraitPath").GetString()!)).ToArray();
        if (rows.Length == 0 || rows.Select(r => r.Id).Distinct().Count() != rows.Length) throw new InvalidOperationException("Invalid Light costume fallback portrait list.");
        var light = doc.RootElement.GetProperty("LightCostumeIndices").EnumerateArray().Select(x => x.GetInt32()).ToHashSet();
        var battleFallbacks = doc.RootElement.GetProperty("NewCostumes").EnumerateArray().Concat(doc.RootElement.GetProperty("CostumeUpdates").EnumerateArray())
            .Where(row => light.Contains(row.GetProperty("CostumeIndex").GetInt32()) && row.TryGetProperty("BattlePortraitPath", out var path)
                && path.GetString()!.StartsWith("Icon/Creature/Monster/", StringComparison.OrdinalIgnoreCase))
            .Select(row => row.GetProperty("CostumeIndex").GetInt32()).ToArray();
        var source = reader.ReadToEnd().Replace("__SPRK_LIGHT_FALLBACK_PATHS__", string.Join(",", rows.Select(r => "{"+r.Id+","+System.Text.Json.JsonSerializer.Serialize(r.Path)+"}")))
            .Replace("__SPRK_LIGHT_BATTLE_FALLBACK_IDS__", string.Join(",", battleFallbacks));
        var compilation = CSharpCompilation.Create("SprkImageRecovery", new[] { CSharpSyntaxTree.ParseText(source) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        var images = new[] { 10, 15 }.Select(value => new ResourceDescription("SprkTpIcons." + value + ".png",
            () => typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.TpIcons." + value + ".png")!, true));
        using var compiled = new MemoryStream(); var result = compilation.Emit(compiled, manifestResources: images);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        TestLightBattlePortraits(compiled.ToArray());
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
            if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, clientRoot)))
                throw new InvalidOperationException("Image patch staged. Close game clients and shared-asset workers before installation.");
            var backup = Path.Combine("target", "image-recovery-backups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")); Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            var helperPath = Path.Combine(managed, "SprkImageRecovery.dll");
            if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkImageRecovery.dll"));
            File.Move(staged, dll, true); File.Move(helperPath + ".staged", helperPath, true);
        }
        Console.WriteLine(stageOnly ? "Staged image recovery patch." : "Installed image recovery patch. Restart the client.");
    }
}
