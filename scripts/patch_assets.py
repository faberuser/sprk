#!/usr/bin/env python3
"""
King's Raid Asset Patcher

Patches resources.assets to change the QueryHost URL from HTTPS to HTTP.
This allows the game to connect to a local private server.

Usage:
    python patch_assets.py "path/to/King's Raid_Data/resources.assets"
    
    To restore:
    python patch_assets.py --restore "path/to/King's Raid_Data/resources.assets"
"""

import sys
import os
import shutil

# Original URL
ORIGINAL_URL = b'https://kr-apne1-patchsrc.masangsoft.com/Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json'

# Replacement URL - MUST be same length (87 bytes)
# http://127.0.0.1:8080/Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json = 68 chars
# Need 19 chars of padding - use spaces which are valid in URL paths before the query
# Actually, we need to keep it as valid JSON string, so pad inside the path
REPLACEMENT_URL = b'http://127.0.0.1:8080/Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json                   '

# Alternative: just change https to http and keep the rest (will fail DNS but shows the approach)
# We can also try replacing just the protocol
# Note: extra space to maintain length
HTTPS_TO_HTTP = (b'https://', b'http:// ')


def patch_file(assets_path):
    """Patch the assets file."""

    if not os.path.exists(assets_path):
        print(f"ERROR: File not found: {assets_path}")
        return False

    # Create backup
    backup_path = assets_path + ".backup"
    if not os.path.exists(backup_path):
        print(f"Creating backup: {backup_path}")
        shutil.copy2(assets_path, backup_path)
    else:
        print(f"Backup already exists: {backup_path}")

    # Read file
    print(f"Reading: {assets_path}")
    with open(assets_path, 'rb') as f:
        data = f.read()

    original_size = len(data)
    print(f"File size: {original_size:,} bytes")

    # Check if already patched
    if b'http://127.0.0.1:8080' in data:
        print("File appears to already be patched!")
        return True

    # Find the original URL
    if ORIGINAL_URL not in data:
        print(f"WARNING: Original URL not found in file")
        print(f"Looking for: {ORIGINAL_URL.decode()}")

        # Try to find any QueryHost
        idx = data.find(b'"QueryHost"')
        if idx != -1:
            # Extract context
            start = idx
            end = min(len(data), idx + 200)
            context = data[start:end]
            print(f"Found QueryHost at offset 0x{idx:X}:")
            # Find the URL within the context
            url_start = context.find(b'http')
            if url_start != -1:
                url_end = context.find(b'"', url_start)
                if url_end != -1:
                    found_url = context[url_start:url_end]
                    print(
                        f"  Current URL: {found_url.decode('utf-8', errors='ignore')}")
        return False

    # Verify lengths match
    if len(REPLACEMENT_URL) != len(ORIGINAL_URL):
        print(f"ERROR: URL lengths don't match!")
        print(f"  Original: {len(ORIGINAL_URL)} bytes")
        print(f"  Replacement: {len(REPLACEMENT_URL)} bytes")
        return False

    # Replace
    count = data.count(ORIGINAL_URL)
    print(f"Found {count} occurrence(s) of the original URL")

    data = data.replace(ORIGINAL_URL, REPLACEMENT_URL)

    # Verify size unchanged
    if len(data) != original_size:
        print(f"ERROR: File size changed! Aborting.")
        return False

    # Write patched file
    print(f"Writing patched file...")
    with open(assets_path, 'wb') as f:
        f.write(data)

    print(f"""
╔══════════════════════════════════════════════════════════════╗
║  ✓ Patch successful!                                         ║
╠══════════════════════════════════════════════════════════════╣
║  Original URL:                                               ║
║  {ORIGINAL_URL.decode()[:56]}...
║                                                              ║
║  Patched URL:                                                ║
║  {REPLACEMENT_URL.decode().strip()[:56]}...
╠══════════════════════════════════════════════════════════════╣
║  Next steps:                                                 ║
║  1. Start the private server (kings-raid-server.exe)         ║
║  2. Launch the game                                          ║
║                                                              ║
║  No hosts file modification needed!                          ║
╚══════════════════════════════════════════════════════════════╝
""")

    return True


def restore_backup(assets_path):
    """Restore from backup."""
    backup_path = assets_path + ".backup"

    if not os.path.exists(backup_path):
        print(f"ERROR: No backup found: {backup_path}")
        return False

    print(f"Restoring from: {backup_path}")
    shutil.copy2(backup_path, assets_path)
    print("✓ Restored original file")
    return True


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        print("\nUsage examples:")
        print('  python patch_assets.py "D:\\Games\\Kings Raid\\King\'s Raid_Data\\resources.assets"')
        print('')
        print('To restore original:')
        print('  python patch_assets.py --restore "D:\\Games\\Kings Raid\\King\'s Raid_Data\\resources.assets"')
        sys.exit(1)

    if sys.argv[1] == '--restore':
        if len(sys.argv) < 3:
            print("ERROR: Please provide the path to resources.assets")
            sys.exit(1)
        success = restore_backup(sys.argv[2])
    else:
        success = patch_file(sys.argv[1])

    sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()
