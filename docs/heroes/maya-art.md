# Maya: art slots and stand-ins

Maya (hero 14, `Hero.Maya`, beast Seves) shipped unfinished: some of her art
is missing or a developer placeholder. SPRK fills those slots with existing
sprites. This page lists every slot, what it shows today and how to swap in
better art later. Implementation status: [maya.md](maya.md).

## How the client finds a sprite

UI code asks for a **sprite path** such as `Portrait/Illust/Maya_5`.
`NGameAsset.NNguiAtlas.AtlasDataContainer.GetAtlasLink` resolves it in this order:

1. `SprkAtlasAliases` (`client/managed/Assembly-CSharp/NGameAsset.NNguiAtlas/SprkAtlasAliases.cs`):
   SPRK's list of `path -> (atlas code, sprite name)`. It wins over the game's index.
2. The `NguiAsset` section of `Assetbundle/BundlePack.bin` (the game's 44 MB
   index of every sprite path; there is no writer for it, so SPRK never edits it).

An atlas code such as `NguiAtlas.UI.Portrait.Hero6` names the bundle
`Assetbundle/<spec>/NguiAtlas_UI_Portrait_Hero6.unity3d` (spec folders
`HighAndMidSpec`, `LowLargeSpec`, ...), which holds one texture and an NGUI
sprite list.

## Slots

