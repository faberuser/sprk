namespace NGameAsset;

public static class DefineSound
{
	public const string UI_WorldMap_Coin = "UI.WorldMap.Coin";

	public const string UI_Battle_UsingSkill = "UI.Battle.UsingSkill";

	public const string UI_Campaign_Victory = "UI.Campaign.Victory";

	public const string UI_Campaign_VictoryBgm = "UI.Campaign.VictoryBGM";

	public const string UI_Campaign_Lose = "UI.Campaign.Lose";

	public const string UI_Campaign_LoseBgm = "UI.Campaign.LoseBGM";

	public const string UI_Battle_BattleStart = "UI.Battle.BattleStart";

	public const string UI_Campaign_GetExp = "UI.Campaign.GetExp";

	public const string UI_Campaign_LevelUp = "UI.Campaign.LevelUp";

	public const string UI_Campaign_RewardItem = "UI.Campaign.RewardItem";

	public const string UI_Boss_Warning = "UI.Boss.Warning";

	public const string UI_GuildArena_HeroPick = "UI2.Guild.GuildHeroPick";

	public const string UI_GuildArena_SaveDeck = "UI2.Guild.GuildGuildSaveDeck";

	public const string UI_GuildArena_VSEffect = "UI2.Guild.GuildVSEffect";

	public const string UI_Eclipse_BattleStart = "UI.Eclipse.BattleStart";

	public const string UI_Eclipse_RedMoon_Start = "UI.Eclipse.Start";

	public const string UI_Eclipse_RedMoon_End = "UI.Eclipse.End";

	public const string UI_Eclipse_FadeIn = "UI.Eclipse.BattleFadein";

	public const string UI_Eclipse_FadeOut = "UI.Eclipse.BattleFadeout";

	public const string UI_Godking_Dungeon_Open = "UI.GodKing.DungeonOpen";

	public const string UI_Godking_Dungeon_Close = "UI.GodKing.DungeonClose";

	public const string EntryMenu_KainAscension = "EntryMenu.KainAscensionBGM";

	public const string UI_Common_Button_04 = "UI.Common.Button04";

	public const string UI_Common_Button_05 = "UI.Common.Button05";

	public const string UI_Common_Button_06 = "UI.Common.Button06";

	public const string UI_Common_Button_07 = "UI.Common.Button07";

	public const string UI_Common_Button_08 = "UI.Common.Button08";

	public const string UI_SoulWeapon_EtherInsert = "UI.SoulWeapon.EtherInsert";

	public const string UI_SoulWeapon_EtherFull = "UI.SoulWeapon.EtherFull";

	public const string UI_SoulWeapon_EtherStart = "UI.SoulWeapon.EtherStart";

	public const string UI_SoulWeapon_EtherSuccess = "UI.SoulWeapon.EtherSuccess";

	public const string UI_SoulWeapon_EtherFailed = "UI.SoulWeapon.EtherFailed";

	public const string UI_SoulWeapon_ReLocate = "UI.SoulWeapon.ReLocate";

	public const string UI_SoulWeapon_ResultStatusOption = "UI.SoulWeapon.ResultStatusOption";

	public const string UI_SoulWeapon_ResultStatus = "UI.SoulWeapon.ResultStatus";

	public const string UI_Pet_Awaken = "Pet.System.Awaken";

	public const string UI_Pet_PluggedIn = "Pet.System.TierupgradeMatter";

	public const string UI_Pet_MinorUnion = "Pet.System.Tierupgrade";

	public const string UI_Pet_LegendUnion = "Pet.System.TierupgradeLegend";

	public const string UI_PunishmentMain_Bgm = "Raid.PunishmentRaid.SelectLobby";

	public const string DOWN_SOUND_PREFIX = "DOWN";

	public const string DAMAGE_SOUND_PREFIX = "DAMAGE";

	public const string DEAD_SOUND_PREFIX = "DEAD";

	public const string BEGIN_FIRE_SKILL_SOUND_PREFIX = "BeginFireSkill";

	public const string PARTY_SELECT_SOUND_PREFIX = "Select";

	public const string VICTORY_SOUND_PREFIX = "Victory";

	public static string BeginFireSkillSound(int skillIndex)
	{
		return $"BeginFireSkill.{skillIndex}";
	}

	public static string DamageSound(string heroCode)
	{
		return $"Damage.{heroCode}";
	}

	public static string DeadSound(string heroCode)
	{
		return $"Dead.{heroCode}";
	}

	public static string HeroSelectSound(string heroCode)
	{
		return $"Select.{heroCode}";
	}

	public static string VictorySound(string heroCode)
	{
		return $"Victory.{heroCode}";
	}

	public static bool GetBundlePath(string audioName, out string bundleFileName, out string assetName)
	{
		int num = audioName.LastIndexOf('/');
		if (num < 1 || num >= audioName.Length - 1)
		{
			bundleFileName = string.Empty;
			assetName = string.Empty;
			return false;
		}
		string arg = audioName.Substring(0, num).Replace('/', '_').Replace('.', '_');
		bundleFileName = $"Sound_{arg}.unity3d";
		assetName = audioName.Substring(num + 1);
		return true;
	}
}
