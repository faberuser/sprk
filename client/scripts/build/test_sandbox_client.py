from pathlib import Path
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

    def test_remove_deletes_a_marked_sandbox_without_links(self):
        with tempfile.TemporaryDirectory() as folder:
            test = Path(folder) / "sprk-client-test"
            (test / "King's Raid_Data/Managed").mkdir(parents=True)
            (test / sandbox.MARKER).write_text("test")
            self.assertEqual(sandbox.links_inside(test), [])
            sandbox.remove(test)
            self.assertFalse(test.exists())

    def test_create_refuses_an_existing_folder_and_a_non_game_folder(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            with self.assertRaises(SystemExit):
                sandbox.create(root, root / "sandbox")
            self.assertFalse((root / "sandbox").exists())


if __name__ == "__main__":
    unittest.main()
