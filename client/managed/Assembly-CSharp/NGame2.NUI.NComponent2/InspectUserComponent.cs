using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class InspectUserComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/InspectUser/InspectUserComponent";

	public static PrefabAuto<InspectUserComponent> PrefabAuto = new PrefabAuto<InspectUserComponent>("Prefab2/Component/InspectUser/InspectUserComponent");

	public UILabel LabelName;

	public UILabel LabelContent;

	public Transform ArenaTierRoot;

	public UISpriteSelector NameSelector;

	public GameObject[] BronzeLevel;

	public GameObject[] SilverLevel;

	public GameObject[] GoldLevel;

	public GameObject[] PlatinumLevel;

	public GameObject[] DiamondLevel;

	public GameObject[] MasterLevel;

	public GameObject[] RoyalMasterLevel;

	public void Open(InspectUserData userData, CachedAccountInfo accountInfo, int rank)
	{
		UIHelper.SetLabelText(LabelName, LocalizationManager.GetStringDirect(userData.StringKey));
		CachedUserInfo userInfo = accountInfo.UserInfo;
		MatchTierData matchTierData = null;
		if (userInfo == null)
		{
			return;
		}
		if (userData.Type == InspectUserType.ArenaTier)
		{
			matchTierData = GameRoot.Instance.MatchTierDataContainer.GetData(userInfo.TierIndex);
		}
		UIHelper.EnableGameObject(ArenaTierRoot, userData.Type == InspectUserType.ArenaTier && matchTierData != null);
		UIHelper.EnableGameObject(LabelContent, userData.Type != InspectUserType.ArenaTier || matchTierData == null);
		switch (userData.Type)
		{
		case InspectUserType.ArenaTier:
			if (matchTierData != null)
			{
				MatchTierType tier = matchTierData.Tier;
				int level = matchTierData.Level;
				if (tier == MatchTierType.Challenger && rank == 0)
				{
					NameSelector.Select("Champion");
				}
				else
				{
					NameSelector.SelectEnum(tier);
				}
				for (int i = 0; i < BronzeLevel.Length; i++)
				{
					UIHelper.EnableGameObject(BronzeLevel[i], tier == MatchTierType.Bronze && i == level);
				}
				for (int j = 0; j < SilverLevel.Length; j++)
				{
					UIHelper.EnableGameObject(SilverLevel[j], tier == MatchTierType.Silver && j == level);
				}
				for (int k = 0; k < GoldLevel.Length; k++)
				{
					UIHelper.EnableGameObject(GoldLevel[k], tier == MatchTierType.Gold && k == level);
				}
				for (int l = 0; l < PlatinumLevel.Length; l++)
				{
					UIHelper.EnableGameObject(PlatinumLevel[l], tier == MatchTierType.Platinum && l == level);
				}
				for (int m = 0; m < DiamondLevel.Length; m++)
				{
					UIHelper.EnableGameObject(DiamondLevel[m], tier == MatchTierType.Diamond && m == level);
				}
				for (int n = 0; n < MasterLevel.Length; n++)
				{
					UIHelper.EnableGameObject(MasterLevel[n], tier == MatchTierType.Master && n == level);
				}
				for (int num3 = 0; num3 < RoyalMasterLevel.Length; num3++)
				{
					UIHelper.EnableGameObject(RoyalMasterLevel[num3], tier == MatchTierType.RoyalMaster && num3 == level);
				}
			}
			else
			{
				UIHelper.SetLabelText(LabelContent, "-");
			}
			break;
		case InspectUserType.ArenaRank:
			UIHelper.SetLabelText(LabelContent, rank + 1);
			break;
		case InspectUserType.ArenaScore:
		{
			string text = LocalizationManager.GetString(Match.SCORE_POSTFIX);
			string text2 = userInfo.MatchScore + text;
			UIHelper.SetLabelText(LabelContent, text2);
			break;
		}
		case InspectUserType.ArenaSuccesiveWin:
			UIHelper.SetLabelText(LabelContent, userInfo.SuccessiveWin);
			break;
		case InspectUserType.ArenaWinRatio:
		{
			int num2 = 0;
			if (userInfo.SeasonWin + userInfo.SeasonLose > 0)
			{
				num2 = userInfo.SeasonWin * 100 / (userInfo.SeasonWin + userInfo.SeasonLose);
			}
			UIHelper.SetLabelText(LabelContent, num2 + "%");
			break;
		}
		case InspectUserType.ArenaWinLose:
		{
			string stringDirect = LocalizationManager.GetStringDirect("ARENA_BATTLE_WIN", userInfo.SeasonWin);
			string stringDirect2 = LocalizationManager.GetStringDirect("ARENA_BATTLE_LOSE", userInfo.SeasonLose);
			UIHelper.SetLabelText(LabelContent, stringDirect + " / " + stringDirect2);
			break;
		}
		case InspectUserType.AccessTime:
		{
			Vespa_String_Extension_Method2.TryParseNetworkTime2(userInfo.LastLoginTime, out var o_dateTime);
			int num4 = (int)(NGame2.NUI.NWindow.Common.GetUTCNow() - o_dateTime).TotalDays;
			if (num4 == 0)
			{
				UIHelper.SetLabelText(LabelContent, LocalizationManager.GetString(Guild.GUILD_LASTLOGINTIME_TODAY));
				break;
			}
			UIHelper.SetLabelText(LabelContent, LocalizationManager.GetString(Guild.GUILD_LASTLOGINTIME_DAY, num4));
			break;
		}
		case InspectUserType.ChapterIndex:
		{
			CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(userInfo.LastChapterIndex);
			if (data != null)
			{
				string stringDirect3 = LocalizationManager.GetStringDirect(data.Name);
				UIHelper.SetLabelText(LabelContent, stringDirect3);
			}
			else
			{
				UIHelper.SetLabelText(LabelContent, string.Empty);
			}
			break;
		}
		case InspectUserType.NumHero:
			UIHelper.SetLabelText(LabelContent, accountInfo.HeroInfos.Count);
			break;
		case InspectUserType.MaxHeroLevel:
		{
			int num = 0;
			foreach (KeyValuePair<int, CachedHeroInfo> heroInfo in accountInfo.HeroInfos)
			{
				if (heroInfo.Value.Level > num)
				{
					num = heroInfo.Value.Level;
				}
			}
			UIHelper.SetLabelText(LabelContent, "Lv." + num);
			break;
		}
		case InspectUserType.GuildRank:
			if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
			{
				GuildMemberInfo guildMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Get(userInfo.AccountId);
				if (guildMemberInfo != null)
				{
					UIHelper.SetLabelText(LabelContent, ManageGuildMemberComponent.GetGuildRankString(guildMemberInfo.Rank));
				}
			}
			break;
		case InspectUserType.Title:
		case InspectUserType.WorldBossRank:
		case InspectUserType.WorldBossGuildRank:
		case InspectUserType.WorldBossDamage:
		case InspectUserType.WorldBossMaxDamage:
		case InspectUserType.WorldBossJoinCount:
		case InspectUserType.WorldBossTotalCount:
			break;
		}
	}
}
