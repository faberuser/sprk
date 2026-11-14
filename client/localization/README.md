# SPRK text

`sprk.tsv` contains English text followed by German, French, Japanese, Korean,
Russian, Simplified Chinese and Traditional Chinese, separated by literal tabs.
Write line breaks inside a value as `\n`. Keep numbered placeholders and NGUI
markup intact; translators may reorder placeholders.

Use `SprkText.Get("Caption")` at the point where UI text is displayed and
`SprkText.Format("Complete sentence with {0}", value)` for variable messages.
Do not translate protocol fields, object names, preference keys, player names,
or strings used to drive a state machine. Resolve static lists when displaying
them, so a language selected after class initialization is respected.

After editing the catalog:

```powershell
python client/scripts/build/build_sprk_localization.py
python -m unittest client.scripts.build.test_sprk_localization
python client/dev.py build
```

The generated `Assembly-CSharp/SprkText.Generated.cs` is committed with the
catalog. Every managed build rejects stale output, empty translations, duplicate
keys and mismatched formatting. The tests also check direct source references,
graphics help, dynamic caption lists and table-patch language coverage.

The automation action `localization-probe`, with `text` set to a supported
Unity language name, checks every catalog entry and opens native login settings.
It requires the Login scene and an isolated `localization-*` automation profile.
Run it in all eight languages, capture screenshots, then send `restore`.

## Audit scope

The November 2026 audit covered the SPRK assemblies, restored portal prompts,
native source UI literals, login presentation captions, account server messages,
inventory conversion and Honor buff results, and all JSON localization patches.
The existing skill-formula catalog already covers the same seven non-English
languages. The 86 table-patch keys are present in all eight languages. Korean
and German default costume names intentionally use `{1}` for the hero name;
`CostumeData.BuildLocalize_CostumeName` supplies that argument.

Technical diagnostics, automation fixtures, protocol identifiers, conventional
graphics abbreviations (FXAA, FPS, HD), and original song titles are not translated.
The asset archive and launcher are separate from this managed game-text catalog.
Visual verification covers representative settings screens; it is not a native
speaker review of every translation or a playthrough of every feature.
