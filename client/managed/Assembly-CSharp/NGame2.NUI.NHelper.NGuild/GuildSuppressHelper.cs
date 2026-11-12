using System;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NGuild;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.GetGuildSuppressStatusBoard;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NHelper.NGuild;

public static class GuildSuppressHelper
{
	public static void RequestGuildSuppressStatusBoard(Action<NShared.GetGuildSuppressStatusBoard.Response> action)
	{
		NShared.GetGuildSuppressStatusBoard.Request request = new NShared.GetGuildSuppressStatusBoard.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildSuppressStatusBoard.Response response)
		{
			Vespa_System_Action_Extension_Method.fire(action, response);
		});
	}

	public static int GetRemainSeconds(GuildSuppressSessionData sessionData, GuildSuppressSessionInfo sessionInfo)
	{
		if (sessionData == null || sessionInfo == null)
		{
			return -1;
		}
		GuildSuppressSessionState state = sessionInfo.State;
		if (state == GuildSuppressSessionState.Apply && Vespa_String_Extension_Method2.TryParseNetworkTime2(sessionInfo.ApplyEndTime, out var o_dateTime))
		{
			return Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
		}
		switch (state)
		{
		case GuildSuppressSessionState.ApplyEnded:
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(sessionData.GlobalBattleStart, out o_dateTime))
			{
				return Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
			}
			break;
		case GuildSuppressSessionState.Battle:
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(sessionInfo.BattleEndTime, out o_dateTime))
			{
				return Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
			}
			break;
		default:
		{
			GuildSuppressSessionData sessionData2 = GetSessionData((int)sessionInfo.SessionIndex + 1);
			if (sessionData2 != null && Vespa_String_Extension_Method2.TryParseNetworkTime2(sessionData2.GlobalApplyStart, out o_dateTime))
			{
				return Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
			}
			break;
		}
		}
		return -1;
	}

	public static void CreateDungeonList(Action<int, int, int> action)
	{
		GuildSuppressSessionInfo sessionInfo = GetSessionInfo();
		if (sessionInfo == null)
		{
			return;
		}
		GuildSuppressSessionData sessionData = GetSessionData((int)sessionInfo.SessionIndex);
		if (sessionData != null)
		{
			int[] level = sessionData.Level;
			int[] suppressDungeonIndex = sessionData.SuppressDungeonIndex;
			int raidIndex = sessionData.RaidIndex;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(level); i++)
			{
				int i_argB = level[i];
				int i_argC = suppressDungeonIndex[i];
				Vespa_System_Action_Extension_Method.fire(action, raidIndex, i_argB, i_argC);
			}
		}
	}

	public static void CreateDungeonList(GuildSuppressSessionInfo sessionInfo, Action<int, int, int> action)
	{
		if (sessionInfo == null)
		{
			return;
		}
		GuildSuppressSessionData sessionData = GetSessionData((int)sessionInfo.SessionIndex);
		if (sessionData != null)
		{
			int[] level = sessionData.Level;
			int[] suppressDungeonIndex = sessionData.SuppressDungeonIndex;
			int raidIndex = sessionData.RaidIndex;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(level); i++)
			{
				int i_argB = level[i];
				int i_argC = suppressDungeonIndex[i];
				Vespa_System_Action_Extension_Method.fire(action, raidIndex, i_argB, i_argC);
			}
		}
	}

	public static CreatureData GetBossCreatureData(int raidIndex, int level, out CampaignWaveData bossWaveData)
	{
		RaidData raidData = GetRaidData(raidIndex, level);
		bossWaveData = null;
		if (raidData == null)
		{
			return null;
		}
		return GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(raidData.ChapterIndex, raidData.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, out bossWaveData);
	}

	public static bool GetBossHp(int sessionIndex, int raidIndex, int level, long remainHp, out long bossRemainHp, out long bossMaxHp)
	{
		bossRemainHp = 0L;
		bossMaxHp = 1L;
		GuildSuppressSessionData sessionData = GetSessionData(sessionIndex);
		bool flag = false;
		if (sessionData != null && __Extension_Method_ICollection__.COUNT(sessionData.Level) > 0)
		{
			int last = Vespa_Generic_IList_Extension_Method.GetLast(sessionData.Level);
			flag = level == last;
		}
		else
		{
			ICollection<int> raidLevels = GameRoot.Instance.RaidDataContainer.GetRaidLevels(raidIndex);
			if (__Extension_Method_ICollection__.COUNT(raidLevels) > 0)
			{
				flag = !raidLevels.Contains(level + 1);
			}
		}
		if (flag)
		{
			return false;
		}
		RaidData raidData = GetRaidData(raidIndex, level);
		if (raidData == null)
		{
			return false;
		}
		GuildManager.GetGuildSuppressHpInfo(UIHelper.GetMonsterInfoFromDungeon(raidData.ChapterIndex, raidData.DungeonIndex, DungeonDifficultyType.Easy, isScenario: false, showDuplicated: true), out bossMaxHp);
		if (remainHp.Equals(0L))
		{
			bossRemainHp = bossMaxHp;
		}
		else
		{
			bossRemainHp = remainHp;
		}
		return true;
	}

	public static BattleDefineData GetBattleDefineData(int raidIndex, int level)
	{
		RaidData raidData = GetRaidData(raidIndex, level);
		if (raidData == null)
		{
			return null;
		}
		return GetDungeonData(raidData.ChapterIndex, raidData.DungeonIndex)?.GetBattleDefine(isScenario: false);
	}

	public static void CreateBanComponent(BanRuleData banRuleData, Action<RaidBanType, string> action)
	{
		if (banRuleData == null)
		{
			return;
		}
		RaidBanRuleData[] banRuleDataAry = banRuleData.GetBanRuleDataAry();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(banRuleDataAry); i++)
		{
			RaidBanRuleData raidBanRuleData = banRuleDataAry[i];
			if (raidBanRuleData == null || raidBanRuleData.BanType == RaidBanType.None)
			{
				continue;
			}
			if (raidBanRuleData.BanType == RaidBanType.CodeName)
			{
				string[] array = raidBanRuleData.BanValue.Split(',');
				foreach (string i_argB in array)
				{
					Vespa_System_Action_Extension_Method.fire(action, raidBanRuleData.BanType, i_argB);
				}
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(action, raidBanRuleData.BanType, raidBanRuleData.BanValue);
			}
		}
	}

	public static void SetBossPortrait(string portraitPath, UITexture texture, Component root)
	{
		AssetTexture plainTexture = UIHelper.GetPlainTexture(portraitPath);
		UIHelper.ResetPlaneAssetTextureAsync(root, texture, plainTexture);
	}

	public static void SetBossNameLabel(int raidIndex, int level, UILabel nameLabel, UILabel namePrefixLabel)
	{
		SetBossNameLabel(GetBossCreatureData(raidIndex, level, out var bossWaveData), bossWaveData, nameLabel, namePrefixLabel);
	}

	public static void SetBossNameLabel(CreatureData creatureData, CampaignWaveData bossWaveData, UILabel nameLabel, UILabel namePrefixLabel)
	{
		if (creatureData != null)
		{
			string visualInfo = bossWaveData?.BossVisualInfo;
			string creatureName = string.Empty;
			string creatureNamePrefix = string.Empty;
			GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(creatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
			UIHelper.SetLabelText(nameLabel, creatureName);
			UIHelper.SetLabelText(namePrefixLabel, creatureNamePrefix);
		}
	}

	public static void SetHeroPortrait(int heroIndex, bool isBattlePortrait, UITexture texture, Component root)
	{
		string text = ((!isBattlePortrait) ? UIAvatarHelper.GetIconPath(heroIndex) : UIAvatarHelper.GetBattlePortraitPath(heroIndex));
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(text);
			UIHelper.ResetPlaneAssetTextureAsync(root, texture, plainTexture);
		}
	}

	public static void SetGuildLogo(int logoIndex, int logoBgIndex, UITexture logoTexture, UITexture bgTexture, Component root)
	{
		logoIndex = ((logoIndex <= 0) ? 1 : logoIndex);
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(logoIndex);
		UIHelper.ResetPlaneAssetTextureAsync(root, logoTexture, guildLogoIcon);
		logoBgIndex = ((logoBgIndex <= 0) ? 1 : logoBgIndex);
		AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(logoBgIndex);
		UIHelper.ResetPlaneAssetTextureAsync(root, bgTexture, guildBackGround2D);
	}

	public static void SetGuildCountyTexture(string countryCode, UITexture texture, Component root)
	{
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(root, texture, plainTexture);
		}
	}

	public static void SetGuildMemberCount(int guildMemberCount, int guildLevel, UILabel countLabel)
	{
		int maxMemberCount = GetMaxMemberCount(guildLevel);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(guildMemberCount.ToString()).Append("/").Append(maxMemberCount.ToString());
		UIHelper.SetLabelText(countLabel, stringBuilder.ToString());
	}

	public static int GetMaxMemberCount(int guildLevel)
	{
		return GameRoot.Instance.GuildLevelDataContainer.GetData(guildLevel)?.MaxMember ?? 0;
	}

	public static void SetRank(int rank, UISpriteSelector rankSelector, UILabel rankLabel)
	{
		bool flag = false;
		rank++;
		if ((uint)(rank - 1) <= 2u)
		{
			flag = true;
			if (rankSelector != null)
			{
				rankSelector.SelectInt(rank);
			}
		}
		else
		{
			string text = ((rank == 0) ? "-" : rank.ToString());
			UIHelper.SetLabelText(rankLabel, text);
		}
		UIHelper.EnableGameObject(rankSelector, flag);
		UIHelper.EnableGameObject(rankLabel, !flag);
	}

	public static void SetGuildLevel(UILabel levelLabel, int level)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Lv.").Append(level.ToString());
		UIHelper.SetLabelText(levelLabel, stringBuilder.ToString());
	}

	public static BanRuleData GetBanRuleData(GuildSuppressDungeonData suppressDungeonData)
	{
		if (suppressDungeonData == null)
		{
			return null;
		}
		return GetBanRuleData(suppressDungeonData.BanIndex);
	}

	private static BanRuleData GetBanRuleData(int banIndex)
	{
		return GameRoot.Instance.BanRuleDataContainer.GetData(banIndex);
	}

	private static CampaignDungeonData GetDungeonData(int chapterIndex, int dungeonIndex)
	{
		return GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
	}

	private static RaidData GetRaidData(int raidIndex, int level)
	{
		return GameRoot.Instance.RaidDataContainer.GetData(raidIndex, level);
	}

	private static GuildSuppressSessionData GetSessionData(int sessionIndex)
	{
		return RestoredPortal.ConquestSession(GameRoot.Instance.GuildSuppressSessionDataContainer, sessionIndex);
	}

	private static GuildSuppressSessionInfo GetSessionInfo()
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return null;
		}
		return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager?.GuildSuppressManager.GetGuildSuppressSessionInfo();
	}
}
