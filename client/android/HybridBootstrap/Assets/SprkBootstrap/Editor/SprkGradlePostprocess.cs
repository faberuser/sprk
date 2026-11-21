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
        string project = Path.GetDirectoryName(Application.dataPath);
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
}
