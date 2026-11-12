using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SprkLauncher.Core;

public sealed class UpdateEngine
{
    const int ManifestLimit = 16 * 1024 * 1024;
    readonly string root;
    readonly string work;
    readonly LauncherConfig config;
    readonly HttpClient http;
    readonly Action ensureGameClosed;
    readonly IProgress<UpdateProgress>? progress;
    readonly Uri manifestUri;
    readonly JsonSerializerOptions json = new() { WriteIndented = true };

    public UpdateEngine(string clientRoot, LauncherConfig config, HttpClient http,
        Action? ensureGameClosed = null, IProgress<UpdateProgress>? progress = null)
    {
        root = System.IO.Path.GetFullPath(clientRoot);
        work = System.IO.Path.Combine(root, ".sprk-launcher");
        this.config = config;
        this.http = http;
        this.ensureGameClosed = ensureGameClosed ?? (() => { });
        this.progress = progress;
        manifestUri = new Uri(config.ManifestUrl, UriKind.Absolute);
        if (manifestUri.Scheme != "https" && !(manifestUri.Scheme == "http" && manifestUri.IsLoopback))
            throw new InvalidDataException("Use an HTTPS update URL. HTTP is supported for localhost testing.");
        if (!manifestUri.IsLoopback && string.IsNullOrWhiteSpace(config.ManifestPublicKeyPem))
            throw new InvalidDataException("Configure ManifestPublicKeyPem before connecting to your hosted update server.");
        LocalPath(config.GameExecutable);
        CheckLinks(root);
        CheckLinks(work);
        Directory.CreateDirectory(work);
    }

