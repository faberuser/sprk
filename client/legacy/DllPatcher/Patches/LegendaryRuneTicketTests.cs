using System.Security.Cryptography;
using System.Text.Json.Nodes;

namespace DllPatcher;

partial class Program
{
    static void TestLegendaryRuneTickets()
    {
        var client = Path.Combine(Path.GetTempPath(), "sprk-rune-tickets-" + Guid.NewGuid().ToString("N"));
        var root = Path.Combine(client, "King's Raid_Data/Documents/Patch/StandaloneWindows");
        Directory.CreateDirectory(Path.Combine(root, "TableJit"));
        try
        {
            static void Require(bool condition, string message)
            {
                if (!condition) throw new InvalidOperationException("Rune ticket test: " + message);
            }
            static byte[] Pack(object value)
            {
                using var bytes = new MemoryStream();
                using var writer = new BinaryWriter(bytes);
                if (value is int number)
                {
                    writer.Write((byte)0xce);
                    writer.Write(System.Net.IPAddress.HostToNetworkOrder(number));
                }
                else if (value is string text)
                {
                    var utf8 = System.Text.Encoding.UTF8.GetBytes(text);
                    writer.Write((byte)0xd9); writer.Write(checked((byte)utf8.Length)); writer.Write(utf8);
                }
                else if (value is object[] items)
                {
                    writer.Write((byte)0xdc);
                    writer.Write(System.Net.IPAddress.HostToNetworkOrder(checked((short)items.Length)));
                    foreach (var item in items) writer.Write(Pack(item));
                }
                else throw new InvalidOperationException("Unsupported test value.");
                return bytes.ToArray();
            }
            static byte[] Containers(byte[][] items)
            {
                using var bytes = new MemoryStream();
                using var writer = new BinaryWriter(bytes);
                writer.Write(items.Length);
                var offset = 0;
                foreach (var item in items) { writer.Write(offset); offset += item.Length; }
                foreach (var item in items) writer.Write(item);
                return bytes.ToArray();
            }
            var codes = new[] { "OLD", "NEW", "RUNE_SWIFTNESS_5", "RUNE_LIFE_5", "RUNE_TENACITY_5", "RUNE_VITALITY_5" }
                .Concat(Enumerable.Range(0, 20).Select(i => "RUNE_OTHER_" + i)).ToArray();
            var metadata = new JsonObject { ["files"] = new JsonObject() };
            var originals = new Dictionary<string, byte[]>();
            foreach (var name in new[] { "SelectItem", "ItemGroup" })
            {
                codes[0] = name == "SelectItem" ? "PACKAGE_RUNE_6_SELECTION" : "PACKAGE_RUNE_5_GROUP";
                codes[1] = name == "SelectItem" ? "PACKAGE_RUNE_6_SELECTION_2022" : "PACKAGE_RUNE_5_2022_GROUP";
                var pool = new[] { 100115, 100136, 100137, 100138 }.Concat(Enumerable.Range(200000, 20)).Cast<object>().ToArray();
                var rates = Enumerable.Range(2, 24).Select(i => (object)new object[] { i, 100 }).ToArray();
                var rows = name == "SelectItem"
                    ? new[] { Pack(new object[] { 0, new object[] { 100115 }, 42 }), Pack(new object[] { 1, pool, 43 }) }
                    : new[] { Pack(new object[] { 0, 0, 0, 0, 100, new object[] { new object[] { 2, 100 } } }),
                              Pack(new object[] { 1, 0, 0, 0, 2400, rates }) };
                var strings = Containers(codes.Select(code => Pack(code)).ToArray());
                // Two opaque hash headers, an unrelated payload, and a string pool.
                var data = new byte[] { 2, 0, 0, 0, 11, 12, 1, 0, 0, 0, 13 }
                    .Concat(Containers(rows.Concat(new[] { new byte[] { 0xc0 }, strings }).ToArray())).ToArray();
                var relative = "TableJit/" + name + "Table.jit";
                File.WriteAllBytes(Path.Combine(root, relative), data);
                originals[relative] = data;
                metadata["files"]![relative] = new JsonObject { ["originalSha256"] = "preserve-me", ["sha256"] = Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant() };
            }
            var metadataPath = Path.Combine(root, "SPRK-NativeData.json");
            File.WriteAllText(metadataPath, metadata.ToJsonString());
            void RejectVerification(string message)
            {
                try { RestoreLegendaryRuneTickets(client, true); }
                catch (InvalidOperationException) { return; }
                throw new InvalidOperationException("Rune ticket verifier accepted " + message);
            }
            RejectVerification("legacy pools");
            RestoreLegendaryRuneTickets(client, false);
            foreach (var (relative, before) in originals)
            {
                var after = File.ReadAllBytes(Path.Combine(root, relative));
                Require(before.AsSpan(0, 11).SequenceEqual(after.AsSpan(0, 11)), "hash headers changed");
                Require(!before.SequenceEqual(after), "legacy pool was not replaced");
                var a = new RuneTicketJit(before); var b = new RuneTicketJit(after);
                for (var i = 1; i < a.Payloads.Count; i++)
                    Require(a.Payloads[i].SequenceEqual(b.Payloads[i]), "unrelated container changed");
            }
            var snapshot = Directory.GetFiles(root, "*", SearchOption.AllDirectories).ToDictionary(path => path, File.ReadAllBytes);
            RestoreLegendaryRuneTickets(client, false);
            Require(snapshot.All(pair => pair.Value.SequenceEqual(File.ReadAllBytes(pair.Key))), "second install changed bytes");
            var updated = JsonNode.Parse(File.ReadAllText(metadataPath))!;
            Require(updated["files"]!.AsObject().All(pair => pair.Value!["originalSha256"]!.GetValue<string>() == "preserve-me"), "original hashes changed");
            updated["files"]!["TableJit/SelectItemTable.jit"]!["sha256"] = "invalid";
            File.WriteAllText(metadataPath, updated.ToJsonString());
            RejectVerification("a corrupt native hash");
            Console.WriteLine("PASS: rune ticket upgrade, preservation, idempotence and corruption checks.");
        }
        finally { Directory.Delete(client, true); }
    }
}
