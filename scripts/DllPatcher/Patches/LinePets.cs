using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static MethodDefinition LinePetBook(ModuleDefinition module) => module.GetType("NGame2.NUI.NWindow.NPet.PetListBook")
        .Methods.Single(m => m.Name == "SetupPetPortraitComponent");

    static void PatchLinePetBook(ModuleDefinition module)
    {
        var method = LinePetBook(module);
        foreach (var call in method.Body.Instructions.Where(i => i.Operand is MethodReference m && m.Name == "get_IsLinePet").ToArray())
        {
            if (call.Next.OpCode != OpCodes.Brtrue && call.Next.OpCode != OpCodes.Brtrue_S)
                throw new InvalidOperationException("Unexpected LINE pet book exclusion; client left unchanged.");
            // Restore September 2022: list all pets matching the grade filter.
            // Preserve IsLinePet itself for interaction, feeding and adventure rules.
            call.OpCode = OpCodes.Pop; call.Operand = null;
            call.Next.OpCode = OpCodes.Nop; call.Next.Operand = null;
        }
        VerifyLinePetBook(module);
    }

    static void VerifyLinePetBook(ModuleDefinition module)
    {
        var method = LinePetBook(module);
        var calls = method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().ToArray();
        if (calls.Any(m => m.Name == "get_IsLinePet") || !calls.Any(m => m.Name == "get_CreatureGrade")
            || !calls.Any(m => m.Name == "SetGrayScale") || !calls.Any(m => m.Name == "Open"))
            throw new InvalidOperationException("LINE pet index restoration/grade/ownership behavior is incomplete.");
        // The edit consumes the same CreatureData but removes both its Boolean
        // result and branch. This method's foreach finally uses leave/PopAll,
        // which the arena-specific stack checker does not support.
        if (!method.Body.Instructions.Any(i => i.OpCode == OpCodes.Pop && i.Next?.OpCode == OpCodes.Nop))
            throw new InvalidOperationException("Missing neutralized LINE exclusion.");
        Console.WriteLine("PASS: LINE pets visible in native index; rarity filter and owned/unowned portrait handling preserved.");
    }

    static void InstallLinePets(string root, bool stageOnly, bool verify = false)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data/Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        if (verify) { VerifyLinePetBook(game.MainModule); return; }
        PatchLinePetBook(game.MainModule);
        var staged = dll + ".line-pets-staged"; game.Write(staged);
        using (var check = AssemblyDefinition.ReadAssembly(staged)) VerifyLinePetBook(check.MainModule);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Close game clients before installing LINE pet restoration.");
        var backup = Path.Combine("target/line-pet-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")); Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll")); File.Move(staged, dll, true);
        Console.WriteLine("Installed LINE pet index restoration. Restart the game.");
    }
}
