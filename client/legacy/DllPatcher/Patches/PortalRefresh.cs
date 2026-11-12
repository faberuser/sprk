using Mono.Cecil;

namespace DllPatcher;
partial class Program
{
    // Focused refresh of the conquest session hooks in RestoredPortal on an
    // already-patched client. The shared
    // --portal-only path re-runs every common patch, several of which are not
    // reapplicable to a patched assembly. The full --build-release path is unchanged.
    static readonly HashSet<string> ConquestPortalHelpers = new() {
        "ConquestSession", "ConquestSessions", "AcceptServerConquestSessions", "CompareConquestSessions" };
    static void RefreshPortalHelpers(string root, bool stageOnly)
    {
        var dll = Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll");
        var staged = dll + ".portal-staged";
        var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true }))
        {
            if (game.MainModule.GetType("RestoredPortal") == null)
                throw new InvalidOperationException("Run the full patcher first; RestoredPortal is not installed.");
            // Native data reduction prunes RestoredPortal; replace only the live
            // conquest session hooks (plus their new comparer), not the whole type.
            InstallRestoredPortalType(game.MainModule, ConquestPortalHelpers);
            game.Write(staged);
        }
        Console.WriteLine("Staged refreshed portal helpers: " + staged);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll, dll + ".before-portal-refresh-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
        File.Move(staged, dll, true);
        Console.WriteLine("Installed refreshed portal helpers.");
    }
}
