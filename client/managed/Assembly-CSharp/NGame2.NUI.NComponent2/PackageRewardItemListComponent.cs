using System.Collections.Generic;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PackageRewardItemListComponent : LinkFactoryComponent
{
	public enum ERewardType
	{
		Fixed,
		RandomSelect
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Reward/PackageRewardItemListComponent";

	public static PrefabAuto<PackageRewardItemListComponent> PrefabAuto = new PrefabAuto<PackageRewardItemListComponent>("Resources/Prefab2/Component/Reward/PackageRewardItemListComponent");

	public UILabel Label_FixedRewardType;

	public UILabel Label_RandomRewardType;

	public UILabel Label_RandomSelect;

	public UISimpleGrid RewardItemListGrid;

	private List<ItemIconComponent> _ListItems = new List<ItemIconComponent>();

	private List<CurrencyIconComponent> _ListCurrencyIcon = new List<CurrencyIconComponent>();

	public void Open(ERewardType rewardType, List<RewardElementData> rewardElementList, int SelectCount = 0)
	{
		UIHelper.EnableGameObject(Label_FixedRewardType, rewardType == ERewardType.Fixed);
		UIHelper.EnableGameObject(Label_RandomRewardType, rewardType == ERewardType.RandomSelect);
		UIHelper.EnableGameObject(Label_RandomSelect, rewardType == ERewardType.RandomSelect);
		if (rewardType == ERewardType.RandomSelect)
		{
			UIHelper.SetLabelText(Label_RandomSelect, LocalizationManager.GetString(Inventory.PACKAGE_RANDOM_SELECT, SelectCount));
		}
		SetupRewardItemList(rewardElementList);
	}

	private void SetupRewardItemList(List<RewardElementData> rewardElementList)
	{
		ClearRewardItemList();
		SortRewardItemList(rewardElementList).ForEach(delegate(RewardElementData element)
		{
			CreateRewardItem(element);
		});
		UIHelper.RepositionGrid(RewardItemListGrid);
	}

	private List<RewardElementData> SortRewardItemList(List<RewardElementData> rewardElementList)
	{
		List<RewardElementData> list = new List<RewardElementData>();
		List<RewardElementData> list2 = new List<RewardElementData>();
		List<RewardElementData> list3 = new List<RewardElementData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardElementList); i++)
		{
			RewardElementData rewardElementData = rewardElementList[i];
			if (EnumTools.Parse(rewardElementData.ItemCode, CurrencyType.None) != CurrencyType.None)
			{
				list.Add(rewardElementData);
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode == null)
			{
				continue;
			}
			if (dataByCode.Type == ItemType.Equip)
			{
				if (GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(dataByCode.Index) != null)
				{
					list2.Add(rewardElementData);
				}
			}
			else
			{
				list3.Add(rewardElementData);
			}
		}
		list.Sort(delegate(RewardElementData l, RewardElementData r)
		{
			int num = (int)EnumTools.Parse(l.ItemCode, CurrencyType.None);
			int value = (int)EnumTools.Parse(r.ItemCode, CurrencyType.None);
			return num.CompareTo(value);
		});
		list2.Sort(delegate(RewardElementData l, RewardElementData r)
		{
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(l.ItemCode);
			ItemData<EquipItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(r.ItemCode);
			int partType = (int)data.SubData.PartType;
			int partType2 = (int)data2.SubData.PartType;
			int num = partType.CompareTo(partType2);
			return (num != 0) ? num : data.Index.CompareTo(data2.Index);
		});
		list3.Sort(delegate(RewardElementData l, RewardElementData r)
		{
			ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(l.ItemCode);
			ItemData dataByCode3 = GameRoot.Instance.ItemDataContainer.GetDataByCode(r.ItemCode);
			int type = (int)dataByCode2.Type;
			int type2 = (int)dataByCode3.Type;
			int num = type.CompareTo(type2);
			return (num != 0) ? num : dataByCode2.Index.CompareTo(dataByCode3.Index);
		});
		list.AddRange(list2);
		list.AddRange(list3);
		return list;
	}

	private void ClearRewardItemList()
	{
		FactoryBase.ReleaseAndClearList(_ListCurrencyIcon);
		FactoryBase.ReleaseAndClearList(_ListItems);
	}

	private void CreateRewardItem(RewardElementData rewardElement)
	{
		if (rewardElement == null)
		{
			return;
		}
		if (rewardElement.IsGold() || rewardElement.IsGem())
		{
			CurrencyType currencyType = EnumTools.Parse(rewardElement.ItemCode, CurrencyType.None);
			CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(RewardItemListGrid, CurrencyIconComponent.PrefabAuto);
			int itemMax = rewardElement.ItemMax;
			CurrencyIconComponent.Listener listener = null;
			int num = itemMax;
			currencyIconComponent.Open(currencyType, num, listener);
			_ListCurrencyIcon.Add(currencyIconComponent);
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElement.ItemCode);
		if (dataByCode != null)
		{
			ItemIconComponent itemIconComponent = RewardItemListGrid.AddGridItem(ItemIconComponent.PrefabAuto);
			if (dataByCode.Type == ItemType.Equip)
			{
				EquipItemInfo equipItemInfo = new EquipItemInfo();
				equipItemInfo.ItemIndex = dataByCode.Index;
				equipItemInfo.Star = rewardElement.ItemStarMax;
				itemIconComponent.Open(dataByCode, equipItemInfo, null);
			}
			else
			{
				itemIconComponent.Open(dataByCode);
				int itemMax2 = rewardElement.ItemMax;
				Color? color = null;
				int num2 = itemMax2;
				itemIconComponent.SetItemCount(num2, color);
			}
			_ListItems.Add(itemIconComponent);
		}
	}

	public int GetComponentHeight()
	{
		int num = (_ListCurrencyIcon.Count + _ListItems.Count - 1) / RewardItemListGrid.Grid.maxPerLine + 1;
		return (int)Mathf.Abs(RewardItemListGrid.transform.localPosition.y) + num * (int)RewardItemListGrid.Grid.cellHeight;
	}
}
