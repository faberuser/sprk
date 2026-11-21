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
        string line = $"[{Time.realtimeSinceStartup,6:F2}s] " + (fail ? "FAIL " : "") + msg;
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

    // Starts every read at once and waits a single time: reading one file per frame cost
    // ~1.8 s of black screen for the 54 game assemblies.
    IEnumerator ReadAll(List<string> rels, Dictionary<string, byte[]> into)
    {
        var pending = new List<(string rel, UnityWebRequest req)>();
        foreach (var rel in rels)
        {
            string local = Override(rel);
            if (local != null) { into[rel] = File.ReadAllBytes(local); continue; }
            string url = Path.Combine(Application.streamingAssetsPath, "sprk", rel);
            if (!url.Contains("://")) url = "file://" + url;
            var req = UnityWebRequest.Get(url);
            req.SendWebRequest();
            pending.Add((rel, req));
        }
        while (pending.Any(p => !p.req.isDone)) yield return null;
        foreach (var (rel, req) in pending)
        {
            into[rel] = req.result == UnityWebRequest.Result.Success ? req.downloadHandler.data : null;
            req.Dispose();
        }
    }

    bool probes;

    IEnumerator Start()
    {
        Application.targetFrameRate = 60;
        Log($"device={SystemInfo.deviceModel} abi={(IntPtr.Size == 8 ? "64" : "32")}-bit os={SystemInfo.operatingSystem}");
        Log($"gfx={SystemInfo.graphicsDeviceType} {SystemInfo.graphicsDeviceName}");

        byte[] indexBytes = null;
        yield return Read("index.txt", b => indexBytes = b);
        if (indexBytes == null) { overlay = true; Log("index.txt missing", true); yield break; }
        var index = Encoding.UTF8.GetString(indexBytes).Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        overlay = index.Contains("debug:overlay");
        probes = index.Contains("debug:probes");

        // Server: the game reads SPRK_HOST_URL (SprkLauncher sets it on Windows). Chosen here,
        // before any game code runs. "server:local" (written by the dev scripts) skips the
        // picker and uses the local server through adb reverse.
        string host = ServerPicker.LocalUrl, hostName = ServerPicker.LocalName, updateUrl = null;
        if (!index.Contains("server:local"))
        {
            picker = new ServerPicker();
            Log("waiting for server picker");
            while (!picker.Done) yield return null;
            host = picker.Chosen.url;
            hostName = picker.Chosen.name;
            updateUrl = picker.Chosen.UpdateUrl;
            picker = null;
        }
        Environment.SetEnvironmentVariable("SPRK_HOST_URL", host);
        Log($"server {hostName}: {host}");
        var aotNames = index.Where(l => l.StartsWith("aot:")).Select(l => l.Substring(4)).ToList();
        var hotNames = index.Where(l => l.StartsWith("hot:")).Select(l => l.Substring(4)).ToList();

        // Update code and platform-neutral data from the chosen server's signed release,
        // as SprkLauncher does on Windows. Unattended dev runs ("server:local") skip it:
        // they push their own files (dev.py hot, push_patch.sh).
        if (updateUrl != null)
        {
            byte[] keyBytes = null;
            yield return Read("update-public.pem", b => keyBytes = b);
            try { updater = new SprkUpdater(updateUrl, keyBytes == null ? null : Encoding.UTF8.GetString(keyBytes), hotNames, (m, f) => Log(m, f)); }
            catch (Exception e) { overlay = true; Log("update key: " + e.Message, true); yield break; }
            Log($"updating from {updateUrl}");
            // Run() returns only after a successful update or the player's "Play without updating";
            // the failed attempts it logged must not stop the game from starting.
            int failuresBeforeUpdate = failures;
            yield return updater.Run();
            failures = failuresBeforeUpdate;
            hotNames.AddRange(updater.ExtraHotNames);
            updater = null;
        }
        var files = new Dictionary<string, byte[]>();
        var t0 = Time.realtimeSinceStartup;
        yield return ReadAll(aotNames.Select(n => $"aot/{n}.dll.bytes").Concat(hotNames.Select(n => $"hot/{n}.dll.bytes")).ToList(), files);
        Log($"read {files.Count} assemblies in {(Time.realtimeSinceStartup - t0):F2}s");

        // 1. Supplementary metadata for AOT generics instantiated by interpreted code.
        int aotOk = 0;
        foreach (var name in aotNames)
        {
            byte[] dll = files[$"aot/{name}.dll.bytes"];
            if (dll == null) { Log($"aot {name}: missing", true); continue; }
            var err = RuntimeApi.LoadMetadataForAOTAssembly(dll, HomologousImageMode.SuperSet);
            if (err == LoadImageErrorCode.OK) aotOk++; else Log($"aot {name}: {err}", true);
        }
        Log($"AOT metadata loaded: {aotOk}");

        // 2. The game's own assemblies, unchanged from the Windows build.
        t0 = Time.realtimeSinceStartup;
        var hot = new Dictionary<string, Assembly>();
        foreach (var name in hotNames)
        {
            byte[] dll = files[$"hot/{name}.dll.bytes"];
            if (dll == null) { Log($"hot {name}: missing", true); continue; }
            // il2cpp cannot serve embedded resources of assemblies loaded from bytes; the game reads
            // them from the image instead (SprkEmbeddedResources in Assembly-CSharp).
            AppDomain.CurrentDomain.SetData("SPRK_HOT_DLL:" + name, dll);
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
        if (probes) yield return Probes(game, index);

        // 8. Repacked built-in content (scenes, Resources, MonoScripts, GLES shaders).
        var content = index.Where(l => l.StartsWith("content:")).Select(l => l.Substring(8)).ToList();
        if (content.Count > 0) yield return LoadContent(content);

        string start = index.Where(l => l.StartsWith("start:")).Select(l => l.Substring(6)).FirstOrDefault();
        if (start == null || failures > 0)
        {
            Log(failures == 0 ? "RESULT: PASS" : $"RESULT: {failures} failure(s)");
            yield break;
        }
        yield return StartGame(game, start);
    }

    // Feasibility probes from the first milestone; only with "debug:probes" in index.txt.
    IEnumerator Probes(Assembly game, List<string> index)
    {
        Invoke(game, "NVespa.UnityStartup", "RuntimeInit_AfterSceneLoad");
        var loaded = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var sceneComps = loaded.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Component>(true)).ToList();
        Log($"scene {loaded.name}: components={sceneComps.Count} missing={sceneComps.Count(c => c == null)} behaviours=[{string.Join(",", sceneComps.OfType<MonoBehaviour>().GroupBy(c => c.GetType().Name).OrderByDescending(g => g.Count()).Take(10).Select(g => $"{g.Key}x{g.Count()}"))}]");

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
    }

    static readonly List<AssetBundle> contentBundles = new List<AssetBundle>();
    // A player keeps its MonoScripts and always-included shaders resident. Loaded from
    // bundles they are ordinary assets, and the first single-mode scene load would
    // unload them (Shader.Find then fails; MonoBehaviours lose their scripts).
    static readonly List<UnityEngine.Object> keepAlive = new List<UnityEngine.Object>();

    IEnumerator LoadContent(List<string> files)
    {
        // Built into the APK (StreamingAssets/sprk/content); a copy pushed to the device
        // (persistentDataPath/sprk/content) takes precedence. AssetBundle.LoadFromFile reads
        // bundles inside the APK directly (Unity stores them uncompressed).
        string pushed = Path.Combine(Application.persistentDataPath, "sprk", "content");
        string packaged = Path.Combine(Application.streamingAssetsPath, "sprk", "content");
        foreach (var f in files)
        {
            var t = Time.realtimeSinceStartup;
            string path = File.Exists(Path.Combine(pushed, f)) ? Path.Combine(pushed, f) : Path.Combine(packaged, f);
            var req = AssetBundle.LoadFromFileAsync(path);
            yield return req;
            if (req.assetBundle == null) { Log($"content {f}: failed to load", true); continue; }
            contentBundles.Add(req.assetBundle);
            string extra = "";
            if (f.Contains("ggm"))
            {
                // MonoScripts must be in memory before MonoBehaviours deserialize.
                var all = req.assetBundle.LoadAllAssetsAsync();
                yield return all;
                keepAlive.AddRange(all.allAssets);
                extra = $" scripts={all.allAssets.Length}";
            }
            if (f.Contains("shaders"))
            {
                // Shader.Find only sees loaded shaders.
                var all = req.assetBundle.LoadAllAssetsAsync<Shader>();
                yield return all;
                keepAlive.AddRange(all.allAssets);
                extra = $" shaders={all.allAssets.Length} unsupported={all.allAssets.Cast<Shader>().Count(s => !s.isSupported)}";
            }
            Log($"content {f}: mounted in {(Time.realtimeSinceStartup - t):F2}s{extra} ({(path.StartsWith(pushed) ? "device copy" : "APK")})");
        }

        if (!probes) yield break;
        // Resources now resolve through the patched player ResourceManager.
        try
        {
            var loc = Resources.Load<TextAsset>("Localization/Localization_BasicEnglish");
            Log(loc != null ? $"Resources.Load Localization_BasicEnglish: {loc.bytes.Length} bytes" : "Resources.Load Localization_BasicEnglish: null", loc == null);
            var res = contentBundles.FirstOrDefault(x => x.name.Contains("resources"));
            if (res != null)
            {
                var viaBundle = res.LoadAsset<GameObject>("prefab2/window/gameoption/myinfowindow");
                Log(viaBundle == null ? "bundle LoadAsset MyInfoWindow: null" : $"bundle LoadAsset MyInfoWindow: missing={MissingIn(viaBundle)}");
            }
            var win = Resources.Load<GameObject>("Prefab2/Window/GameOption/MyInfoWindow");
            if (win == null) Log("Resources.Load MyInfoWindow: null", true);
            else
            {
                // Informational only: the scene start below is the real test.
                var inst = Instantiate(win);
                var comps = inst.GetComponentsInChildren<Component>(true);
                int missing = comps.Count(c => c == null);
                var kinds = string.Join(",", comps.Where(c => c != null).GroupBy(c => c.GetType().Name).OrderByDescending(g => g.Count()).Take(8).Select(g => $"{g.Key}x{g.Count()}"));
                Log($"Resources.Load MyInfoWindow: {inst.GetComponentsInChildren<Transform>(true).Length} transforms, missing={missing} [{kinds}]");
                Destroy(inst);
            }
        }
        catch (Exception e) { Log($"Resources probe: {e.GetType().Name}: {e.Message}", true); }
    }

    static int MissingIn(GameObject root)
    {
        var inst = Instantiate(root);
        int missing = inst.GetComponentsInChildren<Component>(true).Count(c => c == null);
        Destroy(inst);
        return missing;
    }

    // On-screen probe log; off unless index.txt has a "debug:overlay" line (the log file is always written).
    bool overlay;
    int gameLogLines;

    IEnumerator StartGame(Assembly game, string scene)
    {
        Log($"RESULT: PASS (starting {scene})");
        DontDestroyOnLoad(gameObject);
        overlay = false;
        foreach (var cam in FindObjectsByType<Camera>(FindObjectsSortMode.None)) if (cam.name == "BootCamera") Destroy(cam.gameObject);
        var seen = new Dictionary<string, int>();
        Application.logMessageReceived += (msg, stack, type) =>
        {
            if (type == LogType.Log || type == LogType.Warning || msg.StartsWith(Tag)) return;
            // Full stack for the first occurrence of each distinct error only.
            string key = type + msg;
            seen.TryGetValue(key, out int n);
            seen[key] = n + 1;
            if (n > 0 || gameLogLines > 200) return;
            gameLogLines++;
            string frames = string.Join(" < ", (stack ?? "").Split('\n').Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("UnityEngine.Debug")).Take(8));
            Log($"[{type}] {msg} @ {frames}");
        };
        var load = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
        if (load == null) { Log($"scene {scene}: not found in loaded bundles", true); yield break; }
        yield return load;
        Invoke(game, "NVespa.UnityStartup", "RuntimeInit_AfterSceneLoad");
        var loaded = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var sceneComps = loaded.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Component>(true)).ToList();
        Log($"scene {loaded.name}: components={sceneComps.Count} missing={sceneComps.Count(c => c == null)} behaviours=[{string.Join(",", sceneComps.OfType<MonoBehaviour>().GroupBy(c => c.GetType().Name).OrderByDescending(g => g.Count()).Take(10).Select(g => $"{g.Key}x{g.Count()}"))}]");
        yield return FrameStats();
    }

    // Every 5 s: average / p95 / worst frame time, frames over 50 ms (visible stutter),
    // gen-0 collections and managed heap. Lines start with "perf " for easy grepping.
    IEnumerator FrameStats()
    {
        var times = new List<float>(512);
        int gc0 = GC.CollectionCount(0);
        float window = Time.realtimeSinceStartup, last = window;
        while (true)
        {
            yield return null;
            float now = Time.realtimeSinceStartup;
            times.Add((now - last) * 1000f);
            last = now;
            if (now - window < 5f) continue;
            times.Sort();
            int gc = GC.CollectionCount(0);
            var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Log($"perf scene={s.name} fps={times.Count / (now - window):F1} avg={times.Average():F1}ms p95={times[(int)(times.Count * 0.95f)]:F1}ms " +
                $"max={times[times.Count - 1]:F0}ms hitches>50ms={times.Count(t => t > 50f)} gc0={gc - gc0} heap={GC.GetTotalMemory(false) >> 20}MB " +
                $"target={Application.targetFrameRate}");
            times.Clear();
            gc0 = gc;
            window = now;
        }
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

    ServerPicker picker;
    SprkUpdater updater;

    void OnGUI()
    {
        if (picker != null) { picker.OnGUI(); return; }
        if (updater != null) { updater.OnGUI(); return; }
        if (!overlay) return;
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
