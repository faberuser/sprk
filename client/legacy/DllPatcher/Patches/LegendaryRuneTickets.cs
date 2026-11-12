using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;

partial class Program
{
    static void RestoreLegendaryRuneTickets(string clientRoot, bool verify)
    {
        clientRoot = Path.GetFullPath(clientRoot);
        if (!verify)
            foreach (var process in System.Diagnostics.Process.GetProcesses())
                using (process)
                    if ((process.ProcessName == "King's Raid" || process.ProcessName.StartsWith("SprkTest-"))
                        && process.MainModule?.FileName is string executable
                        && executable.StartsWith(clientRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Close game clients before installing rune ticket tables.");

        var root = Path.Combine(clientRoot, "King's Raid_Data/Documents/Patch/StandaloneWindows");
        var metadataPath = Path.Combine(root, "SPRK-NativeData.json");
        var originals = new Dictionary<string, byte[]> { [metadataPath] = File.ReadAllBytes(metadataPath) };
        var metadata = JsonNode.Parse(originals[metadataPath])!;
        var changes = new Dictionary<string, byte[]>();
        static string Hash(byte[] data) => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();

        foreach (var (name, oldCode, newCode, fields) in new[] {
            ("SelectItem", "PACKAGE_RUNE_6_SELECTION", "PACKAGE_RUNE_6_SELECTION_2022", new[] { 1 }),
            ("ItemGroup", "PACKAGE_RUNE_5_GROUP", "PACKAGE_RUNE_5_2022_GROUP", new[] { 4, 5 }) })
        {
            var relative = $"TableJit/{name}Table.jit";
            var path = Path.Combine(root, relative);
            var original = originals[path] = File.ReadAllBytes(path);
            var jit = new RuneTicketJit(original);
            var (index, target) = jit.Row(oldCode);
            var (_, source) = jit.Row(newCode);
            if (name == "SelectItem")
            {
                var pool = RuneTicketJit.Array(source[1]).Select(RuneTicketJit.Integer).ToArray();
                if (pool.Length != 24 || !new long[] { 100115, 100136, 100137, 100138 }.All(pool.Contains))
                    throw new InvalidOperationException("Unexpected 2022 legendary selector pool.");
            }
            else
            {
                var rates = RuneTicketJit.Array(source[5]).Select(RuneTicketJit.Array)
                    .Where(row => RuneTicketJit.Integer(row[1]) > 0)
                    .ToDictionary(row => jit.Strings[checked((int)RuneTicketJit.Integer(row[0]))], row => RuneTicketJit.Integer(row[1]));
                if (rates.Count != 24 || RuneTicketJit.Integer(source[4]) != 2400 || rates.Values.Sum() != 2400
                    || new[] { "RUNE_SWIFTNESS_5", "RUNE_LIFE_5", "RUNE_TENACITY_5", "RUNE_VITALITY_5" }
                        .Any(code => !rates.TryGetValue(code, out var rate) || rate != 100))
                    throw new InvalidOperationException("Unexpected 2022 legendary random pool.");
            }
            var needsUpdate = fields.Any(field => !target[field].SequenceEqual(source[field]));
            if (verify && needsUpdate)
                throw new InvalidOperationException($"Legacy {name} pool has not been upgraded.");
            foreach (var field in fields) target[field] = source[field];
            if (needsUpdate) jit.ReplaceRow(index, target);
            var output = needsUpdate ? jit.Encoded() : original;
            var entry = metadata["files"]![relative];
            if (verify)
            {
                if (entry?["sha256"]?.GetValue<string>() != Hash(original))
                    throw new InvalidOperationException($"Native data integrity mismatch: {relative}");
            }
            else
            {
                if (entry == null)
                    metadata["files"]![relative] = entry = new JsonObject { ["originalSha256"] = Hash(original) };
                entry["sha256"] = Hash(output);
                changes[path] = output;
            }
        }
        if (!verify)
        {
            // Validate both tables before writing either, preserving existing original hashes.
            changes[metadataPath] = Encoding.UTF8.GetBytes(metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
            var changed = changes.Where(pair => !originals[pair.Key].SequenceEqual(pair.Value)).ToArray();
            var backup = Path.GetFullPath(Path.Combine("target/legendary-rune-ticket-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")));
            foreach (var (path, _) in changed)
            {
                if (!File.ReadAllBytes(path).SequenceEqual(originals[path]))
                    throw new InvalidOperationException("Rune ticket files changed during preparation.");
                var destination = Path.Combine(backup, Path.GetRelativePath(root, path));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.WriteAllBytes(destination, originals[path]);
            }
            foreach (var (path, bytes) in changed)
            {
                File.WriteAllBytes(path + ".staged", bytes);
                File.Move(path + ".staged", path, true);
            }
            RestoreLegendaryRuneTickets(clientRoot, true);
        }
        else Console.WriteLine("PASS: legacy random/select tickets use all 24 archived legendary runes; native hashes match.");
    }
}
