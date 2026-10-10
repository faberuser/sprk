using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SprkLauncher.Core;

public sealed class LauncherConfig
{
    public string ManifestUrl { get; set; } = "http://127.0.0.1:8081/updates/stable/manifest.json";
    public string GameExecutable { get; set; } = "King's Raid.exe";
    public string ManifestPublicKeyPem { get; set; } = "";
    public bool AutoLaunch { get; set; } = true;
    public string SelectedProfile { get; set; } = "local";
    public List<ServerProfile> Profiles { get; set; } = [];

    public void EnsureProfiles()
    {
        if (Profiles.Count != 0) return;
        Profiles = [
            new() { Id = "local", Name = "Local", HostUrl = "http://127.0.0.1:8080/host.json", ManifestUrl = "http://127.0.0.1:8081/updates/stable/manifest.json" },
            new() { Id = "public", Name = "Public", HostUrl = "https://play.krinfo.net/host.json", ManifestUrl = "https://updates.krinfo.net/updates/stable/manifest.json" }
        ];
        var match = Profiles.FirstOrDefault(p => p.ManifestUrl == ManifestUrl);
        if (match == null)
        {
            // Preserve custom update endpoints; the game host must be configured
            // explicitly rather than guessed from a possibly separate CDN host.
            match = new() { Id = "custom", Name = "Existing server", ManifestUrl = ManifestUrl, HostUrl = "" };
            Profiles.Add(match);
        }
        SelectedProfile = match.Id;
    }

    [JsonIgnore] public ServerProfile CurrentProfile => Profiles.SingleOrDefault(p => p.Id == SelectedProfile)
        ?? throw new InvalidDataException("Select a valid server profile.");

    public LauncherConfig ForProfile(bool requireHost = true)
    {
        var profile = CurrentProfile;
        profile.Validate(requireHost);
        return new() { ManifestUrl = profile.ManifestUrl, GameExecutable = GameExecutable,
            ManifestPublicKeyPem = profile.ManifestPublicKeyPem ?? ManifestPublicKeyPem, AutoLaunch = AutoLaunch };
    }

    public void Save(string path)
    {
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        File.Move(temporary, path, true);
    }

    public static LauncherConfig Load(string path)
    {
        var document = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidDataException("sprk-launcher.json must contain a configuration object.");
        var repaired = false;
        // Repair configurations produced by Windows PowerShell 5.1 Get-Content
        // serialization without changing the URL or other user preferences.
        if (document[nameof(ManifestPublicKeyPem)] is JsonObject wrapper
            && wrapper["value"] is JsonValue value && value.TryGetValue<string>(out var pem))
        {
            document[nameof(ManifestPublicKeyPem)] = pem;
            repaired = true;
        }
        var config = document.Deserialize<LauncherConfig>()
            ?? throw new InvalidDataException("Empty sprk-launcher.json.");
        if (repaired)
        {
            var temporary = path + ".tmp";
            File.WriteAllText(temporary, document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
            File.Move(temporary, path, true);
        }
        return config;
    }
}

public sealed class ServerProfile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string HostUrl { get; set; } = "";
    public string ManifestUrl { get; set; } = "";
    public string? ManifestPublicKeyPem { get; set; }
    public override string ToString() => Name;
    public void Validate(bool requireHost = true)
    {
        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(Name))
            throw new InvalidDataException("Server profiles need an ID and name.");
        foreach (var value in requireHost ? new[] { HostUrl, ManifestUrl } : new[] { ManifestUrl })
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                !(uri.Scheme == "https" || uri.Scheme == "http" && uri.IsLoopback) ||
                uri.UserInfo.Length != 0 || uri.Fragment.Length != 0)
                throw new InvalidDataException("Configure the profile's game and update URLs: HTTPS, or HTTP on localhost.");
    }
}

public sealed class UpdateManifest
{
    [JsonPropertyName("schema_version")] public int SchemaVersion { get; set; }
    [JsonPropertyName("version")] public string Version { get; set; } = "";
    [JsonPropertyName("files")] public List<UpdateFile> Files { get; set; } = [];
    [JsonPropertyName("deleted_files")] public List<string> DeletedFiles { get; set; } = [];
}

public sealed class UpdateFile
{
    [JsonPropertyName("path")] public string Path { get; set; } = "";
    [JsonPropertyName("size")] public long Size { get; set; }
    [JsonPropertyName("sha256")] public string Sha256 { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
}

public sealed record UpdateProgress(string Message, int Percent = -1, long Downloaded = 0, long Total = 0);
public sealed record UpdateResult(string Version, int DownloadedFiles, long DownloadedBytes);

public sealed class Transaction
{
    public string Version { get; set; } = "";
    public bool Committed { get; set; }
    public List<TransactionEntry> Entries { get; set; } = [];
}

public sealed class TransactionEntry
{
    public string Path { get; set; } = "";
    public bool HadOriginal { get; set; }
    public bool Delete { get; set; }
}
