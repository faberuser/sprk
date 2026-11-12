using System.Collections.Generic;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class OrdealArenaRewardAreaComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/OrdealArena/OrdealArenaRewardAreaComponent";

	public static PrefabAuto<OrdealArenaRewardAreaComponent> PrefabAuto = new PrefabAuto<OrdealArenaRewardAreaComponent>("Resources/Prefab2/Component/OrdealArena/OrdealArenaRewardAreaComponent");

	public UISpriteSelector SeletorTier;

	public UISpriteSelector SeletorTierName;

	public UISimpleGrid GridReward;

	private List<ItemIconComponent> _listItemComponent = new List<ItemIconComponent>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_listItemComponent);
	}

	public void Open(MatchTierType tierType, List<OrdealArenaRewardAreaData> listData)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(listData))
		{
			SetTier(tierType.ToString());
			SetRewardItem(listData);
		}
	}

	private void SetTier(string matchTierTypeString)
	{
		if (SeletorTier != null)
		{
			SeletorTier.Select(matchTierTypeString);
		}
		if (SeletorTierName != null)
		{
			SeletorTierName.Select(matchTierTypeString);
		}
	}

	private void SetRewardItem(List<OrdealArenaRewardAreaData> listData)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(listData); i++)
		{
			OrdealArenaRewardAreaData ordealArenaRewardAreaData = listData[i];
			if (ordealArenaRewardAreaData == null || ordealArenaRewardAreaData.RewardIndex == 0)
			{
				continue;
			}
			int rewardIndex = ordealArenaRewardAreaData.RewardIndex;
			List<RewardElementData> rewardItemList = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex).GetRewardItemList();
			if (__Extension_Method_ICollection__.IsNullOrEmpty(rewardItemList))
			{
				continue;
			}
			foreach (RewardElementData item in rewardItemList)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
				if (dataByCode != null)
				{
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(GridReward, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(dataByCode);
					int itemMin = item.ItemMin;
					Color? color = null;
					int num = itemMin;
					itemIconComponent.SetItemCount(num, color);
					_listItemComponent.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(GridReward);
	}
}
