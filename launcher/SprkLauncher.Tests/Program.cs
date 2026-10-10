using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SprkLauncher.Core;

var tests = new (string Name, Func<Task> Run)[]
{
    ("Downloads only changed files; applies deletions; resumes by byte range", UpdatesAndResume),
    ("Corrupt download leaves installed files untouched", CorruptDownload),
    ("Interrupted install rolls back; committed install survives recovery", Recovery),
    ("Locked game files roll back earlier replacements", LockedFileRollback),
    ("Rejects unsafe, duplicate, and external update paths", UnsafeManifests),
    ("Authenticates signed payload and rejects unsigned or forged manifests", Signatures),
    ("Concurrent launchers cannot update the same installation", Locking),
    ("Repairs PowerShell PEM metadata without changing configuration preferences", ConfigRepair),
    ("Profiles preserve legacy settings and persist selection and signing keys", ProfileConfiguration),
    ("Switching profiles checks the selected endpoint and replaces shared files", ProfileUpdates),
};
foreach (var test in tests) { await test.Run(); Console.WriteLine($"PASS {test.Name}"); }
Console.WriteLine($"{tests.Length} launcher checks passed.");
if (args.Length == 2 && args[0] == "--verify-client")
{
    ClientProfileSupport.EnsureInstalled(args[1]);
    Console.WriteLine("PASS installed client supports per-process server selection");
}

static void Require(bool value, string message) { if (!value) throw new Exception(message); }
static async Task Reject(Func<Task> action)
{
    try { await action(); } catch (Exception error) when (error is IOException || error is InvalidDataException || error is CryptographicException || error is InvalidOperationException) { return; }
    throw new Exception("Expected the unsafe operation to fail");
}

static async Task UpdatesAndResume()
{
    using var fixture = new Fixture();
    fixture.Server.Files["Managed/keep.dll"] = Encoding.UTF8.GetBytes("unchanged");
    fixture.Server.Files["Managed/change.dll"] = Encoding.UTF8.GetBytes("updated code");
    fixture.Server.Files["TableJit/new.jit"] = Encoding.UTF8.GetBytes("new table");
    fixture.Write("Managed/keep.dll", "unchanged");
    fixture.Write("Managed/change.dll", "old code");
    fixture.Write("Managed/deleted.dll", "old helper");
    fixture.Server.Manifest.DeletedFiles = ["Managed/deleted.dll"];
    fixture.Server.Refresh();
    var updated = fixture.Server.Manifest.Files.Single(f => f.Path == "Managed/change.dll");
    fixture.Write($".sprk-launcher/downloads/{updated.Sha256}.bin.part", "upda");
    var result = await fixture.Engine.UpdateAsync();
    Require(result.DownloadedFiles == 2, "Unchanged files must not download");
    Require(fixture.Read("Managed/change.dll") == "updated code", "DLL update");
    Require(fixture.Read("TableJit/new.jit") == "new table", "Table update");
    Require(!File.Exists(fixture.Path("Managed/deleted.dll")), "Deleted helper remains");
    Require(fixture.Server.Ranges.Contains(4), "Resume request missing");
    var downloads = fixture.Server.Downloads;
    Require((await fixture.Engine.UpdateAsync()).DownloadedFiles == 0, "Second launch should not download");
    Require(fixture.Server.Downloads == downloads, "Unexpected repeat download");
}

static async Task CorruptDownload()
{
    using var fixture = new Fixture();
    fixture.Server.Files["a.dll"] = Encoding.UTF8.GetBytes("replacement");
    fixture.Server.Files["b.dll"] = Encoding.UTF8.GetBytes("good payload");
    fixture.Write("a.dll", "original a"); fixture.Write("b.dll", "original b");
    fixture.Server.Refresh();
    fixture.Server.CorruptPath = "b.dll";
    await Reject(() => fixture.Engine.UpdateAsync());
    Require(fixture.Read("a.dll") == "original a" && fixture.Read("b.dll") == "original b", "Files changed before all downloads verified");
}

