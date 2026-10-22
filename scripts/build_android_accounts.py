"""Build a signed x86 Android SPRK account and local-lobby test APK."""

import argparse
import json
import lzma
from pathlib import Path
import subprocess
import tarfile
import urllib.request
import zipfile

from build_android_endpoint_test import CONFIG_ENTRY, SIGNER_SHA256, is_signature, patch_config, read_config, sha256

TOOLS = {
    "apktool_3.0.3.jar": (
        "https://github.com/iBotPeaches/Apktool/releases/download/v3.0.3/apktool_3.0.3.jar",
        "dbf930b076c6b9be08d57c449cacefc3bdd6b71ebd59b3066fc0e1f5b14f9423"),
    "frida-gadget-16.7.19-android-x86.so.xz": (
        "https://github.com/frida/frida/releases/download/16.7.19/frida-gadget-16.7.19-android-x86.so.xz",
        "2eace718f4a7540c8017ecb5d12daf85245881ef650bd8f68360e06eeccd0543"),
    "frida-il2cpp-bridge-0.9.1.tgz": (
        "https://registry.npmjs.org/frida-il2cpp-bridge/-/frida-il2cpp-bridge-0.9.1.tgz",
        "7e6220dde81653beadf1ea0197654fffd1da3249d3a8c034702ca6f7b5c921cc"),
    "uber-apk-signer-1.3.0.jar": (
        "https://github.com/patrickfav/uber-apk-signer/releases/download/v1.3.0/uber-apk-signer-1.3.0.jar",
        SIGNER_SHA256),
}
SOURCE_HASH = "f10e636e24433c91cb765b1e598e09d6e8490e8cbeb38c20692cd848675fc08e"
ROOT = Path(__file__).resolve().parent.parent


def run(*args):
    subprocess.run([str(arg) for arg in args], check=True)


