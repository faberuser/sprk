using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static MethodDefinition AwakeningAnimation(ModuleDefinition module) => module
        .GetType("NGame2.NUI.NWindow.PurifyHeroRoom").NestedTypes.Single(t => t.Name.StartsWith("<coAwakeHeroAnim>"))
        .Methods.Single(m => m.Name == "MoveNext");

    static void PatchAwakenDelay(ModuleDefinition module)
    {
        var method = AwakeningAnimation(module);
        var waits = method.Body.Instructions.Where(i => i.OpCode == OpCodes.Newobj &&
            i.Operand is MethodReference r && r.DeclaringType.FullName == "UnityEngine.WaitForSeconds").ToArray();
        if (waits.Length == 0) { VerifyAwakenDelay(module); return; }
        if (waits.Length != 1 || waits[0].Previous.OpCode != OpCodes.Ldc_R4 || !Equals(waits[0].Previous.Operand, 3f))
            throw new InvalidOperationException("Unexpected awakening delay; refusing to patch.");
        // Yield for one frame instead of waiting three seconds. Keep the native
        // request success checks, full-screen cinematic loading, and callbacks.
        waits[0].Previous.OpCode = OpCodes.Ldnull;
        waits[0].Previous.Operand = null;
        waits[0].OpCode = OpCodes.Nop;
        waits[0].Operand = null;
        VerifyAwakenDelay(module);
    }

    static void VerifyAwakenDelay(ModuleDefinition module)
    {
        var body = AwakeningAnimation(module).Body.Instructions;
        if (body.Any(i => i.Operand is MethodReference r && r.DeclaringType.FullName == "UnityEngine.WaitForSeconds") ||
            body.Count(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "<>2__current" &&
                i.Previous.OpCode == OpCodes.Nop && i.Previous.Previous.OpCode == OpCodes.Ldnull) != 1)
            throw new InvalidOperationException("Awakening must yield one frame without a timed delay.");
        foreach (var name in new[] { "PlayAwake", "OpenGetNewHero", "Close", "Apply", "fire" })
            if (!body.Any(i => i.Operand is MethodReference r && r.Name == name))
                throw new InvalidOperationException("Missing native awakening operation: " + name);
    }

    static void InstallAwakenDelay(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".awaken-delay-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            PatchAwakenDelay(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var verified = AssemblyDefinition.ReadAssembly(staged)) VerifyAwakenDelay(verified.MainModule);
        if (!stageOnly) {
            var backup = Path.Combine("target", "awaken-delay-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified removal of the three-second awakening delay." :
            "Installed and verified awakening delay removal. Restart the client.");
    }
}
