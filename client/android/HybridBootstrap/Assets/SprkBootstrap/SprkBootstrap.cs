using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HybridCLR;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// AOT entry point of the Android HybridCLR proof player.
///
/// Loads the unchanged SPRK game assemblies (Assembly-CSharp + Sprk*.dll built by
/// client/dev.py build) through HybridCLR, then runs feasibility probes. It never
/// references game code at compile time; everything is reflection so that the
/// game stays in client/managed only.
///
/// Inputs (StreamingAssets/sprk, overridable by pushing the same layout to
/// persistentDataPath/sprk with adb):
///   index.txt        lines "aot:Name" / "hot:Name" / "bundle:relative/path"
///   aot/Name.dll.bytes   stripped AOT assemblies (generic metadata supplement)
///   hot/Name.dll.bytes   hot-update assemblies
/// </summary>
public class SprkBootstrap : MonoBehaviour
{
    const string Tag = "[SPRK-HYBRID] ";
    readonly List<string> lines = new List<string>();
    Vector2 scroll;
    int failures;

    void Log(string msg, bool fail = false)
    {
        if (fail) failures++;
        string line = (fail ? "FAIL " : "") + msg;
        lines.Add(line);
        if (fail) Debug.LogError(Tag + line); else Debug.Log(Tag + line);
        // Written per line so a native crash still leaves the last completed step on disk.
        try { File.AppendAllText(ProbeFile, line + "\n"); } catch { }
    }

    static string ProbeFile => Path.Combine(Application.persistentDataPath, "sprk-probe.txt");

    void Awake()
    {
        try { File.Delete(ProbeFile); } catch { }
    }

    string Override(string rel)
    {
        string p = Path.Combine(Application.persistentDataPath, "sprk", rel);
        return File.Exists(p) ? p : null;
    }

