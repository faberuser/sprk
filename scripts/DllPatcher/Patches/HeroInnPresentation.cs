using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static void VerifyHeroInnPresentation(ModuleDefinition module)
    {
        var group = module.GetType("NGame2.NUI.NWindow.HeroInnViewButtonGroup");
        var setter = group.Methods.Single(m => m.Name == "SetListener");
        if (setter.Body.Instructions.Count != 4 || setter.Body.Instructions[2].OpCode != OpCodes.Stfld
            || ((FieldReference)setter.Body.Instructions[2].Operand).Name != "_listener"
            || group.Methods.Any(m => m.Name == "HideUnsupportedButtons"))
            throw new InvalidOperationException("Inn still overrides prefab action types/visibility.");
        var init = module.GetType("NGame2.NUI.NWindow.HeroInnView").Methods.Single(m => m.Name == "InitUIBtnGroup");
        if (init.Body.Instructions.Count(i => i.Operand is MethodReference m && m.Name == "SetListener") != 3
            || !init.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "IsHaveHero")
            || !init.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "IsSelectedHero")
            || init.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4_0))
            throw new InvalidOperationException("Inn must use the native owned/selected group policy.");
        VerifyArenaResultStack(init); VerifyArenaResultStack(setter);
        Console.WriteLine("PASS: native Inn group policy and prefab button actions restored; no recruitment override remains.");
    }

    static void InstallHeroInnPresentation(string root, bool stageOnly, bool verify = false)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data/Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        if (verify) { VerifyHeroInnPresentation(game.MainModule); return; }
        PatchHeroInnRecruiting(game.MainModule.GetType("NGame2.NUI.NWindow.HeroInnView"));
        PatchHeroInnButtons(game.MainModule, null, null);
        VerifyHeroInnPresentation(game.MainModule);
        var staged = dll + ".inn-presentation-staged"; game.Write(staged);
        using (var check = AssemblyDefinition.ReadAssembly(staged)) VerifyHeroInnPresentation(check.MainModule);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Close game clients before installing Inn presentation.");
        var backup = Path.Combine("target/inn-presentation-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")); Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll")); File.Move(staged, dll, true);
        Console.WriteLine("Installed pre-doomsday Inn presentation. Restart the game.");
    }
}
