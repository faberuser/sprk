using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Engine-independent part of the Android update step (also compiled by
/// client/android/UpdateTests): which files of a SPRK release the APK installs,
/// where they go, and the release signature check.
///
/// Android installs from the same signed release manifest as SprkLauncher on Windows
/// (/updates/stable/manifest.json). Of its files it takes the game assemblies, which
/// HybridCLR loads unchanged, and the platform-neutral patch data (tables,
/// localization, PlainAsset, LoginPresentation, video). Asset bundles differ per
/// platform and come from the game's own patch downloader; the game's patch records
/// (PatchInfo/, patch.json, patch.bjs) belong to that downloader.
/// </summary>
public static class SprkUpdate
{
    public const string ManagedPrefix = "King's Raid_Data/Managed/";
    public const string PatchPrefix = "King's Raid_Data/Documents/Patch/StandaloneWindows/";
    public const string ReleasePrefix = "/updates/releases/";
    public const string AssetPrefix = "/updates/assets/objects/";
    static readonly string[] SkippedPatchPrefixes = { "Assetbundle/", "PatchInfo/" };
    static readonly string[] SkippedPatchNames = { "patch.json", "patch.bjs" };

    [Serializable] public class Signature { public string algorithm; public string payload; public string value; }
    [Serializable] public class Outer { public Signature signature; }
    [Serializable] public class ReleaseFile { public string path; public long size; public string sha256; public string url; }
    [Serializable]
    public class Release
    {
        public int schema_version;
        public string version;
        public string asset_set;
        public List<ReleaseFile> files = new List<ReleaseFile>();
        public List<string> deleted_files = new List<string>();
    }

    /// <summary>Hot-update assemblies the bootstrap loads: the APK's list plus any Sprk feature assembly.</summary>
    public static bool IsHotAssembly(string name, ICollection<string> apkHotNames) =>
        apkHotNames.Contains(name) || name.StartsWith("Sprk", StringComparison.Ordinal);

    /// <summary>
    /// Device path, relative to the app's files directory, for a release path; null when
    /// Android does not use the file.
    /// </summary>
    public static string Target(string releasePath, ICollection<string> apkHotNames)
    {
        if (!SafeRelative(releasePath)) return null;
        if (releasePath.StartsWith(ManagedPrefix, StringComparison.Ordinal))
        {
            string file = releasePath.Substring(ManagedPrefix.Length);
            if (file.Contains("/") || !file.EndsWith(".dll", StringComparison.Ordinal)) return null;
            string name = file.Substring(0, file.Length - 4);
            return IsHotAssembly(name, apkHotNames) ? "sprk/hot/" + name + ".dll.bytes" : null;
        }
        if (releasePath.StartsWith(PatchPrefix, StringComparison.Ordinal))
        {
            string inner = releasePath.Substring(PatchPrefix.Length);
            foreach (var p in SkippedPatchPrefixes) if (inner.StartsWith(p, StringComparison.Ordinal)) return null;
            foreach (var n in SkippedPatchNames) if (inner == n) return null;
            return "Documents/Patch/StandaloneWindows/" + inner;
        }
        return null;
    }

    /// <summary>The same path rules as the update server and SprkLauncher.</summary>
    public static bool SafeRelative(string path)
    {
        if (string.IsNullOrEmpty(path) || path.Contains("\\")) return false;
        foreach (var part in path.Split('/'))
        {
            if (part.Length == 0 || part == "." || part == ".." || part.StartsWith(".") || part.EndsWith(".") || part.EndsWith(" "))
                return false;
            foreach (char c in part) if (char.IsControl(c) || ":<>\"|?*".IndexOf(c) >= 0) return false;
        }
        return true;
    }

    public static bool ValidSha256(string value)
    {
        if (value == null || value.Length != 64) return false;
        foreach (char c in value) if (!(c >= '0' && c <= '9' || c >= 'a' && c <= 'f')) return false;
        return true;
    }

    /// <summary>Checks a release before anything is downloaded; throws on anything unexpected.</summary>
    public static void Validate(Release release)
    {
        if (release == null || release.schema_version != 1 || string.IsNullOrEmpty(release.version) || release.files == null || release.files.Count == 0)
            throw new InvalidDataException("Unsupported or empty release manifest.");
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in release.files)
        {
            if (f == null || !SafeRelative(f.path) || f.size < 0 || !ValidSha256(f.sha256) || !paths.Add(f.path))
                throw new InvalidDataException("Invalid release file entry.");
            bool code = f.url != null && f.url.StartsWith(ReleasePrefix + release.version + "/files/", StringComparison.Ordinal);
            if (!code && f.url != AssetPrefix + f.sha256)
                throw new InvalidDataException("Invalid download URL for " + f.path);
        }
    }

    /// <summary>
    /// The authenticated manifest bytes. A signed manifest is verified with the APK's
    /// key and only its payload is used; an unsigned one is accepted only from loopback.
    /// </summary>
    public static byte[] Authenticate(Outer outer, byte[] raw, RSAParameters? key, bool loopback)
    {
        var signature = outer?.signature;
        if (signature == null || string.IsNullOrEmpty(signature.payload))
        {
            if (!loopback) throw new InvalidDataException("The update manifest is not signed.");
            return raw;
        }
        if (signature.algorithm != "RSA-SHA256") throw new InvalidDataException("Unsupported manifest signature.");
        byte[] payload = Convert.FromBase64String(signature.payload);
        if (key == null)
        {
            if (!loopback) throw new InvalidDataException("No update key to verify this server.");
            return payload;
        }
        using (var rsa = RSA.Create())
        {
            rsa.ImportParameters(key.Value);
            if (rsa.KeySize < 2048 || !rsa.VerifyData(payload, Convert.FromBase64String(signature.value), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                throw new InvalidDataException("The update manifest signature is invalid.");
        }
        return payload;
    }

    /// <summary>RSA public key from a PEM "PUBLIC KEY" (SubjectPublicKeyInfo) block.</summary>
    public static RSAParameters ParsePublicKey(string pem)
    {
        const string begin = "-----BEGIN PUBLIC KEY-----", end = "-----END PUBLIC KEY-----";
        int a = pem.IndexOf(begin, StringComparison.Ordinal), b = pem.IndexOf(end, StringComparison.Ordinal);
        if (a < 0 || b < a) throw new InvalidDataException("Expected a PEM public key.");
        var der = Convert.FromBase64String(pem.Substring(a + begin.Length, b - a - begin.Length).Replace("\r", "").Replace("\n", "").Trim());
        int pos = 0;
        Expect(der, ref pos, 0x30);           // SubjectPublicKeyInfo
        int algorithm = Expect(der, ref pos, 0x30);
        pos += algorithm;                     // rsaEncryption OID + NULL
        int bits = Expect(der, ref pos, 0x03);
        if (bits < 1 || der[pos++] != 0) throw new InvalidDataException("Invalid public key bit string.");
        Expect(der, ref pos, 0x30);           // RSAPublicKey
        var modulus = Integer(der, ref pos);
        var exponent = Integer(der, ref pos);
        return new RSAParameters { Modulus = modulus, Exponent = exponent };
    }

    static int Expect(byte[] der, ref int pos, byte tag)
    {
        if (pos >= der.Length || der[pos++] != tag) throw new InvalidDataException("Invalid public key.");
        int length = der[pos++];
        if ((length & 0x80) != 0)
        {
            int count = length & 0x7f;
            if (count < 1 || count > 4) throw new InvalidDataException("Invalid public key length.");
            length = 0;
            for (int i = 0; i < count; i++) length = (length << 8) | der[pos++];
        }
        if (length < 0 || pos + length > der.Length) throw new InvalidDataException("Truncated public key.");
        return length;
    }

    static byte[] Integer(byte[] der, ref int pos)
    {
        int length = Expect(der, ref pos, 0x02);
        int start = pos;
        pos += length;
        while (length > 1 && der[start] == 0) { start++; length--; } // unsigned big-endian
        var value = new byte[length];
        Array.Copy(der, start, value, 0, length);
        return value;
    }

    public static string Sha256File(string path)
    {
        using (var sha = SHA256.Create())
        using (var stream = File.OpenRead(path))
        {
            var hash = sha.ComputeHash(stream);
            var text = new StringBuilder(64);
            foreach (var b in hash) text.Append(b.ToString("x2"));
            return text.ToString();
        }
    }

    /// <summary>Update manifest URL for a server's host.json URL when none is configured.</summary>
    public static string DefaultManifestUrl(string hostUrl)
    {
        var uri = new Uri(hostUrl);
        // The local stack runs the update service beside the game, on port 8081.
        if (uri.IsLoopback && uri.Port == 8080) return $"{uri.Scheme}://{uri.Host}:8081/updates/stable/manifest.json";
        return uri.GetLeftPart(UriPartial.Authority) + "/updates/stable/manifest.json";
    }
}
