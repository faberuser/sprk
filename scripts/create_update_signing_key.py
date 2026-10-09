#!/usr/bin/env python3
"""Create a release-signing private key and public key. Never ship the private key."""
import argparse
from pathlib import Path
import subprocess

from publish_client_update import find_openssl


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, default=Path("update-signing"))
    args = parser.parse_args()
    try:
        args.output.mkdir(parents=True, exist_ok=True)
        private = args.output / "private.pem"
        public = args.output / "public.pem"
        if private.exists() or public.exists():
            raise ValueError("Signing keys already exist; keep them for later releases")
        openssl = find_openssl()
        subprocess.run([openssl, "genpkey", "-algorithm", "RSA", "-pkeyopt", "rsa_keygen_bits:3072", "-out", str(private)], check=True, capture_output=True)
        private.chmod(0o600)
        subprocess.run([openssl, "pkey", "-in", str(private), "-pubout", "-out", str(public)], check=True, capture_output=True)
        print(f"Private signing key: {private} (keep on your publishing machine)")
        print(f"Launcher public key: {public}")
    except (OSError, ValueError, subprocess.CalledProcessError) as error:
        parser.exit(1, f"Key generation failed: {error}\n")


if __name__ == "__main__":
    main()
