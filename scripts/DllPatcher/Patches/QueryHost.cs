using System.Text;

namespace DllPatcher;
partial class Program {
    // QueryHost in resources.assets is the host-list URL the client requests when
    // the launcher does not pass SPRK_HOST_URL (ResolveHostUrl falls back to it).
    // It is a fixed-length string inside a serialized TextAsset, so it is replaced
    // in place and padded with spaces; the asset layout never changes.
    const string QueryHostUrl = "http://127.0.0.1:8080/sprk/live/1/host_1.0_304dbdffe094.json";
    const int QueryHostLength = 87;
    static readonly byte[] QueryHostField = Encoding.ASCII.GetBytes("\"QueryHost\" : \"");

    static string QueryHostAsset(string root) =>
        Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "resources.assets"));

    /// Offset of the single QueryHost value and its current (unpadded) URL.
    static (int Offset, string Url) FindQueryHost(byte[] data) {
        var span = data.AsSpan();
        int first = span.IndexOf(QueryHostField);
        if (first < 0 || span[(first + 1)..].IndexOf(QueryHostField) >= 0)
            throw new InvalidOperationException("resources.assets must contain exactly one QueryHost setting.");
        int offset = first + QueryHostField.Length;
        if (offset + QueryHostLength >= data.Length || data[offset + QueryHostLength] != (byte)'"')
            throw new InvalidOperationException($"QueryHost value is not the expected {QueryHostLength}-byte string.");
        var value = Encoding.ASCII.GetString(data, offset, QueryHostLength);
        var url = value.TrimEnd(' ');
        if (url.Length == 0 || url.Contains(' ') || url.Contains('"'))
            throw new InvalidOperationException("QueryHost value is malformed: " + value);
        return (offset, url);
    }

    static void InstallQueryHost(string root, bool stageOnly) {
        var asset = QueryHostAsset(root);
        var data = File.ReadAllBytes(asset);
        var (offset, url) = FindQueryHost(data);
        if (url == QueryHostUrl) { VerifyQueryHostFile(root); return; }
        if (!Uri.TryCreate(url, UriKind.Absolute, out var current) || current.Scheme is not ("http" or "https"))
            throw new InvalidOperationException("QueryHost is not an http(s) URL; refusing to replace: " + url);
        Encoding.ASCII.GetBytes(QueryHostUrl.PadRight(QueryHostLength)).CopyTo(data, offset);
        var staged = asset + ".query-host-staged";
        File.WriteAllBytes(staged, data);
        VerifyQueryHostFile(root, true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("QueryHost staged. Close game clients before installation.");
        File.Copy(asset, asset + ".before-query-host-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged, asset, true);
        VerifyQueryHostFile(root);
    }

    static void VerifyQueryHostFile(string root, bool staged = false) {
        var asset = QueryHostAsset(root) + (staged ? ".query-host-staged" : "");
        var data = File.ReadAllBytes(asset);
        var (_, url) = FindQueryHost(data);
        if (url != QueryHostUrl)
            throw new InvalidOperationException($"QueryHost must be {QueryHostUrl}, found {url}.");
        Console.WriteLine("Verified QueryHost: " + url);
    }
}
