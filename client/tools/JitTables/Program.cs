using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using NShared.NJit;

// Edits TableJit (.jit) files through the game's own table containers.
//
// Every table is rebuilt the way the original developers produced it: rows go
// through the container's JSON loading path (JsonMarshalerUsingReflection and
// the container's hooks) and the game's JitPacker serializes the result.
// Before writing a patched table, the tool rebuilds the unpatched table the same
// way and refuses to continue unless that copy is byte-identical to the input.
// See docs/jit-tables.md.

const string Usage = """
    JitTables <command> ...
      roundtrip <jit-dir> <Table>...              Load and re-serialize; reports byte identity.
      verify    <jit-dir> <Table>...              Dump every row, rebuild from the dump; reports byte identity.
      rebuild   <jit-dir> <Table> <out.jit>           Write the unpatched rebuild (to diff a failed verify).
      dump      <jit-dir> <Table> <out.json> [key...]   Write rows (all, or the given keys) as patchable JSON.
      build     <input-dir> <patch-dir> <out-dir>   Apply every *.json patch; write changed tables,
                localization and SPRK-Native*.json. Both dirs use the native patch layout
                (TableJit/, LocalizationJit/); <input-dir> must be the pinned baseline.
      bjs <file.bjs>                             Prints a PlainAsset JsonBin file as JSON.
      bundle-index <BundlePack.bin> <regex> [--values]  Lists index entries whose key matches (optionally with values).
      loc-get <file.jit> <key>...                Prints the text for localization keys.
      loc-verify <file.jit>...                   Round-trip localization files; reports byte identity.
      sprites   <BundlePack.bin> [regex]         List sprite paths and their atlas / sprite names.
    """;

if (args.Length < 2)
{
    Console.Error.WriteLine(Usage);
    return 2;
}

