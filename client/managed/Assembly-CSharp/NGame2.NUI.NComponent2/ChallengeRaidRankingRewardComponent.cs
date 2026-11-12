using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ChallengeRaidRankingRewardComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Raid/ChallengeRaidRankingRewardComponent";

	public static PrefabAuto<ChallengeRaidRankingRewardComponent> PrefabAuto = new PrefabAuto<ChallengeRaidRankingRewardComponent>("Prefab2/Component/Raid/ChallengeRaidRankingRewardComponent");

	public UIScrollGrid Grid_RewardItem;

	public UILabel RankingTop;

	public UILabel RankingOver2;

	public UILabel RankingPercent;

	public UILabel RankingScore;

	private List<UIScrollItemBehaviour> _itemIconComponentList = new List<UIScrollItemBehaviour>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClearItemIconList();
	}

	public void SetRankData(int minRank, int maxRank, int rewardIndex)
	{
		ClearItemIconList();
		if (minRank == maxRank)
		{
			string stringDirect = LocalizationManager.GetStringDirect("GUILD_RANKCOUNT", minRank);
			SetText(RankingTop, stringDirect);
		}
		else if (minRank > 0 && maxRank == 0)
		{
			string stringDirect2 = LocalizationManager.GetStringDirect("CHALLENGE_RAID_RANK_COUNT_01", minRank);
			SetText(RankingTop, stringDirect2);
		}
		else
		{
			string stringDirect3 = LocalizationManager.GetStringDirect("WORLDBOSS_RANK_COUNT", minRank, maxRank);
			SetText(RankingOver2, stringDirect3);
		}
		InitReward(rewardIndex);
	}

	public void SetPercentageData(int percentage, int rewardIndex)
	{
		ClearItemIconList();
		string stringDirect = LocalizationManager.GetStringDirect("WORLDBOSS_RANK_COUNT_PERCENT", percentage);
		SetText(RankingPercent, stringDirect);
		InitReward(rewardIndex);
	}

	public void SetScoreData(long minScore, long maxScore, int rewardIndex, int worldBossPoint)
	{
		ClearItemIconList();
		string text = ((maxScore != 0L) ? LocalizationManager.GetStringDirect("WORLDBOSS_SCORE_RANGE", Vespa_Long_Extension_Method.ToCommaString(minScore), Vespa_Long_Extension_Method.ToCommaString(maxScore)) : LocalizationManager.GetStringDirect("WORLDBOSS_SCORE_RANGE_OVER", Vespa_Long_Extension_Method.ToCommaString(minScore)));
		SetText(RankingScore, text);
		InitReward(rewardIndex, worldBossPoint);
	}

	public void SetData(ChallengeRaidRewardData challengeRaidRewardData)
	{
		int minRank = challengeRaidRewardData.MinRank;
		int maxRank = challengeRaidRewardData.MaxRank;
		int percentage = challengeRaidRewardData.Percentage;
		int rewardIndex = challengeRaidRewardData.RewardIndex;
		if (minRank != 0)
		{
			SetRankData(minRank, maxRank, rewardIndex);
		}
		else if (percentage != 0)
		{
			SetPercentageData(percentage, rewardIndex);
		}
	}

	private void SetText(UILabel label, string text)
	{
		UIHelper.EnableGameObject(RankingTop, RankingTop == label);
		UIHelper.EnableGameObject(RankingOver2, RankingOver2 == label);
		UIHelper.EnableGameObject(RankingPercent, RankingPercent == label);
		UIHelper.EnableGameObject(RankingScore, RankingScore == label);
		UIHelper.SetLabelText(label, text);
	}

	private void InitReward(int rewardIndex, int worldBossPoint = 0)
	{
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data != null)
		{
			int GroupCount;
			foreach (RewardElementData item in data.GetRewardItemListWithGroupCount(out GroupCount))
			{
				if (data == null)
				{
					continue;
				}
				CurrencyType currencyType = item.GetCurrencyType();
				if (currencyType != CurrencyType.None)
				{
					CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(Grid_RewardItem, CurrencyIconComponent.PrefabAuto);
					int itemMin = item.ItemMin;
					CurrencyIconComponent.Listener listener = null;
					int num = itemMin;
					currencyIconComponent.Open(currencyType, num, listener);
					_itemIconComponentList.Add(currencyIconComponent);
					continue;
				}
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
				if (dataByCode != null)
				{
					int itemMin2 = item.ItemMin;
					long count = itemMin2;
					int itemMax = item.ItemMax;
					long maxCount = itemMax;
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_RewardItem, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(dataByCode);
					if (dataByCode.Type != ItemType.Equip)
					{
						itemIconComponent.SetItemMinMax(count, maxCount);
					}
					_itemIconComponentList.Add(itemIconComponent);
				}
			}
		}
		if (worldBossPoint > 0)
		{
			CurrencyIconComponent currencyIconComponent2 = UIHelper.AddGridItem(Grid_RewardItem, CurrencyIconComponent.PrefabAuto);
			CurrencyIconComponent.Listener listener2 = null;
			currencyIconComponent2.Open(CurrencyType.WorldBossPoint, worldBossPoint, listener2);
			_itemIconComponentList.Add(currencyIconComponent2);
		}
		UIHelper.RepositionGrid(Grid_RewardItem);
	}

	private void ClearItemIconList()
	{
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
	}
}
