using System.Security.Cryptography;

namespace DllPatcher;

partial class Program
{
    static void VerifyGodkingBackground(string root)
    {
        // The reconstructed MidSpec/LowSpec bundles were Android (platform 13,
        // Unity 2018). Use the shipped Windows (platform 19, Unity 2020) variants.
        // Preserve the lower-resolution Windows texture for the low preset.
        var directory = Path.Combine(root, "King's Raid_Data", "Documents", "Patch",
            "StandaloneWindows", "Assetbundle");
        foreach (var (spec, expected) in new[] {
            ("MidSpec", "60f072c67bbf1ff5373acd14d2bc3a3fdc9b9f038aaec87c5c5ff4c98e693e73"),
            ("LowSpec", "709934a9682e58f6140a538bd6b291834fc4c485ff9fa968294c157384f4581b") })
        {
            var path = Path.Combine(directory, spec, "NguiAtlas_UI_UI_Tower3.unity3d");
            if (!File.Exists(path) || Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
                    .ToLowerInvariant() != expected)
                throw new InvalidOperationException("Missing Windows trial background for " + spec
                    + "; apply the tracked client/overrides/files assets before patching.");
        }
        Console.WriteLine("Verified Windows Trial of the God King background bundles for MidSpec and LowSpec.");
    }
}
