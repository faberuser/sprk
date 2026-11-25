using System.Diagnostics;
using System.IO;
using UnityEditor.Android;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Fills the player's ResourceManager with the game's Resources container (pointing into
/// the repacked content bundles) before Gradle packs the APK. Skipped when the content
/// has not been repacked yet (../sprk-android/bootstrap/content/resources-map.json).
/// </summary>
public class SprkGradlePostprocess : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 100;

    public void OnPostGenerateGradleAndroidProject(string unityLibrary)
    {
        MakeProfileable(unityLibrary);
        string project = Path.GetDirectoryName(Application.dataPath);
        KeepGameStartupForBootstrap(unityLibrary, project);
        string repo = Path.GetFullPath(Path.Combine(project, "..", "..", ".."));
        string map = Path.Combine(repo, "..", "sprk-android", "bootstrap", "content", "resources-map.json");
        string ggm = Path.Combine(unityLibrary, "src", "main", "assets", "bin", "Data", "globalgamemanagers");
        if (!File.Exists(map)) { Debug.LogWarning($"[SprkGradlePostprocess] no {map}; Resources stay empty"); return; }
        if (!File.Exists(ggm)) throw new FileNotFoundException("globalgamemanagers not found in Gradle project", ggm);

        string script = Path.Combine(project, "content", "patch_player_resources.py");
        var psi = new ProcessStartInfo("python", $"-I \"{script}\" \"{ggm}\" \"{map}\"")
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
        };
        using var p = Process.Start(psi);
        string stdout = p.StandardOutput.ReadToEnd(), stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();
        Debug.Log("[SprkGradlePostprocess] " + stdout.Trim());
        if (p.ExitCode != 0) throw new System.Exception("[SprkGradlePostprocess] patch failed: " + stderr);
    }

    // A game compiled into the player has its [RuntimeInitializeOnLoadMethod] hooks
    // (NVespa.UnityStartup) in RuntimeInitializeOnLoads.json, so Unity would run them at
    // launch, before the server picker, the update and the content mount. SprkBootstrap
    // calls them itself at the right time (as it does for interpreted code), so the game
    // assemblies' entries are removed. Entries are flat JSON objects in the "root" array.
    static void KeepGameStartupForBootstrap(string unityLibrary, string project)
    {
        string file = Path.Combine(unityLibrary, "src", "main", "assets", "bin", "Data", "RuntimeInitializeOnLoads.json");
        string names = Path.Combine(project, "HotUpdate", "hot-update.txt");
        if (!File.Exists(file) || !File.Exists(names)) return;
        var game = new System.Collections.Generic.HashSet<string>(
            System.Linq.Enumerable.Where(System.Linq.Enumerable.Select(File.ReadAllLines(names), l => l.Trim()), l => l.Length > 0));
        string json = File.ReadAllText(file);
        int removed = 0;
        string result = System.Text.RegularExpressions.Regex.Replace(json, @"\{[^{}]*\}\s*,?\s*", m =>
        {
            var assembly = System.Text.RegularExpressions.Regex.Match(m.Value, "\"assemblyName\"\\s*:\\s*\"([^\"]*)\"");
            if (!assembly.Success || !game.Contains(assembly.Groups[1].Value)) return m.Value;
            removed++;
            return "";
        });
        // A removed last entry leaves "...},]"; JSON allows no trailing comma.
        result = System.Text.RegularExpressions.Regex.Replace(result, @",\s*\]", "]");
        if (removed == 0) return;
        File.WriteAllText(file, result);
        Debug.Log($"[SprkGradlePostprocess] {removed} game startup hooks left to SprkBootstrap");
    }

    // <profileable android:shell="true"/> lets `adb shell simpleperf` sample the release APK
    // on a phone (CPU profiles of the player and the HybridCLR interpreter). It has no
    // runtime cost and does not make the app debuggable.
    static void MakeProfileable(string unityLibrary)
    {
        string manifest = Path.Combine(unityLibrary, "src", "main", "AndroidManifest.xml");
        string xml = File.ReadAllText(manifest);
        if (xml.Contains("<profileable")) return;
        int app = xml.IndexOf("<application", System.StringComparison.Ordinal);
        int close = app < 0 ? -1 : xml.IndexOf('>', app);
        if (close < 0) throw new System.Exception("[SprkGradlePostprocess] no <application> in " + manifest);
        if (xml[close - 1] == '/')
        {
            xml = xml.Substring(0, close - 1) + "><profileable android:shell=\"true\" /></application>" + xml.Substring(close + 1);
        }
        else
        {
            xml = xml.Insert(close + 1, "<profileable android:shell=\"true\" />");
        }
        File.WriteAllText(manifest, xml);
        Debug.Log("[SprkGradlePostprocess] profileable by shell");
    }
}
