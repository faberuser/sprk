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