    // Keep this handle for the entire update AND game-start operation.
    public FileStream AcquireLock()
    {
        CheckLinks(work);
        var path = System.IO.Path.Combine(work, "launcher.lock");
        CheckLinks(path);
        try
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException error)
        {
            throw new IOException("Another launcher is using this client. Close it before retrying.", error);
        }
    }

    public string GamePath => LocalPath(config.GameExecutable);
    public string InstalledVersion
    {
        get
        {
            var path = System.IO.Path.Combine(work, "installed.json");
            if (!File.Exists(path)) return "Not checked yet";
            CheckLinks(path);
            return JsonSerializer.Deserialize<UpdateResult>(File.ReadAllText(path))?.Version ?? "Unknown";
        }
    }

    public async Task<UpdateResult> UpdateAsync(CancellationToken token = default)
    {
        ensureGameClosed();
        Recover();
        Report("Checking for updates…");
        using var response = await http.GetAsync(manifestUri, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();
        var bytes = await ReadLimitedAsync(response.Content, ManifestLimit, token);
        var manifest = ParseManifest(bytes);
        ValidateManifest(manifest);
        var changed = new List<UpdateFile>();
        for (var i = 0; i < manifest.Files.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var file = manifest.Files[i];
            Report($"Checking files {i + 1}/{manifest.Files.Count}", (i + 1) * 100 / manifest.Files.Count);
            if (!await MatchesAsync(LocalPath(file.Path), file, token)) changed.Add(file);
        }
        var removals = manifest.DeletedFiles.Where(path => File.Exists(LocalPath(path))).ToList();
        long total = 0;
        foreach (var file in changed) total = checked(total + file.Size);
        long completed = 0;
        foreach (var file in changed)
        {
            await DownloadAsync(file, completed, total, token);
            completed += file.Size;
        }
        token.ThrowIfCancellationRequested();
        ensureGameClosed();
        var result = new UpdateResult(manifest.Version, changed.Count, total);
        if (changed.Count > 0 || removals.Count > 0)
        {
            Report("Installing update…");
            Apply(manifest.Version, changed, removals);
        }
        WriteJson(System.IO.Path.Combine(work, "installed.json"), result);
        // Verified cache entries are no longer needed after a complete install.
        foreach (var file in changed) File.Delete(CachePath(file));
        Report($"Ready · version {manifest.Version}", 100, total, total);
        return result;
    }

    public UpdateManifest ParseManifest(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes);
        if (!string.IsNullOrWhiteSpace(config.ManifestPublicKeyPem))
        {
            if (!document.RootElement.TryGetProperty("signature", out var signature)
                || signature.GetProperty("algorithm").GetString() != "RSA-SHA256")
                throw new InvalidDataException("The update manifest is not signed by your server.");
            var payload = Convert.FromBase64String(signature.GetProperty("payload").GetString()!);
            if (payload.Length > ManifestLimit) throw new InvalidDataException("Manifest is too large.");
            var value = Convert.FromBase64String(signature.GetProperty("value").GetString()!);
            using var rsa = RSA.Create();
            rsa.ImportFromPem(config.ManifestPublicKeyPem);
            if (rsa.KeySize < 2048 || !rsa.VerifyData(payload, value, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                throw new InvalidDataException("The update manifest signature is invalid.");
            // All installation decisions use the authenticated payload, never
            // the unsigned outer convenience fields.
            bytes = payload;
        }
        return JsonSerializer.Deserialize<UpdateManifest>(bytes)
            ?? throw new InvalidDataException("Empty update manifest.");
    }

    void ValidateManifest(UpdateManifest manifest)
    {
        if (manifest.SchemaVersion != 1 || !Regex.IsMatch(manifest.Version, @"\A[A-Za-z0-9][A-Za-z0-9_.-]{0,127}\z"))
            throw new InvalidDataException("Unsupported update manifest version.");
        if (manifest.Files is null || manifest.DeletedFiles is null || manifest.Files.Count == 0
            || manifest.Files.Count + manifest.DeletedFiles.Count > 100000)
            throw new InvalidDataException("Invalid update file list.");
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in manifest.Files)
        {
            if (file is null || file.Size < 0 || !Regex.IsMatch(file.Sha256 ?? "", @"\A[a-fA-F0-9]{64}\z"))
                throw new InvalidDataException("Invalid update file size or SHA-256 hash.");
            LocalPath(file.Path);
            if (!paths.Add(file.Path)) throw new InvalidDataException("Duplicate update path.");
            DownloadUri(file.Url);
        }
        foreach (var path in manifest.DeletedFiles)
        {
            LocalPath(path);
            if (!paths.Add(path)) throw new InvalidDataException("Conflicting update/deletion paths.");
        }
        foreach (var path in paths)
        {
            var parent = path.LastIndexOf('/');
            while (parent > 0)
            {
                if (paths.Contains(path[..parent])) throw new InvalidDataException("Conflicting file/directory paths.");
                parent = path.LastIndexOf('/', parent - 1);
            }
        }
    }

    public string LocalPath(string relative)
    {
        if (string.IsNullOrEmpty(relative) || relative.Contains('\\')) throw new InvalidDataException("Invalid client file path.");
        foreach (var part in relative.Split('/'))
        {
            var stem = part.Split('.')[0];
            if (part.Length == 0 || part.StartsWith('.') || part.EndsWith('.') || part.EndsWith(' ')
                || part.Any(c => char.IsControl(c) || "\\:<>\"|?*".Contains(c))
                || Regex.IsMatch(stem, @"\A(CON|PRN|AUX|NUL|COM[1-9¹²³]|LPT[1-9¹²³])\z", RegexOptions.IgnoreCase))
                throw new InvalidDataException($"Unsafe client file path: {relative}");
        }
        if (relative.Equals("SprkLauncher.exe", StringComparison.OrdinalIgnoreCase)
            || relative.Equals("sprk-launcher.json", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Game updates cannot replace the running launcher or its configuration.");
        var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(root, relative.Replace('/', System.IO.Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root.TrimEnd(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Update path escapes the client directory.");
        CheckLinks(path);
        if (Directory.Exists(path)) throw new InvalidDataException($"A directory occupies the update file path: {relative}");
        return path;
    }

    void CheckLinks(string path)
    {
        for (var current = path; current is not null; current = System.IO.Path.GetDirectoryName(current))
        {
            // File.Exists follows links and can hide a dangling link. Read the
            // entry's attributes directly before creating or replacing a file.
            try
            {
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException($"Symlinks/junctions are not supported in the client installation: {current}");
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            if (current.Equals(root, StringComparison.OrdinalIgnoreCase)) break;
        }
    }

    Uri DownloadUri(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("/updates/releases/", StringComparison.Ordinal)
            || path.StartsWith("//") || path.Contains('\\') || path.Contains('?') || path.Contains('#'))
            throw new InvalidDataException("Invalid release download URL.");
        var uri = new Uri(manifestUri, path);
        if (uri.GetLeftPart(UriPartial.Authority) != manifestUri.GetLeftPart(UriPartial.Authority)
            || !uri.AbsolutePath.StartsWith("/updates/releases/", StringComparison.Ordinal))
            throw new InvalidDataException("Downloads must come from the configured update server.");
        return uri;
    }

    string CachePath(UpdateFile file)
    {
        var cache = System.IO.Path.Combine(work, "downloads");
        CheckLinks(cache);
        Directory.CreateDirectory(cache);
        var path = System.IO.Path.Combine(cache, file.Sha256.ToLowerInvariant() + ".bin");
        CheckLinks(path);
        return path;
    }

    static async Task<bool> MatchesAsync(string path, UpdateFile file, CancellationToken token)
    {
        if (!File.Exists(path) || new FileInfo(path).Length != file.Size) return false;
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, true);
        var hash = await SHA256.HashDataAsync(input, token);
        return Convert.ToHexString(hash).Equals(file.Sha256, StringComparison.OrdinalIgnoreCase);
    }

    async Task DownloadAsync(UpdateFile file, long completed, long total, CancellationToken token)
    {
        var cached = CachePath(file);
        if (await MatchesAsync(cached, file, token)) return;
        File.Delete(cached);
        var partial = cached + ".part";
        CheckLinks(partial);
        // Partial files are addressed by their expected hash, so they can be
        // resumed across launches, even if the channel has moved on.
        for (var attempt = 0; attempt < 3; attempt++)
        {
            token.ThrowIfCancellationRequested();
            var offset = File.Exists(partial) ? new FileInfo(partial).Length : 0;
            if (offset > file.Size) { File.Delete(partial); offset = 0; }
            if (offset != file.Size || !File.Exists(partial))
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, DownloadUri(file.Url));
                    if (offset > 0) request.Headers.Range = new RangeHeaderValue(offset, null);
                    using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                    if (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable && offset > 0)
                    { File.Delete(partial); continue; }
                    response.EnsureSuccessStatusCode();
                    if (response.StatusCode == HttpStatusCode.PartialContent)
                    {
                        var range = response.Content.Headers.ContentRange;
                        if (range?.From != offset || range.Length != file.Size || range.To != file.Size - 1)
                            throw new InvalidDataException("Invalid resumed download range.");
                    }
                    else if (response.StatusCode == HttpStatusCode.OK) offset = 0;
                    else throw new InvalidDataException("Unexpected download response.");
                    await using var output = new FileStream(partial, offset == 0 ? FileMode.Create : FileMode.Append,
                        FileAccess.Write, FileShare.None, 128 * 1024, true);
                    await using var input = await response.Content.ReadAsStreamAsync(token);
                    var buffer = new byte[128 * 1024];
                    int count;
                    while ((count = await input.ReadAsync(buffer, token)) > 0)
                    {
                        if (offset + count > file.Size) throw new InvalidDataException("Download exceeds its advertised size.");
                        await output.WriteAsync(buffer.AsMemory(0, count), token);
                        offset += count;
                        Report($"Downloading {System.IO.Path.GetFileName(file.Path)}", total == 0 ? 100 : (int)((completed + offset) * 100.0 / total), completed + offset, total);
                    }
                    await output.FlushAsync(token);
                    output.Flush(true);
                }
                catch (Exception error) when ((error is HttpRequestException || error is IOException) && attempt < 2)
                { await Task.Delay(TimeSpan.FromSeconds(attempt + 1), token); continue; }
            }
            if (await MatchesAsync(partial, file, token))
            { File.Move(partial, cached, true); return; }
            File.Delete(partial);
            if (attempt == 2) throw new InvalidDataException($"Hash verification failed for {file.Path}.");
        }
        throw new IOException($"Unable to download {file.Path}. Retry to resume the update.");
    }

    void Apply(string version, List<UpdateFile> files, List<string> removals)
    {
        var backup = System.IO.Path.Combine(work, "backup");
        var incoming = System.IO.Path.Combine(work, "incoming");
        CheckLinks(backup); CheckLinks(incoming);
        var transaction = new Transaction { Version = version };
        foreach (var file in files)
        {
            var target = LocalPath(file.Path);
            var staged = InsideWork(incoming, file.Path);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(staged)!);
            File.Copy(CachePath(file), staged, true);
            transaction.Entries.Add(new() { Path = file.Path, HadOriginal = File.Exists(target) });
        }
        foreach (var path in removals)
            transaction.Entries.Add(new() { Path = path, HadOriginal = File.Exists(LocalPath(path)), Delete = true });
        WriteJson(System.IO.Path.Combine(work, "transaction.json"), transaction);
        try
        {
            foreach (var entry in transaction.Entries)
            {
                var target = LocalPath(entry.Path);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target)!);
                if (entry.HadOriginal)
                {
                    var saved = InsideWork(backup, entry.Path);
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(saved)!);
                    File.Move(target, saved);
                }
                if (!entry.Delete) File.Move(InsideWork(incoming, entry.Path), target);
            }
            transaction.Committed = true;
            WriteJson(System.IO.Path.Combine(work, "transaction.json"), transaction);
        }
        catch
        {
            Recover();
            throw;
        }
        Recover();
    }

    public void Recover()
    {
        var path = System.IO.Path.Combine(work, "transaction.json");
        CheckLinks(path);
        if (!File.Exists(path)) return;
        ensureGameClosed();
        Report("Recovering interrupted update…");
        var transaction = JsonSerializer.Deserialize<Transaction>(File.ReadAllText(path))
            ?? throw new InvalidDataException("Invalid installation recovery journal.");
        foreach (var entry in transaction.Entries)
        {
            var target = LocalPath(entry.Path);
            var saved = InsideWork(System.IO.Path.Combine(work, "backup"), entry.Path);
            if (transaction.Committed) continue;
            if (File.Exists(saved))
            {
                File.Delete(target);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target)!);
                File.Move(saved, target);
            }
            else if (!entry.HadOriginal) File.Delete(target);
        }
        if (transaction.Committed)
            WriteJson(System.IO.Path.Combine(work, "installed.json"), new UpdateResult(transaction.Version, 0, 0));
        // Remove the journal last, so recovery can be retried after a crash.
        DeleteWorkDirectory("incoming");
        DeleteWorkDirectory("backup");
        File.Delete(path);
    }

    string InsideWork(string directory, string relative)
    {
        LocalPath(relative); // Validate client-relative components too.
        var path = System.IO.Path.Combine(directory, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
        CheckLinks(path);
        return path;
    }

    void DeleteWorkDirectory(string name)
    {
        var directory = System.IO.Path.Combine(work, name);
        CheckLinks(directory);
        if (!Directory.Exists(directory)) return;
        foreach (var path in Directory.EnumerateFileSystemEntries(directory, "*", SearchOption.AllDirectories)) CheckLinks(path);
        Directory.Delete(directory, true);
    }

    void WriteJson<T>(string path, T value)
    {
        CheckLinks(path); CheckLinks(path + ".tmp");
        using (var output = new FileStream(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(output, value, json);
            output.Flush(true);
        }
        File.Move(path + ".tmp", path, true);
    }

    void Report(string message, int percent = -1, long downloaded = 0, long total = 0)
        => progress?.Report(new(message, percent, downloaded, total));

    static async Task<byte[]> ReadLimitedAsync(HttpContent content, int limit, CancellationToken token)
    {
        if (content.Headers.ContentLength > limit) throw new InvalidDataException("Manifest is too large.");
        await using var input = await content.ReadAsStreamAsync(token);
        using var output = new MemoryStream();
        var buffer = new byte[64 * 1024];
        int count;
        while ((count = await input.ReadAsync(buffer, token)) > 0)
        {
            if (output.Length + count > limit) throw new InvalidDataException("Manifest is too large.");
            output.Write(buffer, 0, count);
        }
        return output.ToArray();
    }
}
