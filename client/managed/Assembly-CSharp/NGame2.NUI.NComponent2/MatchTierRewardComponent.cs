using System;
using System.Collections.Generic;
using NGame2.NContentsDefine;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MatchTierRewardComponent : LinkFactoryComponent, InitOnCreate
{
	[Serializable]
	public class LevelInfo
	{
		public GameObject Root;

		public Transform TierIconParent;

		public UILabel MatchScoreLabel;

		public UILabel MatchRankLabel;

		public UILabel DailyRewardLabel;

		public UISpriteSelector DailyBoxRewardIcon;

		public UILabel SeasonRewardLabel;

		public UILabel SeasonRewardExpItemCountLabel;

		public GameObject RankObject;

		public UILabel MinRankLabel;

		public UILabel MaxRankLabel;

		public UILabel RankCenterLabel;
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Match/MatchTierRewardComponent";

	public static PrefabAuto<MatchTierRewardComponent> PrefabAuto = new PrefabAuto<MatchTierRewardComponent>("Resources/Prefab2/Component/Match/MatchTierRewardComponent");

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

	public UILabel FirstRewardPointLabel;

	public GameObject FirstRewardObject;

	public UISpriteSelector RewardSelector;

	public int FirstRewardOffset;

	private List<MatchTierIconComponent> _tierIconList;

	private MatchTierType _tierType;

	private MatchPvpType _pvpType;

	private bool IsChampion;

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
		List<MatchRewardDataKey2> championsOfMatch = ContentsDefine.MatchArena.GetChampionsOfMatch(pvpType);
		int num = 0;
		List<MatchTierData> tierDataListOfType = GameRoot.Instance.MatchTierDataContainer.GetTierDataListOfType(tierType);
		tierDataListOfType.Sort((MatchTierData lhs, MatchTierData rhs) => lhs.Index.CompareTo(rhs.Index));
		for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(tierDataListOfType); num2++)
		{
			MatchTierData matchTierData = tierDataListOfType[num2];
			if (matchTierData == null)
			{
				continue;
			}
			int index = matchTierData.Index;
			List<MatchRewardData> rewardDataListOfTierIndex = GameRoot.Instance.MatchRewardDataContainer.GetRewardDataListOfTierIndex(index);
			if (rewardDataListOfTierIndex.Count == 0)
			{
				continue;
			}
			rewardDataListOfTierIndex.Sort((MatchRewardData lhs, MatchRewardData rhs) => rhs.MinRank.CompareTo(lhs.MinRank));
			for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(rewardDataListOfTierIndex); num3++)
			{
				if (num >= __Extension_Method_ICollection__.COUNT(LevelInfos))
				{
					break;
				}
				MatchRewardData matchRewardData = rewardDataListOfTierIndex[num3];
				if (championsOfMatch == null || !championsOfMatch.Contains(matchRewardData.GETKEY()))
				{
					LevelInfo levelInfo = LevelInfos[num];
					CreateFixedRank(matchTierData, matchRewardData, levelInfo);
					num++;
				}
			}
		}
		if (RewardSelector != null)
		{
			RewardSelector.Select(pvpType.ToString());
		}
		UpdateFirstRewardPosition();
	}

	public void OpenChampion(MatchPvpType pvpType)
	{
		Init(MatchTierType.Challenger, pvpType, isChampion: true);
		List<MatchRewardDataKey2> championsOfMatch = ContentsDefine.MatchArena.GetChampionsOfMatch(pvpType);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(championsOfMatch) && i < __Extension_Method_ICollection__.COUNT(LevelInfos); i++)
		{
			LevelInfo levelInfo = LevelInfos[i];
			MatchRewardDataKey2 key = championsOfMatch[i];
			MatchRewardData data = GameRoot.Instance.MatchRewardDataContainer.GetData(key);
			if (data != null)
			{
				MatchTierData data2 = GameRoot.Instance.MatchTierDataContainer.GetData(data.TierIndex);
				if (data2 != null)
				{
					CreateFixedRank(data2, data, levelInfo);
				}
			}
		}
		if (RewardSelector != null)
		{
			RewardSelector.Select(pvpType.ToString());
		}
		UpdateFirstRewardPosition();
	}

	private void Init(MatchTierType tierType, MatchPvpType pvpType, bool isChampion)
	{
		_tierType = tierType;
		_pvpType = pvpType;
		IsChampion = isChampion;
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
			UIHelper.SetLabelText(levelInfo.DailyRewardLabel, rewardData.DailyReward);
			UIHelper.SetLabelText(levelInfo.SeasonRewardLabel, rewardData.SeasonReward);
			UIHelper.SetLabelText(levelInfo.SeasonRewardExpItemCountLabel, rewardData.SeasonRewardExpItemCount);
			if (rewardData.FirstReward > 0)
			{
				UIHelper.SetLabelText(FirstRewardPointLabel, rewardData.FirstReward);
			}
			SetTierIcon(tierData, rewardData, levelInfo);
			SetRank(rewardData, levelInfo);
			SetMatchScore(tierData, rewardData, levelInfo);
			SetDailyBoxReward(levelInfo);
		}
	}

	private void UpdateFirstRewardPosition()
	{
		if (FirstRewardObject == null)
		{
			return;
		}
		GameObject firstRewardObject = FirstRewardObject;
		LevelInfo[] levelInfos = LevelInfos;
		foreach (LevelInfo levelInfo in levelInfos)
		{
			if ((bool)levelInfo.Root && levelInfo.Root.activeSelf)
			{
				Vector3 localPosition = firstRewardObject.transform.localPosition;
				localPosition.y = levelInfo.Root.transform.localPosition.y + (float)FirstRewardOffset;
				firstRewardObject.transform.localPosition = localPosition;
			}
		}
	}

	private void SetTierIcon(MatchTierData tierData, MatchRewardData rewardData, LevelInfo levelInfo)
	{
		MatchTierIconComponent matchTierIconComponent = MatchTierIconComponent.PrefabAuto.Factory.Create(levelInfo.TierIconParent);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(matchTierIconComponent.transform);
		matchTierIconComponent.Open(tierData.Tier, tierData.Level, VisualTierRank.CreateFromTableMinRank(rewardData, _pvpType.ToMatchArenaType()), _pvpType.ToMatchArenaType());
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
			if (visualTierRank2.Value > 0)
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
					string rankLabelText = UIHelper.GetRankLabelText(visualTierRank2);
					rankLabelText = rankLabelText + " " + LocalizationManager.GetStringDirect("MATCH_RANK_WITHIN");
					UIHelper.SetLabelText(levelInfo.MatchRankLabel, rankLabelText);
				}
				else
				{
					string rankLabelText2 = UIHelper.GetRankLabelText(visualTierRank);
					UIHelper.SetLabelText(levelInfo.MatchRankLabel, rankLabelText2);
				}
			}
			else
			{
				string rankLabelText3 = UIHelper.GetRankLabelText(visualTierRank2);
				rankLabelText3 = rankLabelText3 + " " + LocalizationManager.GetStringDirect("MATCH_RANK_WITHIN");
				UIHelper.SetLabelText(levelInfo.MatchRankLabel, rankLabelText3);
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

	private void SetDailyBoxReward(LevelInfo levelInfo)
	{
		if (levelInfo.DailyBoxRewardIcon != null)
		{
			if (IsChampion)
			{
				levelInfo.DailyBoxRewardIcon.Select("Champion");
			}
			else
			{
				levelInfo.DailyBoxRewardIcon.Select(_tierType.ToString());
			}
		}
	}
}
