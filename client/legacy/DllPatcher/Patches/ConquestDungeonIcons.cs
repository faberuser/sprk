using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;
partial class Program
{
    // The baked conquest dungeon rows 290-292 lost IconPath and
    // StatusBoardPortraitPath: they came from the old restoration hook, which
    // copied neither field. The ban list and status board then show empty boss
    // frames. Point those fields at the archived strings that the same bosses'
    // intact rows (287-289) already use; no new strings or rows are added.
    static void RestoreConquestDungeonIcons(string root, bool verify)
    {
        var patch = Path.GetFullPath(Path.Combine(root, "King's Raid_Data/Documents/Patch/StandaloneWindows"));
        const string relative = "TableJit/GuildSuppressDungeonTable.jit";
        var path = Path.Combine(patch, relative);
        var metadataPath = Path.Combine(patch, "SPRK-NativeData.json");
        var original = File.ReadAllBytes(path);
        var table = new RuneTicketJit(original);
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RestoredGuildConquest.json")!;
        var restored = JsonNode.Parse(stream)!["GuildSuppressDungeon"]!.AsArray()
            .ToDictionary(row => row!["Index"]!.GetValue<int>(), row => row!);
        int[] targets = { 290, 291, 292 }, references = { 287, 288, 289 };
        if (!targets.All(restored.ContainsKey)) throw new InvalidDataException("Missing restored conquest dungeon rows.");
        int StringIndex(string value)
        {
            var index = Array.IndexOf(table.Strings, value);
            if (index < 0) throw new InvalidDataException("Conquest dungeon table has no string " + value);
            return index;
        }
        static bool IsInteger(byte[] field) => field.Length > 0 && (field[0] <= 0x7f || field[0] >= 0xe0 || field[0] is >= 0xcc and <= 0xd3);
        string Expected(int id, string field) => restored[id][field]!.GetValue<string>();
        // Find each string column from the intact rows of the same three bosses.
        int Slot(string field)
        {
            var rows = references.Select(id => NumericJitRow(table, id).Fields).ToArray();
            var keys = targets.Select(id => StringIndex(Expected(id, field))).ToArray();
            var slots = Enumerable.Range(0, rows.Min(r => r.Count))
                .Where(i => rows.Select((row, n) => IsInteger(row[i]) && RuneTicketJit.Integer(row[i]) == keys[n]).All(x => x)).ToArray();
            if (slots.Length != 1) throw new InvalidDataException($"Unknown conquest dungeon schema for {field}.");
            return slots[0];
        }
        var portrait = Slot("PortraitPath");
        var fields = new[] { "IconPath", "StatusBoardPortraitPath" }.ToDictionary(f => f, Slot);
        static byte[] Number(int value) => new byte[] { 0xd2, (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
        foreach (var id in targets)
        {
            var (index, row) = NumericJitRow(table, id);
            if (RuneTicketJit.Integer(row[portrait]) != StringIndex(Expected(id, "PortraitPath")))
                throw new InvalidDataException($"Conquest dungeon {id} is not the expected boss.");
            foreach (var (field, slot) in fields)
            {
                var key = StringIndex(Expected(id, field));
                if (verify)
                {
                    if (!IsInteger(row[slot]) || RuneTicketJit.Integer(row[slot]) != key)
                        throw new InvalidOperationException($"Conquest dungeon {id} is missing {field}.");
                }
                else row[slot] = Number(key);
            }
            if (!verify) table.ReplaceRow(index, row);
        }
        var metadata = JsonNode.Parse(File.ReadAllBytes(metadataPath))!;
        static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (verify)
        {
            if (metadata["files"]![relative]?["sha256"]?.GetValue<string>() != Hash(original))
                throw new InvalidOperationException("Conquest dungeon table integrity mismatch.");
            Console.WriteLine("PASS: conquest dungeons 290-292 have boss icons and status-board portraits, with valid native integrity metadata.");
            return;
        }
        var bytes = table.Encoded();
        if (!bytes.SequenceEqual(original))
        {
            if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
                throw new InvalidOperationException("Close clients before changing conquest dungeon tables.");
            var backup = Path.Combine("target/conquest-dungeon-icon-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")); Directory.CreateDirectory(backup);
            File.Copy(path, Path.Combine(backup, "GuildSuppressDungeonTable.jit"));
            File.Copy(metadataPath, Path.Combine(backup, "SPRK-NativeData.json"));
            var entry = metadata["files"]![relative];
            if (entry == null) metadata["files"]![relative] = entry = new JsonObject { ["originalSha256"] = Hash(original) };
            entry["sha256"] = Hash(bytes);
            File.WriteAllBytes(path + ".staged", bytes); File.Move(path + ".staged", path, true);
            File.WriteAllText(metadataPath, metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        RestoreConquestDungeonIcons(root, true);
    }
}
