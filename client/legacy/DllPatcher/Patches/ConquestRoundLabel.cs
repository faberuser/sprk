using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;
partial class Program
{
    // English renders the conquest ban list's week label (the Korean 회차) as
    // "{1} time(s)"; the portal already calls the same week "Round". Native
    // format arguments start at {1}. Only the English dictionary changes.
    const string ConquestRoundKey = "GUILD_SUPPRESS_SESSION_NUMBER";
    const string ConquestRoundText = "Round {1}";

    static void InstallConquestRoundLabel(string root, bool verify)
    {
        var patch = Path.GetFullPath(Path.Combine(root, "King's Raid_Data/Documents/Patch/StandaloneWindows"));
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data/Managed"));
        const string relative = "LocalizationJit/LocalizationAll_English.jit";
        var path = Path.Combine(patch, relative);
        // Both the data and combat manifests pin the English dictionary.
        var manifests = new[] { "SPRK-NativeData.json", "SPRK-NativeCombat.json" }.Select(n => Path.Combine(patch, n)).ToArray();
        var original = File.ReadAllBytes(path);
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
            var dll = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(dll) ? Assembly.LoadFrom(dll) : null;
        };
        var assembly = Assembly.LoadFrom(Path.Combine(managed, "Assembly-CSharp.dll"));
        var build = assembly.GetType("NClientData.NLocalization.BuildTools_for_Localize")!;
        var hash = assembly.GetType("NShared.NJit.JitOption")!.GetMethod("StringToHashCode")!;
        IDictionary Entries(byte[] bytes, out object table)
        {
            table = build.GetMethod("Deserialize")!.Invoke(null, new object[] { bytes })!;
            var dictionary = CTMember(table, "_dic"); CTCall(dictionary, "DevOnly_ForceLoadAllRawData");
            return (IDictionary)CTCall(dictionary, "JsonLoad_Get");
        }
        var entries = Entries(original, out var current);
        object key = original[0] == 1 ? hash.Invoke(null, new object[] { ConquestRoundKey })! : ConquestRoundKey;
        if (!entries.Contains(key)) throw new InvalidDataException("Missing localization " + ConquestRoundKey);
        var metadata = manifests.ToDictionary(m => m, m => JsonNode.Parse(File.ReadAllBytes(m))!);
        string? Pinned(JsonNode node) => node["files"]![relative]?["sha256"]?.GetValue<string>();
        if (verify)
        {
            if (!Equals(entries[key], ConquestRoundText))
                throw new InvalidOperationException($"Conquest round label is \"{entries[key]}\", expected \"{ConquestRoundText}\".");
            if (metadata.Values.Any(node => Pinned(node) != CTHash(original)))
                throw new InvalidOperationException("English localization integrity mismatch.");
            Console.WriteLine("PASS: Guild Conquest ban list weeks read \"Round N\" in English, with valid native integrity metadata.");
            return;
        }
        if (!Equals(entries[key], ConquestRoundText))
        {
            if (metadata.Values.Any(node => Pinned(node) != CTHash(original)))
                throw new InvalidOperationException("English localization integrity mismatch.");
            var fresh = Activator.CreateInstance(current.GetType())!;
            var target = (IDictionary)CTCall(CTMember(fresh, "_dic"), "JsonLoad_Get");
            foreach (DictionaryEntry pair in entries) target.Add(pair.Key, pair.Value);
            target[key] = ConquestRoundText;
            using var reader = new BinaryReader(new MemoryStream(original)); reader.BaseStream.Position = 1;
            var code = reader.ReadBytes(reader.ReadInt32()); var tableBytes = reader.ReadBytes(reader.ReadInt32());
            var packed = (byte[])CTCall(CTMember(fresh, "_packer"), "SerializeBytes", Path.GetFileName(relative), tableBytes, code, 0);
            var baked = new[] { original[0] }.Concat(packed).ToArray();
            var actual = Entries(baked, out _);
            if (actual.Count != target.Count) throw new InvalidOperationException("Localization count mismatch");
            foreach (DictionaryEntry pair in target)
                if (!actual.Contains(pair.Key) || !Equals(actual[pair.Key], pair.Value)) throw new InvalidOperationException("Localization readback mismatch");
            if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
                throw new InvalidOperationException("Close clients before changing localization.");
            var backup = Path.Combine("target/conquest-round-label-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")); Directory.CreateDirectory(backup);
            File.Copy(path, Path.Combine(backup, Path.GetFileName(path)));
            foreach (var manifest in manifests) File.Copy(manifest, Path.Combine(backup, Path.GetFileName(manifest)));
            foreach (var node in metadata.Values) node["files"]![relative]!["sha256"] = CTHash(baked);
            File.WriteAllBytes(path + ".staged", baked); File.Move(path + ".staged", path, true);
            foreach (var (manifest, node) in metadata)
                File.WriteAllText(manifest, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        InstallConquestRoundLabel(root, true);
    }
}