| Slot | Sprite path(s) | Shows today | Source |
| --- | --- | --- | --- |
| Full illustration (hero info, recruit) | `Portrait/Illust/Maya_5` | **Stand-in:** her UI portrait `NguiAtlas.UI.Portrait.Hero6 / Maya_5` (256×632) | alias |
| UT1, S1 (Hunter's Whetstone, item 103014) | `Icon/Item/TREASURE_UNIQUE_MAYA_3` | **Stand-in:** her S1 skill icon `NguiAtlas.Icon.Skill2 / 1410` | alias (missing in game) |
| UT2, S2 (Beast-Calling Whistle, item 102014) | `Icon/Item/TREASURE_UNIQUE_MAYA_2` | **Stand-in:** her S2 skill icon `Icon.Skill2 / 1420` | alias (game icon is a placeholder with a red "2") |
| UT3, S3 (Fang Charm, item 101014) | `Icon/Item/TREASURE_UNIQUE_MAYA_1` | **Stand-in:** her S3 skill icon `Icon.Skill2 / 1430` | alias (game icon is a placeholder with a red "T") |
| UT4, S4 (Pack Leader's Collar, item 104014) | `Icon/Item/TREASURE_UNIQUE_MAYA_4` | **Stand-in:** her S4 skill icon `Icon.Skill2 / 1440` | alias (missing in game) |
| Light transcendence items 1-5 | `Icon/Item/LIGHTNESS_MAYA_1` ... `_5` | **Stand-in:** her Dark icon `NguiAtlas.Icon.Item.Awaken2 / DARKNESS_MAYA_5` (purple frame instead of gold) | alias (missing in game) |
| Perks T3 (8) | `Icon/Skill/60001410` ... `60001441` | **Composed:** her skill icons (1410-1440) with the released Light gold / Dark purple frame and tint, lifted from Selene's perks by `client/tools/PerkIcons/make_perk_icons.py`; PNGs embedded in `SprkImageRecovery` (`SprkIcons.Skill.<index>.png`) and drawn by `AssetTexture` before any atlas. The atlas aliases to the plain icons stay because `UIHelper.GetSkillIcon2` requires an atlas entry | embedded PNG |
| Perks T5 Light / Dark | `Icon/Skill/60001450`, `60001451` | **Composed:** hero icon `Maya_5` (Light) and `DARKNESS_MAYA_5` (Dark) in the same frames | embedded PNG |
| Soulstone (item 111014) | `Icon/Item/SOULSTONE_MAYA_1` | **Composed:** Selene's soulstone art with Maya's face (from `Maya_5`) in the hero badge; PNG embedded in `SprkImageRecovery` as `SprkIcons.Item.SOULSTONE_MAYA_1.png` (the atlas alias to her UW icon stays as the fallback entry) | embedded PNG |
| Soul weapon display model | creature 901014 (Manage Soul Weapon, Advance screens) | **Maya's own mesh on a stand-in rig:** Maya has no display model (Selene's is `Creature_Object_SeleneUnique`). 901014 loads Selene's rig (idle animation, `Object.SeleneUnique`) and `NodeTreeUIPlayer` swaps its `Hero_Selene_Unique01_Weapon` mesh and materials for `Hero_Maya_Unique01_Weapon`, which ships only inside her treasure-reveal effect `Scene.Gacha.GetUniqueItem.Maya`. Placement: `SoulWeaponNodeTreeTable` position `[-0.2,2.35,0]` (centred on the magic circle), scale 1.8 (`client/tables/maya/65-soul-weapon-display.json`) | code + table |
| Default costumes (avatar, Hero Index, costume screens) | `CostumeTable` 1403-1405 (`client/tables/maya/18-costumes.json`), server `tables/HeroShopSupport.json` `Costumes` | Her own 3-5 star portraits and icons, named like other heroes' defaults (`COSTUME_NAME_1403`-`1405`, all 8 languages in `client/tables/maya/98-costume-names.json`); the game had no rows, which crashed the Hero Index and left the soul weapon avatar empty (`SelectSoulWeaponInfo` still falls back to her hero icon) | table |
| Special Shop preview | `ClientCreatureTable` `PayShopScale`/`PayShopOffset` | Tuned to 500 / `[140,-500]` to match other heroes (the game had 350 / `[-80,-432]`) | `client/tables/maya/16-client-creature.json` |
| See Motions list | `HeroAnimationViewTable` 1400-1417 (`client/tables/maya/17-animation-view.json`) | Rows for her own clips (Skill1-3 = 1410/1420/1430 from her skill trees); the list shows only motions her model has | table |
| Soulstone 3D reveal | soulstone `Item3DPath` | **Stand-in:** `Scene.Gacha.SoulStone.Selene.1` | `client/tables/maya/63-soulstone-item.json` |
| Soul weapon motions | `SoulWeaponGet`, `SoulWeaponSkill1` | **Missing:** Maya's model has neither; liberation and use play without her own motion | none |
| Cinematics (node trees) | `NodeTree/Hero/Maya`: `GetNewHero`, `Transcendence`, `GetSoulWeapon`; `NodeTree/Item/3dModel_SoulWeapon`: `SOULWEAPON_MAYA_REINFORCESUCCESS/FAIL` | **Stand-in:** Maya's pack has only her skill trees, so `SprkNodeTreeFallbacks` copies Selene's trees when the pack loads. Recruit: Maya's `Get` motion, name text and sound with Selene's camera at `PositionScale` 1.3 (pulled back because Maya is taller; at 1.0 she was cut off at the left edge). Maya has no `Get` camera: the Pre-Doomsday archive's only `Get.Maya` entry is her recruit sound. Transcendence: Maya's own motion, camera (`OrthoScale` 1, because her camera stores field of view in degrees; Selene's tree uses 45, which gave the stretched view) and effect, Selene's chime. Liberation: Maya with Selene's camera, soulstone and soul models | `client/managed/Assembly-CSharp/NGame2.NNodeTree/SprkNodeTreeFallbacks.cs` |
| Purification camera | `ClientCreatureTable` `Hero.Maya` `PurifyCameraOffset`/`PurifyDarknessOffset` | Tuned: offset `[0,-0.7,0]` frames her kneeling purify pose (the game left all zeros); darkness `[0.1,0.65,0.45]` | `client/tables/maya/16-client-creature.json` |
| Unique weapon (Serrated Claws, Satis) | `Icon/Item/WEAPON_UNIQUE_MAYA_1` | Her own art | game |
| Hero icons | `Icon/Creature/Hero/Maya`, `_3` `_4` `_5`, `Icon/Item/HERO_MAYA` | Her own art (stars 1-2 use 3) | game |
| UI portraits | `Portrait/Hero/Maya`, `_1` ... `_5` | Her own art (`NguiAtlas.UI.Portrait.Hero6`) | game |
| Battle portraits | `Battle/Portrait/Hero/Maya`, `_3` `_4` `_5` | Her own art (`NguiAtlas.Battle.Portrait.Hero1`) | game |
| Dark transcendence items | `Icon/Item/DARKNESS_MAYA_3` `_4` `_5` | Her own art (`Awaken2`) | game |
| 3D model, animations, Seves | bundles `Creature_Hero_Maya*.unity3d`, `Creature_Hero_Maya_Pet*.unity3d` | Her own | game |
| Skill effects, recruit effect, transcend effect | `Effect_Skill_Hero_Maya_*`, `Effect_GetHero_Maya_*`, `Effect_Transcendence_Maya` | Her own | game |
| Unique treasure gacha effects | `Scene.Gacha.Treasure.Maya.1` ... `.4` (item data) | **Missing:** no bundles exist; check the treasure reveal when testing | none |
| Loading screen, achievement portrait, costumes | none | Not present; other heroes have `NguiAtlas_Loading_Hero_<Name>` etc. | none |

## Replacing a stand-in

**With a sprite that already exists in the game** (another hero's icon, a
newer Maya sprite in a later baseline): change the entry in
`SprkAtlasAliases.cs`. Find the atlas and sprite name for any path with

```powershell
cd client
dotnet run --project tools/JitTables -c Release -- sprites "../../sprk-client/King's Raid_Data/Documents/Patch/StandaloneWindows/Assetbundle/BundlePack.bin" "TREASURE_UNIQUE_SELENE"
```

To see what a sprite looks like, crop it from its `NguiAtlas_*.unity3d` with
UnityPy (texture plus the `mSprites` rectangles of the atlas MonoBehaviour).
Remove an entry to show the game's own sprite again.

**With new artwork** (for example a real 1022×1022 illustration, the size of
`Portrait/Illust/Kasel_5` in `NguiAtlas.Portrait.Illust.Basic1`):

1. Build a Unity asset bundle with the same Unity version as the game
   (2020.3.20f1) containing a texture and an NGUI `UIAtlas` with the sprite.
   Name it like the game's (`NguiAtlas_Portrait_Illust_SprkMaya.unity3d`) and
   add one copy per spec folder that needs it.
2. Ship it under `client/overrides/files/King's Raid_Data/Documents/Patch/StandaloneWindows/Assetbundle/...`.
3. Teach the client the new atlas: `AtlasDataContainer.Get(atlasCode)` reads
   atlas metadata (bundle file name, size, sprite list) from `BundlePack.bin`'s
   `NguiAtlas` section. Extend `SprkAtlasAliases` with an atlas table that
   `AtlasDataContainer.Get` consults first, the same way paths are aliased.
4. Point the slot's path at the new atlas and sprite.

Test every replaced slot in the real game with
[client automation](../client-automation.md): hero info (illustration), item
info and inventory (treasure, transcendence and soulstone icons), and the
recruit screen.