static Task Recovery()
{
    using var fixture = new Fixture();
    fixture.Write("a.dll", "partial new");
    fixture.Write("new.dll", "partial added");
    fixture.Write(".sprk-launcher/backup/a.dll", "original");
    fixture.Write(".sprk-launcher/transaction.json", JsonSerializer.Serialize(new Transaction
    {
        Version = "2", Entries = [new() { Path = "a.dll", HadOriginal = true }, new() { Path = "new.dll" }]
    }));
    fixture.Engine.Recover(); fixture.Engine.Recover();
    Require(fixture.Read("a.dll") == "original" && !File.Exists(fixture.Path("new.dll")), "Rollback failed");
    fixture.Write("a.dll", "committed new");
    fixture.Write(".sprk-launcher/backup/a.dll", "original");
    fixture.Write(".sprk-launcher/transaction.json", JsonSerializer.Serialize(new Transaction
    {
        Version = "3", Committed = true, Entries = [new() { Path = "a.dll", HadOriginal = true }]
    }));
    fixture.Engine.Recover();
    Require(fixture.Read("a.dll") == "committed new" && fixture.Engine.InstalledVersion == "3", "Committed update was rolled back");
    return Task.CompletedTask;
}

static async Task LockedFileRollback()
{
    if (!OperatingSystem.IsWindows()) return;
    using var fixture = new Fixture();
    fixture.Server.Files["a.dll"] = Encoding.UTF8.GetBytes("new a");
    fixture.Server.Files["b.dll"] = Encoding.UTF8.GetBytes("new b");
    fixture.Write("a.dll", "old a"); fixture.Write("b.dll", "old b");
    fixture.Server.Refresh();
    using (var locked = new FileStream(fixture.Path("b.dll"), FileMode.Open, FileAccess.Read, FileShare.Read))
        await Reject(() => fixture.Engine.UpdateAsync());
    Require(fixture.Read("a.dll") == "old a" && fixture.Read("b.dll") == "old b", "Failed replacement did not roll back");
}

static async Task UnsafeManifests()
{
    foreach (var path in new[] { "../escape.dll", "C:/escape", "a\\b", "a/CON.dll", ".sprk-launcher/journal", "SprkLauncher.exe", "sprk-launcher.json" })
    {
        using var fixture = new Fixture();
        fixture.Server.Files[path] = Encoding.UTF8.GetBytes("evil"); fixture.Server.Refresh();
        await Reject(() => fixture.Engine.UpdateAsync());
        Require(fixture.Server.Downloads == 0, "Unsafe path downloaded");
    }
    using var duplicate = new Fixture();
    duplicate.Server.Files["A.dll"] = Encoding.UTF8.GetBytes("a");
    duplicate.Server.Files["a.dll"] = Encoding.UTF8.GetBytes("b"); duplicate.Server.Refresh();
    await Reject(() => duplicate.Engine.UpdateAsync());
    using var external = new Fixture();
    external.Server.Files["safe.dll"] = Encoding.UTF8.GetBytes("a"); external.Server.Refresh();
    external.Server.Manifest.Files[0].Url = "https://evil.example/payload.dll";
    await Reject(() => external.Engine.UpdateAsync());
}

