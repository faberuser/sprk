"""Reproduce the pinned illustration runtime and single-file NCNN model artifact.

No model is installed into the client. The writable active model is provisioned
only by an explicit viewer request. The archive hash pins binaries and weights.
"""
import argparse
import hashlib
import io
import json
from pathlib import Path
import struct
import urllib.request
import zipfile

URL = "https://github.com/xinntao/Real-ESRGAN/releases/download/v0.2.5.0/realesrgan-ncnn-vulkan-20220424-windows.zip"
SHA256 = "abc02804e17982a3be33675e4d471e91ea374e65b70167abc09e31acb412802d"
VERSION = "realesrgan-anime-20220424-v1"
INTERFACE = "sprk-ncnn-rrdb-rgb-x4-v1"


def pack_model(param: bytes, weights: bytes) -> bytes:
    if not (0 < len(param) <= 512 * 1024 and 0 < len(weights) <= 64 * 1024**2):
        raise ValueError("Model exceeds interface limits")
    if not param.startswith(b"7767517") or b"data" not in param or b"output" not in param:
        raise ValueError("Expected NCNN param text with data/output tensor names")
    return b"SPRKSR01" + struct.pack("<II", len(param), len(weights)) + param + weights


def prepare(archive: bytes, client: Path | None, server: Path | None):
    if hashlib.sha256(archive).hexdigest() != SHA256:
        raise ValueError("Illustration runtime archive hash mismatch")
    with zipfile.ZipFile(io.BytesIO(archive)) as bundle:
        def read(name):
            matches = [n for n in bundle.namelist() if n.endswith("/" + name) or n == name]
            if len(matches) != 1:
                raise ValueError("Missing/ambiguous runtime member: " + name)
            return bundle.read(matches[0])
        param = read("models/realesrgan-x4plus-anime.param")
        weights = read("models/realesrgan-x4plus-anime.bin")
        model = pack_model(param, weights)
        descriptor = {
            "version": VERSION, "interface": INTERFACE, "native_scale": 4,
            "size": len(model), "sha256": hashlib.sha256(model).hexdigest(),
            "url": "https://updates.krinfo.net/updates/models/" + VERSION + "/model.sprkmodel",
            "source": URL, "archive_sha256": SHA256,
        }
        if client:
            client.mkdir(parents=True, exist_ok=True)
            for name in ("realesrgan-ncnn-vulkan.exe", "vcomp140.dll"):
                (client / name).write_bytes(read(name))
            (client / "default-model.json").write_text(json.dumps(descriptor, indent=2) + "\n", encoding="utf-8")
        if server:
            output = server / VERSION
            output.mkdir(parents=True, exist_ok=True)
            for name, data in (("model.sprkmodel", model), ("metadata.json", (json.dumps(descriptor, indent=2) + "\n").encode())):
                target = output / name
                if target.exists() and target.read_bytes() != data:
                    raise ValueError("Refusing to replace versioned model artifact: " + str(target))
                target.write_bytes(data)
        return descriptor


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--client", type=Path, help="Managed/SprkIllustrationRuntime output directory")
    parser.add_argument("--server", type=Path, help="Versioned server artifact root")
    parser.add_argument("--archive", type=Path)
    parser.add_argument("--pack-model", type=Path, help="Package a compatible replacement; does not download or install it")
    parser.add_argument("--param", type=Path)
    parser.add_argument("--weights", type=Path)
    args = parser.parse_args()
    if args.pack_model:
        if not (args.param and args.weights) or args.client or args.server:
            parser.error("--pack-model requires --param and --weights, without --client/--server")
        model = pack_model(args.param.read_bytes(), args.weights.read_bytes())
        with args.pack_model.open("xb") as output:
            output.write(model)
        print("Packaged " + INTERFACE + ": " + hashlib.sha256(model).hexdigest())
        return
    if not (args.client or args.server):
        parser.error("Specify --client and/or --server")
    archive = args.archive.read_bytes() if args.archive else urllib.request.urlopen(URL, timeout=120).read(100 * 1024**2)
    print(json.dumps(prepare(archive, args.client, args.server), indent=2))


if __name__ == "__main__":
    main()
