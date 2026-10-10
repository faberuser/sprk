import contextlib
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
