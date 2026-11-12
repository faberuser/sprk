using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class OrdealArenaRewardRatingComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/OrdealArena/OrdealArenaRewardRatingComponent";

	public static PrefabAuto<OrdealArenaRewardRatingComponent> PrefabAuto = new PrefabAuto<OrdealArenaRewardRatingComponent>("Resources/Prefab2/Component/OrdealArena/OrdealArenaRewardRatingComponent");

	public UILabel LabelRatingArrange;

	public UISimpleGrid GridReward;

	private List<CurrencyIconComponent> _listCurrencyIconComponent = new List<CurrencyIconComponent>();

	private List<ItemIconComponent> _listItemComponent = new List<ItemIconComponent>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_listItemComponent);
		FactoryBase.ReleaseAndClearList(_listCurrencyIconComponent);
	}

	public void Open(OrdealArenaRewardRatingData data)
	{
		if (data != null)
		{
			SetRating(data.GetMinRating(), data.GetMaxRating());
			SetReward(data.RewardIndex);
		}
	}

	public void SetRating(int min, int max)
	{
		UIHelper.SetLabelText(LabelRatingArrange, $"{min}~{max}");
	}

	public void SetReward(int rewardIndex)
	{
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(rewardItemList))
		{
			return;
		}
		foreach (RewardElementData item in rewardItemList)
		{
			CurrencyType currencyType = item.GetCurrencyType();
			if (currencyType != CurrencyType.None)
			{
				CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(GridReward, CurrencyIconComponent.PrefabAuto);
				int itemMax = item.ItemMax;
				CurrencyIconComponent.Listener listener = null;
				int num = itemMax;
				currencyIconComponent.Open(currencyType, num, listener);
				_listCurrencyIconComponent.Add(currencyIconComponent);
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(GridReward, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(dataByCode);
				int itemMin = item.ItemMin;
				Color? color = null;
				int num2 = itemMin;
				itemIconComponent.SetItemCount(num2, color);
				_listItemComponent.Add(itemIconComponent);
			}
		}
	}
}