static Task Signatures()
{
    using var fixture = new Fixture();
    using var rsa = RSA.Create(2048);
    var config = new LauncherConfig { ManifestPublicKeyPem = rsa.ExportSubjectPublicKeyInfoPem() };
    var engine = new UpdateEngine(fixture.Root, config, fixture.Http);
    var payload = JsonSerializer.SerializeToUtf8Bytes(new UpdateManifest { SchemaVersion = 1, Version = "signed" });
    var signature = rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    var envelope = JsonSerializer.SerializeToUtf8Bytes(new { version = "forged-outer", signature = new
    {
        algorithm = "RSA-SHA256", payload = Convert.ToBase64String(payload), value = Convert.ToBase64String(signature)
    } });
    Require(engine.ParseManifest(envelope).Version == "signed", "Unsigned outer fields were trusted");
    Reject(() => { engine.ParseManifest(payload); return Task.CompletedTask; }).GetAwaiter().GetResult();
    signature[0] ^= 1;
    var forged = JsonSerializer.SerializeToUtf8Bytes(new { signature = new
    {
        algorithm = "RSA-SHA256", payload = Convert.ToBase64String(payload), value = Convert.ToBase64String(signature)
    } });
    Reject(() => { engine.ParseManifest(forged); return Task.CompletedTask; }).GetAwaiter().GetResult();
    Reject(() => { _ = new UpdateEngine(fixture.Root, new() { ManifestUrl = "https://host.example/updates/stable/manifest.json" }, fixture.Http); return Task.CompletedTask; }).GetAwaiter().GetResult();
    return Task.CompletedTask;
}

static Task ConfigRepair()
{
    using var fixture = new Fixture();
    using var rsa = RSA.Create(2048);
    var pem = rsa.ExportSubjectPublicKeyInfoPem();
    var path = fixture.Path("sprk-launcher.json");
    fixture.Write("sprk-launcher.json", JsonSerializer.Serialize(new
    {
        ManifestUrl = "https://raid.example.com/updates/stable/manifest.json",
        GameExecutable = "King's Raid.exe", AutoLaunch = false,
        ManifestPublicKeyPem = new { value = pem, PSPath = "D:/operator/public.pem", ReadCount = 1 }
    }));
    var config = LauncherConfig.Load(path);
    Require(config.ManifestPublicKeyPem == pem && !config.AutoLaunch && config.ManifestUrl.StartsWith("https://raid.example.com"), "Configuration preferences changed during repair");
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    Require(document.RootElement.GetProperty("ManifestPublicKeyPem").ValueKind == JsonValueKind.String, "PEM was not persisted as a plain string");
    Require(LauncherConfig.Load(path).ManifestPublicKeyPem == pem, "Normal configurations stopped loading");
    return Task.CompletedTask;
}

static Task Locking()
{
    using var fixture = new Fixture();
    using var first = fixture.Engine.AcquireLock();
    Reject(() => { using var second = fixture.Engine.AcquireLock(); return Task.CompletedTask; }).GetAwaiter().GetResult();
    return Task.CompletedTask;
}

static async Task ProfileConfiguration()
{
    using var fixture = new Fixture();
    var config = new LauncherConfig { ManifestPublicKeyPem = "shared key" };
    config.EnsureProfiles();
    Require(config.SelectedProfile == "local", "Legacy local installation changed servers");
    Require(config.ForProfile().ManifestUrl.Contains("127.0.0.1:8081"), "Local update URL lost");
    config.SelectedProfile = "public";
    Require(config.ForProfile().ManifestPublicKeyPem == "shared key", "Pinned key not inherited");
    config.CurrentProfile.ManifestPublicKeyPem = "public key";
    config.Save(fixture.Path("sprk-launcher.json"));
    config = LauncherConfig.Load(fixture.Path("sprk-launcher.json"));
    Require(config.SelectedProfile == "public" && config.ForProfile().ManifestPublicKeyPem == "public key", "Profile selection/key not saved");
    Require(config.CurrentProfile.HostUrl == "https://play.krinfo.net/host.json", "Game bootstrap URL lost");
    foreach (var url in new[] { "http://public.example/host.json", "https://user:secret@public.example/host.json", "file:///tmp/host.json" })
    {
        config.CurrentProfile.HostUrl = url;
        await Reject(() => { config.ForProfile(); return Task.CompletedTask; });
    }
    var custom = new LauncherConfig { ManifestUrl = "https://custom.example/stable/manifest.json" };
    custom.EnsureProfiles();
    Require(custom.CurrentProfile.ManifestUrl == custom.ManifestUrl, "Custom manifest was replaced");
    Require(custom.ForProfile(requireHost: false).ManifestUrl == custom.ManifestUrl, "Legacy headless update requires a game URL");
    await Reject(() => { custom.ForProfile(); return Task.CompletedTask; });
    config.SelectedProfile = "unknown";
    await Reject(() => { config.ForProfile(); return Task.CompletedTask; });
}

