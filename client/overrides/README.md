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

## Android bundles in the Windows asset set

A scan of every Windows bundle (28,896) found 649 Android builds (build target 13) in
`Documents/Patch/StandaloneWindows/Assetbundle`, many of them byte-identical to the
Android originals. The 2020 Windows player refuses the 55 built with Unity 5.x ("created
for another build target"), so those effects, atlases and sounds never appeared:
Xanadus' Curtain of Darkness (S2) hits and loops, Ice Tortoise, Lizardman Lakreil and
Lizardman Garrow raid skills, UI effects (Calendar, Guild Raid gauge, World Map clouds),
UI atlases (Guild Construction, School portraits, Tower) and three sounds. The others
(Unity 2018/2020 Android builds) load on Windows and are left as they are. The root
`Assetbundle/Shader.unity3d` is also an old Android build, but Windows uses
`Unity_2020/Shader.unity3d`, so it is left alone.

A second scan checked the 49,928 bundle/asset references of `BundlePack.bin` against the
bundles. Some Windows rebuilds lost or renamed objects the game still asks for (for
example the Giant Slime boss: its Windows bundles name everything `Raid_GiantSlime...`
while the game loads `Boss_GiantSlime...`, so the boss had no model or animations).
Where the Android original has the requested objects, has no embedded shaders and keeps
every object the game references, the bundle is restored from it: Giant Slime (model and
animations), Chase's facial animations, Golem SuperElite and Wasp Mandible skills, the
Luna NPC, Lewisia's unique object, Cleo's recruit effect, the raider jukebox, Cleo (Re) S2,
Shamilla's soul weapon projectile and Xanadus' S1 projectile (its `_Smoke`).

All of these were made by `client/scripts/build/retarget_android_bundles.py` (UnityPy):
it retargets the serialized files to StandaloneWindows64, stores mobile-only textures
uncompressed and drops preload entries into `unity_builtin_extra` that no object uses;
shader references stay as they are and are checked against `Unity_2020/Shader.unity3d`.
Tiers missing on Android (LowSpec) use the MidSpec original. Every override loads in the
Windows player with all its assets (`bundle-load-probe`, docs/client-automation.md); in a
Xanadus battle the S2 and S1 effects load. `convert_bundles.py` converts them for Android.

Android originals that embed Unity's Standard shader (GLES only) come from the live
Android client archived on 2022-09-01 (`Pre-Doomsday/com.vespainteractive.KingsRaid/files/
Documents/Patch/Android/Assetbundle`), with `--standard-donor`: the Standard shader is
replaced by the D3D11 build embedded in a native Windows bundle (the Windows copy of the
same bundle for effects, where it only serves a stray Default-Material; the
variant-richest `OriginalSpec/Creature_Hero_Ybb_Star0` for creature skins). Restored this
way: the Eclipse skins of Lich, Giant Worm SuperElite, Horned Bear Wild and the Fire and
Warrior Spiders; Zafir's unique-item gacha effect; Crow's and Shamilla's soul weapon
casting; Luna (Re) S3 casting; Ripine S1; Tanya's 2021 daily look idle; the Valance shop
result. `bundle-load-probe` loads every one with all assets, and every material's shader
reports `isSupported` (only the NGUI atlases resolve their shader at run time, as the
native atlases do).

Not fixed (the 2022 client has the same layout, so these are how the game shipped):
- Thirteen bundles keep some requested objects only in the root copy, not in the tier
  folders (Valance and Yanne attacks/skills, Requina's Glory League victory, the
  Apocalypsion field).
- `Creature_Hero_Kasel_test`, `Creature_Hero_Ybb_Animation` and one Mirianne facial clip
  are test or unused entries missing from every client.
- `1.GameObject.Effect_WorldBoss_Xanadus_Skill2_Loop_Fog` loads with the probe but
  returned null during a battle.
