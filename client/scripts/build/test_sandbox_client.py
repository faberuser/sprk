import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest

from client.scripts.build import sandbox_client as sandbox


class SandboxSafetyTests(unittest.TestCase):
    def test_remove_refuses_a_folder_it_did_not_create(self):
        with tempfile.TemporaryDirectory() as folder:
            game = Path(folder) / "sprk-client"
            (game / "King's Raid_Data/Documents").mkdir(parents=True)
            with self.assertRaises(SystemExit):
                sandbox.remove(game)
            self.assertTrue((game / "King's Raid_Data/Documents").is_dir())

    def test_create_refuses_a_non_game_folder(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            with self.assertRaises(SystemExit):
                sandbox.create(root, root / "sandbox")
            self.assertFalse((root / "sandbox").exists())


@unittest.skipUnless(os.name == "nt", "Windows process and junction checks")
class RunningProgramTests(unittest.TestCase):
    def setUp(self):
        self.start = os.getcwd()
        self.addCleanup(os.chdir, self.start)

    def relative(self, folder: Path) -> Path:
        """`../name` from a sibling directory, like ../sprk-client-test from the repository."""
        os.chdir(folder.parent)
        return Path("..") / folder.parent.name / folder.name

    def test_finds_programs_by_relative_or_absolute_folder(self):
        folder = Path(sys.executable).parent
        for path in (folder, folder / "Lib" / "..", self.relative(folder)):
            with self.subTest(path=str(path)):
                self.assertTrue(any(f"PID {os.getpid()}" in entry for entry in sandbox.processes_in(path)))

    def test_remove_refuses_while_a_program_runs_from_the_copy(self):
        with tempfile.TemporaryDirectory() as folder:
            test_copy = Path(folder) / "sprk-client-test"
            test_copy.mkdir()
            (test_copy / sandbox.MARKER).write_text("test")
            program = test_copy / "waiting.exe"
            shutil.copy2(Path(os.environ["SystemRoot"]) / "System32/ping.exe", program)
            running = subprocess.Popen([str(program), "-n", "30", "127.0.0.1"], stdout=subprocess.DEVNULL)
            try:
                relative = self.relative(test_copy)
                with self.assertRaisesRegex(SystemExit, "Close these first"):
                    sandbox.remove(relative)
                self.assertTrue((test_copy / sandbox.MARKER).is_file())
            finally:
                running.kill()
                running.wait()
                os.chdir(self.start)  # leave the folder before it is deleted
            sandbox.remove(test_copy)
            self.assertFalse(test_copy.exists())


if __name__ == "__main__":
    unittest.main()
