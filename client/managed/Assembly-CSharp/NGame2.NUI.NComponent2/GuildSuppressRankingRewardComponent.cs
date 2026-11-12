using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressRankingRewardComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressRankingRewardComponent";

	public static PrefabAuto<GuildSuppressRankingRewardComponent> PrefabAuto = new PrefabAuto<GuildSuppressRankingRewardComponent>("Prefab2/Component/GuildSuppress/GuildSuppressRankingRewardComponent");

	public UISimpleGrid Grid_RewardItem;

	public UILabel RankingTop;

	public UILabel RankingOver2;

	public UILabel RankingPercent;

	public UILabel Desc;

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

	public void SetPercentageData(int percentage, int rewardIndex)
	{
		string stringDirect = LocalizationManager.GetStringDirect("WORLDBOSS_RANK_COUNT_PERCENT", percentage);
		SetText(RankingPercent, stringDirect);
		InitReward(rewardIndex);
	}

	private void SetServerRewardData(GuildSuppressServerRewardType rewardType, string rewardCode)
	{
		if (rewardType == GuildSuppressServerRewardType.Booster)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_RewardItem, ItemIconComponent.PrefabAuto);
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardCode);
			itemIconComponent.Open(dataByCode);
			_itemIconComponentList.Add(itemIconComponent);
		}
		UIHelper.RepositionGrid(Grid_RewardItem);
	}

	public void SetData(GuildSuppressRewardData rewardData)
	{
		int minRank = rewardData.MinRank;
		int maxRank = rewardData.MaxRank;
		int percentage = rewardData.Percentage;
		int rewardIndex = rewardData.RewardIndex;
		GuildSuppressServerRewardType rewardServerBuffType = rewardData.RewardServerBuffType;
		string rewardServerBuffCode = rewardData.RewardServerBuffCode;
		ClearItemIconList();
		if (rewardServerBuffType != GuildSuppressServerRewardType.None && !String_Extension_Method.IsNullOrEmpty(rewardServerBuffCode))
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
		UIHelper.SetLabelText(Desc, LocalizationManager.GetStringDirect(rewardData.DescKey));
	}

	private void SetText(UILabel label, string text)
	{
		UIHelper.EnableGameObject(RankingTop, RankingTop == label);
		UIHelper.EnableGameObject(RankingOver2, RankingOver2 == label);
		UIHelper.EnableGameObject(RankingPercent, RankingPercent == label);
		UIHelper.SetLabelText(label, text);
	}

	private void InitReward(int rewardIndex)
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
		UIHelper.RepositionGrid(Grid_RewardItem);
	}

	private void ClearItemIconList()
	{
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
	}
}
