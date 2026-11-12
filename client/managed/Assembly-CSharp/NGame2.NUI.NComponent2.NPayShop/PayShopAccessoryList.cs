using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopAccessoryList : PayShopBaseTabList<AccessoryCostumeData, PayShopAccessoryComponent, PayShopAccessoryPartTabComponent, string>, IPayShopProductList
{
	public override PayShopIndexType PayShopIndexType => PayShopIndexType.Accessory;

	public override void CreateList()
	{
		CreateList("New");
	}

	public override List<AccessoryCostumeData> GetDataList(string category)
	{
		IList<AccessoryCostumeData> dataByCategory = GameRoot.Instance.AccessoryCostumeDataContainer.GetDataByCategory(category);
		List<AccessoryCostumeData> list = new List<AccessoryCostumeData>();
		list.AddRange(dataByCategory);
		return list;
	}

	protected override List<AccessoryCostumeData> Filtering(List<AccessoryCostumeData> datas)
	{
		return FilteringData(datas);
	}

	protected static List<AccessoryCostumeData> FilteringData(List<AccessoryCostumeData> datas)
	{
		List<AccessoryCostumeData> list = new List<AccessoryCostumeData>();
		if (datas != null)
		{
			int i = default(int);
			for (; i < datas.Count; i++)
			{
				AccessoryCostumeData accessoryCostumeData = datas[i];
				if (accessoryCostumeData == null || !accessoryCostumeData.IsBuy)
				{
					continue;
				}
				int num = 0;
				while (true)
				{
					if (num < list.Count)
					{
						if (list[num].Index == accessoryCostumeData.Index)
						{
							break;
						}
						num++;
						continue;
					}
					list.Add(accessoryCostumeData);
					break;
				}
			}
		}
		return list;
	}

	protected override PrefabAuto<PayShopAccessoryComponent> GetComponentPrefabAuto()
	{
		return PayShopAccessoryComponent.PrefabAuto;
	}

	protected override PrefabAuto<PayShopAccessoryPartTabComponent> GetTabPrefabAuto()
	{
		return PayShopAccessoryPartTabComponent.PrefabAuto;
	}

	protected override int Sort(AccessoryCostumeData a, AccessoryCostumeData b)
	{
		int num = 0;
		num = -a.New.CompareTo(b.New);
		if (num != 0)
		{
			return num;
		}
		return -a.Index.CompareTo(b.Index);
	}

	protected override AccessoryCostumeData GetData(int index)
	{
		return GameRoot.Instance.AccessoryCostumeDataContainer.GetData(index);
	}

	public static PayShopFlagType CheckCategoryFlag()
	{
		PayShopCategoryComponent.NewFlagCheckClass newFlagCheckClass = PayShopCategoryComponent.LoadNewFlagCheckClass(NewPayShopCategoryType.Accessory.ToString());
		PayShopFlagType payShopFlagType = PayShopFlagType.None;
		IList<AccessoryCostumeData> allData = GameRoot.Instance.AccessoryCostumeDataContainer.GetAllData();
		List<AccessoryCostumeData> list = new List<AccessoryCostumeData>();
		list.AddRange(allData);
		foreach (AccessoryCostumeData item in FilteringData(list))
		{
			if (item.New == 1)
			{
				if (newFlagCheckClass.NewFlagConfirmValue == 0)
				{
					DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
					newFlagCheckClass.InitNewFlagConfirmValue(currentServerLocalTime);
					continue;
				}
				if (newFlagCheckClass.IsNew(item.OpenDate))
				{
					payShopFlagType |= PayShopFlagType.New;
					newFlagCheckClass.NewFlagValue = item.OpenDate;
				}
			}
			if (BaseCostume.GetPriceDiscountPercentage(item) > 0)
			{
				payShopFlagType |= PayShopFlagType.Sale;
			}
		}
		PayShopCategoryComponent.SaveNewFlagCheckClass(NewPayShopCategoryType.Accessory.ToString(), newFlagCheckClass);
		return payShopFlagType;
	}
}
