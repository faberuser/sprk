using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Android update rules (SprkUpdate.cs). With a local release published
// (client-updates/stable/manifest.json), also checks it end to end: signature with the
// repository's public key, validation, and which files the APK would install.
var repo = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));
var json = new JsonSerializerOptions { IncludeFields = true };
var hot = File.ReadAllLines(Path.Combine(repo, "client/android/HybridBootstrap/HotUpdate/hot-update.txt"))
    .Select(l => l.Trim()).Where(l => l.Length > 0).ToHashSet();
int passed = 0;
void Check(bool ok, string what)
{
    if (!ok) throw new Exception("FAIL " + what);
    passed++;
    Console.WriteLine("PASS " + what);
}
void Throws(Action action, string what)
{
    try { action(); }
    catch (Exception e) when (e is InvalidDataException || e is FormatException || e is CryptographicException) { Check(true, what); return; }
    Check(false, what);
}

// Mapping
const string M = SprkUpdate.ManagedPrefix, P = SprkUpdate.PatchPrefix;
Check(SprkUpdate.Target(M + "Assembly-CSharp.dll", hot) == "sprk/hot/Assembly-CSharp.dll.bytes", "game assembly goes to the hot folder");
Check(SprkUpdate.Target(M + "SprkNewFeature.dll", hot) == "sprk/hot/SprkNewFeature.dll.bytes", "a new Sprk assembly is installed too");
Check(SprkUpdate.Target(M + "UnityEngine.CoreModule.dll", hot) == null, "engine and AOT assemblies stay in the APK");
Check(SprkUpdate.Target(P + "TableJit/x.jit", hot) == "Documents/Patch/StandaloneWindows/TableJit/x.jit", "tables go to the patch folder");
Check(SprkUpdate.Target(P + "PlainAsset/a/b.bjs", hot) == "Documents/Patch/StandaloneWindows/PlainAsset/a/b.bjs", "plain assets are platform-neutral");
foreach (var skipped in new[] { P + "Assetbundle/x.unity3d", P + "PatchInfo/patch.json", P + "patch.json", P + "patch.bjs", "UnityPlayer.dll", "King's Raid_Data/level0" })
    Check(SprkUpdate.Target(skipped, hot) == null, "not installed on Android: " + skipped);
Check(SprkUpdate.Target(P + "../../evil", hot) == null && SprkUpdate.Target(P + "a\\b", hot) == null, "unsafe paths are rejected");

// Default update URL
Check(SprkUpdate.DefaultManifestUrl("http://127.0.0.1:8080/host.json") == "http://127.0.0.1:8081/updates/stable/manifest.json", "local server uses the local update port");
Check(SprkUpdate.DefaultManifestUrl("https://kr.example.com/host.json") == "https://kr.example.com/updates/stable/manifest.json", "remote server uses its own origin");

// Signing: a throwaway key, and the PEM parser against .NET's own.
using (var rsa = RSA.Create(2048))
{
    string pem = rsa.ExportSubjectPublicKeyInfoPem();
    var parsed = SprkUpdate.ParsePublicKey(pem);
    var expected = rsa.ExportParameters(false);
    Check(parsed.Modulus.SequenceEqual(expected.Modulus) && parsed.Exponent.SequenceEqual(expected.Exponent), "PEM public key parses like .NET");
    var payload = Encoding.UTF8.GetBytes("{\"schema_version\":1}");
    var signature = rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    var outer = new SprkUpdate.Outer { signature = new SprkUpdate.Signature { algorithm = "RSA-SHA256", payload = Convert.ToBase64String(payload), value = Convert.ToBase64String(signature) } };
    Check(SprkUpdate.Authenticate(outer, null, parsed, false).SequenceEqual(payload), "a valid signature returns the payload");
    var forged = new SprkUpdate.Outer { signature = new SprkUpdate.Signature { algorithm = "RSA-SHA256", payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"schema_version\":2}")), value = outer.signature.value } };
    Throws(() => SprkUpdate.Authenticate(forged, null, parsed, false), "a changed payload is rejected");
    Throws(() => SprkUpdate.Authenticate(new SprkUpdate.Outer(), payload, parsed, false), "an unsigned remote manifest is rejected");
    Check(SprkUpdate.Authenticate(new SprkUpdate.Outer(), payload, parsed, true) == payload, "an unsigned manifest is accepted from loopback");
    Throws(() => SprkUpdate.Authenticate(outer, null, null, false), "a remote server needs a key");
}

// Validation
var good = new SprkUpdate.Release { schema_version = 1, version = "v1", files = { new SprkUpdate.ReleaseFile { path = M + "a.dll", size = 1, sha256 = new string('a', 64), url = "/updates/releases/v1/files/x" } } };
SprkUpdate.Validate(good);
Check(true, "a well-formed release validates");
good.files[0].url = "/updates/releases/other/files/x";
Throws(() => SprkUpdate.Validate(good), "a file from another release is rejected");
good.files[0].url = SprkUpdate.AssetPrefix + new string('b', 64);
Throws(() => SprkUpdate.Validate(good), "an asset URL must match its hash");

// The real local release, if one is published.
var manifest = Path.Combine(repo, "client-updates/stable/manifest.json");
if (File.Exists(manifest))
{
    var raw = File.ReadAllBytes(manifest);
    var outer = JsonSerializer.Deserialize<SprkUpdate.Outer>(raw, json);
    var key = SprkUpdate.ParsePublicKey(File.ReadAllText(Path.Combine(repo, "config/client-update-public.pem")));
    var release = JsonSerializer.Deserialize<SprkUpdate.Release>(SprkUpdate.Authenticate(outer, raw, key, false), json);
    SprkUpdate.Validate(release);
    Check(true, $"local release {release.version} verifies with config/client-update-public.pem and validates");
    var plan = release.files.Select(f => (f, target: SprkUpdate.Target(f.path, hot))).Where(p => p.target != null).ToList();
    var hotFiles = plan.Count(p => p.target.StartsWith("sprk/hot/"));
    Check(hot.All(n => plan.Any(p => p.target == $"sprk/hot/{n}.dll.bytes")), $"every APK hot assembly is in the release ({hotFiles} hot files)");
    Check(!plan.Any(p => p.target.Contains("/Assetbundle/") || p.target.Contains("/PatchInfo/")), "no Windows bundles or patch records");
    Console.WriteLine($"     Android installs {plan.Count} of {release.files.Count} files, {plan.Sum(p => p.f.size) / 1048576.0:F0} MB");
}
else Console.WriteLine("(no local release: skipped the end-to-end check)");
Console.WriteLine($"{passed} Android update checks passed.");
