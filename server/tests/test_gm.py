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
        rows = [{"AccountId": i, "Username": f"user{i}", "Nickname": "ชื่อ, \"Hero\"\nTwo",
                 "Online": i % 2 == 1, "LastOnlineTime": None if i == 2 else "2026-11-05 01:02:03"}
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
            self.assertEqual(exported[-1], {**rows[-1], "AccountId": "101", "Online": "True"})
            self.assertEqual((exported[1]["Online"], exported[1]["LastOnlineTime"]), ("False", ""))

    def test_export_json_empty_and_exact_page_boundary(self):
        rows = [{"AccountId": i, "Username": None, "Nickname": "英雄", "Online": False, "LastOnlineTime": None}
                for i in range(1, 101)]
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
        player = {"AccountId": 100, "Username": "user", "Nickname": None, "Online": True, "LastOnlineTime": None}
        first = {"Players": [player], "NextAfter": 100}
        # An older server without online status must not produce a partial export.
        legacy = {"Players": [{"AccountId": 101, "Username": "u", "Nickname": None}], "NextAfter": None}
        for failed_page in ({}, {"Players": [], "NextAfter": 100}, first, legacy):
            with self.subTest(page=failed_page), tempfile.TemporaryDirectory() as directory:
                output = Path(directory) / "players.csv"
                with patch.dict(os.environ, {"SPRK_GM_KEY": "test-key"}), \
                        patch.object(gm, "post", side_effect=[first, failed_page]), \
                        contextlib.redirect_stdout(io.StringIO()):
                    self.assertFalse(gm.cmd_export_players({}, output))
                self.assertFalse(output.exists())

    def test_export_overwrites_existing_file_only_on_success(self):
        player = {"AccountId": 1, "Username": "user", "Nickname": None, "Online": False, "LastOnlineTime": None}
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "players.json"
            output.write_text("existing")
            with patch.dict(os.environ, {"SPRK_GM_KEY": "test-key"}), \
                    patch.object(gm, "post", return_value={}), contextlib.redirect_stdout(io.StringIO()):
                self.assertFalse(gm.cmd_export_players({}, output))
            self.assertEqual(output.read_text(), "existing")
            with patch.dict(os.environ, {"SPRK_GM_KEY": "test-key"}), \
                    patch.object(gm, "post", return_value={"Players": [player], "NextAfter": None}), \
                    contextlib.redirect_stdout(io.StringIO()):
                self.assertTrue(gm.cmd_export_players({}, output))
            self.assertEqual(json.loads(output.read_text(encoding="utf-8")), [player])
            self.assertEqual(sorted(p.name for p in Path(directory).iterdir()), ["players.json"])

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

    def test_festival_commands_need_no_player_and_send_the_override(self):
        with tempfile.TemporaryDirectory() as directory:
            with patch.object(gm, "CONFIG_FILE", Path(directory) / "config.json"):
                status = {"Festivals": [{"Key": "halloween", "Open": True, "End": "2026-11-08 00:00:00",
                                         "Override": {"Mode": "Open"}}]}
                with patch.object(gm, "post", return_value=status) as post, contextlib.redirect_stdout(io.StringIO()) as out:
                    with patch("sys.argv", ["gm.py", "festival", "halloween", "open", "--hours", "3"]):
                        gm.main()
                    self.assertEqual(post.call_args.args[1:], ("/cheat/festivals", {"Key": "halloween", "Mode": "Open", "Hours": 3}))
                    with patch("sys.argv", ["gm.py", "festivals"]):
                        gm.main()
                    self.assertEqual(post.call_args.args[1:], ("/cheat/festivals", {}))
                self.assertIn("OPEN   halloween", out.getvalue())
                self.assertIn("[GM Open]", out.getvalue())

    def test_world_boss_command_forces_by_name_and_resets(self):
        with tempfile.TemporaryDirectory() as directory:
            with patch.object(gm, "CONFIG_FILE", Path(directory) / "config.json"):
                status = {"Active": 4, "Rotation": 5, "Forced": {"Until": "2026-11-27 00:00:00"}}
                with patch.object(gm, "post", return_value=status) as post, contextlib.redirect_stdout(io.StringIO()) as out:
                    with patch("sys.argv", ["gm.py", "world-boss", "protianus", "--hours", "6"]):
                        gm.main()
                    self.assertEqual(post.call_args.args[1:], ("/cheat/world-boss", {"Index": 4, "Hours": 6}))
                    with patch("sys.argv", ["gm.py", "world-boss", "auto"]):
                        gm.main()
                    self.assertEqual(post.call_args.args[2]["Index"], 0)
                    with patch("sys.argv", ["gm.py", "world-boss"]):
                        gm.main()
                    self.assertEqual(post.call_args.args[1:], ("/cheat/world-boss", {}))
                self.assertIn("Active: Protianus  [GM until", out.getvalue())
                self.assertIn("Rotation this week: Xanadus", out.getvalue())

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

    def test_name_tag_commands_post_gm_actions_and_shrink_icons(self):
        import base64
        try:
            from PIL import Image
        except ImportError:
            self.skipTest("Pillow is not installed")
        tag = {"Name": "wao", "Label": "WAO", "TextColor": "A8E6A1", "BackgroundColor": "1E1F22", "HasIcon": True, "Members": 0}
        with tempfile.TemporaryDirectory() as directory:
            icon = Path(directory) / "leaf.png"
            Image.new("RGBA", (300, 150), (110, 200, 90, 255)).save(icon)
            config = Path(directory) / "config.json"
            config.write_text(json.dumps({"server": gm.DEFAULT_SERVER, "account_id": 42}))
            with patch.object(gm, "CONFIG_FILE", config), \
                    patch.object(gm, "post", return_value={"Tag": tag, "AccountId": 42, "Removed": "wao"}) as post, \
                    contextlib.redirect_stdout(io.StringIO()) as out:
                with patch("sys.argv", ["gm.py", "name-tag", "create", "wao", "WAO", "--icon", str(icon), "--text-color", "a8e6a1"]):
                    gm.main()
                path, fields = post.call_args.args[1:]
                self.assertEqual(path, "/cheat/name-tag")
                self.assertEqual({k: v for k, v in fields.items() if k != "Icon"},
                                 {"Action": "create", "Name": "wao", "Label": "WAO", "TextColor": "a8e6a1"})
                with Image.open(io.BytesIO(base64.b64decode(fields["Icon"]))) as sent:
                    self.assertEqual((sent.format, sent.size), ("PNG", (64, 32)))
                # Assign/remove default to the selected account; remove maps to unassign.
                with patch("sys.argv", ["gm.py", "name-tag", "assign", "wao"]):
                    gm.main()
                self.assertEqual(post.call_args.args[2], {"Action": "assign", "Name": "wao", "AccountId": 42})
                with patch("sys.argv", ["gm.py", "name-tag", "remove", "7"]):
                    gm.main()
                self.assertEqual(post.call_args.args[2], {"Action": "unassign", "AccountId": 7})
                with patch("sys.argv", ["gm.py", "name-tag", "update", "wao", "--no-icon", "--label", "GM"]):
                    gm.main()
                self.assertEqual(post.call_args.args[2], {"Action": "update", "Name": "wao", "Label": "GM", "RemoveIcon": "true"})
            self.assertIn("Icon shrunk to 64x32 px", out.getvalue())

    def test_name_tag_icon_without_pillow_accepts_only_small_pngs(self):
        small = b"\x89PNG\r\n\x1a\n" + b"\x00\x00\x00\x0dIHDR" + (32).to_bytes(4, "big") * 2 + b"\x00" * 40
        large = b"\x89PNG\r\n\x1a\n" + b"\x00\x00\x00\x0dIHDR" + (512).to_bytes(4, "big") * 2 + b"\x00" * 40
        with tempfile.TemporaryDirectory() as directory, patch.dict("sys.modules", {"PIL": None}):
            for name, data in (("small.png", small), ("large.png", large)):
                (Path(directory) / name).write_bytes(data)
            self.assertTrue(gm.prepare_icon(str(Path(directory) / "small.png")))
            with self.assertRaisesRegex(ValueError, "Install Pillow"):
                gm.prepare_icon(str(Path(directory) / "large.png"))


if __name__ == "__main__":
    unittest.main()
