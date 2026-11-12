using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RaidHelper
{
	public static HeroInfo GetRaidBossInfo(RaidData raidData)
	{
		GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(raidData.ChapterIndex, raidData.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, out var _, out var _, out var bossHeroInfo);
		return bossHeroInfo;
	}

	public static int GetFieldRaidLevel(RaidData raidData)
	{
		return raidData.Level;
	}

	public static int GetRaidBossLevel(int raidIndex, int raidLevel)
	{
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		if (data == null)
		{
			return 1;
		}
		return GetRaidBossLevel(data);
	}

	public static int GetRaidBossLevel(RaidData raidData)
	{
		return GetRaidBossInfo(raidData)?.Level ?? 1;
	}

	public static void InitPortrait(Component owner, UIBundleLoader bundleTex, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		if (owner == null || bundleTex == null)
		{
			return;
		}
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, difficulty, scenario: false, out var _, out var _, out var bossHeroInfo) == null)
		{
			VespaDebug.LogError("Failed to get bossMonsterInfo : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return;
		}
		if (bossHeroInfo == null)
		{
			VespaDebug.LogError("Failed to get monster hero info : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(bossHeroInfo.HeroIndex);
		if (data == null)
		{
			VespaDebug.LogError("Failed to get monster hero data : " + bossHeroInfo.HeroIndex);
			return;
		}
		string monsterCirclePortraitPath = UIHelper.GetMonsterCirclePortraitPath(data.CodeName, bossHeroInfo.Star);
		UIHelper.SetBundleTexture(bundleTex, monsterCirclePortraitPath);
	}

	public static void InitPortrait(Component owner, UIBundleLoader bundleTex, IRaidData raidData)
	{
		if (raidData != null)
		{
			DungeonDifficultyType difficulty = (DungeonDifficultyType)raidData.Difficulty;
			InitPortrait(owner, bundleTex, raidData.ChapterIndex, raidData.DungeonIndex, difficulty);
		}
	}

	public static void InitPortrait(Component owner, UITexture texture, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		if (owner == null || texture == null)
		{
			return;
		}
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, difficulty, scenario: false, out var _, out var _, out var bossHeroInfo) == null)
		{
			VespaDebug.LogError("Failed to get bossMonsterInfo : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return;
		}
		if (bossHeroInfo == null)
		{
			VespaDebug.LogError("Failed to get monster hero info : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(bossHeroInfo.HeroIndex);
		if (data == null)
		{
			VespaDebug.LogError("Failed to get monster hero data : " + bossHeroInfo.HeroIndex);
			return;
		}
		AssetTexture monsterCirclePortraitAsset = UIHelper.GetMonsterCirclePortraitAsset(data.CodeName, bossHeroInfo.Star);
		UIHelper.ResetPlaneAssetTextureAsync(owner, texture, monsterCirclePortraitAsset);
	}

	public static void InitPortrait(Component owner, UITexture texture, IRaidData raidData)
	{
		if (raidData != null)
		{
			DungeonDifficultyType difficulty = (DungeonDifficultyType)raidData.Difficulty;
			if (!Helper.CheckDifficulty(raidData.ChapterIndex, raidData.DungeonIndex, difficulty))
			{
				difficulty = (DungeonDifficultyType)Helper.GetDifficulty(raidData.ChapterIndex, raidData.DungeonIndex);
			}
			InitPortrait(owner, texture, raidData.ChapterIndex, raidData.DungeonIndex, difficulty);
		}
	}

	public static void SetBossMonsterInfo(RaidData raidData, UILabel Label_Name, UILabel Label_Prefix, UILabel Label_Level)
	{
		if (raidData == null)
		{
			return;
		}
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(raidData.ChapterIndex, raidData.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, out var _, out var bossWaveDetailInfo, out var bossHeroInfo) == null)
		{
			VespaDebug.LogError("Failed to get bossMonsterInfo : " + raidData);
			return;
		}
		if (bossHeroInfo == null)
		{
			VespaDebug.LogError("Failed to get monster hero info : " + raidData);
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(bossHeroInfo.HeroIndex);
		if (data == null)
		{
			VespaDebug.LogError("Failed to get monster hero data : " + bossHeroInfo.HeroIndex);
			return;
		}
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(data, bossWaveDetailInfo.VisualInfo, LocalizationManager.GetStringDirect, out var creatureName, out var creatureNamePrefix);
		string creatureDesc = string.Empty;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureDesc(data, bossWaveDetailInfo.VisualInfo, LocalizationManager.GetStringDirect, out creatureDesc);
		UIHelper.SetLabelText(Label_Level, bossHeroInfo.Level);
		UIHelper.SetLabelText(Label_Prefix, creatureNamePrefix);
		UIHelper.SetLabelText(Label_Name, creatureName);
	}

	public static bool BootyItemDestroyed(string CreatedTimeUTC)
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(CreatedTimeUTC, out var o_dateTime))
		{
			return false;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildRaidBootyItemDestroyMin");
		DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
		return o_dateTime.AddMinutes(valueInt).CompareTo(uTCNow) <= 0;
	}

	public static string GetRemainTimeUntilDestroy(string CreatedTimeUTC)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(CreatedTimeUTC, out var o_dateTime))
		{
			return $"{num:D2}:{num2:D2}:{num3:D2}";
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildRaidBootyItemDestroyMin");
		DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
		DateTime dateTime = o_dateTime.AddMinutes(valueInt);
		if (dateTime.CompareTo(uTCNow) >= 0)
		{
			TimeSpan timeSpan = dateTime - uTCNow;
			num = (int)timeSpan.TotalHours;
			num2 = timeSpan.Minutes;
			num3 = timeSpan.Seconds;
		}
		return $"{num:D2}:{num2:D2}:{num3:D2}";
	}

	public static TimeSpan? GetRemainTimeUntilDestroyTimeSpan(string CreatedTimeUTC)
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(CreatedTimeUTC, out var o_dateTime))
		{
			return null;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildRaidBootyItemDestroyMin");
		DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
		DateTime dateTime = o_dateTime.AddMinutes(valueInt);
		if (dateTime.CompareTo(uTCNow) >= 0)
		{
			return dateTime - uTCNow;
		}
		return null;
	}

	public static int GetRaidMaxLevel(int raidIndex)
	{
		NShared.RaidInfo raidInfo = GlobalAutoBehaviour<AccountManager>.instance.RaidManager.Get(raidIndex);
		int raidMinLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(raidIndex);
		if (raidInfo == null)
		{
			return raidMinLevel;
		}
		return System.Math.Max(raidInfo.RaidLevel, raidMinLevel);
	}

	public static bool IsOpenedHardMode(RaidData raidData)
	{
		return RestoredPortal.IsOpenedDragonHard(raidData);
	}

	public static bool IsOpenedFieldRaid(RaidData raidData)
	{
		if (raidData.Type != RaidType.FieldRaid)
		{
			return false;
		}
		return IsOpenedRaidByDungeonIndex(raidData);
	}

	public static bool IsOpenedTechnoMagicRaid(RaidData raidData)
	{
		if (raidData.Type != RaidType.TechnoMagic)
		{
			return false;
		}
		return IsOpenedRaidByDungeonIndex(raidData);
	}

	public static bool IsOpenedRaidByDungeonIndex(RaidData raidData)
	{
		if (__Extension_Method_ICollection__.COUNT(raidData.OpenCondition) < 2)
		{
			return false;
		}
		int chapterIndex = raidData.OpenCondition[0];
		int linkedDungeonIndex = raidData.OpenCondition[1];
		bool result = false;
		int originalDungeonIndex = GameRoot.Instance.CampaignDungeonDataContainer.GetOriginalDungeonIndex(linkedDungeonIndex);
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, originalDungeonIndex) == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager != null)
		{
			result = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, originalDungeonIndex);
		}
		return result;
	}

	public static ChallengeRaidDiff ChallengeRaidDiffClamp(int diff)
	{
		diff = System.Math.Min(diff, 4);
		diff = System.Math.Max(diff, 1);
		return (ChallengeRaidDiff)diff;
	}

	public static bool CanEnterRaid(RaidData enterRaidData)
	{
		if (enterRaidData == null)
		{
			VespaDebug.LogError("RaidHelper::CanEnterRaid raidData == null");
			return false;
		}
		return true;
	}

	public static bool CanEnterHardMode()
	{
		IList<RaidData> raidDataListByType = GameRoot.Instance.RaidDataContainer.GetRaidDataListByType(RaidType.DragonHard);
		int num = 1;
		foreach (RaidData item in raidDataListByType)
		{
			num = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(item.Index);
			if (item.Level <= num && IsOpenedHardMode(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanEnterParty(PartyDungeonData enterPartyDungeonData)
	{
		if (enterPartyDungeonData == null)
		{
			VespaDebug.LogError("RaidHelper::CanEnterParty PartyDungeonData == null");
			return false;
		}
		return true;
	}

	public static bool CanEnterFieldRaid()
	{
		foreach (RaidData item in GameRoot.Instance.RaidDataContainer.GetRaidDataListByType(RaidType.FieldRaid))
		{
			if (item.Level <= 1 && IsOpenedFieldRaid(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanEnterTechnoMagicRaid()
	{
		foreach (RaidData item in GameRoot.Instance.RaidDataContainer.GetRaidDataListByType(RaidType.TechnoMagic))
		{
			int raidMinLevel = GameRoot.Instance.RaidDataContainer.GetRaidMinLevel(item.Index);
			if (item.Level <= raidMinLevel && IsOpenedTechnoMagicRaid(item))
			{
				return true;
			}
		}
		return false;
	}

	public static string FieldRaidOpenNode(RaidData raidData)
	{
		string text = "";
		if (__Extension_Method_ICollection__.COUNT(raidData.OpenCondition) < 2)
		{
			return "";
		}
		int num = raidData.OpenCondition[0];
		int num2 = raidData.OpenCondition[1];
		text = $"{num}-{num2}";
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(num, num2);
		if (data != null && __Extension_Method_ICollection__.COUNT(data.Code) != 0)
		{
			text = LocalizationManager.GetStringDirect(data.Code[0]);
		}
		return text;
	}

	public static RaidData GetRaidDataSingleVersion(int chapterIndex, int dungeonIndex)
	{
		RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(chapterIndex, dungeonIndex);
		if (raidDataByDungeon == null)
		{
			return null;
		}
		return GameRoot.Instance.RaidDataContainer.GetData(raidDataByDungeon.SingleRaidIndex, raidDataByDungeon.Level);
	}

	public static RaidData GetRaidDataSingleVersion(RaidData raidData)
	{
		if (raidData == null)
		{
			return null;
		}
		return GameRoot.Instance.RaidDataContainer.GetData(raidData.SingleRaidIndex, raidData.Level);
	}

	public static RaidData GetRaidDataSingleVersionByLevel(int raidIndex, int level)
	{
		return GetRaidDataSingleVersion(GameRoot.Instance.RaidDataContainer.GetData(raidIndex, level));
	}

	public static RaidData GetRaidDataMultiVersionBySingleVersion(int singleRaidIndex, int level)
	{
		int multiRaidIndexBySingleRaidIndex = GameRoot.Instance.RaidDataContainer.GetMultiRaidIndexBySingleRaidIndex(singleRaidIndex);
		if (multiRaidIndexBySingleRaidIndex == 0)
		{
			return null;
		}
		return GameRoot.Instance.RaidDataContainer.GetData(multiRaidIndexBySingleRaidIndex, level);
	}

	public static RaidData GetRaidDataMultiVersionWithSingle(int chapterIndex, int dungeonIndex)
	{
		RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(chapterIndex, dungeonIndex);
		if (raidDataByDungeon == null)
		{
			return null;
		}
		return GetRaidDataMultiVersionBySingleVersion(raidDataByDungeon.Index, raidDataByDungeon.Level);
	}

	public static Tuple<List<CreatureTagType>, int> GetClassEnterBanRuleData(RaidBanRuleData banData)
	{
		if (banData.BanType == RaidBanType.ClassEnterPermission)
		{
			if (!BanRuleData.GetClassEnterPermissionValue(banData.BanValue, out var heroTagTypeList, out var heroEnterCount, out var isAll))
			{
				return null;
			}
			if (isAll)
			{
				heroTagTypeList = FilterHelper.GetListOrderCreatureTagType();
			}
			return Tuple.Create(heroTagTypeList, heroEnterCount);
		}
		return null;
	}

	public static DungeonDifficultyType GetChallengeDungeonDifficulty()
	{
		return DungeonDifficultyType.Easy;
	}
}
