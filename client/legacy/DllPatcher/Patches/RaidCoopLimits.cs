using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;

partial class Program
{
    // Keep complete native caches and all unrelated table changes intact.
    static void InstallRaidCoopLimits(string clientRoot, bool stageOnly, bool verifyOnly = false)
    {
        clientRoot = Path.GetFullPath(clientRoot);
        var managed = Path.Combine(clientRoot, "King's Raid_Data", "Managed");
        var patch = Path.Combine(clientRoot, "King's Raid_Data", "Documents", "Patch", "StandaloneWindows");
        const string relative = "TableJit/RaidTable.jit";
        var path = Path.Combine(patch, relative);
        var original = File.ReadAllBytes(path);
        var metadataPath = Path.Combine(patch, "SPRK-NativeData.json");
        var metadataBytes = File.ReadAllBytes(metadataPath);
        var metadata = JsonNode.Parse(metadataBytes)!;
        string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (metadata["files"]![relative]!["sha256"]!.GetValue<string>() != Hash(original))
            throw new InvalidOperationException("Installed raid table integrity hash differs.");
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
            var dll = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(dll) ? Assembly.LoadFrom(dll) : null;
        };
        var assembly = Assembly.LoadFrom(Path.Combine(managed, "Assembly-CSharp.dll"));
        var type = assembly.GetType("NShared.RaidDataContainer")!;
        var parent = type.BaseType!.BaseType!;
        const BindingFlags hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        object Call(object obj, string name, params object?[] values) => obj.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(m => m.Name == name && m.GetParameters().Length == values.Length &&
                m.GetParameters().Select((p, i) => values[i] == null || p.ParameterType.IsInstanceOfType(values[i])).All(v => v))
            .Invoke(obj, values)!;
        object Packer(object c) => parent.GetProperty("Packer", hidden)!.GetValue(c)!;
        object Read(byte[] bytes) {
            var c = Activator.CreateInstance(type)!;
            Call(Packer(c), "Deserialize", bytes); return c;
        }
        int Number(object row, string key) => Convert.ToInt32(row.GetType().GetProperty(key)!.GetValue(row));
        string Snapshot(object row) => JsonSerializer.Serialize(row, row.GetType());
        var container = Read(original);
        var rows = ((IEnumerable)Call(container, "GetAllData")).Cast<object>().ToList();
        var limits = new Dictionary<int, int> { [32] = 3, [33] = 3, [34] = 4, [201] = 4, [202] = 4, [203] = 4 };
        for (int index = 70001; index <= 70015; index += 2) limits.Add(index, 4);
        int changed = 0;
        foreach (var (index, cap) in limits) {
            var selected = rows.Where(r => Number(r, "Index") == index).ToArray();
            bool challenge = index >= 70001;
            if (selected.Length != (challenge ? 4 : index < 100 ? 2 : 9)) throw new InvalidOperationException("Unexpected stages: " + index);
            foreach (var row in selected) {
                if (Number(row, "Type") != (challenge ? 7 : index < 100 ? 12 : 14)
                    || Number(row, "DeckCount") != (challenge ? 8 : index is 32 or 33 ? 6 : 7)
                    || (challenge && Number(row, "PlayerCount") != 3))
                    throw new InvalidOperationException("Unexpected raid type or party size: " + index);
                if (Number(row, "DeckCountPerPlayer") != cap) changed++;
                row.GetType().GetProperty("DeckCountPerPlayer")!.SetValue(row, cap);
            }
        }
        var expected = rows.ToDictionary(r => (Number(r, "Index"), Number(r, "Level")), Snapshot);
        var fresh = Activator.CreateInstance(type)!;
        var list = (IList)Call(parent.GetProperty("BaseList", hidden)!.GetValue(fresh)!, "JsonLoad_GetOrNew");
        foreach (var row in rows) list.Add(row);
        Call(parent.GetField("_firstData", hidden)!.GetValue(fresh)!, "JsonLoad_Set", rows[0]);
        var dictionary = (IDictionary)Call(type.BaseType!.GetProperty("BaseDic", hidden)!.GetValue(fresh)!, "JsonLoad_Get");
        var keyType = assembly.GetType("NShared.RaidDataKey2")!;
        foreach (var row in rows) dictionary.Add(Activator.CreateInstance(keyType, Number(row, "Index"), Number(row, "Level"))!, row);
        Call(fresh, "OnEndLoadFromJsonData");
        using var header = new BinaryReader(new MemoryStream(original));
        var code = header.ReadBytes(header.ReadInt32()); var table = header.ReadBytes(header.ReadInt32());
        var baked = changed == 0 ? original : (byte[])Call(Packer(fresh), "SerializeBytes", "RaidTable.jit", table, code, 0);
        void Verify(byte[] bytes) {
            var c = Read(bytes); var actual = ((IEnumerable)Call(c, "GetAllData")).Cast<object>().ToArray();
            if (actual.Length != expected.Count) throw new InvalidOperationException("Raid count changed.");
            foreach (var row in actual) {
                var key = (Number(row, "Index"), Number(row, "Level"));
                if (!expected.TryGetValue(key, out var snapshot) || Snapshot(row) != snapshot || Snapshot(Call(c, "GetData", key.Item1, key.Item2)) != snapshot)
                    throw new InvalidOperationException("Raid readback differs: " + key);
            }
        }
        Verify(baked);
        if (verifyOnly) {
            if (changed != 0) throw new InvalidOperationException("Installed co-op hero limits need updating.");
            if (!NativeStaticData.ValidateInstalled(managed)) throw new InvalidOperationException("Missing native data metadata.");
            Console.WriteLine($"Verified all {rows.Count} native raid rows and co-op hero limits, including 32 party Challenge Raid stages (4 per player, 8 total).");
            return;
        }
        var staged = Path.GetFullPath("target/raid-coop-limits/staged");
        Directory.CreateDirectory(Path.Combine(staged, "TableJit"));
        File.WriteAllBytes(Path.Combine(staged, relative), baked);
        metadata["files"]![relative]!["sha256"] = Hash(baked);
        var metadataOutput = JsonSerializer.SerializeToUtf8Bytes(metadata, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllBytes(Path.Combine(staged, "SPRK-NativeData.json"), metadataOutput);
        Console.WriteLine($"Verified {rows.Count} native rows; {changed} stage limits updated. Staged: {staged}");
        if (stageOnly) return;
        foreach (var process in Process.GetProcesses()) using (process) {
            if (!process.ProcessName.Contains("Raid", StringComparison.OrdinalIgnoreCase) && !process.ProcessName.StartsWith("SprkTest-")) continue;
            if (process.MainModule?.FileName is string exe && (exe.StartsWith(clientRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || exe.StartsWith(Path.GetFullPath("target/client-automation/clients") + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Close game clients and shared-asset workers before installation: " + process.Id);
        }
        if (!File.ReadAllBytes(path).SequenceEqual(original) || !File.ReadAllBytes(metadataPath).SequenceEqual(metadataBytes))
            throw new InvalidOperationException("Installed files changed during staging.");
        var backup = Path.GetFullPath(Path.Combine("target/raid-coop-limits/backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")));
        Directory.CreateDirectory(backup);
        File.WriteAllBytes(Path.Combine(backup, "RaidTable.jit"), original);
        File.WriteAllBytes(Path.Combine(backup, "SPRK-NativeData.json"), metadataBytes);
        File.WriteAllBytes(path, baked); File.WriteAllBytes(metadataPath, metadataOutput);
        Verify(File.ReadAllBytes(path));
        Console.WriteLine("Installed and verified co-op hero limits. Backup: " + backup);
    }
}
