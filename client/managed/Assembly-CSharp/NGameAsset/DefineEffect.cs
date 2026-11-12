namespace NGameAsset;

public static class DefineEffect
{
	public static class VersionHistory
	{
		public const string Version_20150623 = "20150623";
	}

	public const string BundleTypeName = "[Effect]";

	public const string VersionCode = "20150623";

	public static string ObjectName = "MainObject";

	public const string Prefab_Battle_EclipseStart = "Resources/Prefab2/Window/SoulWeapon/EclipseBattleStart";

	public const string Code_Battle_BattleStart = "UI.Battle.BattleStart";

	public const string Code_Battle_ArenaStart = "UI.Arena.Count";

	public const string Code_Battle_EclipseStart = "UI.Arena.Count";

	public const string Code_Battle_BossWave = "UI.Battle.BossWave";

	public const string Code_Battle_OnAuto = "UI.Battle.OnAuto";

	public const string Code_Battle_SkillReady = "UI.Battle.SkillReady";

	public const string Code_Battle_UsingSkill = "UI.Battle.UsingSkill";

	public const string Code_BattleEnd_Lose = "UI.BattleEnd.Lose";

	public const string Code_BattleEnd_Victory_ImageFont = "UI.BattleEnd.Victory.ImageFont";

	public const string Code_BattleEnd_Victory_Mark = "UI.BattleEnd.Victory";

	public const string Code_BattleEnd_Victory_Star = "UI.BattleEnd.Victory.Star";

	public const string Code_Common_GetItem = "UI.Common.GetItem";

	public const string Code_Common_HeroesLevelUp = "UI.Common.HeroesLevelUp";

	public const string Code_Common_HeroesLevelUpText = "UI.Common.HeroesLevelUp.Text";

	public static readonly string[] CodeArray_Battle = new string[11]
	{
		"UI.Battle.BattleStart", "UI.Battle.BossWave", "UI.Battle.OnAuto", "UI.Battle.SkillReady", "UI.Battle.UsingSkill", "UI.BattleEnd.Lose", "UI.BattleEnd.Victory.ImageFont", "UI.BattleEnd.Victory", "UI.BattleEnd.Victory.Star", "UI.Common.GetItem",
		"UI.Common.HeroesLevelUp"
	};

	public static string BundleCode(string code)
	{
		return "Effect." + (String_Extension_Method.IsNullOrEmpty(code) ? "(noname)" : code);
	}
}
