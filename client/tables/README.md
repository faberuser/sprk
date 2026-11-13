# Table and localization patches

Every `*.json` file below this folder (any depth, except this README) is a
patch for one game table (`"table": "SkillTable"`) or for localization
(`"localization": {...}`). `python client/scripts/build/build_tables.py`
applies them to the pinned baseline and writes the results to
`client/overrides/files/`. Format and workflow: [docs/jit-tables.md](../../docs/jit-tables.md).

Group patches by feature, one folder each (for example `maya/`).
