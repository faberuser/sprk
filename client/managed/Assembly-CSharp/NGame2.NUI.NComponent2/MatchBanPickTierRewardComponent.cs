using System;
using System.Collections.Generic;
using NGame2.NContentsDefine;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MatchBanPickTierRewardComponent : LinkFactoryComponent, InitOnCreate
{
	[Serializable]
	public class LevelInfo
	{
		public GameObject Root;

		public UIGrid RewardGrid;

		public UIResizeByGrid ResizedBg;

		[Header("랭킹 표시")]
		public Transform TierIconParent;

		public GameObject RankObject;

		public UILabel MinRankLabel;

		public UILabel RankCenterLabel;

		public UILabel MaxRankLabel;

		[Header("결투장 점수")]
		public UILabel MatchScoreLabel;

		[Header("결투장 랭킹")]
		public UILabel MatchRankLabel;

		[Header("주간 보상")]
		public GameObject WeeklyRewardRoot;

		public UISpriteSelector WeeklyBoxRewardIcon;

		public UILabel WeeklyBoxRewardCount;

		public UILabel WeeklyRewardCount;

		public UILabel WeeklyRewardExpItemCount;

		[Header("승리 보상")]
		public GameObject BanPickWinReward;

		public UILabel BanPickWinRewardLabel;
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Match/MatchBanPickTierRewardComponent";

	public const string GlobalPrefabPath = "Resources/Prefab2/Component/Match/GlobalBanPickTierRewardComponent";

	public static PrefabAuto<MatchBanPickTierRewardComponent> PrefabAuto = new PrefabAuto<MatchBanPickTierRewardComponent>("Resources/Prefab2/Component/Match/MatchBanPickTierRewardComponent");

	public static PrefabAuto<MatchBanPickTierRewardComponent> GlobalPrefabAuto = new PrefabAuto<MatchBanPickTierRewardComponent>("Resources/Prefab2/Component/Match/GlobalBanPickTierRewardComponent");

	public GameObject BronzeName;

	public GameObject SilverName;

	public GameObject GoldName;

	public GameObject PlatinumName;

	public GameObject DiamondName;

	public GameObject MasterName;

	public GameObject RoyalMasterName;

	public GameObject ChallengerName;

	public GameObject ChampionName;

	public LevelInfo[] LevelInfos;

	private List<MatchTierIconComponent> _tierIconList;

	private MatchTierType _tierType;

	private MatchPvpType _pvpType;

	private bool _isChampion;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(MatchTierType tierType, MatchPvpType pvpType)
	{
		Init(tierType, pvpType, isChampion: false);
		List<MatchBanPickTierData> tierDataListOfType = GameRoot.Instance.GlobalBanPickTierDataContainer.GetTierDataListOfType(tierType);
		if (__Extension_Method_ICollection__.COUNT(tierDataListOfType) == 0)
		{
			return;
		}
		List<MatchRewardDataKey2> championsOfMatch = ContentsDefine.MatchArena.GetChampionsOfMatch(pvpType);
		int num = 0;
		tierDataListOfType.Sort((MatchBanPickTierData lhs, MatchBanPickTierData rhs) => lhs.Index.CompareTo(rhs.Index));
		for (int num2 = 0; num2 < tierDataListOfType.Count; num2++)
		{
			MatchBanPickTierData matchBanPickTierData = tierDataListOfType[num2];
			if (matchBanPickTierData == null)
			{
				continue;
			}
			int index = matchBanPickTierData.Index;
			List<MatchBanPickTierRewardData> rewardDataListOfTierIndex = GameRoot.Instance.GlobalBanPickTierRewardDataContainer.GetRewardDataListOfTierIndex(index);
			if (__Extension_Method_ICollection__.COUNT(rewardDataListOfTierIndex) == 0)
			{
				continue;
			}
			rewardDataListOfTierIndex.Sort((MatchBanPickTierRewardData lhs, MatchBanPickTierRewardData rhs) => rhs.MinRank.CompareTo(lhs.MinRank));
			for (int num3 = 0; num3 < rewardDataListOfTierIndex.Count; num3++)
			{
				if (num >= LevelInfos.Length)
				{
					break;
				}
				MatchBanPickTierRewardData matchBanPickTierRewardData = rewardDataListOfTierIndex[num3];
				if (championsOfMatch == null || !championsOfMatch.Contains(matchBanPickTierRewardData.GETKEY()))
				{
					LevelInfo levelInfo = LevelInfos[num];
					CreateFixedRank(matchBanPickTierData, matchBanPickTierRewardData, levelInfo);
					num++;
				}
			}
		}
	}

	public void OpenChampion(MatchPvpType pvpType)
	{
		Init(MatchTierType.Challenger, pvpType, isChampion: true);
		List<MatchRewardDataKey2> championsOfMatch = ContentsDefine.MatchArena.GetChampionsOfMatch(pvpType);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(championsOfMatch) && i < __Extension_Method_ICollection__.COUNT(LevelInfos); i++)
		{
			LevelInfo levelInfo = LevelInfos[i];
			MatchRewardDataKey2 key = championsOfMatch[i];
			MatchBanPickTierRewardData data = GameRoot.Instance.GlobalBanPickTierRewardDataContainer.GetData(key);
			if (data != null)
			{
				MatchBanPickTierData data2 = GameRoot.Instance.GlobalBanPickTierDataContainer.GetData(data.TierIndex);
				if (data2 != null)
				{
					CreateFixedRank(data2, data, levelInfo);
				}
			}
		}
	}

	private void Init(MatchTierType tierType, MatchPvpType pvpType, bool isChampion)
	{
		_tierType = tierType;
		_pvpType = pvpType;
		_isChampion = isChampion;
		if (_tierIconList == null)
		{
			_tierIconList = new List<MatchTierIconComponent>();
		}
		LevelInfo[] levelInfos = LevelInfos;
		for (int i = 0; i < levelInfos.Length; i++)
		{
			UIHelper.EnableGameObject(levelInfos[i].Root, enable: false);
		}
		InitName(isChampion);
	}

	public void Close()
	{
		FactoryBase.ReleaseAndClearList(_tierIconList);
	}

	private new void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			Close();
		}
	}

	private void InitName(bool isChampion)
	{
		UIHelper.EnableGameObject(BronzeName, _tierType == MatchTierType.Bronze);
		UIHelper.EnableGameObject(SilverName, _tierType == MatchTierType.Silver);
		UIHelper.EnableGameObject(GoldName, _tierType == MatchTierType.Gold);
		UIHelper.EnableGameObject(PlatinumName, _tierType == MatchTierType.Platinum);
		UIHelper.EnableGameObject(DiamondName, _tierType == MatchTierType.Diamond);
		UIHelper.EnableGameObject(MasterName, _tierType == MatchTierType.Master);
		UIHelper.EnableGameObject(RoyalMasterName, _tierType == MatchTierType.RoyalMaster);
		UIHelper.EnableGameObject(ChallengerName, _tierType == MatchTierType.Challenger && !isChampion);
		UIHelper.EnableGameObject(ChampionName, isChampion);
	}

	private void CreateFixedRank(MatchTierData tierData, MatchRewardData rewardData, LevelInfo levelInfo)
	{
		if (tierData != null && rewardData != null && levelInfo != null)
		{
			UIHelper.EnableGameObject(levelInfo.Root, enable: true);
			MatchBanPickTierRewardData banPickRewardData = rewardData as MatchBanPickTierRewardData;
			SetTierIcon(tierData, rewardData, levelInfo);
			SetRank(rewardData, levelInfo);
			SetMatchScore(tierData, rewardData, levelInfo);
			SetWeeklyBoxReward(banPickRewardData, levelInfo);
			SetBanPickWinReward(banPickRewardData, levelInfo);
		}
	}

	private void SetTierIcon(MatchTierData tierData, MatchRewardData rewardData, LevelInfo levelInfo)
	{
		VisualTierRank visualTierRank = VisualTierRank.CreateFromTableMinRank(rewardData, _pvpType.ToMatchArenaType());
		MatchTierIconComponent matchTierIconComponent = MatchTierIconComponent.PrefabAuto.Factory.Create(levelInfo.TierIconParent);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(matchTierIconComponent.transform);
		matchTierIconComponent.Open(tierData.Tier, tierData.Level, visualTierRank, _pvpType.ToMatchArenaType());
		matchTierIconComponent.HideName();
		_tierIconList.Add(matchTierIconComponent);
	}

	private void SetRank(MatchRewardData rewardData, LevelInfo levelInfo)
	{
		VisualTierRank visualTierRank = VisualTierRank.CreateFromTableMinRank(rewardData, _pvpType.ToMatchArenaType());
		VisualTierRank visualTierRank2 = VisualTierRank.CreateFromTableMaxRank(rewardData, _pvpType.ToMatchArenaType());
		if (visualTierRank.Value > 0)
		{
			UIHelper.EnableGameObject(levelInfo.RankObject, enable: true);
			if (visualTierRank.Value > 0)
			{
				if (visualTierRank.Value != visualTierRank2.Value)
				{
					string rankLabelText = UIHelper.GetRankLabelText(visualTierRank);
					string rankLabelText2 = UIHelper.GetRankLabelText(visualTierRank2);
					UIHelper.EnableGameObject(levelInfo.MinRankLabel, enable: true);
					UIHelper.EnableGameObject(levelInfo.MaxRankLabel, enable: true);
					UIHelper.SetLabelText(levelInfo.MinRankLabel, rankLabelText);
					UIHelper.SetLabelText(levelInfo.MaxRankLabel, rankLabelText2);
					UIHelper.SetLabelText(levelInfo.RankCenterLabel, "~");
				}
				else
				{
					string rankLabelText3 = UIHelper.GetRankLabelText(visualTierRank);
					UIHelper.EnableGameObject(levelInfo.MinRankLabel, enable: false);
					UIHelper.EnableGameObject(levelInfo.MaxRankLabel, enable: false);
					UIHelper.SetLabelText(levelInfo.RankCenterLabel, rankLabelText3);
				}
			}
			else
			{
				string rankLabelText4 = UIHelper.GetRankLabelText(visualTierRank);
				rankLabelText4 = rankLabelText4 + " " + LocalizationManager.GetString(NShared.NLocalization.Common.UNDER_POSTFIX);
				UIHelper.EnableGameObject(levelInfo.MinRankLabel, enable: false);
				UIHelper.EnableGameObject(levelInfo.MaxRankLabel, enable: false);
				UIHelper.SetLabelText(levelInfo.RankCenterLabel, rankLabelText4);
			}
		}
		else
		{
			UIHelper.EnableGameObject(levelInfo.RankObject, enable: false);
		}
	}

	private void SetMatchScore(MatchTierData tierData, MatchRewardData rewardData, LevelInfo levelInfo)
	{
		VisualTierRank visualTierRank = VisualTierRank.CreateFromTableMinRank(rewardData, _pvpType.ToMatchArenaType());
		VisualTierRank visualTierRank2 = VisualTierRank.CreateFromTableMaxRank(rewardData, _pvpType.ToMatchArenaType());
		if (visualTierRank.Value > 0)
		{
			if (visualTierRank2.Value > 0)
			{
				if (visualTierRank.Value != visualTierRank2.Value)
				{
					string rankLabelText = UIHelper.GetRankLabelText(visualTierRank);
					string rankLabelText2 = UIHelper.GetRankLabelText(visualTierRank2);
					UIHelper.SetLabelText(levelInfo.MatchRankLabel, rankLabelText + " ~ " + rankLabelText2);
				}
				else
				{
					string rankLabelText3 = UIHelper.GetRankLabelText(visualTierRank);
					UIHelper.SetLabelText(levelInfo.MatchRankLabel, rankLabelText3);
				}
			}
			else
			{
				string rankLabelText4 = UIHelper.GetRankLabelText(visualTierRank2);
				rankLabelText4 = rankLabelText4 + " " + LocalizationManager.GetStringDirect("MATCH_RANK_WITHIN");
				UIHelper.SetLabelText(levelInfo.MatchRankLabel, rankLabelText4);
			}
		}
		else if (tierData.MaxPercent > 0f && tierData.MaxPercent < 100f)
		{
			string stringDirect = LocalizationManager.GetStringDirect("MATCH_TOP_PERCENTAGE", tierData.MaxPercent);
			UIHelper.SetLabelText(levelInfo.MatchRankLabel, stringDirect);
		}
		else
		{
			string stringDirect2 = LocalizationManager.GetStringDirect("MATCH_RANK_UNLIMITED");
			UIHelper.SetLabelText(levelInfo.MatchRankLabel, stringDirect2);
		}
		UIHelper.SetLabelText(levelInfo.MatchScoreLabel, tierData.MinScore);
	}

	private void SetWeeklyBoxReward(MatchBanPickTierRewardData banPickRewardData, LevelInfo levelInfo)
	{
		if (levelInfo.WeeklyBoxRewardIcon != null)
		{
			UIHelper.EnableGameObject(levelInfo.WeeklyRewardRoot, banPickRewardData.WeeklyBoxReward > 0);
			if (_isChampion)
			{
				levelInfo.WeeklyBoxRewardIcon.Select("Champion");
			}
			else
			{
				levelInfo.WeeklyBoxRewardIcon.Select(_tierType.ToString());
			}
		}
		UIHelper.SetLabelText(levelInfo.WeeklyRewardCount, banPickRewardData.WeeklyReward);
		UIHelper.SetLabelText(levelInfo.WeeklyRewardExpItemCount, banPickRewardData.WeeklyRewardExpItemCount);
		int weeklyBoxReward = banPickRewardData.WeeklyBoxReward;
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(weeklyBoxReward);
		if (data == null)
		{
			UIHelper.EnableGameObject(levelInfo.WeeklyRewardRoot, enable: false);
			return;
		}
		RewardData rewardData = data;
		UIHelper.SetLabelText(levelInfo.WeeklyBoxRewardCount, rewardData.Item1CountMin);
	}

	private void SetBanPickWinReward(MatchBanPickTierRewardData banPickRewardData, LevelInfo levelInfo)
	{
		UIHelper.EnableGameObject(levelInfo.BanPickWinReward, banPickRewardData.WinReward > 0);
		int winReward = banPickRewardData.WinReward;
		UIHelper.SetLabelText(levelInfo.BanPickWinRewardLabel, winReward);
	}
}
