using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string MissionClaimAllView = "NGame2.NUI.NWindow.Mission.MissionNodeWindowTopView2";

    static void PatchMissionClaimAll(ModuleDefinition module, ModuleDefinition helper)
    {
        var view = module.GetType(MissionClaimAllView);
        var bridge = helper.GetType("SprkMissionClaimAll");
        MethodReference Hook(string name) => module.ImportReference(bridge.Methods.Single(m => m.Name == name));
        var group = view.Methods.Single(m => m.Name == "InitButtonGroup");
        if (!group.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkMissionClaimAll" && m.Name == "ButtonGroup"))
        {
            var first = group.Body.Instructions[0]; var il = group.Body.GetILProcessor();
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, il.Create(OpCodes.Call, Hook("ButtonGroup")));
            il.InsertBefore(first, il.Create(OpCodes.Starg, group.Parameters[0]));
        }
        var open = view.Methods.Single(m => m.Name == "Open");
        if (!open.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkMissionClaimAll" && m.Name == "Attach"))
        {
            var first = open.Body.Instructions[0]; var il = open.Body.GetILProcessor();
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, il.Create(OpCodes.Call, Hook("Attach")));
        }
        VerifyMissionClaimAll(module);
    }

    static void VerifyMissionClaimAll(ModuleDefinition module)
    {
        var view = module.GetType(MissionClaimAllView);
        foreach (var (name, hook) in new[] { ("InitButtonGroup", "ButtonGroup"), ("Open", "Attach") })
        {
            var method = view.Methods.Single(m => m.Name == name);
            if (method.Body.Instructions.Count(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkMissionClaimAll" && m.Name == hook) != 1)
                throw new InvalidOperationException("Missing or duplicated mission Claim All hook: " + name);
            VerifyArenaResultStack(method);
        }
        foreach (var name in new[] { "MissionNodeWindow", "MissionNodeWindowV3", "MissionNode2Window" })
        {
            var request = module.GetType("NGame2.NUI.NWindow.Mission." + name).Methods.Single(m => m.Name == "RequestAllReward");
            if (!request.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "RequestAllGetReward"))
                throw new InvalidOperationException("Native mission bulk reward request was removed: " + name);
        }
    }

    static void VerifyMissionClaimAllFile(string clientRoot, bool staged)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll" + (staged ? ".mission-claim-all-staged" : "")));
        VerifyMissionClaimAll(assembly.MainModule);
        Console.WriteLine("Verified mission Claim All headers, preserved native requests and IL control flow.");
    }

    static void InstallMissionClaimAll(string clientRoot, bool stageOnly, string? sourcePath = null)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, "SprkMissionClaimAll.dll");
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.MissionClaimAll.cs.txt")!;
        using var reader = new StreamReader(resource);
        var references = Directory.GetFiles(managed, "*.dll").Where(p => Path.GetFileName(p) != "SprkMissionClaimAll.dll")
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkMissionClaimAll", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var bytes = new MemoryStream(); var emitted = compilation.Emit(bytes);
        if (!emitted.Success) throw new InvalidOperationException(string.Join("\n", emitted.Diagnostics));
        bytes.Position = 0; using var helper = AssemblyDefinition.ReadAssembly(bytes);
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        var staged = dll + ".mission-claim-all-staged";
        using (var assembly = AssemblyDefinition.ReadAssembly(sourcePath ?? dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchMissionClaimAll(assembly.MainModule, helper.MainModule);
            assembly.Write(staged);
        }
        using (var verified = AssemblyDefinition.ReadAssembly(staged, new ReaderParameters { AssemblyResolver = resolver }))
        {
            VerifyMissionClaimAll(verified.MainModule); PatchMissionClaimAll(verified.MainModule, helper.MainModule);
            using var repeated = new MemoryStream(); verified.Write(repeated);
            if (!repeated.ToArray().SequenceEqual(File.ReadAllBytes(staged))) throw new InvalidOperationException("Mission Claim All patch must be idempotent.");
        }
        File.WriteAllBytes(helperPath + ".mission-claim-all-staged", bytes.ToArray());
        if (!stageOnly)
        {
            var backup = Path.Combine("target", "mission-claim-all-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup); File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkMissionClaimAll.dll"));
            File.Move(helperPath + ".mission-claim-all-staged", helperPath, true);
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified mission Claim All controls." : "Installed and verified mission Claim All controls. Restart the client.");
    }
}