try
{
    switch (args[0])
    {
        case "roundtrip":
            foreach (string table in args.Skip(2))
            {
                var t = JitTable.Load(args[1], table);
                byte[] rebuilt = t.SerializeMaterialized();
                Report(table, t.Original, rebuilt);
            }
            return 0;
        case "verify":
            bool ok = true;
            foreach (string table in args.Skip(2))
            {
                var t = JitTable.Load(args[1], table);
                byte[] rebuilt = t.Rebuild(t.DumpRows());
                ok &= Report(table, t.Original, rebuilt);
            }
            return ok ? 0 : 1;
        case "rebuild":
        {
            var t = JitTable.Load(args[1], args[2]);
            byte[] rebuilt = t.Rebuild(t.DumpRows());
            File.WriteAllBytes(args[3], rebuilt);
            return Report(args[2], t.Original, rebuilt) ? 0 : 1;
        }
        case "diffrows":
            return JitTable.Load(args[1], args[2]).DiffRows(args.Length > 3 ? int.Parse(args[3]) : 20);
        case "dump":
        {
            var t = JitTable.Load(args[1], args[2]);
            var keys = new HashSet<string>(args.Skip(4));
            var rows = t.DumpRows().Where(r => keys.Count == 0 || keys.Contains(r.Key)).Select(r => r.Tree).ToList();
            var doc = new Dictionary<string, object> { ["table"] = args[2], ["upsert"] = rows };
            File.WriteAllText(args[3], Tree.ToJson(doc));
            Console.WriteLine($"{args[2]}: wrote {rows.Count} rows to {args[3]}");
            return 0;
        }
        case "build":
            return Build(args[1], args[2], args[3]);
        case "bjs":
        {
            // Prints a PlainAsset JsonBin file (.bjs) as JSON; tries plain, then encrypted.
            byte[] bytes = File.ReadAllBytes(args[1]);
            LitJson.JsonData json;
            try { json = NGameAsset.PlainAssetHelper.LoadLitJsonFromJsonBin(bytes, args[1], false); }
            catch { json = NGameAsset.PlainAssetHelper.LoadLitJsonFromJsonBin(bytes, args[1], true); }
            Console.WriteLine(json.ToJson());
            return 0;
        }
        case "bundle-index":
        {
            // Lists every BundlePack.bin entry (section/key -> bundle / name) whose key matches a pattern.
            JsonBin.JsonRead.CreateRoot(File.ReadAllBytes(args[1]), out var indexRoot);
            var keyPattern = new System.Text.RegularExpressions.Regex(args[2], System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (var section in indexRoot.GetFieldInfoList())
            {
                foreach (var field in (section.ValueJsonRead.GetFieldInfoList() ?? new()).Where(f => keyPattern.IsMatch(f.Key)))
                {
                    Console.WriteLine($"{section.Key}/{field.Key} -> {field.ValueJsonRead["BundlePath"].GETSTRING("")} / {field.ValueJsonRead["Name"].GETSTRING("")}");
                    if (args.Length > 3 && args[3] == "--values")
                    {
                        Console.WriteLine("  " + JsonBin.NBinary.BinaryToJson.ToJson(field.ValueJsonRead).ToJson());
                    }
                }
            }
            return 0;
        }
        case "sprites":
        {
            // Lists BundlePack.bin's NguiAsset index: sprite path -> atlas / sprite name.
            JsonBin.JsonRead.CreateRoot(File.ReadAllBytes(args[1]), out var root);
            var pattern = new System.Text.RegularExpressions.Regex(args.Length > 2 ? args[2] : ".", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (var field in root["NguiAsset"].GetFieldInfoList().Where(f => pattern.IsMatch(f.Key)).OrderBy(f => f.Key, StringComparer.Ordinal))
            {
                Console.WriteLine($"{field.Key} -> {field.ValueJsonRead["BundlePath"].GETSTRING("")} / {field.ValueJsonRead["Name"].GETSTRING("")}");
            }
            return 0;
        }
        case "loc-get":
        {
            // Prints the text stored for each localization key.
            var f = LocalizationFile.Load(args[1]);
            foreach (string key in args.Skip(2))
            {
                Console.WriteLine($"{key}	{f.Get(key) ?? "<missing>"}");
            }
            return 0;
        }
        case "loc-verify":
        {
            bool same = true;
            foreach (string path in args.Skip(1))
            {
                var f = LocalizationFile.Load(path);
                same &= Report(Path.GetFileName(path), f.Original, f.Serialize());
            }
            return same ? 0 : 1;
        }
        default:
            Console.Error.WriteLine(Usage);
            return 2;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}

static bool Report(string table, byte[] original, byte[] rebuilt)
{
    bool same = original.AsSpan().SequenceEqual(rebuilt);
    Console.WriteLine($"{table}: {original.Length} -> {rebuilt.Length} bytes, identical={same}");
    return same;
}

// <input-dir> and <out-dir> are native patch folders (the layout of
// King's Raid_Data/Documents/Patch/StandaloneWindows): TableJit/, LocalizationJit/
// and the SPRK-Native*.json integrity metadata. The input must be the pinned
// baseline, never a client that already contains these patches.
static int Build(string inputDir, string patchDir, string outDir)
{
    var patches = Directory.GetFiles(patchDir, "*.json", SearchOption.AllDirectories)
        .OrderBy(p => Path.GetRelativePath(patchDir, p), StringComparer.Ordinal)
        .Select(p => (Path: p, Doc: (Dictionary<string, object>)MiniJSON.Json.Deserialize(File.ReadAllText(p))))
        .ToList();
    foreach (var (path, doc) in patches)
    {
        if (doc == null || !(doc.ContainsKey("table") ^ doc.ContainsKey("localization")))
        {
            throw new InvalidDataException(path + ": a patch needs exactly one of \"table\" or \"localization\".");
        }
    }
    string jitDir = Path.Combine(inputDir, "TableJit");
    var written = new List<(string Relative, byte[] Input, byte[] Output)>();
    foreach (var group in patches.Where(p => p.Doc.ContainsKey("table")).GroupBy(p => (string)p.Doc["table"]))
    {
        var t = JitTable.Load(jitDir, group.Key);
        var rows = t.DumpRows();
        // Fidelity gate: the unpatched rows must rebuild the input exactly.
        byte[] baseline = t.Rebuild(rows);
        if (!t.Original.AsSpan().SequenceEqual(baseline))
        {
            // A table whose row type has SPRK-forced getters cannot rebuild byte-identically: the
            // serializer writes the forced value. Accept it only when every other field matches.
            if (!JitTable.ForcedGetterFields.TryGetValue(group.Key, out string[] forced) || !JitTable.SameExcept(group.Key, rows, baseline, forced))
            {
                throw new InvalidOperationException($"{group.Key}: an unpatched rebuild is not byte-identical; refusing to patch.");
            }
            Console.WriteLine($"{group.Key}: rebuild differs only in SPRK-forced getter fields ({string.Join(", ", forced)}), which the game never reads from data.");
        }
        int replaced = 0, added = 0, removed = 0;
        foreach (var (path, doc) in group)
        {
            foreach (IDictionary where in List(doc, "removeWhere"))
            {
                removed += rows.RemoveAll(r => Matches(r.Tree, where));
            }
            foreach (Dictionary<string, object> row in List(doc, "upsert"))
            {
                string key = t.KeyOf(row);
                int index = key == null ? -1 : rows.FindIndex(r => r.Key == key);
                if (index >= 0)
                {
                    rows[index] = new Row(key, row);
                    replaced++;
                }
                else
                {
                    rows.Add(new Row(key, row));
                    added++;
                }
            }
            foreach (Dictionary<string, object> row in List(doc, "append"))
            {
                rows.Add(new Row(t.KeyOf(row), row));
                added++;
            }
        }
        var keys = rows.Where(r => r.Key != null).GroupBy(r => r.Key).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (keys.Count > 0)
        {
            throw new InvalidOperationException($"{group.Key}: duplicate keys after patching: {string.Join(", ", keys)}");
        }
        byte[] bytes = t.Rebuild(rows);
        written.Add(("TableJit/" + group.Key + ".jit", t.Original, bytes));
        Console.WriteLine($"{group.Key}: {replaced} replaced, {added} added, {removed} removed ({bytes.Length} bytes)");
    }

    // Localization patches: {"localization": {"English": {"KEY": "text"}, "Korean": {...}}}
    var byLanguage = new SortedDictionary<string, SortedDictionary<string, string>>(StringComparer.Ordinal);
    foreach (var (path, doc) in patches.Where(p => p.Doc.ContainsKey("localization")))
    {
        foreach (var language in (Dictionary<string, object>)doc["localization"])
        {
            if (!byLanguage.TryGetValue(language.Key, out var strings))
            {
                byLanguage[language.Key] = strings = new SortedDictionary<string, string>(StringComparer.Ordinal);
            }
            foreach (var entry in (Dictionary<string, object>)language.Value)
            {
                if (strings.TryGetValue(entry.Key, out string previous) && previous != (string)entry.Value)
                {
                    throw new InvalidDataException($"{path}: {language.Key} {entry.Key} is set twice with different text.");
                }
                strings[entry.Key] = (string)entry.Value;
            }
        }
    }
    foreach (var (language, strings) in byLanguage)
    {
        string relative = $"LocalizationJit/LocalizationAll_{language}.jit";
        var file = LocalizationFile.Load(Path.Combine(inputDir, relative));
        byte[] baseline = file.Serialize();
        if (!file.Original.AsSpan().SequenceEqual(baseline))
        {
            throw new InvalidOperationException(relative + ": an unpatched rebuild is not byte-identical; refusing to patch.");
        }
        int replaced = 0, added = 0;
        foreach (var (key, text) in strings)
        {
            if (file.Set(key, text))
            {
                replaced++;
            }
            else
            {
                added++;
            }
        }
        byte[] bytes = file.Serialize();
        written.Add((relative, file.Original, bytes));
        Console.WriteLine($"{relative}: {replaced} replaced, {added} added ({bytes.Length} bytes)");
    }

    foreach (var (relative, _, output) in written)
    {
        string target = Path.Combine(outDir, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(target));
        File.WriteAllBytes(target, output);
    }
    Metadata.Update(inputDir, outDir, written);
    return 0;
}

static IEnumerable<object> List(IDictionary doc, string name) => doc.Contains(name) && doc[name] is IList list ? list.Cast<object>() : Array.Empty<object>();

static bool Matches(IDictionary row, IDictionary where)
{
    IDictionaryEnumerator it = where.GetEnumerator();
    while (it.MoveNext())
    {
        DictionaryEntry e = it.Entry;
        if (!row.Contains(e.Key) || Tree.ToJson(row[e.Key]) != Tree.ToJson(Tree.Normalize(e.Value)))
        {
            return false;
        }
    }
    return true;
}

record Row(string Key, Dictionary<string, object> Tree);

// LocalizationAll_<Language>.jit: a MessagePack packer-type marker (1 = hashed)
// followed by a JitPacker holding JitDictionary<int, string>, keyed by
// JitOption.StringToHashCode(key). Original keys are not stored.
sealed class LocalizationFile
{
    const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    object packer;
    IDictionary strings;
    byte[] codeHash;
    byte[] tableHash;

    public byte[] Original { get; private set; }

    public static LocalizationFile Load(string path)
    {
        var f = new LocalizationFile { Original = File.ReadAllBytes(path) };
        if (f.Original[0] != 0x01)
        {
            throw new InvalidDataException(path + ": only hashed localization packers are supported.");
        }
        f.packer = NClientData.NLocalization.BuildTools_for_Localize.Deserialize(f.Original);
        object dic = f.packer.GetType().GetField("_dic", Any).GetValue(f.packer);
        dic.GetType().GetMethod("DevOnly_ForceLoadAllRawData").Invoke(dic, null);
        f.strings = (IDictionary)dic.GetType().GetMethod("JsonLoad_Get").Invoke(dic, null);
        int codeLength = BitConverter.ToInt32(f.Original, 1);
        f.codeHash = codeLength == 0 ? null : f.Original.AsSpan(5, codeLength).ToArray();
        int tableLength = BitConverter.ToInt32(f.Original, 5 + codeLength);
        f.tableHash = f.Original.AsSpan(9 + codeLength, tableLength).ToArray();
        return f;
    }

    public string Get(string key)
    {
        int hash = JitOption.StringToHashCode(key);
        return strings.Contains(hash) ? (string)strings[hash] : null;
    }

    // Returns true when the key already existed.
    public bool Set(string key, string text)
    {
        int hash = JitOption.StringToHashCode(key);
        bool existed = strings.Contains(hash);
        strings[hash] = text;
        return existed;
    }

    public byte[] Serialize()
    {
        var jit = (JitPacker)packer.GetType().GetField("_packer", Any).GetValue(packer);
        byte[] body = jit.SerializeBytes(null, tableHash, codeHash, 0);
        return new byte[] { 0x01 }.Concat(body).ToArray();
    }
}

// Keeps SPRK-NativeData.json (TableJit) and SPRK-NativeLocalization.json
// (LocalizationJit) in step with the files written: sha256 is the new file,
// originalSha256 keeps its existing value or records the baseline input.
static class Metadata
{
    public static void Update(string inputDir, string outDir, List<(string Relative, byte[] Input, byte[] Output)> written)
    {
        foreach (var group in written.GroupBy(w => w.Relative.StartsWith("LocalizationJit/", StringComparison.Ordinal)
                     ? "SPRK-NativeLocalization.json" : "SPRK-NativeData.json"))
        {
            string target = Path.Combine(outDir, group.Key);
            string source = File.Exists(target) ? target : Path.Combine(inputDir, group.Key);
            var root = File.Exists(source)
                ? System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(source)).AsObject()
                : new System.Text.Json.Nodes.JsonObject { ["version"] = 1, ["source"] = "SPRK table patches", ["files"] = new System.Text.Json.Nodes.JsonObject() };
            var files = root["files"].AsObject();
            foreach (var (relative, input, output) in group)
            {
                string original = files[relative]?["originalSha256"]?.GetValue<string>() ?? Sha(input);
                files[relative] = new System.Text.Json.Nodes.JsonObject { ["sha256"] = Sha(output), ["originalSha256"] = original };
            }
            var sorted = new System.Text.Json.Nodes.JsonObject();
            foreach (var entry in files.OrderBy(e => e.Key, StringComparer.Ordinal).ToList())
            {
                files.Remove(entry.Key);
                sorted[entry.Key] = entry.Value;
            }
            root["files"] = sorted;
            var options = new JsonSerializerOptions { WriteIndented = true, NewLine = "\n", Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            File.WriteAllText(target, root.ToJsonString(options) + "\n");
            Console.WriteLine($"{group.Key}: updated {group.Count()} entries");
        }
    }

    static string Sha(byte[] bytes) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
}

sealed class JitTable
{
    const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    static Dictionary<string, Type> containers;

    readonly Type containerType;
    readonly Type rowType;
    byte[] codeHash;
    byte[] tableHash;

    public byte[] Original { get; private set; }

    JitTable(Type containerType)
    {
        this.containerType = containerType;
        for (Type t = containerType; t != null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(JitDataContainer<>))
            {
                rowType = t.GetGenericArguments()[0];
            }
        }
    }

    // Row fields whose getters SPRK hard-codes (client/managed/Assembly-CSharp/NShared/CostumeData.cs);
    // the stored value is never read, and the serializer writes the forced value.
    public static readonly Dictionary<string, string[]> ForcedGetterFields = new()
    {
        ["CostumeTable"] = new[] { "IsOpen", "PreviewableWhenOwned", "IsShow" }
    };

    // True when the rebuilt bytes decode to the same rows, in the same order, apart from the given fields.
    public static bool SameExcept(string table, List<Row> rows, byte[] rebuilt, string[] ignore)
    {
        string dir = Path.Combine(Path.GetTempPath(), "jittables-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllBytes(Path.Combine(dir, table + ".jit"), rebuilt);
            List<Row> back = Load(dir, table).DumpRows();
            if (back.Count != rows.Count)
            {
                return false;
            }
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Key != back[i].Key || Strip(rows[i].Tree, ignore) != Strip(back[i].Tree, ignore))
                {
                    Console.Error.WriteLine($"{table}: row {rows[i].Key} differs after rebuild");
                    return false;
                }
            }
            return true;
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    static string Strip(Dictionary<string, object> tree, string[] ignore)
    {
        var copy = new Dictionary<string, object>(tree);
        foreach (string name in ignore)
        {
            copy.Remove(name);
        }
        return Tree.ToJson(copy);
    }

    public static JitTable Load(string jitDir, string table)
    {
        containers ??= FindContainers();
        if (!containers.TryGetValue(table, out Type type))
        {
            throw new ArgumentException("Unknown table " + table);
        }
        var t = new JitTable(type) { Original = File.ReadAllBytes(Path.Combine(jitDir, table + ".jit")) };
        int codeLength = BitConverter.ToInt32(t.Original, 0);
        t.codeHash = t.Original.AsSpan(4, codeLength).ToArray();
        int tableLength = BitConverter.ToInt32(t.Original, 4 + codeLength);
        t.tableHash = t.Original.AsSpan(8 + codeLength, tableLength).ToArray();
        return t;
    }

    static Dictionary<string, Type> FindContainers()
    {
        var result = new Dictionary<string, Type>();
        foreach (Type type in typeof(JitDataContainerAbstract).Assembly.GetTypes())
        {
            if (type.IsAbstract || !typeof(JitDataContainerAbstract).IsAssignableFrom(type) || type.GetConstructor(Type.EmptyTypes) == null)
            {
                continue;
            }
            try
            {
                var c = (JitDataContainerAbstract)Activator.CreateInstance(type);
                result.TryAdd(c.PathDefine.Path, type);
            }
            catch
            {
                // Containers that need a running game are not editable here.
            }
        }
        return result;
    }

    object NewContainer() => Activator.CreateInstance(containerType);

    static JitPacker PackerOf(object container) => (JitPacker)container.GetType().GetProperty("Packer", Any).GetValue(container);

    static IList Parts(object container) => (IList)typeof(JitPacker).GetField("_containerList", Any).GetValue(PackerOf(container));

    object LoadMaterialized()
    {
        object container = NewContainer();
        PackerOf(container).Deserialize(Original);
        foreach (object part in Parts(container))
        {
            Materialize(part);
        }
        return container;
    }

    // Key order of every dictionary part as stored in the input file.
    List<List<object>> originalKeyOrder;

    List<List<object>> OriginalKeyOrder()
    {
        if (originalKeyOrder == null)
        {
            originalKeyOrder = new List<List<object>>();
            foreach (object part in Parts(LoadMaterialized()))
            {
                originalKeyOrder.Add(DictionaryOf(part)?.Keys.Cast<object>().ToList());
            }
        }
        return originalKeyOrder;
    }

    static IDictionary DictionaryOf(object part) =>
        part.GetType().Name.StartsWith("JitDictionary", StringComparison.Ordinal)
            ? (IDictionary)part.GetType().GetMethod("JsonLoad_Get").Invoke(part, null)
            : null;

    // The serializer writes dictionary entries in insertion order within each
    // hash bucket. Re-insert entries in the input's order, new keys last, so an
    // unpatched rebuild matches the input and a patch changes only its own rows.
    void RestoreKeyOrder(object container)
    {
        IList parts = Parts(container);
        List<List<object>> order = OriginalKeyOrder();
        for (int i = 0; i < parts.Count; i++)
        {
            IDictionary dic = DictionaryOf(parts[i]);
            if (dic == null || order[i] == null)
            {
                continue;
            }
            var entries = new List<DictionaryEntry>();
            IDictionaryEnumerator it = dic.GetEnumerator();
            while (it.MoveNext())
            {
                entries.Add(it.Entry);
            }
            var byKey = new Dictionary<object, object>();
            foreach (DictionaryEntry e in entries)
            {
                byKey[e.Key] = e.Value;
            }
            dic.Clear();
            foreach (object key in order[i])
            {
                if (byKey.Remove(key, out object value))
                {
                    dic.Add(key, value);
                }
            }
            foreach (DictionaryEntry e in entries)
            {
                if (byKey.Remove(e.Key, out object value))
                {
                    dic.Add(e.Key, value);
                }
            }
        }
    }

    static void Materialize(object part)
    {
        Type t = part.GetType();
        if (t.Name.StartsWith("JitDictionary", StringComparison.Ordinal))
        {
            t.GetMethod("DevOnly_ForceLoadAllRawData", Any).Invoke(part, null);
        }
        else if (t.Name.StartsWith("JitRef", StringComparison.Ordinal))
        {
            t.GetMethod("JsonLoad_Set", Any).Invoke(part, new[] { t.GetMethod("Eval", Any).Invoke(part, null) });
        }
        else if (t.Name.StartsWith("JitList", StringComparison.Ordinal))
        {
            int count = (int)t.GetProperty("Count", Any).GetValue(part);
            MethodInfo get = t.GetMethod("Get", Any);
            var items = Enumerable.Range(0, count).Select(i => get.Invoke(part, new object[] { i })).ToList();
            var list = (IList)t.GetMethod("JsonLoad_GetOrNew", Any).Invoke(part, null);
            list.Clear();
            foreach (object item in items)
            {
                list.Add(item);
            }
        }
        else
        {
            throw new InvalidOperationException("Unknown jit part " + t.FullName);
        }
    }

    byte[] Serialize(object container) => PackerOf(container).SerializeBytes(null, tableHash, codeHash, 0);

    public byte[] SerializeMaterialized() => Serialize(LoadMaterialized());

    public string KeyOf(IDictionary tree)
    {
        object row = JsonMarshalerUsingReflection.Parse(rowType, Tree.Reparse(tree));
        return KeyOfRow(row);
    }

    static string KeyOfRow(object row)
    {
        Type selector = row.GetType().GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "IDataKeySelector`1");
        object key = selector?.GetProperty("Key").GetValue(row);
        return key == null ? null : Convert.ToString(key, CultureInfo.InvariantCulture);
    }

    public List<Row> DumpRows()
    {
        object container = LoadMaterialized();
        var rows = (IList)containerType.GetMethod("GetAllData", BindingFlags.Instance | BindingFlags.Public).Invoke(container, null);
        Dictionary<int, List<object>> operations = OperationsBySkill(container);
        var result = new List<Row>();
        foreach (object row in rows)
        {
            var tree = Tree.FromObject(row);
            if (operations != null)
            {
                int skill = (int)row.GetType().GetProperty("Index").GetValue(row);
                if (operations.TryGetValue(skill, out var ops))
                {
                    foreach (object op in ops)
                    {
                        Tree.AddOperation(tree, op);
                    }
                }
            }
            result.Add(new Row(KeyOfRow(row), tree));
        }
        return result;
    }

    // SkillTable keeps operations beside the rows; the JSON form flattens them
    // into OperationType1, OperationValue1, ... like the original tables.
    static Dictionary<int, List<object>> OperationsBySkill(object container)
    {
        FieldInfo field = container.GetType().GetField("_operationLoadDic", Any);
        if (field == null)
        {
            return null;
        }
        var dic = (IDictionary)field.FieldType.GetMethod("JsonLoad_Get").Invoke(field.GetValue(container), null);
        return dic.Values.Cast<object>()
            .GroupBy(op => (int)op.GetType().GetProperty("SkillIndex").GetValue(op))
            .ToDictionary(g => g.Key, g => g.OrderBy(op => (int)op.GetType().GetProperty("OperationNo").GetValue(op)).ToList());
    }

    // Diagnostic: compares every field of each row with the row parsed back from its dump.
    public int DiffRows(int limit)
    {
        object container = LoadMaterialized();
        var rows = (IList)containerType.GetMethod("GetAllData", BindingFlags.Instance | BindingFlags.Public).Invoke(container, null);
        var dumped = DumpRows();
        int found = 0;
        for (int i = 0; i < rows.Count && found < limit; i++)
        {
            object parsed = JsonMarshalerUsingReflection.Parse(rowType, Tree.Reparse(dumped[i].Tree));
            for (Type t = rowType; t != null && t != typeof(object); t = t.BaseType)
            {
                foreach (FieldInfo f in t.GetFields(Any | BindingFlags.DeclaredOnly))
                {
                    string a = Tree.ToJson(Tree.Plain(f.GetValue(rows[i])));
                    string b = Tree.ToJson(Tree.Plain(f.GetValue(parsed)));
                    if (a != b && found++ < limit)
                    {
                        Console.WriteLine($"row {dumped[i].Key}: {f.Name}: {a} != {b}");
                    }
                }
            }
        }
        return found == 0 ? 0 : 1;
    }

    public byte[] Rebuild(IEnumerable<Row> rows)
    {
        object container = NewContainer();
        var list = new List<object>();
        foreach (Row row in rows)
        {
            list.Add(Tree.Reparse(row.Tree));
        }
        containerType.GetMethod("LoadFromJsonData", Any).Invoke(container, new object[] { list });
        containerType.GetMethod("OnEndLoadFromJsonData", Any).Invoke(container, null);
        RestoreKeyOrder(container);
        Type baseType = containerType;
        while (!(baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(JitDataContainer<>)))
        {
            baseType = baseType.BaseType;
        }
        object baseList = baseType.GetProperty("BaseList", Any).GetValue(container);
        var all = (IList)baseList.GetType().GetMethod("JsonLoad_GetOrNew").Invoke(baseList, null);
        if (all.Count > 0)
        {
            object first = baseType.GetField("_firstData", Any).GetValue(container);
            first.GetType().GetMethod("JsonLoad_Set").Invoke(first, new[] { all[0] });
        }
        return Serialize(container);
    }
}

static class Tree
{
    static readonly JsonSerializerOptions Options = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    static readonly Dictionary<Type, object> Defaults = new();

    public static string ToJson(object tree) => JsonSerializer.Serialize(tree, Options);

    // MiniJSON gives the exact value types the game's JSON loader saw.
    public static object Reparse(object tree) => MiniJSON.Json.Deserialize(JsonSerializer.Serialize(tree));

    public static object Normalize(object value) => Reparse(new List<object> { value }) is IList l ? l[0] : value;

    public static object Plain(object v)
    {
        try
        {
            return Value(v);
        }
        catch
        {
            return v?.ToString();
        }
    }

    public static Dictionary<string, object> FromObject(object obj)
    {
        Type type = obj.GetType();
        if (!Defaults.TryGetValue(type, out object fresh))
        {
            fresh = Activator.CreateInstance(type);
            Defaults[type] = fresh;
        }
        var tree = new Dictionary<string, object>();
        foreach (PropertyInfo p in type.GetProperties())
        {
            if (!p.CanWrite || p.GetIndexParameters().Length > 0)
            {
                continue;
            }
            object value = Value(Read(p, obj));
            if (ToJson(value) == ToJson(Value(Read(p, fresh))))
            {
                continue;
            }
            tree[p.Name] = value;
        }
        return tree;
    }

    // SPRK patches some getters (for example CostumeData.IsShow always returns
    // true); the serializer stores the backing field, so read that when it exists.
    static object Read(PropertyInfo p, object obj)
    {
        FieldInfo backing = p.DeclaringType.GetField("<" + p.Name + ">k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? p.DeclaringType.GetField("_003C" + p.Name + "_003Ek__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        return backing != null ? backing.GetValue(obj) : p.GetValue(obj);
    }

    public static void AddOperation(Dictionary<string, object> tree, object op)
    {
        Type type = op.GetType();
        object fresh = Activator.CreateInstance(type);
        int no = (int)type.GetProperty("OperationNo").GetValue(op);
        foreach (PropertyInfo p in type.GetProperties())
        {
            if (!p.CanWrite || p.Name is "SkillIndex" or "OperationNo")
            {
                continue;
            }
            object value = Value(Read(p, op));
            // OperationType is always written: the loader keys on its presence.
            if (p.Name != "Type" && ToJson(value) == ToJson(Value(Read(p, fresh))))
            {
                continue;
            }
            tree["Operation" + p.Name + no.ToString(CultureInfo.InvariantCulture)] = value;
        }
    }

    static object Value(object v)
    {
        switch (v)
        {
            case null:
                return null;
            case string or bool:
                return v;
            case Enum e:
                return e.ToString();
            case float f:
                return (double)f;
            case double or decimal:
                return Convert.ToDouble(v, CultureInfo.InvariantCulture);
            case IConvertible when v.GetType().IsPrimitive:
                return Convert.ToInt64(v, CultureInfo.InvariantCulture);
            case IDictionary d:
            {
                var result = new Dictionary<string, object>();
                IDictionaryEnumerator it = d.GetEnumerator();
                while (it.MoveNext())
                {
                    result[Convert.ToString(Value(it.Key), CultureInfo.InvariantCulture)] = Value(it.Value);
                }
                return result;
            }
            case IEnumerable list:
                return list.Cast<object>().Select(Value).ToList();
        }
        Type type = v.GetType();
        if (type.Name == "Vector3" && type.Namespace == "NShared")
        {
            // JsonMarshalerUsingReflection reads NShared.Vector3 from a JSON string.
            return ToJson(new Dictionary<string, object>
            {
                ["Mx"] = Value(type.GetProperty("Mx")?.GetValue(v) ?? type.GetField("Mx")?.GetValue(v)),
                ["My"] = Value(type.GetProperty("My")?.GetValue(v) ?? type.GetField("My")?.GetValue(v)),
                ["Mz"] = Value(type.GetProperty("Mz")?.GetValue(v) ?? type.GetField("Mz")?.GetValue(v)),
            });
        }
        if (type.Name == "HashString")
        {
            return v.ToString();
        }
        return FromObject(v);
    }
}