def build(source, output, host_url, decoded=None):
    if sha256(source.read_bytes()) != SOURCE_HASH:
        raise ValueError("Expected the original archived Android 04.88.008 APK")
    output.mkdir(parents=True, exist_ok=False)
    tools = ROOT / "target/android-tools"
    tools.mkdir(parents=True, exist_ok=True)
    for name, (url, digest) in TOOLS.items():
        path = tools / name
        if not path.exists():
            urllib.request.urlretrieve(url, path)
        if sha256(path.read_bytes()) != digest:
            raise ValueError(f"Dependency checksum mismatch: {name}")
    with tarfile.open(tools / "frida-il2cpp-bridge-0.9.1.tgz", "r:gz") as archive:
        bridge = archive.extractfile("package/dist/index.js").read()
        bridge_license = archive.extractfile("package/LICENSE.md").read()
    adapter = (ROOT / "scripts/android/accounts.js").read_bytes()
    lobby = (ROOT / "scripts/android/lobby.js").read_bytes()
    # Decoding is disposable. --decoded may reuse an original analysis decode.
    if decoded is None:
        decoded = output / "decoded"
        run("java", "-jar", tools / "apktool_3.0.3.jar", "d", source, "-r", "-o", decoded)
    activity = decoded / "smali_classes2/com/vespa/kingsraid/MainActivity.smali"
    smali = activity.read_text(encoding="utf-8")
    old = ".method static constructor <clinit>()V\n    .locals 0\n\n    return-void\n.end method"
    new = '.method static constructor <clinit>()V\n    .locals 1\n\n    const-string v0, "sprk-gadget"\n    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n\n    return-void\n.end method'
    if old in smali:
        activity.write_text(smali.replace(old, new, 1), encoding="utf-8")
    elif new not in smali:
        raise ValueError("Unexpected MainActivity static initializer")
    # Remove the obsolete protection SDK's Application bootstrap, retaining the
    # existing class name and MultiDex startup so the manifest stays unchanged.
    front = decoded / "smali/com/inca/security/Proxy/AppGuardFrontApplication.smali"
    if not front.is_file():
        raise ValueError("Expected original AppGuard Application wrapper")
    front.write_text('''.class public Lcom/inca/security/Proxy/AppGuardFrontApplication;
.super Landroidx/multidex/MultiDexApplication;
.source "SprkApplication.java"

.method public constructor <init>()V
    .locals 0
    invoke-direct {p0}, Landroidx/multidex/MultiDexApplication;-><init>()V
    return-void
.end method
''', encoding="utf-8")
    rebuilt = output / "rebuilt-intermediate.apk"
    run("java", "-jar", tools / "apktool_3.0.3.jar", "b", decoded, "-o", rebuilt)
    with zipfile.ZipFile(rebuilt) as archive:
        dex = archive.read("classes2.dex")
        application_dex = archive.read("classes.dex")
    added = {
        "assets/sprk/frida-il2cpp-bridge-LICENSE.md": bridge_license,
        "assets/sprk/THIRD-PARTY.txt": (
            "Frida Gadget 16.7.19 (unmodified upstream Android x86 binary)\n"
            "https://github.com/frida/frida/releases/tag/16.7.19\n"
            "Source and licensing: https://github.com/frida/frida/tree/16.7.19\n"
            "frida-il2cpp-bridge 0.9.1, MIT license (included alongside this file)\n"
            "https://github.com/vfsfitvnm/frida-il2cpp-bridge\n"
        ).encode(),
        "lib/x86/libsprk-gadget.so": lzma.decompress((tools / "frida-gadget-16.7.19-android-x86.so.xz").read_bytes()),
        "lib/x86/libsprk-gadget.config.so": json.dumps({
            "interaction": {"type": "script", "path": "libsprk-gadget.script.so", "on_change": "ignore"},
            "runtime": "qjs", "teardown": "full",
        }).encode(),
        "lib/x86/libsprk-gadget.script.so": bridge + b"\n" + adapter + b"\n" + lobby,
    }
    unsigned = output / "sprk-accounts-intermediate.apk"
    with zipfile.ZipFile(source) as archive, zipfile.ZipFile(unsigned, "w") as target:
        patched_config, config = patch_config(archive.read(CONFIG_ENTRY), host_url)
        replacements = {"classes.dex": application_dex, "classes2.dex": dex, CONFIG_ENTRY: patched_config}
        for entry in archive.infolist():
            if not is_signature(entry.filename):
                target.writestr(entry, replacements.get(entry.filename, archive.read(entry.filename)))
        for name, content in added.items():
            target.writestr(name, content, compress_type=zipfile.ZIP_DEFLATED)
    signer = tools / "uber-apk-signer-1.3.0.jar"
    run("java", "-jar", signer, "--apks", unsigned, "--out", output / "signed")
    signed, = (output / "signed").glob("*.apk")
    verification = subprocess.run(["java", "-jar", str(signer), "--apks", str(signed), "--onlyVerify"],
                                  check=True, capture_output=True, text=True)
    (output / "signature-verification.txt").write_text(verification.stdout + verification.stderr, encoding="utf-8")
    with zipfile.ZipFile(source) as archive, zipfile.ZipFile(signed) as target:
        if target.testzip():
            raise ValueError("Signed APK has an invalid ZIP entry")
        old_names = {n for n in archive.namelist() if not is_signature(n)}
        new_names = {n for n in target.namelist() if not is_signature(n)}
        if new_names - old_names != set(added) or old_names - new_names:
            raise ValueError("Unexpected APK payload additions or removals")
        changed = sorted(n for n in old_names if archive.read(n) != target.read(n))
        if changed != sorted(replacements):
            raise ValueError(f"Unexpected changed entries: {changed}")
        for name, content in {**replacements, **added}.items():
            if target.read(name) != content:
                raise ValueError(f"Final payload mismatch: {name}")
        _, final_config = read_config(target.read(CONFIG_ENTRY))
        if final_config != config:
            raise ValueError("Final config changed")
    if sha256(source.read_bytes()) != SOURCE_HASH:
        raise ValueError("Original APK changed")
    report = {
        "signed_apk": str(signed), "signed_sha256": sha256(signed.read_bytes()),
        "source_sha256": SOURCE_HASH, "host_url": host_url,
        "changed_entries": changed, "added_entries": {n: sha256(b) for n, b in added.items()},
        "adapter_sha256": sha256(adapter), "lobby_adapter_sha256": sha256(lobby),
        "dependencies": {n: h for n, (_, h) in TOOLS.items()},
        "signature_verified": True, "unchanged_payload_verified": True,
        "runtime_tested": False, "native_abis": ["x86"],
        "signing": "Same development certificate as the endpoint-test APK; not a production publisher key",
        "authentication": "Server password login/register, access-token verification, then native login with a game ticket",
        "legacy_appguard_bootstrap_disabled": True,
        "passwords_persisted": False, "refresh_tokens_persisted": False,
        "frida_interaction": "Local bundled script only; no debugger listener or remote scripts",
    }
    (output / "build-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=ROOT.parent / "Pre-Doomsday/KingsRaid 9-1-2022.apk")
    parser.add_argument("--host-url", default="http://127.0.0.1:8080/host.json")
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--decoded", type=Path, help="Disposable original apktool decode to reuse")
    args = parser.parse_args()
    build(args.source.resolve(strict=True), args.output.resolve(), args.host_url,
          args.decoded.resolve(strict=True) if args.decoded else None)
