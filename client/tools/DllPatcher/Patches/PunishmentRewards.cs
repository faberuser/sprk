using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;

partial class Program
{
    // Read the same tracked material curves as the server. Assign absolute values
    // so repeated focused installs and full release builds cannot stack the buff.
    static void InstallPunishmentRewards(string clientRoot, bool verify)
    {
        clientRoot = Path.GetFullPath(clientRoot);
        if (!verify)
            foreach (var process in System.Diagnostics.Process.GetProcesses())
                using (process)
                    if ((process.ProcessName == "King's Raid" || process.ProcessName.StartsWith("SprkTest-"))
                        && process.MainModule?.FileName is string executable
                        && executable.StartsWith(clientRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Close game clients before installing punishment reward tables.");

        using var source = JsonDocument.Parse(typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.DivinePunishmentSupport.json")!);
        var tables = source.RootElement.GetProperty("Tables");
        var materials = new HashSet<int> { 45122, 45128, 45129, 45131 };
        var ids = tables.GetProperty("InfinityReward").EnumerateArray()
            .Where(r => materials.Contains(r.GetProperty("ItemIndex").GetInt32()))
            .Select(r => r.GetProperty("InfinityIndex").GetInt32()).ToHashSet();
        var expected = tables.GetProperty("Infinity").EnumerateArray()
            .Where(r => ids.Contains(r.GetProperty("Index").GetInt32()))
            .ToDictionary(r => r.GetProperty("Index").GetInt32());
        if (ids.Count != 18 || expected.Count != ids.Count)
            throw new InvalidDataException("Unexpected Divine Punishment material curve set.");

        var root = Path.Combine(clientRoot, "King's Raid_Data/Documents/Patch/StandaloneWindows");
        const string relative = "TableJit/InfinityTable.jit";
        var path = Path.Combine(root, relative);
        var metadataPath = Path.Combine(root, NativeStaticData.ManifestName);
        var original = File.ReadAllBytes(path);
        var metadataOriginal = File.ReadAllBytes(metadataPath);
        var metadata = JsonNode.Parse(metadataOriginal)!;
        static string Hash(byte[] data) => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
        var entry = metadata["files"]![relative];
        if (entry != null && entry["sha256"]!.GetValue<string>() != Hash(original))
            throw new InvalidDataException("Infinity table integrity mismatch before reward update.");

        var jit = new RuneTicketJit(original);
        var found = new HashSet<int>();
        var changed = 0;
        for (var i = 0; i < jit.Payloads.Count - 1; i++)
        {
            var payload = jit.Payloads[i];
            if (payload.Length == 0 || payload[0] != 0x94) continue;
            var fields = RuneTicketJit.Array(payload);
            var id = checked((int)RuneTicketJit.Integer(fields[0]));
            if (!expected.TryGetValue(id, out var curve)) continue;
            if (!found.Add(id)) throw new InvalidDataException("Duplicate punishment reward curve.");
            if (RuneTicketJit.Integer(fields[1]) != curve.GetProperty("StartValue").GetInt64()
                || !RuneTicketJit.Array(fields[2]).Select(RuneTicketJit.Integer)
                    .SequenceEqual(curve.GetProperty("IncreaseValues").EnumerateArray().Select(v => v.GetInt64())))
                throw new InvalidDataException("Unexpected punishment reward curve shape: " + id);
            var multiplier = curve.GetProperty("Multiplier").GetInt32();
            if (multiplier < 1 || multiplier > 127) throw new InvalidDataException("Unsupported reward multiplier.");
            if (RuneTicketJit.Integer(fields[3]) == multiplier) continue;
            if (verify) throw new InvalidDataException("Punishment reward preview needs updating: " + id);
            fields[3] = new[] { (byte)multiplier };
            jit.ReplaceRow(i, fields);
            changed++;
        }
        if (!found.SetEquals(ids)) throw new InvalidDataException("Missing punishment reward curves.");
        if (verify)
        {
            if (entry == null || !NativeStaticData.ValidateInstalled(Path.Combine(clientRoot, "King's Raid_Data/Managed")))
                throw new InvalidDataException("Missing native reward integrity metadata.");
            Console.WriteLine("PASS: all 18 punishment material preview curves match the server; native hashes match.");
            return;
        }
        if (changed > 0 || entry == null)
        {
            var output = changed == 0 ? original : jit.Encoded();
            if (entry == null) metadata["files"]![relative] = entry = new JsonObject { ["originalSha256"] = Hash(original) };
            entry["sha256"] = Hash(output);
            var backup = Path.GetFullPath(Path.Combine("target/punishment-reward-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")));
            Directory.CreateDirectory(backup);
            File.WriteAllBytes(Path.Combine(backup, "InfinityTable.jit"), original);
            File.WriteAllBytes(Path.Combine(backup, NativeStaticData.ManifestName), metadataOriginal);
            File.WriteAllBytes(path + ".staged", output);
            File.WriteAllText(metadataPath + ".staged", metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
            File.Move(path + ".staged", path, true);
            File.Move(metadataPath + ".staged", metadataPath, true);
        }
        InstallPunishmentRewards(clientRoot, true);
    }
}
