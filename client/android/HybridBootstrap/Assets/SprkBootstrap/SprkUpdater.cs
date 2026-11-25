using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// The Android counterpart of SprkLauncher's update: after the server is chosen and
/// before any game code loads, installs the game assemblies and platform-neutral patch
/// data of the server's signed release (see <see cref="SprkUpdate"/>).
///
/// Each file is checked by size and SHA-256 (re-hashed only when its size or write time
/// changed), changed objects download in parallel with byte-range resume, are verified,
/// and are then moved into place. A crash during the install leaves a marker, and the
/// next check repairs every file before the game loads.
/// </summary>
public sealed class SprkUpdater
{
    const int Parallel = 4;
    const int Attempts = 3;

    readonly string root = Application.persistentDataPath;
    readonly string manifestUrl;
    readonly RSAParameters? key;
    readonly ICollection<string> apkHot;
    readonly IDictionary<string, string> apkCode;
    readonly Action<string, bool> log;

    string status = "Checking for updates…";
    string detail = "";
    float progress = -1f;
    string error;
    bool canPlayAnyway, retry, playAnyway;
    // A compiled-in game that differs from the server's code: ask for the new APK.
    List<string> apkOutdated;
    bool apkAnswered, apkDownloadOpened;

    public bool Done { get; private set; }
    /// <summary>Hot assemblies the release adds beyond the APK's list (new Sprk feature assemblies).</summary>
    public List<string> ExtraHotNames { get; } = new List<string>();

    string Work => Path.Combine(root, "sprk", "update");
    string Objects => Path.Combine(Work, "objects");
    string Marker => Path.Combine(Work, "installing.txt");
    string Installed => Path.Combine(Work, "installed.json");
    string HashCache => Path.Combine(Work, "file-hashes.json");

    [Serializable] class CachedHash { public string path; public long size; public long ticks; public string sha256; }
    [Serializable] class HashStore { public List<CachedHash> files = new List<CachedHash>(); }
    [Serializable] class InstalledState { public string version; public string asset_set; }

    /// <param name="apkCode">For an APK with the game compiled in: its game assemblies'
    /// SHA-256 (game-code.txt). The release's assemblies are then not installed; when they
    /// differ, the player is asked to download the new APK. Null for interpreted code.</param>
    public SprkUpdater(string manifestUrl, string publicKeyPem, ICollection<string> apkHotNames, IDictionary<string, string> apkCode, Action<string, bool> log)
    {
        this.manifestUrl = manifestUrl;
        apkHot = apkHotNames;
        this.apkCode = apkCode;
        this.log = log;
        if (!string.IsNullOrWhiteSpace(publicKeyPem)) key = SprkUpdate.ParsePublicKey(publicKeyPem);
    }

    public IEnumerator Run()
    {
        while (true)
        {
            error = null;
            yield return Attempt();
            if (error == null) { Done = true; yield break; }
            log("update failed: " + error, true);
            canPlayAnyway = File.Exists(Installed) && !File.Exists(Marker);
            retry = playAnyway = false;
            while (!retry && !playAnyway) yield return null;
            if (playAnyway) { log("playing without the update", false); Done = true; yield break; }
        }
    }

    IEnumerator Attempt()
    {
        status = "Checking for updates…"; detail = ""; progress = -1f;
        var uri = new Uri(manifestUrl);
        byte[] raw = null;
        using (var request = UnityWebRequest.Get(uri))
        {
            request.timeout = 60;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) { error = $"Cannot reach the update server ({request.error})."; yield break; }
            raw = request.downloadHandler.data;
        }

        SprkUpdate.Release release;
        try
        {
            var outer = JsonUtility.FromJson<SprkUpdate.Outer>(Encoding.UTF8.GetString(raw));
            var payload = SprkUpdate.Authenticate(outer, raw, key, uri.IsLoopback);
            release = JsonUtility.FromJson<SprkUpdate.Release>(Encoding.UTF8.GetString(payload));
            SprkUpdate.Validate(release);
        }
        catch (Exception e) { error = e.Message; yield break; }
        raw = null;

