using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class WorldBossRankingRewardComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/WorldBoss/WorldBossRankingRewardComponent";

	public const string GlobalPrefabPath = "Prefab2/Component/WorldBoss/WorldBossGlobalRankingRewardComponent";

	public const string ScoreRewardPrefabPath = "Prefab2/Component/WorldBoss/WorldBossScoreRewardComponent";

	public static PrefabAuto<WorldBossRankingRewardComponent> PrefabAuto = new PrefabAuto<WorldBossRankingRewardComponent>("Prefab2/Component/WorldBoss/WorldBossRankingRewardComponent");

	public static PrefabAuto<WorldBossRankingRewardComponent> GlobalPrefabAuto = new PrefabAuto<WorldBossRankingRewardComponent>("Prefab2/Component/WorldBoss/WorldBossGlobalRankingRewardComponent");

	public static PrefabAuto<WorldBossRankingRewardComponent> ScoreRewardPrefabAuto = new PrefabAuto<WorldBossRankingRewardComponent>("Prefab2/Component/WorldBoss/WorldBossScoreRewardComponent");

	public UISimpleGrid Grid_RewardItem;

	public UILabel RankingTop;

	public UILabel RankingOver2;

	public UILabel RankingPercent;

	private List<UIScrollItemBehaviour> _itemIconComponentList = new List<UIScrollItemBehaviour>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClearItemIconList();
	}

	public void SetData(WorldBossRewardData worldBossRewardData)
	{
		int minRank = worldBossRewardData.MinRank;
		int maxRank = worldBossRewardData.MaxRank;
		int percentage = worldBossRewardData.Percentage;
		int rewardIndex = worldBossRewardData.RewardIndex;
		WorldBossServerRewardType rewardServerBuffType = worldBossRewardData.RewardServerBuffType;
		string rewardServerBuffCode = worldBossRewardData.RewardServerBuffCode;
		ClearItemIconList();
		if (rewardServerBuffType != WorldBossServerRewardType.None && !String_Extension_Method.IsNullOrEmpty(rewardServerBuffCode))
		{
			SetServerRewardData(rewardServerBuffType, rewardServerBuffCode);
		}
		if (minRank != 0)
		{
			SetRankData(minRank, maxRank, rewardIndex);
		}
		else if (percentage != 0)
		{
			SetPercentageData(percentage, rewardIndex);
		}
	}

	private void SetRankData(int minRank, int maxRank, int rewardIndex)
	{
		if (minRank == maxRank)
		{
			string stringDirect = LocalizationManager.GetStringDirect("GUILD_RANKCOUNT", minRank);
			SetText(RankingTop, stringDirect);
		}
		else
		{
			string stringDirect2 = LocalizationManager.GetStringDirect("WORLDBOSS_RANK_COUNT", minRank, maxRank);
			SetText(RankingOver2, stringDirect2);
		}
		InitReward(rewardIndex);
	}

	private void SetPercentageData(int percentage, int rewardIndex)
	{
		string stringDirect = LocalizationManager.GetStringDirect("WORLDBOSS_RANK_COUNT_PERCENT", percentage);
		SetText(RankingPercent, stringDirect);
		InitReward(rewardIndex);
	}

	private void SetServerRewardData(WorldBossServerRewardType rewardType, string rewardCode)
	{
		if (rewardType == WorldBossServerRewardType.Booster)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_RewardItem, ItemIconComponent.PrefabAuto);
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardCode);
			itemIconComponent.Open(dataByCode);
			_itemIconComponentList.Add(itemIconComponent);
		}
		UIHelper.RepositionGrid(Grid_RewardItem);
	}

	private void SetText(UILabel label, string text)
	{
		UIHelper.EnableGameObject(RankingTop, RankingTop == label);
		UIHelper.EnableGameObject(RankingOver2, RankingOver2 == label);
		UIHelper.EnableGameObject(RankingPercent, RankingPercent == label);
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
