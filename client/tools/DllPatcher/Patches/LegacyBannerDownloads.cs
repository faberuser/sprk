using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;

partial class Program
{
    const string LegacyBoardUrl = "https://d2012by2bl35hc.cloudfront.net/live/";
    const string BannerShopType = "NGame2.NUI.NManager.PayShopManagement";
    const string BannerLobbyType = "NGame2.NUI.NManager.NLobby.LobbyManagement";

    static MethodDefinition BannerMethod(ModuleDefinition module, string type, string name) =>
        module.GetType(type).Methods.Single(m => m.Name == name);

    static void PatchLegacyBannerDownloads(ModuleDefinition module)
    {
        foreach (var method in new[] {
            BannerMethod(module, BannerShopType, "LoadNewBannerImageUrlInfo"),
            BannerMethod(module, BannerLobbyType, "AddRightAwayBannerDic"),
            BannerMethod(module, BannerLobbyType, "GetBoardURL") })
        {
            method.Body = new MethodBody(method);
            var il = method.Body.GetILProcessor();
            if (method.ReturnType.MetadataType == MetadataType.String) il.Emit(OpCodes.Ldstr, "");
            else if (method.ReturnType.MetadataType != MetadataType.Void)
                throw new InvalidOperationException("Unexpected banner method signature: " + method.FullName);
            il.Emit(OpCodes.Ret);
        }

        // The native JIT is also compiled into PreloadInAppBytes. Rebuild its
        // string container and resize only the backing field/array for this table.
        foreach (var field in module.GetTypes().SelectMany(t => t.Fields)
            .Where(f => f.InitialValue.Length > 0 && Encoding.UTF8.GetString(f.InitialValue).Contains(LegacyBoardUrl)).ToArray())
        {
            var table = new RuneTicketJit(field.InitialValue);
            table.ClearString(LegacyBoardUrl);
            var bytes = table.Encoded();
            var oldSize = field.InitialValue.Length;
            var owner = field.DeclaringType;
            var name = "__StaticArrayInitTypeSize=" + bytes.Length;
            var sizedType = owner.NestedTypes.FirstOrDefault(t => t.Name == name);
            if (sizedType == null)
            {
                sizedType = new TypeDefinition("", name, TypeAttributes.NestedPrivate | TypeAttributes.Sealed | TypeAttributes.ExplicitLayout,
                    ((TypeDefinition)field.FieldType).BaseType) { PackingSize = 1, ClassSize = bytes.Length };
                owner.NestedTypes.Add(sizedType);
            }
            var references = module.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody)
                .SelectMany(m => m.Body.Instructions)
                .Where(i => i.OpCode == OpCodes.Ldtoken && i.Operand is FieldReference f && f.FullName == field.FullName).ToArray();
            if (references.Length != 1) throw new InvalidOperationException("Unexpected preloaded constant table references.");
            var length = references[0].Previous?.Previous?.Previous;
            if (length?.OpCode != OpCodes.Ldc_I4 || (int)length.Operand != oldSize
                || length.Next.OpCode != OpCodes.Newarr || length.Next.Next.OpCode != OpCodes.Dup)
                throw new InvalidOperationException("Unexpected preloaded constant table initializer.");
            length.Operand = bytes.Length;
            field.FieldType = sizedType;
            field.InitialValue = bytes;
        }
        VerifyLegacyBannerModule(module);
    }

    static void VerifyLegacyBannerModule(ModuleDefinition module)
    {
        foreach (var (type, name) in new[] { (BannerShopType, "LoadNewBannerImageUrlInfo"), (BannerLobbyType, "AddRightAwayBannerDic") })
        {
            var method = BannerMethod(module, type, name);
            if (method.Body.Instructions.Count != 1 || method.Body.Instructions[0].OpCode != OpCodes.Ret)
                throw new InvalidOperationException("Banner download code remains: " + name);
        }
        var getter = BannerMethod(module, BannerLobbyType, "GetBoardURL");
        if (getter.Body.Instructions.Count != 2 || getter.Body.Instructions[0].OpCode != OpCodes.Ldstr
            || (string)getter.Body.Instructions[0].Operand != "" || getter.Body.Instructions[1].OpCode != OpCodes.Ret)
            throw new InvalidOperationException("Board URL getter is still active.");
        var preload = module.GetType("NShared.NJit.PreloadInAppBytes").Methods.Single(m => m.Name == ".cctor");
        var tableName = preload.Body.Instructions.Single(i => i.OpCode == OpCodes.Ldstr && (string)i.Operand == "DefaultTableData/ConstantTable");
        var size = tableName.Next;
        var backing = (FieldDefinition)size.Next.Next.Next.Operand;
        if (size.OpCode != OpCodes.Ldc_I4 || (int)size.Operand != backing.InitialValue.Length
            || ((TypeDefinition)backing.FieldType).ClassSize != backing.InitialValue.Length)
            throw new InvalidOperationException("Invalid preloaded constant table array size.");
        var fallback = new RuneTicketJit(backing.InitialValue);
        var (_, board) = fallback.Row("BoardURL");
        if (fallback.Strings[checked((int)RuneTicketJit.Integer(board[1]))] != "")
            throw new InvalidOperationException("Preloaded BoardURL constant is not empty.");
        using var stream = new MemoryStream();
        module.Write(stream);
        var bytes = stream.ToArray();
        foreach (var encoding in new[] { Encoding.UTF8, Encoding.Unicode })
            if (bytes.AsSpan().IndexOf(encoding.GetBytes("d2012by2bl35hc.cloudfront.net")) >= 0)
                throw new InvalidOperationException("Legacy hostname remains in the assembly.");
    }

    static void InstallLegacyBannerRemoval(string clientRoot, bool verify)
    {
        clientRoot = Path.GetFullPath(clientRoot);
        var dll = Path.Combine(clientRoot, "King's Raid_Data/Managed/Assembly-CSharp.dll");
        var root = Path.Combine(clientRoot, "King's Raid_Data/Documents/Patch/StandaloneWindows");
        var tablePath = Path.Combine(root, "TableJit/ConstantTable.jit");
        var metadataPath = Path.Combine(root, "SPRK-NativeData.json");
        var original = File.ReadAllBytes(tablePath);
        var jit = new RuneTicketJit(original);
        jit.ClearString(LegacyBoardUrl);
        var cleaned = jit.Encoded();
        var (_, row) = jit.Row("BoardURL");
        if (jit.Strings[checked((int)RuneTicketJit.Integer(row[1]))] != "")
            throw new InvalidOperationException("BoardURL constant is not empty.");
        var metadata = JsonNode.Parse(File.ReadAllBytes(metadataPath))!;
        static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        // ConstantTable belongs to multiple native manifests.
        var owners = new Dictionary<string,JsonNode>();
        foreach (var name in new[] { "SPRK-NativeCombat.json", "SPRK-NativeLocalization.json" }) {
            var path = Path.Combine(root,name);
            if (!File.Exists(path)) continue;
            var owner = JsonNode.Parse(File.ReadAllBytes(path))!;
            if (owner["files"]?["TableJit/ConstantTable.jit"] is not JsonNode existing) continue;
            var expected = existing["sha256"]!.GetValue<string>();
            if (expected != Hash(original) && expected != Hash(cleaned))
                throw new InvalidOperationException("ConstantTable integrity mismatch: " + name);
            if (verify && expected != Hash(original))
                throw new InvalidOperationException("Stale ConstantTable integrity: " + name);
            existing["sha256"] = Hash(cleaned);
            owners.Add(path,owner);
        }
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver });
        if (verify)
        {
            VerifyLegacyBannerModule(assembly.MainModule);
            if (!original.SequenceEqual(cleaned) || metadata["files"]!["TableJit/ConstantTable.jit"]?["sha256"]?.GetValue<string>() != Hash(original))
                throw new InvalidOperationException("Legacy board URL or invalid native table integrity remains.");
            Console.WriteLine("PASS: banner download methods disabled, board URL removed from DLL/native table, table hash verified.");
            return;
        }
        PatchLegacyBannerDownloads(assembly.MainModule);
        var backup = Path.GetFullPath(Path.Combine("target/legacy-banner-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")));
        Directory.CreateDirectory(backup);
        foreach (var path in new[] { dll, tablePath, metadataPath }.Concat(owners.Keys)) File.Copy(path, Path.Combine(backup, Path.GetFileName(path)));
        var entry = metadata["files"]!["TableJit/ConstantTable.jit"];
        if (entry == null) metadata["files"]!["TableJit/ConstantTable.jit"] = entry = new JsonObject { ["originalSha256"] = Hash(original) };
        entry["sha256"] = Hash(cleaned);
        assembly.Write(dll + ".legacy-banner-staged");
        assembly.Dispose();
        File.Move(dll + ".legacy-banner-staged", dll, true);
        File.WriteAllBytes(tablePath, cleaned);
        File.WriteAllText(metadataPath, metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        foreach (var owner in owners) File.WriteAllText(owner.Key,owner.Value.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        InstallLegacyBannerRemoval(clientRoot, true);
    }
}
