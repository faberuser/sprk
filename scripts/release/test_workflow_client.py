"""Run the release-selection step as a temporary Python script, like Actions."""
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch
import json


class ReleaseSelectionWorkflowTests(unittest.TestCase):
    def run_selection(self, tag):
        repo = Path(__file__).resolve().parents[2]
        workflow = (repo / ".github/workflows/build-and-deploy.yml").read_text(encoding="utf-8")
        step = workflow.split("        id: reuse\n", 1)[1].split("\n      - name:", 1)[0]
        block = step.split("        run: |\n", 1)[1]
        script = "\n".join(line[10:] if line.startswith("          ") else line
                           for line in block.splitlines())
        with tempfile.TemporaryDirectory(prefix="sprk-actions-python-") as directory:
            temp = Path(directory)
            entry = temp / "actions-step.py"
            output = temp / "output"
            entry.write_text(script, encoding="utf-8")
            env = dict(os.environ, GITHUB_WORKSPACE=str(repo), GITHUB_OUTPUT=str(output), INPUT_TAG=tag,
                       FORCE_CLIENT_BUILD="false" if tag else "true", RELEASE_REPOSITORY="owner/repo")
            # -I excludes PYTHONPATH and the current directory: imports must use
            # the explicit checkout path, not incidental local interpreter setup.
            result = subprocess.run([sys.executable, "-I", str(entry)], cwd=repo,
                                    env=env, capture_output=True, text=True)
            return result, output.read_text() if output.exists() else None

    def test_forced_build_runs_from_temporary_script_without_network_access(self):
        result, output = self.run_selection("")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(output, "tag=\n")

    def test_invalid_override_is_rejected_before_network_access(self):
        result, output = self.run_selection("invalid/release")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Expected a client-<version> release tag", result.stderr)
        self.assertIsNone(output)


class PullRequestClientBuildTests(unittest.TestCase):
    def test_pr_runs_release_build_with_throwaway_key_and_no_secrets(self):
        repo = Path(__file__).resolve().parents[2]
        workflow = (repo / ".github/workflows/test-pr.yml").read_text(encoding="utf-8")
        job = workflow.split("  client-release:\n", 1)[1].split("\n  native-tests:", 1)[0]
        self.assertIn("runs-on: windows-latest", job)
        self.assertIn("build_client_release.py", job)
        self.assertIn("--check-only", job)
        self.assertNotIn("--publish", job)
        self.assertNotIn("secrets.", job)
        self.assertNotIn("CLIENT_UPDATE_SIGNING_KEY", job)
        self.assertNotIn("contents: write", job)


class NativeBattleReleaseSelectionTests(unittest.TestCase):
    def select(self, releases):
        from scripts.release import latest_client_release
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "output"
            with patch.dict(os.environ, RELEASE_REPOSITORY="owner/repo", GITHUB_OUTPUT=str(output)), \
                    patch("subprocess.check_output", return_value=json.dumps(releases).encode()) as request:
                latest_client_release.main()
                request.assert_called_once_with(["gh", "api", "repos/owner/repo/releases?per_page=100"])
            return output.read_text()

    def test_both_server_jobs_use_the_shared_selection_and_run_in_parallel(self):
        repo = Path(__file__).resolve().parents[2]
        workflow = (repo / ".github/workflows/test-pr.yml").read_text(encoding="utf-8")
        self.assertEqual(workflow.count("run: python3 scripts/release/latest_client_release.py"), 2)
        self.assertNotIn("needs:", workflow)

    @staticmethod
    def release(tag, date, **overrides):
        value = dict(tag_name=tag, published_at=date, draft=False, prerelease=False,
                     assets=[dict(name=name, state="uploaded") for name in
                             ("client-update-manifest.json", "client-update.tar.gz")])
        value.update(overrides)
        return value

    def test_selects_newest_complete_published_client_and_ignores_other_releases(self):
        self.assertEqual(self.select([
            self.release("client-new", "2026-10-24T08:00:00Z"),
            self.release("client-old", "2026-10-23T08:00:00Z"),
            self.release("worker-runtime-newer", "2026-10-25T08:00:00Z"),
            self.release("client-draft", "2026-10-25T08:00:00Z", draft=True),
            self.release("client-preview", "2026-10-25T08:00:00Z", prerelease=True),
            self.release("client-incomplete", "2026-10-25T08:00:00Z", assets=[]),
            self.release("client-not-uploaded", "2026-10-25T08:00:00Z", assets=[
                dict(name="client-update-manifest.json", state="uploaded"),
                dict(name="client-update.tar.gz", state="new")]),
            self.release("client-invalid\nvalue", "2026-10-25T08:00:00Z"),
        ]), "tag=client-new\n")

    def test_missing_client_release_fails_instead_of_skipping_native_checks(self):
        with self.assertRaisesRegex(SystemExit, "No complete published client release"):
            self.select([self.release("worker-runtime-only", "2026-10-24T08:00:00Z")])


if __name__ == "__main__":
    unittest.main()
