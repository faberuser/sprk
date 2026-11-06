using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;
partial class Program
{
    static (int Index, List<byte[]> Fields) NumericJitRow(RuneTicketJit table, int key)
    {
        var found = new List<(int, List<byte[]>)>();
        for (int i = 0; i < table.Payloads.Count - 1; i++)
        {
            var payload = table.Payloads[i];
            if (payload.Length == 0 || !((payload[0] & 0xf0) == 0x90 || payload[0] is 0xdc or 0xdd)) continue;
            var fields = RuneTicketJit.Array(payload);
            if (fields.Count > 0 && RuneTicketJit.Integer(fields[0]) == key) found.Add((i, fields));
        }
        return found.Single();
    }

    static void RestoreLinePetCurrency(string root, bool verify)
    {
        var patch = Path.GetFullPath(Path.Combine(root, "King's Raid_Data/Documents/Patch/StandaloneWindows"));
        const string relative = "TableJit/NewEquipGachaTable.jit";
        var path = Path.Combine(patch, relative);
        var metadataPath = Path.Combine(patch, "SPRK-NativeData.json");
        var original = File.ReadAllBytes(path);
        var table = new RuneTicketJit(original);
        var empty = Array.IndexOf(table.Strings, "");
        if (empty < 0) throw new InvalidDataException("Missing empty string in summon table.");
        static byte[] Number(int value) => new byte[] { 0xd2, (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
        foreach (var id in new[] { 26, 27, 29, 30 })
        {
            var (index, row) = NumericJitRow(table, id);
            var (_, standard) = NumericJitRow(table, id is 26 or 27 ? 25 : 28);
            if (row.Count != 45 || standard.Count != 45) throw new InvalidDataException("Unknown summon table schema.");
            var price = checked((int)RuneTicketJit.Integer(standard[13]) * 2);
            if (price <= 0) throw new InvalidDataException("Normal pet summon must have a ruby price.");
            var fields = new Dictionary<int, byte[]> {
                [6] = Number(0), [7] = Number(empty), [8] = Number(empty), [13] = Number(price),
                [30] = new byte[] { 0xc2 }, [43] = new byte[] { 0xc2 }
            };
            if (verify)
            {
                foreach (var (field, expected) in fields)
                {
                    bool equal = field is 30 or 43 ? row[field].SequenceEqual(expected)
                        : RuneTicketJit.Integer(row[field]) == RuneTicketJit.Integer(expected);
                    if (!equal) throw new InvalidOperationException($"LINE summon {id} has incorrect price, ticket restriction or event period (field {field}).");
                }
            }
            else
            {
                foreach (var (field, value) in fields) row[field] = value;
                table.ReplaceRow(index, row);
            }
        }
        var metadata = JsonNode.Parse(File.ReadAllBytes(metadataPath))!;
        static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (verify)
        {
            if (metadata["files"]![relative]?["sha256"]?.GetValue<string>() != Hash(original))
                throw new InvalidOperationException("Summon table integrity mismatch.");
            Console.WriteLine("PASS: LINE summons cost twice normal ruby prices, are permanent, accept rubies, and have valid native integrity metadata.");
            return;
        }
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Close clients before changing summon tables.");
        var backup = Path.Combine("target/line-pet-currency-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")); Directory.CreateDirectory(backup);
        File.Copy(path, Path.Combine(backup, "NewEquipGachaTable.jit"));
        File.Copy(metadataPath, Path.Combine(backup, "SPRK-NativeData.json"));
        var bytes = table.Encoded();
        var entry = metadata["files"]![relative];
        if (entry == null) metadata["files"]![relative] = entry = new JsonObject { ["originalSha256"] = Hash(original) };
        entry["sha256"] = Hash(bytes);
        File.WriteAllBytes(path + ".staged", bytes); File.Move(path + ".staged", path, true);
        File.WriteAllText(metadataPath, metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        RestoreLinePetCurrency(root, true);
    }
}