        // The game code is part of this APK: a release with other code needs the new APK.
        // Asked once per launch (a retry after a failed download does not ask again).
        if (apkCode != null && !apkAnswered)
        {
            var differences = SprkUpdate.CodeDifferences(release, apkCode);
            if (differences.Count > 0)
            {
                log($"APK update available: release {release.version} has other game code ({string.Join(", ", differences.Take(6))}{(differences.Count > 6 ? $", +{differences.Count - 6} more" : "")})", false);
                apkOutdated = differences;
                while (!apkAnswered) yield return null;
                apkOutdated = null;
                log("playing with this APK's game code", false);
            }
            apkAnswered = true;
        }

        bool installCode = apkCode == null;
        var plan = new List<(SprkUpdate.ReleaseFile file, string target)>();
        foreach (var f in release.files)
        {
            string target = SprkUpdate.Target(f.path, apkHot, installCode);
            if (target != null) plan.Add((f, target));
        }
        var targets = new HashSet<string>(plan.Select(p => p.target), StringComparer.OrdinalIgnoreCase);
        var removals = release.deleted_files.Select(p => SprkUpdate.Target(p, apkHot, installCode))
            .Where(t => t != null && !targets.Contains(t)).ToList();
        log($"release {release.version} ({release.asset_set}): {plan.Count} files for Android", false);

        // 1. Check installed files (off the main thread: a first run hashes ~750 MB).
        var hashes = LoadHashes();
        int checkedCount = 0;
        var check = Task.Run(() =>
        {
            var changed = new List<(SprkUpdate.ReleaseFile file, string target)>();
            foreach (var p in plan)
            {
                if (!Matches(p.file, p.target, hashes)) changed.Add(p);
                Interlocked.Increment(ref checkedCount);
            }
            return changed;
        });
        while (!check.IsCompleted)
        {
            status = "Checking files…";
            detail = $"{checkedCount} / {plan.Count}";
            progress = plan.Count == 0 ? 1f : (float)checkedCount / plan.Count;
            yield return null;
        }
        if (check.IsFaulted) { error = "Checking files failed: " + check.Exception.GetBaseException().Message; yield break; }
        var changedFiles = check.Result;
        SaveHashes(hashes);

        // 2. Download each changed object once.
        var objects = changedFiles.GroupBy(p => p.file.sha256).Select(g => g.First().file).ToList();
        long total = objects.Sum(o => o.size), finished = 0;
        Directory.CreateDirectory(Objects);
        var queue = new Queue<SprkUpdate.ReleaseFile>(objects);
        var active = new List<Download>();
        while (queue.Count > 0 || active.Count > 0)
        {
            try
            {
                while (active.Count < Parallel && queue.Count > 0)
                {
                    var download = new Download(queue.Dequeue(), Objects, uri);
                    download.Start();
                    active.Add(download);
                }
                foreach (var d in active.ToList())
                {
                    if (!d.Finished) continue;
                    active.Remove(d);
                    if (d.Error == null) { finished += d.File.size; continue; }
                    if (++d.Failures >= Attempts) { error = $"Download of {d.File.path} failed: {d.Error}"; break; }
                    d.Start(); // resumes from the partial file
                    active.Add(d);
                }
            }
            catch (Exception e) { error = "Download failed: " + e.Message; }
            if (error != null) { foreach (var d in active) d.Cancel(); yield break; }
            long current = finished + active.Sum(d => d.Received);
            status = "Downloading update…";
            detail = $"{objects.Count} files · {current / 1048576f:F0} / {total / 1048576f:F0} MB";
            progress = total == 0 ? 1f : Mathf.Clamp01((float)current / total);
            yield return null;
        }

        // 3. Install (moves verified downloads into place; a marker covers interruptions).
        if (changedFiles.Count > 0 || removals.Any(t => File.Exists(Path.Combine(root, t))))
        {
            status = "Installing update…"; detail = ""; progress = -1f;
            var install = Task.Run(() => Install(changedFiles, removals, hashes));
            while (!install.IsCompleted) yield return null;
            if (install.IsFaulted) { error = "Installing the update failed: " + install.Exception.GetBaseException().Message; yield break; }
            SaveHashes(hashes);
        }
        File.WriteAllText(Installed, JsonUtility.ToJson(new InstalledState { version = release.version, asset_set = release.asset_set }));
        try { Directory.Delete(Objects, true); } catch (IOException) { }

