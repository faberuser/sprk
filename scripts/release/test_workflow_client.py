"""Run the release-selection step as a temporary Python script, like Actions."""
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest


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
            env = dict(os.environ, GITHUB_WORKSPACE=str(repo), GITHUB_OUTPUT=str(output), INPUT_TAG=tag)
            # -I excludes PYTHONPATH and the current directory: imports must use
            # the explicit checkout path, not incidental local interpreter setup.
            result = subprocess.run([sys.executable, "-I", str(entry)], cwd=repo,
                                    env=env, capture_output=True, text=True)
            return result, output.read_text() if output.exists() else None

    def test_blank_override_selects_normal_build_from_temporary_script(self):
        result, output = self.run_selection("")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(output, "tag=\n")

    def test_invalid_override_is_rejected_before_network_access(self):
        result, output = self.run_selection("invalid/release")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Expected a client-<version> release tag", result.stderr)
        self.assertIsNone(output)


if __name__ == "__main__":
    unittest.main()
