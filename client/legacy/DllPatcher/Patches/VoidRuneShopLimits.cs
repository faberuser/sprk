using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DllPatcher;

partial class Program
{
    // Zero is the native unlimited-stock value. Keep price, currency, reset
    // schedule, availability and every other item exactly as installed.
    static void InstallVoidRuneShopLimits(string clientRoot, bool verify)
    {
        clientRoot = Path.GetFullPath(clientRoot);
        var patch = Path.Combine(clientRoot, "King's Raid_Data/Documents/Patch/StandaloneWindows");
        const string relative = "TableJit/ShopItemTable.jit";
        var path = Path.Combine(patch, relative);
        var metadataPath = Path.Combine(patch, NativeStaticData.ManifestName);
        var original = File.ReadAllBytes(path);
        var metadataOriginal = File.ReadAllBytes(metadataPath);
        var metadata = JsonNode.Parse(metadataOriginal)!;
        static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var entry = metadata["files"]![relative] ?? throw new InvalidDataException("Missing shop integrity metadata.");
        if (entry["sha256"]!.GetValue<string>() != Hash(original)) throw new InvalidDataException("Shop integrity mismatch.");
        var table = new RuneTicketJit(original);
        var codes = Enumerable.Range(1, 5).Select(i => "VOID_RUNE_" + i).ToHashSet();
        var seen = new HashSet<string>();
        int offers = 0, changed = 0;
        for (var i = 0; i < table.Payloads.Count - 1; i++)
        {
            var payload = table.Payloads[i];
            if (payload.Length == 0 || payload[0] != 0x9e) continue;
            var fields = RuneTicketJit.Array(payload);
            var codeIndex = checked((int)RuneTicketJit.Integer(fields[4]));
            if (codeIndex < 0 || codeIndex >= table.Strings.Length || !codes.Contains(table.Strings[codeIndex])) continue;
            seen.Add(table.Strings[codeIndex]); offers++;
            if (RuneTicketJit.Integer(fields[7]) == 0) continue;
            if (verify) throw new InvalidDataException("Void Rune shop offer is still limited: " + table.Strings[codeIndex]);
            fields[7] = new byte[] { 0 };
            table.ReplaceRow(i, fields); changed++;
        }
        if (!seen.SetEquals(codes)) throw new InvalidDataException("Expected shop offers for all five Void Runes.");
        VerifyShopPurchaseLimitsFile(clientRoot, false);
        if (verify)
        {
            Console.WriteLine($"PASS: all {offers} native Void Rune offers are unlimited; prices and other fields preserved; native hash verified.");
            return;
        }
        if (changed > 0)
        {
            if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, clientRoot)))
                throw new InvalidOperationException("Close game clients before installing Void Rune shop tables.");
            var output = table.Encoded(); entry["sha256"] = Hash(output);
            var backup = Path.GetFullPath(Path.Combine("target/void-rune-shop-limit-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")));
            Directory.CreateDirectory(backup);
            File.WriteAllBytes(Path.Combine(backup, "ShopItemTable.jit"), original);
            File.WriteAllBytes(Path.Combine(backup, NativeStaticData.ManifestName), metadataOriginal);
            if (!File.ReadAllBytes(path).SequenceEqual(original) || !File.ReadAllBytes(metadataPath).SequenceEqual(metadataOriginal))
                throw new InvalidOperationException("Shop files changed during preparation.");
            File.WriteAllBytes(path + ".staged", output);
            File.WriteAllText(metadataPath + ".staged", metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
            File.Move(path + ".staged", path, true);
            File.Move(metadataPath + ".staged", metadataPath, true);
        }
        InstallVoidRuneShopLimits(clientRoot, true);
    }
}
