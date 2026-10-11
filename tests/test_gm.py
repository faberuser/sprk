import contextlib
import csv
import importlib.util
import io
import json
import os
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location("gm", Path(__file__).resolve().parents[1] / "gm.py")
gm = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gm)


class GmTests(unittest.TestCase):
    def test_export_all_pages_prompts_once_without_selected_account(self):
        rows = [{"AccountId": i, "Username": f"user{i}", "Nickname": "ชื่อ, \"Hero\"\nTwo"}
                for i in range(1, 102)]
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "players.csv"
            with patch.object(gm, "CONFIG_FILE", Path(directory) / "config.json"), \
                    patch.dict(os.environ, {"SPRK_GM_KEY": ""}), \
                    patch.object(gm.getpass, "getpass", return_value="test-key") as prompt, \
                    patch.object(gm, "post", side_effect=[
                        {"Players": rows[:100], "NextAfter": 100},
                        {"Players": rows[100:], "NextAfter": None},
                    ]) as post, contextlib.redirect_stdout(io.StringIO()), \
                    patch("sys.argv", ["gm.py", "export-players", "--output", str(output)]):
                gm.main()
            prompt.assert_called_once()
            self.assertEqual([call.args[2] for call in post.call_args_list],
                             [{"Query": "", "After": 0}, {"Query": "", "After": 100}])
            self.assertTrue(all(call.kwargs == {"key": "test-key"} for call in post.call_args_list))
            with output.open(encoding="utf-8", newline="") as stream:
                exported = list(csv.DictReader(stream))
            self.assertEqual(len(exported), 101)
            self.assertEqual(exported[-1], {**rows[-1], "AccountId": "101"})

    def test_export_json_empty_and_exact_page_boundary(self):
        rows = [{"AccountId": i, "Username": None, "Nickname": "英雄"} for i in range(1, 101)]
        for initial in ([], rows):
            with self.subTest(count=len(initial)), tempfile.TemporaryDirectory() as directory:
                output = Path(directory) / "players.json"
                pages = ([{"Players": initial, "NextAfter": 100}] if initial else [])
                pages.append({"Players": [], "NextAfter": None})
                with patch.dict(os.environ, {"SPRK_GM_KEY": "test-key"}), \
                        patch.object(gm, "post", side_effect=pages), contextlib.redirect_stdout(io.StringIO()):
                    self.assertTrue(gm.cmd_export_players({"server": gm.DEFAULT_SERVER}, output))
                self.assertEqual(json.loads(output.read_text(encoding="utf-8")), initial)

    def test_export_failure_does_not_write_partial_results(self):
        first = {"Players": [{"AccountId": 100, "Username": "user", "Nickname": None}], "NextAfter": 100}
        for failed_page in ({}, {"Players": [], "NextAfter": 100}, first):
            with self.subTest(page=failed_page), tempfile.TemporaryDirectory() as directory:
                output = Path(directory) / "players.csv"
                with patch.dict(os.environ, {"SPRK_GM_KEY": "test-key"}), \
                        patch.object(gm, "post", side_effect=[first, failed_page]), \
                        contextlib.redirect_stdout(io.StringIO()):
                    self.assertFalse(gm.cmd_export_players({}, output))
                self.assertFalse(output.exists())

    def test_export_preserves_existing_file(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "players.json"
            output.write_text("existing")
            with patch.object(gm, "post") as post, contextlib.redirect_stdout(io.StringIO()):
                self.assertFalse(gm.cmd_export_players({}, output))
            post.assert_not_called()
            self.assertEqual(output.read_text(), "existing")

    def test_account_selection_and_explicit_currency_only(self):
        with tempfile.TemporaryDirectory() as directory:
            with patch.object(gm, "CONFIG_FILE", Path(directory) / "config.json"):
                gm.save_config({"server": gm.DEFAULT_SERVER, "session_id": "old-session"})
                with patch("sys.argv", ["gm.py", "account", "42"]):
                    gm.main()
                self.assertEqual(gm.target(gm.load_config()), {"AccountId": 42})
                self.assertNotIn("session_id", gm.load_config())
                with patch.object(gm, "post", return_value={}) as post, contextlib.redirect_stdout(io.StringIO()):
                    with patch("sys.argv", ["gm.py", "--account-id", "43", "currency", "--gold", "10"]):
                        gm.main()
                    self.assertEqual(post.call_args.args[2], {"AccountId": 43, "Gold": 10, "Gem": 0, "Stamina": 0})
                self.assertEqual(gm.load_config()["account_id"], 42)

    def test_lookup_needs_no_player_target_and_key_is_header_only(self):
        class Response:
            def __enter__(self): return self
            def __exit__(self, *args): pass
            def read(self): return b'{"Players":[]}'
        with patch.dict(os.environ, {"SPRK_GM_KEY": "test-secret"}), patch.object(gm.urllib.request, "build_opener") as opener:
            opener.return_value.open.return_value = Response()
            with contextlib.redirect_stdout(io.StringIO()):
                gm.cmd_players({"server": gm.DEFAULT_SERVER}, "Raider", 100)
            request = opener.return_value.open.call_args.args[0]
            self.assertEqual(request.get_header("Authorization"), "Bearer test-secret")
            self.assertEqual(request.data, b"Query=Raider&After=100")
            self.assertNotIn("test-secret", request.full_url)
            self.assertIs(opener.call_args.args[0], gm.NoRedirect)
        self.assertIsNone(gm.NoRedirect().redirect_request(None, None, 302, None, None, "https://other/"))


if __name__ == "__main__":
    unittest.main()
