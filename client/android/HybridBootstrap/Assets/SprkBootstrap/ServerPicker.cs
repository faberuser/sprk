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

    // Laid out on SprkUi's scaled canvas, large enough for touch.
    public void OnGUI()
    {
        SprkUi.Begin();
        SprkUi.Backdrop();
        var content = SprkUi.Panel(1240f, adding ? 900f : 860f, 160f);
        GUILayout.BeginArea(content);

        if (!adding)
        {
            GUILayout.Label("Choose a server", SprkUi.Style("titleLeft"), GUILayout.Height(70));
            GUILayout.Label("Pick where to play, or add your community's server.", SprkUi.Style("bodyLeft"), GUILayout.Height(50));
            GUILayout.Space(24);
            scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none, GUILayout.Height(420));
            foreach (var p in All.ToList())
            {
                bool selected = p.name == Selected.name;
                var row = GUILayoutUtility.GetRect(content.width - 30f, 116f, GUILayout.Height(116f));
                bool deletable = p.name != LocalName;
                var pick = deletable ? new Rect(row.x, row.y, row.width - 196f, row.height) : row;
                if (GUI.Button(pick, GUIContent.none, SprkUi.Style(selected ? "rowSelected" : "row")))
                {
                    store.selected = p.name;
                    Save();
                }
                // Radio mark, name and address drawn over the row.
                SprkUi.Radio(new Rect(pick.x + 34f, pick.y + pick.height / 2f - 18f, 36f, 36f), selected);
                GUI.Label(new Rect(pick.x + 96f, pick.y + 14f, pick.width - 120f, 48f), p.name, SprkUi.Style("label"));
                GUI.Label(new Rect(pick.x + 96f, pick.y + 60f, pick.width - 120f, 40f), p.url, SprkUi.Style("small"));
                if (deletable && GUI.Button(new Rect(row.xMax - 180f, row.y, 180f, row.height), "Delete", SprkUi.Style("secondary")))
                {
                    store.servers.Remove(p);
                    if (store.selected == p.name) store.selected = LocalName;
                    Save();
                }
                GUILayout.Space(14);
            }
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            if (Selected.name == LocalName)
                GUILayout.Label("Local reaches the PC's local server through `adb reverse` (USB or emulator).", SprkUi.Style("small"));
            GUILayout.Space(16);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add server", SprkUi.Style("secondary"), GUILayout.Width(360f), GUILayout.Height(120))) { adding = true; error = ""; }
            GUILayout.Space(24);
            if (GUILayout.Button("Play", SprkUi.Style("primary"), GUILayout.Height(120)))
            {
                Chosen = Selected;
                Done = true;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            var field = SprkUi.Style("field");
            var label = SprkUi.Style("label");
            var small = SprkUi.Style("small");
            GUILayout.Label("Add a server", SprkUi.Style("titleLeft"), GUILayout.Height(70));
            GUILayout.Space(16);
            GUILayout.Label("Name", label, GUILayout.Height(44));
            newName = GUILayout.TextField(newName, 40, field, GUILayout.Height(88));
            GUILayout.Space(16);
            GUILayout.Label("host.json URL", label, GUILayout.Height(44));
            newUrl = GUILayout.TextField(newUrl, 300, field, GUILayout.Height(88));
            GUILayout.Label("https://your-server.example/host.json (plain http only for 127.0.0.1)", small);
            GUILayout.Space(12);
            GUILayout.Label("Update URL (optional)", label, GUILayout.Height(44));
            newUpdate = GUILayout.TextField(newUpdate, 300, field, GUILayout.Height(88));
            GUILayout.Label("Empty: the server's /updates/stable/manifest.json", small);
            if (error.Length > 0) GUILayout.Label(error, SprkUi.Style("errorLeft"));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel", SprkUi.Style("secondary"), GUILayout.Width(360f), GUILayout.Height(120))) adding = false;
            GUILayout.Space(24);
            if (GUILayout.Button("Save", SprkUi.Style("primary"), GUILayout.Height(120)))
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
        SprkUi.End();
    }
}