    IEnumerator Read(string rel, Action<byte[]> done)
    {
        string local = Override(rel);
        if (local != null) { done(File.ReadAllBytes(local)); yield break; }
        string url = Path.Combine(Application.streamingAssetsPath, "sprk", rel);
        if (!url.Contains("://")) url = "file://" + url;
        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            done(req.result == UnityWebRequest.Result.Success ? req.downloadHandler.data : null);
        }
    }

    IEnumerator Start()
    {
        Application.targetFrameRate = 30;
        Log($"device={SystemInfo.deviceModel} abi={(IntPtr.Size == 8 ? "64" : "32")}-bit os={SystemInfo.operatingSystem}");
        Log($"gfx={SystemInfo.graphicsDeviceType} {SystemInfo.graphicsDeviceName}");

        byte[] indexBytes = null;
        yield return Read("index.txt", b => indexBytes = b);
        if (indexBytes == null) { Log("index.txt missing", true); yield break; }
        var index = Encoding.UTF8.GetString(indexBytes).Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();

        // 1. Supplementary metadata for AOT generics instantiated by interpreted code.
        int aotOk = 0;
        foreach (var name in index.Where(l => l.StartsWith("aot:")).Select(l => l.Substring(4)))
        {
            byte[] dll = null;
            yield return Read($"aot/{name}.dll.bytes", b => dll = b);
            if (dll == null) { Log($"aot {name}: missing", true); continue; }
            var err = RuntimeApi.LoadMetadataForAOTAssembly(dll, HomologousImageMode.SuperSet);
            if (err == LoadImageErrorCode.OK) aotOk++; else Log($"aot {name}: {err}", true);
        }
        Log($"AOT metadata loaded: {aotOk}");

        // 2. The game's own assemblies, unchanged from the Windows build.
        var t0 = Time.realtimeSinceStartup;
        var hot = new Dictionary<string, Assembly>();
        foreach (var name in index.Where(l => l.StartsWith("hot:")).Select(l => l.Substring(4)))
        {
            byte[] dll = null;
            yield return Read($"hot/{name}.dll.bytes", b => dll = b);
            if (dll == null) { Log($"hot {name}: missing", true); continue; }
            try { hot[name] = Assembly.Load(dll); }
            catch (Exception e) { Log($"hot {name}: {e.GetType().Name}: {e.Message}", true); }
        }
        Log($"hot assemblies loaded: {hot.Count} in {(Time.realtimeSinceStartup - t0):F2}s");

        // 3. Type resolution across the whole graph (forces every reference to bind).
        int types = 0;
        foreach (var kv in hot)
        {
            try { types += kv.Value.GetTypes().Length; }
            catch (ReflectionTypeLoadException e)
            {
                Log($"types {kv.Key}: {e.LoaderExceptions.FirstOrDefault()?.Message}", true);
            }
        }
        Log($"types resolved: {types}");

        if (!hot.TryGetValue("Assembly-CSharp", out var game)) { Log("Assembly-CSharp not loaded", true); yield break; }

        // 4. Replay the game's [RuntimeInitializeOnLoadMethod] hooks (Unity only runs them for build-time assemblies).
        Invoke(game, "NVespa.UnityStartup", "RuntimeInit_BeforeSceneLoad");
        Invoke(game, "NVespa.UnityStartup", "RuntimeInit_AfterSceneLoad");

        // 5. Real serialized game components with Unity callbacks: NGUI UIPanel driven by AnimatedAlpha.
        yield return ComponentProbe(game);

        // 6. Interpreter throughput (rough; compare with the same loop on Windows Mono later).
        yield return null;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var gt = game.GetType("NVespa.UnityStartup");
        var m = gt?.GetMethod("GetTimeString", BindingFlags.Public | BindingFlags.Static);
        int n = 0;
        if (m != null) { for (; n < 20000; n++) m.Invoke(null, null); }
        Log($"perf: {n} interpreted GetTimeString calls in {sw.ElapsedMilliseconds} ms");

        // 7. Content gate: original asset bundles pushed to persistentDataPath/sprk/bundles.
        yield return BundleProbe(index.Where(l => l.StartsWith("bundle:")).Select(l => l.Substring(7)).ToList());

        Log(failures == 0 ? "RESULT: PASS" : $"RESULT: {failures} failure(s)");
    }

    void Invoke(Assembly asm, string type, string method)
    {
        try
        {
            var t = asm.GetType(type, true);
            t.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, null);
            Log($"invoked {type}.{method}");
        }
        catch (Exception e) { Log($"invoke {type}.{method}: {(e.InnerException ?? e).GetType().Name}: {(e.InnerException ?? e).Message}", true); }
    }

    IEnumerator ComponentProbe(Assembly game)
    {
        GameObject go = null;
        Component panel = null;
        try
        {
            var panelType = game.GetType("UIPanel", true);
            var alphaType = game.GetType("AnimatedAlpha", true);
            go = new GameObject("sprk-component-probe");
            go.SetActive(false);
            panel = go.AddComponent(panelType);
            var anim = go.AddComponent(alphaType);
            alphaType.GetField("alpha").SetValue(anim, 0.25f);
            go.SetActive(true); // OnEnable on interpreted components
            Log($"components added: {panel.GetType().FullName}, {anim.GetType().FullName}");
        }
        catch (Exception e) { Log($"component probe: {e.GetType().Name}: {e.Message}", true); yield break; }

        yield return null;
        yield return null;
        try
        {
            var a = (float)panel.GetType().GetProperty("alpha").GetValue(panel);
            if (Mathf.Abs(a - 0.25f) < 0.001f) Log($"callbacks OK: UIPanel.alpha={a} set by AnimatedAlpha.LateUpdate");
            else Log($"callbacks: UIPanel.alpha={a}, expected 0.25", true);
        }
        catch (Exception e) { Log($"component readback: {e.GetType().Name}: {e.Message}", true); }
        Destroy(go);
    }

    IEnumerator BundleProbe(List<string> bundles)
    {
        if (bundles.Count == 0) { Log("bundle probe: none listed"); yield break; }
        string root = Path.Combine(Application.persistentDataPath, "sprk", "bundles");
        float x = -3f;
        foreach (var rel in bundles)
        {
            string path = Path.Combine(root, rel);
            if (!File.Exists(path)) { Log($"bundle {rel}: not pushed", true); continue; }
            Log($"bundle {rel}: loading");
            var req = AssetBundle.LoadFromFileAsync(path);
            yield return req;
            var ab = req.assetBundle;
            if (ab == null) { Log($"bundle {rel}: LoadFromFile returned null", true); continue; }
            var all = ab.LoadAllAssets();
            var kinds = string.Join(",", all.GroupBy(o => o.GetType().Name).Select(g => $"{g.Key}x{g.Count()}"));
            int missing = 0, shadersBad = 0;
            foreach (var o in all.OfType<GameObject>())
            {
                foreach (var mb in o.GetComponentsInChildren<MonoBehaviour>(true)) if (mb == null) missing++;
                foreach (var r in o.GetComponentsInChildren<Renderer>(true))
                    foreach (var mat in r.sharedMaterials)
                        if (mat != null && mat.shader != null && !mat.shader.isSupported) shadersBad++;
            }
            Log($"bundle {rel}: {all.Length} assets [{kinds}] missingScripts={missing} unsupportedShaders={shadersBad}");
            var prefab = all.OfType<GameObject>().FirstOrDefault();
            if (prefab != null)
            {
                var inst = Instantiate(prefab, new Vector3(x, 0, 6), Quaternion.identity);
                x += 2f;
            }
            var tex = all.OfType<Texture2D>().FirstOrDefault();
            if (tex != null) shownTextures.Add(tex);
        }
    }

    readonly List<Texture2D> shownTextures = new List<Texture2D>();

    void OnGUI()
    {
        GUI.skin.label.fontSize = Mathf.Max(14, Screen.height / 45);
        GUILayout.BeginArea(new Rect(10, 10, Screen.width * 0.6f, Screen.height - 20));
        scroll = GUILayout.BeginScrollView(scroll);
        foreach (var l in lines) GUILayout.Label(l);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        float s = Screen.height / 4f, y = 10;
        foreach (var t in shownTextures) { GUI.DrawTexture(new Rect(Screen.width - s - 10, y, s, s), t, ScaleMode.ScaleToFit); y += s + 5; }
    }
}
