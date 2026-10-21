"""Create and verify a signed Android endpoint-test APK without altering its source.

Requires requirements-client.txt, Java, and uber-apk-signer 1.3.0. This only
redirects startup discovery; it does not port Windows login or gameplay patches.
"""

import argparse
import hashlib
import io
import json
from pathlib import Path
import re
import subprocess
from urllib.parse import urlsplit
import zipfile

import UnityPy


CONFIG_ENTRY = "assets/bin/Data/eea9244ef20290443a0879ad48b7e871"
ORIGINAL_HOST = "https://patch.kingsraid.com/4/host_4.88_406e2e237035.json"
SIGNER_SHA256 = "e1299fd6fcf4da527dd53735b56127e8ea922a321128123b9c32d619bba1d835"


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def is_signature(name):
    return bool(re.fullmatch(r"META-INF/(?:MANIFEST\.MF|[^/]+\.(?:SF|RSA|DSA|EC))", name, re.I))


def read_config(data):
    env = UnityPy.load(io.BytesIO(data))
    configs = [obj.read_typetree() for obj in env.objects if obj.type.name == "TextAsset"]
    configs = [obj for obj in configs if obj.get("m_Name") == "Config"]
    if len(configs) != 1:
        raise ValueError("Expected exactly one Config TextAsset")
    script = configs[0]["m_Script"]
    return script, json.loads(script)


def patch_config(data, host_url):
    script, config = read_config(data)
    if config.get("QueryHost") != ORIGINAL_HOST:
        raise ValueError("Source does not contain the expected original QueryHost")
    if config.get("Platform") != "Android":
        raise ValueError("Source config is not Android")
    # Preserve every Unity byte offset by keeping the serialized string length.
    # JSON permits trailing whitespace; padding is outside the URL string.
    old = script.encode("utf-8", errors="surrogateescape")
    new = script.replace(ORIGINAL_HOST, host_url).encode("utf-8", errors="surrogateescape")
    if len(new) > len(old):
        raise ValueError("Host URL is too long for this offset-preserving patch")
    new += b" " * (len(old) - len(new))
    if data.count(old) != 1:
        raise ValueError("Config string is not unique in its serialized asset")
    patched = data.replace(old, new, 1)
    _, result = read_config(patched)
    expected = dict(config, QueryHost=host_url)
    if result != expected or len(patched) != len(data):
        raise ValueError("Patched asset validation failed")
    return patched, result


def verify_apk(source, signed, host_url):
    with zipfile.ZipFile(source) as original, zipfile.ZipFile(signed) as result:
        if original.testzip() or result.testzip():
            raise ValueError("APK CRC verification failed")
        source_names = {n for n in original.namelist() if not is_signature(n)}
        result_names = {n for n in result.namelist() if not is_signature(n)}
        if source_names != result_names:
            raise ValueError("APK payload entry names changed unexpectedly")
        changed = []
        for name in sorted(source_names):
            if original.read(name) != result.read(name):
                changed.append(name)
        if changed != [CONFIG_ENTRY]:
            raise ValueError(f"Unexpected changed payload entries: {changed}")
        expected_asset, _ = patch_config(original.read(CONFIG_ENTRY), host_url)
        if result.read(CONFIG_ENTRY) != expected_asset:
            raise ValueError("Signed Config asset differs from intended patch")
        _, config = read_config(result.read(CONFIG_ENTRY))
        abis = sorted({n.split('/')[1] for n in source_names if n.startswith('lib/') and n.endswith('.so')})
        return config, abis, len(source_names)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--host-url", required=True)
    parser.add_argument("--signer", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True, help="New, unique build directory")
    args = parser.parse_args()
    parsed = urlsplit(args.host_url)
    if parsed.scheme not in ("http", "https") or not parsed.netloc or parsed.username or parsed.password:
        parser.error("--host-url must be an HTTP(S) URL without credentials")
    if parsed.fragment or parsed.query or not parsed.path.endswith("/host.json"):
        parser.error("--host-url must end in /host.json without a query or fragment")
    source = args.source.resolve(strict=True)
    signer = args.signer.resolve(strict=True)
    if sha256(signer.read_bytes()) != SIGNER_SHA256:
        parser.error("Expected checksum-verified uber-apk-signer 1.3.0")
    source_hash = sha256(source.read_bytes())
    # Fail on existing directories rather than overwrite a previous test build.
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    unsigned = output / "sprk-android-endpoint-unsigned.apk"
    with zipfile.ZipFile(source) as original:
        patched, config = patch_config(original.read(CONFIG_ENTRY), args.host_url)
        with zipfile.ZipFile(unsigned, "w") as target:
            target.comment = original.comment
            for entry in original.infolist():
                if not is_signature(entry.filename):
                    target.writestr(entry, patched if entry.filename == CONFIG_ENTRY else original.read(entry.filename))
    subprocess.run(["java", "-jar", str(signer), "--apks", str(unsigned), "--out", str(output / "signed")], check=True)
    signed_files = list((output / "signed").glob("*.apk"))
    if len(signed_files) != 1:
        raise ValueError("Expected one signed APK")
    signed = signed_files[0]
    verify = subprocess.run(
        ["java", "-jar", str(signer), "--apks", str(signed), "--onlyVerify"],
        check=True, text=True, capture_output=True,
    )
    (output / "signature-verification.txt").write_text(verify.stdout + verify.stderr, encoding="utf-8")
    config, abis, payload_count = verify_apk(source, signed, args.host_url)
    if sha256(source.read_bytes()) != source_hash:
        raise ValueError("Source APK changed during the build")
    report = {
        "source_apk": str(source), "source_sha256": source_hash,
        "signed_apk": str(signed), "signed_sha256": sha256(signed.read_bytes()),
        "query_host": config["QueryHost"], "client_version": config["Version"],
        "native_abis": abis, "changed_payload_entries": [CONFIG_ENTRY],
        "verified_payload_entry_count": payload_count,
        "signer_sha256": SIGNER_SHA256,
        "signing": "uber-apk-signer embedded development certificate; not the original publisher certificate",
        "signature_verified": True, "source_unchanged": True,
        "runtime_tested": False,
        "limitations": [
            "Endpoint discovery only; original authentication, protection SDKs and telemetry remain.",
            "AppGuard may reject a modified or re-signed APK; launch testing is required.",
            "x86 build requires a compatible Android environment; no ARM libraries were added.",
            "Archived external assets are not included in this APK and may need restoring separately.",
        ],
    }
    (output / "build-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
