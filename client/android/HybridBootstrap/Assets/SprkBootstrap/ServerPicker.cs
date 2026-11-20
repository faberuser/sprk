using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Server choice for the Android build, the counterpart of SprkLauncher's server profiles.
/// The game reads its server from SPRK_HOST_URL (a host.json URL), which the launcher sets
/// on Windows; here the bootstrap sets it from this list before any game code runs.
///
/// "Local" is built in (127.0.0.1, reached through `adb reverse`). Added servers follow the
/// game's own rule (SprkAccounts.ResolveHostUrl): https, or plain http only for loopback.
/// The list lives in the app's private internal storage.
/// </summary>
public sealed class ServerPicker
{
    [Serializable]
    public class Profile
    {
        public string name;
        public string url;
        /// <summary>Update manifest URL; empty means the server's default (SprkUpdate.DefaultManifestUrl).</summary>
        public string update;
        public string UpdateUrl => string.IsNullOrWhiteSpace(update) ? SprkUpdate.DefaultManifestUrl(url) : update;
    }
    [Serializable] class Store { public List<Profile> servers = new List<Profile>(); public string selected; }

    public const string LocalName = "Local";
    public const string LocalUrl = "http://127.0.0.1:8080/host.json";

    readonly string file;
    Store store;
    bool adding;
    string newName = "", newUrl = "https://", newUpdate = "", error = "";
    Vector2 scroll;

    public bool Done { get; private set; }
    public Profile Chosen { get; private set; }

    public ServerPicker()
    {
        file = Path.Combine(PrivateDirectory(), "sprk-servers.json");
        try { store = JsonUtility.FromJson<Store>(File.ReadAllText(file)); } catch { }
        store ??= new Store();
        store.servers.RemoveAll(p => p == null || p.name == LocalName || Validate(p.url) == null
            || (!string.IsNullOrWhiteSpace(p.update) && ValidateUpdate(p.update) == null));
    }

    public IEnumerable<Profile> All => new[] { new Profile { name = LocalName, url = LocalUrl } }.Concat(store.servers);

    public Profile Selected => All.FirstOrDefault(p => p.name == store.selected) ?? All.First();

    /// <summary>Normalized host.json URL, or null when the game would refuse it.</summary>
    public static string Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        text = text.Trim();
        if (text.EndsWith("/")) text += "host.json";
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri)) return null;
        bool ok = uri.Scheme == "https" || (uri.Scheme == "http" && uri.IsLoopback);
        if (!ok || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment)) return null;
        return uri.AbsoluteUri;
    }

    /// <summary>Normalized update manifest URL, or null when it is not https (http only for loopback).</summary>
    public static string ValidateUpdate(string text)
    {
        if (!Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri)) return null;
        bool ok = uri.Scheme == "https" || (uri.Scheme == "http" && uri.IsLoopback);
        if (!ok || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.Query)) return null;
        return uri.AbsoluteUri;
    }

    void Save()
    {
        try { File.WriteAllText(file, JsonUtility.ToJson(store, true)); }
        catch (Exception e) { error = "Could not save: " + e.Message; }
    }

    static string PrivateDirectory()
    {
        if (Application.platform != RuntimePlatform.Android) return Application.persistentDataPath;
        using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        using var dir = activity.Call<AndroidJavaObject>("getFilesDir");
        return dir.Call<string>("getAbsolutePath");
    }

    // Laid out on a 1920x1080 canvas scaled to the screen, large enough for touch.
    public void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        var w = 1100f;
        var area = new Rect((Screen.width / scale - w) / 2f, 60f, w, 960f);
        var title = new GUIStyle(GUI.skin.label) { fontSize = 56, alignment = TextAnchor.MiddleCenter };
        var label = new GUIStyle(GUI.skin.label) { fontSize = 34 };
        var small = new GUIStyle(GUI.skin.label) { fontSize = 26, wordWrap = true };
        var button = new GUIStyle(GUI.skin.button) { fontSize = 38 };
        var field = new GUIStyle(GUI.skin.textField) { fontSize = 36 };
        GUI.Box(area, GUIContent.none);
        GUILayout.BeginArea(new Rect(area.x + 40, area.y + 30, w - 80, area.height - 60));
        GUILayout.Label("Choose a server", title, GUILayout.Height(90));

        if (!adding)
        {
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(520));
            foreach (var p in All.ToList())
            {
                GUILayout.BeginHorizontal();
                bool selected = p.name == Selected.name;
                var style = new GUIStyle(button) { alignment = TextAnchor.MiddleLeft, fontStyle = selected ? FontStyle.Bold : FontStyle.Normal };
                if (GUILayout.Button((selected ? "● " : "○ ") + p.name + "\n" + p.url, new GUIStyle(style) { fontSize = 30 }, GUILayout.Height(110)))
                {
                    store.selected = p.name;
                    Save();
                }
                if (p.name != LocalName && GUILayout.Button("Delete", button, GUILayout.Width(190), GUILayout.Height(110)))
                {
                    store.servers.Remove(p);
                    if (store.selected == p.name) store.selected = LocalName;
                    Save();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            GUILayout.EndScrollView();
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add server", button, GUILayout.Height(120))) { adding = true; error = ""; }
            GUILayout.Space(20);
            if (GUILayout.Button("Play", button, GUILayout.Height(120)))
            {
                Chosen = Selected;
                Done = true;
            }
            GUILayout.EndHorizontal();
            if (Selected.name == LocalName)
                GUILayout.Label("Local reaches the PC's local server through `adb reverse` (USB or emulator).", small);
        }
        else
        {
            GUILayout.Label("Name", label);
            newName = GUILayout.TextField(newName, 40, field, GUILayout.Height(90));
            GUILayout.Space(16);
            GUILayout.Label("host.json URL", label);
            newUrl = GUILayout.TextField(newUrl, 300, field, GUILayout.Height(90));
            GUILayout.Label("https://your-server.example/host.json (plain http only for 127.0.0.1)", small);
            GUILayout.Space(16);
            GUILayout.Label("Update URL (optional)", label);
            newUpdate = GUILayout.TextField(newUpdate, 300, field, GUILayout.Height(90));
            GUILayout.Label("Empty: the server's /updates/stable/manifest.json", small);
            if (error.Length > 0) GUILayout.Label(error, new GUIStyle(small) { normal = { textColor = new Color(1f, 0.5f, 0.4f) } });
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel", button, GUILayout.Height(120))) adding = false;
            GUILayout.Space(20);
            if (GUILayout.Button("Save", button, GUILayout.Height(120)))
            {
                string url = Validate(newUrl), name = newName.Trim();
                string update = newUpdate.Trim().Length == 0 ? "" : ValidateUpdate(newUpdate);
                if (name.Length == 0 || name == LocalName || store.servers.Any(p => p.name == name)) error = "Enter a new, unique name.";
                else if (url == null) error = "Enter an https:// host.json URL (http only for 127.0.0.1).";
                else if (update == null) error = "Enter an https:// update URL, or leave it empty.";
                else
                {
                    store.servers.Add(new Profile { name = name, url = url, update = update });
                    store.selected = name;
                    Save();
                    adding = false;
                    newName = "";
                    newUrl = "https://";
                    newUpdate = "";
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
        GUI.matrix = Matrix4x4.identity;
    }
}
