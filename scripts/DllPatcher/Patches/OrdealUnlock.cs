using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchOrdealUnlock(ModuleDefinition module)
    {
        var gate = module.GetType("NGame2.NOrdeal.OrdealHelper").Methods.Single(m => m.Name == "CheckOpenCondition");
        var helper = module.GetType("SprkAdventureUnlocks").Methods.Single(m => m.Name == "PortalTutorial");
        foreach (var instruction in gate.Body.Instructions.Where(i => i.Operand is MethodReference m
            && m.DeclaringType.FullName == "NShared.UserTutorialManager" && m.Name == "IsComplete"))
        {
            // Same stack arguments: the tutorial manager followed by the configured index.
            instruction.OpCode = OpCodes.Call;
            instruction.Operand = helper;
        }
        VerifyOrdealUnlock(module);
    }

    static void VerifyOrdealUnlock(ModuleDefinition module)
    {
        var gate = module.GetType("NGame2.NOrdeal.OrdealHelper").Methods.Single(m => m.Name == "CheckOpenCondition");
        var calls = gate.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().ToArray();
        if (calls.Count(m => m.DeclaringType.Name == "SprkAdventureUnlocks" && m.Name == "PortalTutorial") != 1
            || calls.Any(m => m.DeclaringType.FullName == "NShared.UserTutorialManager" && m.Name == "IsComplete")
            || !calls.Any(m => m.Name == "get_isValidInstance")
            || !gate.Body.Instructions.Any(i => Equals(i.Operand, "OrdealArenaOpenCondition")))
            throw new InvalidOperationException("Ordeals must retain account guards and configured tutorial requirement with one skip-aware hook.");
        var helper = module.GetType("SprkAdventureUnlocks");
        bool Calls(string method, string name) => helper.Methods.Single(m => m.Name == method).Body.Instructions
            .Any(i => i.Operand is MethodReference m && m.Name == name);
        if (!helper.Methods.Single(m => m.Name == "PortalTutorial").Body.Instructions
                .Any(i => i.Operand is FieldReference f && f.Name == "IsTutorialSkip")
            || !Calls("PortalTutorial", "TutorialSatisfied")
            || !Calls("TutorialSatisfied", "IsComplete"))
            throw new InvalidOperationException("Missing native completion / tutorial skip policy.");
        VerifyArenaResultStack(gate);
    }

    static void VerifyOrdealUnlockFile(string root, bool staged)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(root, "King's Raid_Data", "Managed",
            "Assembly-CSharp.dll" + (staged ? ".ordeal-unlock-staged" : "")));
        VerifyOrdealUnlock(assembly.MainModule);
        Console.WriteLine("Verified Ordeals skip-aware tutorial gate, native prerequisites and IL stack.");
    }

    static void InstallOrdealUnlock(string root, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".ordeal-unlock-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchOrdealUnlock(assembly.MainModule);
            assembly.Write(staged);
        }
        VerifyOrdealUnlockFile(root, true);
        if (!stageOnly)
        {
            var backup = Path.Combine("target", "ordeal-unlock-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
            VerifyOrdealUnlockFile(root, false);
        }
        Console.WriteLine(stageOnly ? "Staged Ordeals tutorial unlock." : "Installed Ordeals tutorial unlock. Restart the client.");
    }
}