static async Task ProfileUpdates()
{
    using var fixture = new Fixture();
    var config = new LauncherConfig(); config.EnsureProfiles();
    // Two localhost fixtures stand in for independently signed public/local servers.
    config.Profiles[1].HostUrl = "http://localhost:8090/host.json";
    config.Profiles[1].ManifestUrl = "http://localhost:8091/updates/stable/manifest.json";
    foreach (var profile in new[] { "local", "public", "local" })
    {
        config.SelectedProfile = profile;
        fixture.Server.Files["Managed/shared.dll"] = Encoding.UTF8.GetBytes(profile);
        fixture.Server.Refresh();
        fixture.Server.Requests.Clear();
        var engine = new UpdateEngine(fixture.Root, config.ForProfile(), fixture.Http);
        using var updateLock = engine.AcquireLock();
        await engine.UpdateAsync();
        Require(fixture.Read("Managed/shared.dll") == profile, "Same version on another server skipped hash checks");
        Require(fixture.Server.Requests[0] == config.CurrentProfile.ManifestUrl, "Wrong server checked");
        Require(fixture.Server.Requests.All(url => new Uri(url).Authority == new Uri(config.CurrentProfile.ManifestUrl).Authority), "Downloads crossed server origins");
    }
}

sealed class Fixture : IDisposable
{
    public string Root { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sprk-launcher-tests-" + Guid.NewGuid());
    public Server Server { get; } = new();
    public HttpClient Http { get; }
    public UpdateEngine Engine { get; }
    public Fixture() { Directory.CreateDirectory(Root); Http = new HttpClient(Server); Engine = new(Root, new(), Http); }
    public string Path(string relative) => System.IO.Path.Combine(Root, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
    public void Write(string path, string content) { Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path(path))!); File.WriteAllText(Path(path), content); }
    public string Read(string path) => File.ReadAllText(Path(path));
    public void Dispose() { Http.Dispose(); Directory.Delete(Root, true); }
}

sealed class Server : HttpMessageHandler
{
    public Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);
    public UpdateManifest Manifest { get; } = new() { SchemaVersion = 1, Version = "1" };
    public int Downloads;
    public string? CorruptPath;
    public List<long> Ranges { get; } = [];
    public List<string> Requests { get; } = [];
    public void Refresh() => Manifest.Files = Files.Select(pair => new UpdateFile
    {
        Path = pair.Key, Size = pair.Value.Length, Sha256 = Convert.ToHexString(SHA256.HashData(pair.Value)).ToLowerInvariant(),
        Url = "/updates/releases/1/files/" + pair.Key
    }).ToList();
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request.RequestUri!.AbsoluteUri);
        if (request.RequestUri!.AbsolutePath.EndsWith("manifest.json"))
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(Manifest)) });
        Downloads++;
        var path = Uri.UnescapeDataString(request.RequestUri.AbsolutePath.Split("/files/")[1]);
        var original = Files[path];
        var data = path == CorruptPath ? Enumerable.Repeat((byte)'?', original.Length).ToArray() : original;
        var from = request.Headers.Range?.Ranges.First().From ?? 0;
        Ranges.Add(from);
        var response = new HttpResponseMessage(from > 0 ? HttpStatusCode.PartialContent : HttpStatusCode.OK)
        { Content = new ByteArrayContent(data[(int)from..]) };
        if (from > 0) response.Content.Headers.ContentRange = new ContentRangeHeaderValue(from, data.Length - 1, data.Length);
        return Task.FromResult(response);
    }
}
