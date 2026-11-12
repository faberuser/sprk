using System.Collections.Generic;
using NShared;
using NVespa;

namespace NGame2.NHelper;

public class PackageItemHelper
{
	public static void GetRewardElementList(ItemData itemData, List<RewardElementData> ListElement_Fixed, List<RewardElementData> ListElement_Added)
	{
		new List<RewardElementData>();
		GameRoot.Instance.ItemDataContainer.GetData<PackageItemData>(itemData.Code);
		int rewardDataIndex = GetRewardDataIndex(itemData);
		if (rewardDataIndex == 0)
		{
			return;
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardDataIndex);
		if (data == null)
		{
			return;
		}
		int GroupCount = 0;
		data.GetRewardItemListWithGroupCount(out GroupCount);
		List<RewardElementData> ItemList = data.GetRewardOnlyItemList();
		RemoveAndSortRewardElementDataList(ref ItemList);
		foreach (RewardElementData item in ItemList)
		{
			if (item.ItemRate == 1000)
			{
				ListElement_Fixed?.Add(item);
			}
			else
			{
				ListElement_Added?.Add(item);
			}
		}
	}

	public static int GetRewardDataIndex(ItemData itemData)
	{
		if (itemData == null)
		{
			return 0;
		}
		int num = 0;
		string code = itemData.Code;
		if (itemData.Type == ItemType.CinemaPackage)
		{
			return GameRoot.Instance.PackageItemDataContainer.GetData(code)?.RewardIndex1 ?? 0;
		}
		return GameRoot.Instance.ItemDataContainer.GetData<PackageItemData>(code)?.SubData.RewardIndex1 ?? 0;
	}

	public static void RemoveAndSortRewardElementDataList(ref List<RewardElementData> ItemList)
	{
		ItemList.RemoveAll((RewardElementData x) => (EnumTools.Parse(x.ItemCode, CurrencyType.None) == CurrencyType.None && GameRoot.Instance.ItemDataContainer.GetDataByCode(x.ItemCode) == null) ? true : false);
		ItemList.Sort(delegate(RewardElementData x, RewardElementData y)
		{
			if (x.ItemRate == 1000 != (y.ItemRate == 1000))
			{
				if (x.ItemRate == 1000)
				{
					return -1;
				}
				return 1;
			}
			CurrencyType currencyType = EnumTools.Parse(x.ItemCode, CurrencyType.None);
			CurrencyType currencyType2 = EnumTools.Parse(y.ItemCode, CurrencyType.None);
			if (currencyType == CurrencyType.None != (currencyType2 == CurrencyType.None))
			{
				if (currencyType != CurrencyType.None)
				{
					return -1;
				}
				return 1;
			}
			if (currencyType == CurrencyType.Gem != (currencyType2 == CurrencyType.Gem))
			{
				if (currencyType == CurrencyType.Gem)
				{
					return -1;
				}
				return 1;
			}
			if (currencyType != CurrencyType.None)
			{
				return currencyType.CompareTo(currencyType2);
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(x.ItemCode);
			ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(y.ItemCode);
			ItemType type = dataByCode.Type;
			ItemType type2 = dataByCode2.Type;
			if (type != type2)
			{
				return type2.CompareTo(type);
			}
			if (dataByCode.Index != dataByCode2.Index)
			{
				return dataByCode.Index.CompareTo(dataByCode2.Index);
			}
			if (x.ItemMax != y.ItemMax)
			{
				return x.ItemMax.CompareTo(y.ItemMax);
			}
			return (x.ItemStar != y.ItemStar) ? x.ItemStar.CompareTo(y.ItemStar) : dataByCode.Grade.CompareTo(dataByCode2.Grade);
		});
	}

	public static bool IsPackageItem(ItemData itemData)
	{
		if (itemData == null)
		{
			return false;
		}
		if (itemData.Type != ItemType.Package)
		{
			return itemData.Type == ItemType.CinemaPackage;
		}
		return true;
	}
}
