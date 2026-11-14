import json
from pathlib import Path
import re
import tempfile
import unittest

from client.scripts.build import build_sprk_localization as catalog


LITERAL = r'"(?:[^"\\]|\\.)*"'


class LocalizationTests(unittest.TestCase):
    def test_generated_catalog_and_direct_references(self):
        catalog.check()
        entries = catalog.read_entries()
        for path in (catalog.ROOT / 'managed').rglob('*.cs'):
            if path.name.startswith('SprkText') or path.name.endswith('Probe.cs'):
                continue
            source = path.read_text(encoding='utf-8-sig')
            for literal in re.findall(r'SprkText\.(?:Get|Format)\((' + LITERAL + ')', source):
                self.assertIn(json.loads(literal), entries, str(path))

    def test_graphics_help_and_dynamic_labels_are_complete(self):
        entries = catalog.read_entries()
        graphics = (catalog.ROOT / 'managed/SprkGraphicsOptions/SprkGraphicsOptions.cs').read_text(encoding='utf-8')
        for title, description in re.findall(r'new GraphicsHelpInfo\((' + LITERAL + '), (' + LITERAL + r')\)', graphics):
            self.assertIn(json.loads(title), entries)
            self.assertIn(json.loads(description), entries)
        for field in ['FilterPresetNames', 'adjustmentNames']:
            values = re.search(field + r' = new string\[\d+\] \{(.*?)\}', graphics)[1]
            for value in re.findall(LITERAL, values):
                self.assertIn(json.loads(value), entries)
        source = (catalog.ROOT / 'managed/SprkArtifactSourceFilter/SprkArtifactSourceFilter.cs').read_text(encoding='utf-8')
        for value in re.findall(LITERAL, re.search(r'Labels = new string\[\d+\] \{(.*?)\}', source)[1]):
            self.assertIn(json.loads(value), entries)

    def test_missing_locale_and_broken_format_are_rejected(self):
        good = ['Count: {0}'] + ['Anzahl: {0}'] * 7
        cases = [good[:-1], good[:3] + [''] + good[4:], good[:3] + ['Anzahl: {1}'] + good[4:],
                 good[:3] + ['?? {0}'] + good[4:]]
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / 'fixture.tsv'
            for cells in cases:
                path.write_text('\t'.join(cells), encoding='utf-8')
                with self.assertRaises(ValueError):
                    catalog.read_entries(path)

    def test_table_patch_language_key_parity(self):
        languages = {}
        for path in (catalog.ROOT / 'tables').rglob('*.json'):
            for language, values in json.loads(path.read_text(encoding='utf-8')).get('localization', {}).items():
                languages.setdefault(language, {}).update(values)
        for language in catalog.LANGUAGES:
            self.assertEqual(set(languages['English']), set(languages[language]), language)
            self.assertTrue(all(str(value).strip() for value in languages[language].values()))


if __name__ == '__main__':
    unittest.main()
