using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mono.Cecil;

namespace DllPatcher;

partial class Program
{
    // Bake the dedicated ticket and restore regular pools. Native serializers
    // rebuild all lookup caches, including item-code and selector dictionaries.
    static void InstallClassWeaponSelectors(string clientRoot, bool verify)
    {
        clientRoot = Path.GetFullPath(clientRoot);
        var managed = Path.Combine(clientRoot, "King's Raid_Data/Managed");
        var patch = Path.Combine(clientRoot, "King's Raid_Data/Documents/Patch/StandaloneWindows");
        using var input = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ClassWeaponSelectorRules.json")!;
        var rules = JsonNode.Parse(input)!;
        var id = rules["ItemIndex"]!.GetValue<int>();
        var code = rules["ItemCode"]!.GetValue<string>();
        var choices = rules["Choices"]!.AsArray().Select(v => v!.GetValue<int>()).ToArray();
        if (!choices.SequenceEqual(Enumerable.Range(900001, 7))) throw new InvalidDataException("Invalid class weapon pool.");
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var dllOriginal = File.ReadAllBytes(dll);
        using var game = AssemblyDefinition.ReadAssembly(new MemoryStream(dllOriginal), new ReaderParameters { AssemblyResolver = resolver });
        // Reuse the existing icon alias machinery with the standard UW icon.
        CTIcon(game.MainModule, new JsonObject { ["LightTicketCode"] = code, ["IconSourceCode"] = "WEAPON_UNIQUE_SELECT_ITEM" }, verify);
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
            var path = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        using var assemblyBytes = new MemoryStream(); game.Write(assemblyBytes);
        var assembly = Assembly.Load(assemblyBytes.ToArray());
        var originals = new Dictionary<string, byte[]>();
        var outputs = new Dictionary<string, byte[]>();
        var metadata = new Dictionary<string, JsonNode>();
        foreach (var name in new[] { "SPRK-NativeData.json", "SPRK-NativeLocalization.json", "SPRK-NativeCombat.json" }) {
            originals[name] = File.ReadAllBytes(Path.Combine(patch, name));
            metadata[name] = JsonNode.Parse(originals[name])!;
        }
        void Save(string relative, byte[] bytes) {
            var original = originals[relative] = File.ReadAllBytes(Path.Combine(patch, relative));
            foreach (var meta in metadata.Values) if (meta["files"]?[relative] is JsonNode entry) {
                if (entry["sha256"]!.GetValue<string>() != CTHash(original)) throw new InvalidDataException("Native integrity mismatch: " + relative);
                entry["sha256"] = CTHash(bytes);
            }
            var owner = metadata[relative.StartsWith("LocalizationJit/") && !relative.EndsWith("_English.jit") ? "SPRK-NativeLocalization.json" : "SPRK-NativeData.json"];
            if (owner["files"]?[relative] == null) owner["files"]![relative] = new JsonObject { ["originalSha256"] = CTHash(original), ["sha256"] = CTHash(bytes) };
            outputs[relative] = bytes;
        }
        byte[] Bake(string relative, object packer, byte[] original, int skip = 0) {
            using var reader = new BinaryReader(new MemoryStream(original)); reader.BaseStream.Position = skip;
            var hash = reader.ReadBytes(reader.ReadInt32()); var table = reader.ReadBytes(reader.ReadInt32());
            return (byte[])CTCall(packer, "SerializeBytes", Path.GetFileName(relative), table, hash, 0);
        }
        void Table(string name, Action<Dictionary<string, object>> update) {
            var relative = "TableJit/" + name + "Table.jit";
            var original = File.ReadAllBytes(Path.Combine(patch, relative));
            var type = assembly.GetType("NShared." + (name == "Item" ? "ItemBase" : name) + "DataContainer")!;
            object Read(byte[] bytes) {
                var container = Activator.CreateInstance(type)!;
                CTCall(CTMember(container, "Packer"), "Deserialize", bytes); return container;
            }
            var old = Read(original);
            var rows = ((IEnumerable)CTCall(old, "GetAllData")).Cast<object>().ToDictionary(r => CTKey(r).ToString()!);
            var before = rows.ToDictionary(p => p.Key, p => CTSnapshot(p.Value));
            update(rows);
            var expected = rows.ToDictionary(p => p.Key, p => CTSnapshot(p.Value));
            var changed = before.Count != expected.Count || before.Any(p => !expected.TryGetValue(p.Key, out var value) || p.Value != value);
            if (verify && changed) throw new InvalidDataException("Class weapon ticket table needs updating: " + name);
            var bytes = original;
            if (changed) {
                var fresh = Activator.CreateInstance(type)!;
                var list = (IList)CTCall(CTMember(fresh, "BaseList"), "JsonLoad_GetOrNew");
                var dictionary = (IDictionary)CTCall(CTMember(fresh, "BaseDic"), "JsonLoad_Get");
                foreach (var row in rows.Values) { list.Add(row); dictionary.Add(CTKey(row), row); }
                CTCall(CTMember(fresh, "_firstData"), "JsonLoad_Set", rows.Values.First());
                CTCall(fresh, "OnEndLoadFromJsonData"); bytes = Bake(relative, CTMember(fresh, "Packer"), original);
            }
            var check = Read(bytes);
            var actual = ((IEnumerable)CTCall(check, "GetAllData")).Cast<object>().ToDictionary(r => CTKey(r).ToString()!, CTSnapshot);
            if (actual.Count != expected.Count || expected.Any(p => actual.GetValueOrDefault(p.Key) != p.Value)) { var bad = expected.FirstOrDefault(p => actual.GetValueOrDefault(p.Key) != p.Value); throw new InvalidDataException("Native ticket table readback mismatch: " + name + " " + bad.Key + " EXPECTED " + bad.Value + " ACTUAL " + actual.GetValueOrDefault(bad.Key)); }
            foreach (var row in rows.Values)
                if (CTSnapshot(CTCall(check, "GetData", CTKey(row))) != expected[CTKey(row).ToString()!]) throw new InvalidDataException("Native dictionary mismatch: " + name);
            if (name == "Item" && Convert.ToInt32(CTMember(CTCall(check, "GetDataByCode", code), "Index")) != id) throw new InvalidDataException("Missing dedicated ticket code lookup.");
            Save(relative, bytes);
        }
        Table("Item", rows => {
            var template = rows["2000"];
            var row = JsonSerializer.Deserialize(CTSnapshot(template), template.GetType())!;
            void Set(string key, object value) => row.GetType().GetProperty(key)!.SetValue(row, value);
            Set("Index", id); Set("Code", code); Set("BuyGem", rules["Price"]!.GetValue<int>());
            Set("ReplaceNameKey", "ITEM_NAME_" + id);
            Set("ReplaceDesc2Key", "SPRK_CLASS_UW_SELECTOR_DESC");
            if (rows.TryGetValue(id.ToString(), out var existing) && CTSnapshot(existing) != CTSnapshot(row)) throw new InvalidDataException("Dedicated ticket index collision.");
            rows[id.ToString()] = row;
        });
        Table("WeaponUniqueSelectItem", rows => {
            foreach (var regular in rules["RegularSelectors"]!.AsArray()) {
                var row = rows[regular!["Code"]!.GetValue<string>()];
                row.GetType().GetProperty("ItemIndices")!.SetValue(row, regular["Choices"]!.AsArray().Select(v => v!.GetValue<int>()).ToArray());
            }
            var template = rows["WEAPON_UNIQUE_SELECT_ITEM"];
            var rowNew = JsonSerializer.Deserialize(CTSnapshot(template), template.GetType())!;
            rowNew.GetType().GetProperty("ItemCode")!.SetValue(rowNew, code);
            rowNew.GetType().GetProperty("ItemIndices")!.SetValue(rowNew, choices);
            rowNew.GetType().GetProperty("TitleKey")!.SetValue(rowNew, "ITEM_NAME_" + id);
            if (rows.TryGetValue(code, out var existing) && CTSnapshot(existing) != CTSnapshot(rowNew)) throw new InvalidDataException("Dedicated selector code collision.");
            rows[code] = rowNew;
        });
        // Ship English fallback strings in every language without changing existing entries.
        var build = assembly.GetType("NClientData.NLocalization.BuildTools_for_Localize")!;
        var hashMethod = assembly.GetType("NShared.NJit.JitOption")!.GetMethod("StringToHashCode")!;
        var strings = new Dictionary<string, string> { ["ITEM_NAME_" + id] = rules["Name"]!.GetValue<string>(), ["SPRK_CLASS_UW_SELECTOR_DESC"] = rules["Description"]!.GetValue<string>(), ["ITEM_DESC_" + id] = rules["Description"]!.GetValue<string>() };
        foreach (var path in Directory.GetFiles(Path.Combine(patch, "LocalizationJit"), "LocalizationAll_*.jit")) {
            var original = File.ReadAllBytes(path);
            var old = build.GetMethod("Deserialize")!.Invoke(null, new object[] { original })!;
            var oldDic = CTMember(old, "_dic"); CTCall(oldDic, "DevOnly_ForceLoadAllRawData");
            var cache = (IDictionary)CTCall(oldDic, "JsonLoad_Get");
            var changes = strings.Select(p => (Key: original[0] == 1 ? hashMethod.Invoke(null, new object[] { p.Key })! : p.Key, p.Value))
                .Where(p => !cache.Contains(p.Key) || !Equals(cache[p.Key], p.Value)).ToArray();
            if (verify && changes.Length > 0) throw new InvalidDataException("Ticket localization missing: " + path);
            var bytes = original;
            if (changes.Length > 0) {
                var fresh = Activator.CreateInstance(old.GetType())!;
                var target = (IDictionary)CTCall(CTMember(fresh, "_dic"), "JsonLoad_Get");
                foreach (DictionaryEntry pair in cache) target.Add(pair.Key, pair.Value);
                foreach (var pair in changes) target[pair.Key] = pair.Value;
                bytes = new[] { original[0] }.Concat(Bake(path, CTMember(fresh, "_packer"), original, 1)).ToArray();
                var check = build.GetMethod("Deserialize")!.Invoke(null, new object[] { bytes })!;
                var dic = CTMember(check, "_dic"); CTCall(dic, "DevOnly_ForceLoadAllRawData");
                var actual = (IDictionary)CTCall(dic, "JsonLoad_Get");
                if (actual.Count != target.Count) throw new InvalidDataException("Ticket localization count mismatch.");
                foreach (DictionaryEntry pair in target) if (!actual.Contains(pair.Key) || !Equals(actual[pair.Key], pair.Value)) throw new InvalidDataException("Ticket localization readback mismatch.");
            }
            Save("LocalizationJit/" + Path.GetFileName(path), bytes);
        }
        if (verify) { Console.WriteLine("PASS: regular selectors restored; dedicated seven-class ticket, native lookups, icon alias and all localizations verified."); return; }
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, clientRoot))) throw new InvalidOperationException("Close game clients before installing the dedicated ticket.");
        foreach (var pair in metadata) outputs[pair.Key] = Encoding.UTF8.GetBytes(pair.Value.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        foreach (var pair in originals) if (!File.ReadAllBytes(Path.Combine(patch, pair.Key)).SequenceEqual(pair.Value)) throw new InvalidOperationException("Client changed during preparation.");
        if (!File.ReadAllBytes(dll).SequenceEqual(dllOriginal)) throw new InvalidOperationException("Client DLL changed during preparation.");
        var backup = Path.GetFullPath(Path.Combine("target/class-weapon-ticket-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")));
        Directory.CreateDirectory(backup); File.WriteAllBytes(Path.Combine(backup, "Assembly-CSharp.dll"), dllOriginal);
        foreach (var pair in outputs.Where(p => !originals[p.Key].SequenceEqual(p.Value))) {
            var saved = Path.Combine(backup, pair.Key); Directory.CreateDirectory(Path.GetDirectoryName(saved)!); File.WriteAllBytes(saved, originals[pair.Key]);
            var destination = Path.Combine(patch, pair.Key); File.WriteAllBytes(destination + ".staged", pair.Value); File.Move(destination + ".staged", destination, true);
        }
        // Preserve every other existing patch; only add the ticket icon alias.
        if (!verify) { File.WriteAllBytes(dll + ".staged", assemblyBytes.ToArray()); File.Move(dll + ".staged", dll, true); }
        if (!NativeStaticData.ValidateInstalled(managed)) throw new InvalidDataException("Native integrity validation failed.");
        Console.WriteLine("Installed dedicated Class Unique Weapon Selector and restored regular selector pools. Restart the client.");
    }
}
