"""Check patcher resource inputs in the PR packaging checks."""
from pathlib import Path
import unittest
import xml.etree.ElementTree as ET


class ClientBuildInputTests(unittest.TestCase):
    def test_embedded_resources_exist_in_checkout(self):
        project = Path(__file__).resolve().parents[1] / "DllPatcher/DllPatcher.csproj"
        resources = ET.parse(project).findall(".//EmbeddedResource")
        self.assertTrue(resources, "The patcher must declare its embedded resources")
        for resource in resources:
            include = resource.attrib["Include"]
            with self.subTest(resource=include):
                self.assertTrue(
                    any(path.is_file() for path in project.parent.glob(include)),
                    f"Missing patcher resource in checkout: {include}",
                )


if __name__ == "__main__":
    unittest.main()
