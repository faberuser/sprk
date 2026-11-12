import hashlib
import json
from pathlib import Path
import tempfile
import unittest

from client.scripts.build import build_managed_client as build


class ReferenceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.inputs = self.root / "inputs"
        self.inputs.mkdir()
        self.data = b"api reference fixture"
        (self.inputs / "mscorlib.dll").write_bytes(self.data)
        self.lock = {"files": [{"path": "mscorlib.dll", "size": len(self.data),
                                "sha256": hashlib.sha256(self.data).hexdigest()}]}
        self.output = self.root / "output"

    def test_only_locked_runtime_files_are_staged(self):
        (self.inputs / "Assembly-CSharp.dll").write_bytes(b"previous game build")
        (self.inputs / "SprkAccounts.dll").write_bytes(b"previous helper build")
        build.stage_references(self.inputs, self.output, self.lock)
        self.assertEqual([p.name for p in self.output.iterdir()], ["mscorlib.dll"])

    def test_changed_or_missing_reference_rejected_before_copy(self):
        (self.inputs / "mscorlib.dll").write_bytes(b"x" * len(self.data))
        with self.assertRaisesRegex(ValueError, "missing or changed"):
            build.stage_references(self.inputs, self.output, self.lock)
        (self.inputs / "mscorlib.dll").unlink()
        with self.assertRaisesRegex(ValueError, "missing or changed"):
            build.stage_references(self.inputs, self.output, self.lock)
        self.assertFalse(self.output.exists())

    def test_built_assemblies_and_path_traversal_cannot_be_locked(self):
        for name in ("../escape.dll", "..\\escape.dll", "Assembly-CSharp.dll", "SprkAccounts.dll"):
            with self.subTest(name=name), self.assertRaises(ValueError):
                build.stage_references(self.inputs, self.output, {"files": [{"path": name}]},
                                       built={"Assembly-CSharp", "SprkAccounts"})


class SourceLayoutTests(unittest.TestCase):
    def test_every_directory_is_one_assembly_named_after_it(self):
        projects = build.projects()
        self.assertIn(build.GAME, projects)
        self.assertGreater(len(projects), 1)
        directories = {p.name for p in build.SOURCE.iterdir() if p.is_dir()}
        self.assertEqual(set(projects), directories)

    def test_lock_never_names_an_assembly_built_from_source(self):
        lock = json.loads(build.LOCK.read_text(encoding="utf-8"))
        names = {Path(item["path"]).stem for item in lock["files"]}
        self.assertFalse(names & set(build.projects()))
        self.assertTrue({"mscorlib", "UnityEngine", "UnityEngine.CoreModule"} <= names)

    def test_bootstrap_project_compiles_all_source_once(self):
        with tempfile.TemporaryDirectory() as folder:
            project = build.bootstrap_project(Path(folder) / "bootstrap")
            text = project.read_text(encoding="utf-8")
            self.assertEqual(project.stem, build.GAME)
            self.assertIn("/*/**/*.cs", text)
            self.assertTrue((project.parent / "Directory.Build.props").is_file())


if __name__ == "__main__":
    unittest.main()
