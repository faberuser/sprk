# Client asset overrides

Rebel Clause Memory Archives scene `RebelClause_9_1.bjs` is restored byte for
byte from the archived Android client under
`Pre-Doomsday/com.vespainteractive.KingsRaid/files/Documents/Patch/Android/`.
The installed scene decoded to null (9 bytes), triggering the native `Empty`
dialogue fallback. The restored scene has 31 groups, 14 dialogue lines and one
intentional command that clears the dialogue window.
Its SHA-256 and the replaced file hash are recorded in `SPRK-NativeData.json`.

All 29 Rebel Clause scenes were compared with the archive. The other 27
identical scenes and the existing modified `RebelClause_8_2` are preserved.
The release builder automatically includes these overrides. For a local release,
preserve `DEFAULT_INCLUDES` and also include the restored BJS file.

Native validation: use automation action `archive-dialogue-probe` at Login
with an `archive-dialogue-*` profile. `text` optionally selects a candidate BJS
file before installation; omit it to check the installed scene. `target` selects
one of its 14 dialogue lines (0-13). The probe parses the full scene with the
native reader, resolves every dialogue/speaker key, and displays the chosen
line in the native dialogue window. It does not run the dungeon battle.

## Battle speed buttons

`Assetbundle/NguiAtlas_UI_Prefab_Battle4.unity3d` is the game's battle UI atlas
(the AUTO and x1.5 buttons) with 18 extra sprites, `btn_sprk_speed_{off|x0.25|...|x2}_{on|off}`,
that `SprkBattleSpeed` shows instead of drawing labels at run time. They were baked
once from the native `btn_x1.5_on/off` art and the label art in
`sources/battle-speed/BattleSpeedLabels.png`, into rows of the texture no sprite
used; every original sprite and pixel is unchanged. Android gets the atlas through
`client/android/dev.py assets`.