        foreach (var p in plan)
        {
            if (!p.target.StartsWith("sprk/hot/", StringComparison.Ordinal)) continue;
            string name = Path.GetFileName(p.target);
            name = name.Substring(0, name.Length - ".dll.bytes".Length);
            if (!apkHot.Contains(name)) ExtraHotNames.Add(name);
        }
        log($"update complete: {changedFiles.Count} files changed, {removals.Count} removal candidates", false);
    }

    bool Matches(SprkUpdate.ReleaseFile file, string target, Dictionary<string, CachedHash> hashes)
    {
        var info = new FileInfo(Path.Combine(root, target));
        if (!info.Exists || info.Length != file.size) { lock (hashes) hashes.Remove(target); return false; }
        long ticks = info.LastWriteTimeUtc.Ticks;
        CachedHash known;
        lock (hashes) hashes.TryGetValue(target, out known);
        if (known == null || known.size != info.Length || known.ticks != ticks)
        {
            known = new CachedHash { path = target, size = info.Length, ticks = ticks, sha256 = SprkUpdate.Sha256File(info.FullName) };
            lock (hashes) hashes[target] = known;
        }
        return known.sha256 == file.sha256;
    }

    void Install(List<(SprkUpdate.ReleaseFile file, string target)> changed, List<string> removals, Dictionary<string, CachedHash> hashes)
    {
        File.WriteAllText(Marker, "install in progress");
        var uses = changed.GroupBy(p => p.file.sha256).ToDictionary(g => g.Key, g => g.Count());
        foreach (var (file, target) in changed)
        {
            string source = Path.Combine(Objects, file.sha256), destination = Path.Combine(root, target);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            if (File.Exists(destination)) File.Delete(destination);
            if (--uses[file.sha256] == 0) File.Move(source, destination); else File.Copy(source, destination);
            var info = new FileInfo(destination);
            hashes[target] = new CachedHash { path = target, size = info.Length, ticks = info.LastWriteTimeUtc.Ticks, sha256 = file.sha256 };
        }
        foreach (var target in removals)
        {
            string path = Path.Combine(root, target);
            if (File.Exists(path)) File.Delete(path);
            hashes.Remove(target);
        }
        File.Delete(Marker);
    }

    Dictionary<string, CachedHash> LoadHashes()
    {
        try
        {
            if (File.Exists(HashCache))
                return JsonUtility.FromJson<HashStore>(File.ReadAllText(HashCache)).files
                    .Where(h => h != null && h.path != null).GroupBy(h => h.path, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception e) { log("hash cache ignored: " + e.Message, false); }
        return new Dictionary<string, CachedHash>(StringComparer.OrdinalIgnoreCase);
    }

    void SaveHashes(Dictionary<string, CachedHash> hashes)
    {
        Directory.CreateDirectory(Work);
        string temporary = HashCache + ".tmp";
        File.WriteAllText(temporary, JsonUtility.ToJson(new HashStore { files = hashes.Values.ToList() }));
        if (File.Exists(HashCache)) File.Delete(HashCache);
        File.Move(temporary, HashCache);
    }

    /// <summary>One object: downloads to "&lt;sha&gt;.part" (resuming), verifies, renames to "&lt;sha&gt;".</summary>
    sealed class Download
    {
        public readonly SprkUpdate.ReleaseFile File;
        public int Failures;
        // Set by the verification task, read by the main thread.
        volatile string error;
        volatile bool finished;
        public string Error { get => error; private set => error = value; }
        public bool Finished { get => finished; private set => finished = value; }
        public long Received => request != null ? offset + (long)request.downloadedBytes : (Finished ? File.size : offset);
        readonly string partial, final;
        readonly Uri url;
        UnityWebRequest request;
        long offset;

        public Download(SprkUpdate.ReleaseFile file, string directory, Uri manifest)
        {
            File = file;
            partial = Path.Combine(directory, file.sha256 + ".part");
            final = Path.Combine(directory, file.sha256);
            url = new Uri(manifest, file.url);
            if (url.GetLeftPart(UriPartial.Authority) != manifest.GetLeftPart(UriPartial.Authority))
                throw new InvalidDataException("Downloads must come from the update server.");
        }

        public void Start()
        {
            Error = null;
            Finished = false;
            if (System.IO.File.Exists(final) && new FileInfo(final).Length == File.size) { Finished = true; return; }
            offset = System.IO.File.Exists(partial) ? new FileInfo(partial).Length : 0;
            if (offset >= File.size) { System.IO.File.Delete(partial); offset = 0; }
            request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET)
            {
                downloadHandler = new DownloadHandlerFile(partial, offset > 0) { removeFileOnAbort = false },
                timeout = 600,
            };
            if (offset > 0) request.SetRequestHeader("Range", $"bytes={offset}-");
            var operation = request.SendWebRequest();
            operation.completed += _ => Complete();
        }

        void Complete()
        {
            var r = request;
            request = null;
            long code = r.responseCode;
            string failure = r.result == UnityWebRequest.Result.Success ? null : r.error;
            r.Dispose();
            // A server that ignores the range resends the whole file after the partial bytes.
            if (failure == null && offset > 0 && code != 206) failure = "resume not supported (HTTP " + code + ")";
            if (failure != null)
            {
                if (code == 416 || failure.StartsWith("resume")) TryDelete(partial);
                Error = failure;
                Finished = true;
                return;
            }
            // Verify off the main thread, then publish under the final name.
            Task.Run(() =>
            {
                try
                {
                    if (new FileInfo(partial).Length != File.size || SprkUpdate.Sha256File(partial) != File.sha256)
                    {
                        TryDelete(partial);
                        Error = "hash mismatch";
                    }
                    else
                    {
                        if (System.IO.File.Exists(final)) System.IO.File.Delete(final);
                        System.IO.File.Move(partial, final);
                    }
                }
                catch (Exception e) { Error = e.Message; }
                Finished = true;
            });
        }

        public void Cancel() { request?.Abort(); }

        static void TryDelete(string path) { try { System.IO.File.Delete(path); } catch (IOException) { } }
    }

    // Drawn with SprkUi like the server picker.
    public void OnGUI()
    {
        SprkUi.Begin();
        SprkUi.Backdrop();
        bool dialog = apkOutdated != null || error != null;
        var content = SprkUi.Panel(1180f, dialog ? 640f : 420f);
        GUILayout.BeginArea(content);
        if (apkOutdated != null)
        {
            GUILayout.Label("New version available", SprkUi.Style("title"), GUILayout.Height(80));
            GUILayout.Space(16);
            GUILayout.Label(apkDownloadOpened
                ? "Install the downloaded APK over this app, then start the game again. Your downloaded game data and saved logins are kept."
                : "This server runs a newer version of the game. Download the new APK and install it over this app. Your downloaded game data and saved logins are kept.",
                SprkUi.Style("body"));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Play anyway", SprkUi.Style("secondary"), GUILayout.Width(380f), GUILayout.Height(120))) apkAnswered = true;
            GUILayout.Space(24);
            if (GUILayout.Button(apkDownloadOpened ? "Open download again" : "Download", SprkUi.Style("primary"), GUILayout.Height(120)))
            {
                apkDownloadOpened = true;
                Application.OpenURL(SprkUpdate.ApkDownloadUrl);
            }
            GUILayout.EndHorizontal();
        }
        else if (error == null)
        {
            GUILayout.Label(status, SprkUi.Style("title"), GUILayout.Height(80));
            GUILayout.Space(40);
            SprkUi.ProgressBar(GUILayoutUtility.GetRect(content.width, 28f, GUILayout.Height(28f)), progress);
            GUILayout.Space(28);
            GUILayout.Label(detail, SprkUi.Style("body"), GUILayout.Height(50));
        }
        else
        {
            GUILayout.Label("Update failed", SprkUi.Style("title"), GUILayout.Height(80));
            GUILayout.Space(16);
            GUILayout.Label(error, SprkUi.Style("error"));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (canPlayAnyway)
            {
                if (GUILayout.Button("Play without updating", SprkUi.Style("secondary"), GUILayout.Width(500f), GUILayout.Height(120))) playAnyway = true;
                GUILayout.Space(24);
            }
            if (GUILayout.Button("Retry", SprkUi.Style("primary"), GUILayout.Height(120))) retry = true;
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
        SprkUi.End();
    }
}
