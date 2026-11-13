using System.Collections.Generic;

namespace NGameAsset.NNguiAtlas;

// Sprite paths that SPRK draws from another atlas sprite. BundlePack.bin maps
// every sprite path to an atlas; this table takes precedence for the listed
// paths, so missing or placeholder art can borrow an existing sprite without
// rebuilding the 44 MB index. Art the game never had (Maya's framed perk icons)
// is embedded in SprkImageRecovery (TryIconTexture) and drawn first. See docs/heroes/maya-art.md.
public static class SprkAtlasAliases
{
	private static readonly Dictionary<string, (string AtlasCode, string SpriteName)> Aliases = new Dictionary<string, (string, string)>
	{
		// Maya has no full illustration; use her own UI portrait.
		{ "Portrait/Illust/Maya_5", ("NguiAtlas.UI.Portrait.Hero6", "Maya_5") },
		// Maya's unique treasure icons are missing (3, 4) or developer placeholders (1, 2); each uses the
		// icon of the skill it enhances. Item codes do not follow slot order: _1 is the S3 treasure (UT3),
		// _3 the S1 treasure (UT1).
		{ "Icon/Item/TREASURE_UNIQUE_MAYA_1", ("NguiAtlas.Icon.Skill2", "1430") },
		{ "Icon/Item/TREASURE_UNIQUE_MAYA_2", ("NguiAtlas.Icon.Skill2", "1420") },
		{ "Icon/Item/TREASURE_UNIQUE_MAYA_3", ("NguiAtlas.Icon.Skill2", "1410") },
		{ "Icon/Item/TREASURE_UNIQUE_MAYA_4", ("NguiAtlas.Icon.Skill2", "1440") },
		// Maya's Light transcendence items have no icons; show her face from the Dark icon.
		{ "Icon/Item/LIGHTNESS_MAYA_1", ("NguiAtlas.Icon.Item.Awaken2", "DARKNESS_MAYA_5") },
		{ "Icon/Item/LIGHTNESS_MAYA_2", ("NguiAtlas.Icon.Item.Awaken2", "DARKNESS_MAYA_5") },
		{ "Icon/Item/LIGHTNESS_MAYA_3", ("NguiAtlas.Icon.Item.Awaken2", "DARKNESS_MAYA_5") },
		{ "Icon/Item/LIGHTNESS_MAYA_4", ("NguiAtlas.Icon.Item.Awaken2", "DARKNESS_MAYA_5") },
		{ "Icon/Item/LIGHTNESS_MAYA_5", ("NguiAtlas.Icon.Item.Awaken2", "DARKNESS_MAYA_5") },
		// Maya's soulstone has no icon; other soulstone icons show their hero's face, so use her weapon.
		{ "Icon/Item/SOULSTONE_MAYA_1", ("NguiAtlas.Icon.Item.UniqueWeapon1", "WEAPON_UNIQUE_MAYA_1") },
		// Maya's perks have no icons. UIHelper.GetSkillIcon2 needs an atlas entry to show one,
		// so these remain; AssetTexture draws the framed PNGs embedded in SprkImageRecovery instead.
		{ "Icon/Skill/60001410", ("NguiAtlas.Icon.Skill2", "1410") },
		{ "Icon/Skill/60001411", ("NguiAtlas.Icon.Skill2", "1410") },
		{ "Icon/Skill/60001420", ("NguiAtlas.Icon.Skill2", "1420") },
		{ "Icon/Skill/60001421", ("NguiAtlas.Icon.Skill2", "1420") },
		{ "Icon/Skill/60001430", ("NguiAtlas.Icon.Skill2", "1430") },
		{ "Icon/Skill/60001431", ("NguiAtlas.Icon.Skill2", "1430") },
		{ "Icon/Skill/60001440", ("NguiAtlas.Icon.Skill2", "1440") },
		{ "Icon/Skill/60001441", ("NguiAtlas.Icon.Skill2", "1440") },
		{ "Icon/Skill/60001450", ("NguiAtlas.Icon.Creature.Hero1", "Maya_5") },
		{ "Icon/Skill/60001451", ("NguiAtlas.Icon.Item.Awaken2", "DARKNESS_MAYA_5") }
	};

	public static bool TryGet(string spritePath, out AtlasDataContainer.AtlasLink link)
	{
		if (spritePath != null && Aliases.TryGetValue(spritePath, out var alias))
		{
			link = new AtlasDataContainer.AtlasLink
			{
				AtlasCode = alias.AtlasCode,
				SpriteName = alias.SpriteName
			};
			return true;
		}
		link = null;
		return false;
	}
}
