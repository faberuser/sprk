using System;
using NGame2.NBattleContext;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public static class DataProviderBuilder
{
	public static DefaultBattlePartySettingDataProdiver Build(RaidData raidData, int[] affixIndices, bool isStorageKey = false, NShared.DungeonDifficultyType difficultyType = NShared.DungeonDifficultyType.Easy)
	{
		if (raidData == null)
		{
			return null;
		}
		int chapterIndex = raidData.ChapterIndex;
		int dungeonIndex = raidData.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("Raid DungeonData found : raid=" + raidData?.ToString() + " chapter=" + chapterIndex + " dungeon=" + dungeonIndex);
			return null;
		}
		string storageKey = (isStorageKey ? data.LocalStorageKey : null);
		StaminaType reqStaminaType = data.ReqStaminaType;
		int staminaOfDifficulty = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), difficultyType);
		EnterDungeonData enterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, difficultyType);
		return new DefaultBattlePartySettingDataProdiver(storageKey, reqStaminaType, staminaOfDifficulty, Battle.START_BATTLE, enterDungeon, affixIndices);
	}

	public static DefaultBattlePartySettingDataProdiver Build(string stroageKey, StaminaType staminType, int staminaCount, Enum startButtonTextKey, EnterDungeonData dungeonData, BanRuleData banRule)
	{
		return new DefaultBattlePartySettingDataProdiver(stroageKey, staminType, staminaCount, startButtonTextKey, dungeonData, banRule);
	}
}
