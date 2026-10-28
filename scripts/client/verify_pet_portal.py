"""Exercise both native Portal Pet buttons without login or account actions."""
import argparse
import os
import shutil
from pathlib import Path
import subprocess
import uuid

from verify_graphics_options import prepare, REPO


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--client', type=Path, default=REPO.parent / 'sprk-client')
    parser.add_argument('--staged', action='store_true')
    args = parser.parse_args()
    root = REPO / 'target/pet-portal-validation' / uuid.uuid4().hex[:12]
    prepare(args.client.resolve(), root, installed=True)
    if args.staged:
        managed = Path("King's Raid_Data") / 'Managed'
        for name, suffix in (('Assembly-CSharp.dll', '.pet-presentation-staged'),
                             ('SprkPetPresentation.dll', '.staged')):
            shutil.copy2(args.client / managed / (name + suffix), root / managed / name)
    output = root / 'probe'
    output.mkdir()
    env = dict(os.environ, SPRK_PET_PORTAL_TEST=str(output),
               SPRK_AUTOMATION_DIR=str(root / 'automation'),
               SPRK_AUTOMATION_PROFILE='pet-portal-' + root.name)
    startup = subprocess.STARTUPINFO()
    startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
    startup.wShowWindow = 4
    executable = next(root.glob('SprkTest-graphics-*.exe'))
    print(root, flush=True)
    process = subprocess.Popen([str(executable), '-screen-fullscreen', '0',
        '-screen-width', '640', '-screen-height', '480', '-logFile', str(root / 'unity.log')],
        cwd=root, env=env, startupinfo=startup)
    try:
        code = process.wait(timeout=90)
    except subprocess.TimeoutExpired:
        process.terminate()  # Only this isolated fixture, never the user's client.
        process.wait(timeout=15)
        raise RuntimeError(f'Pet fixture timed out: {root / "unity.log"}')
    result = (output / 'result.txt').read_text() if (output / 'result.txt').exists() else 'FAIL no result'
    print(result)
    if code or not result.startswith('PASS '):
        raise RuntimeError(f'Pet fixture failed: {root / "unity.log"}')


if __name__ == '__main__':
    main()
