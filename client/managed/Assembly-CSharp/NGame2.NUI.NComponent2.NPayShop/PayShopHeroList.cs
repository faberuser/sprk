using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NPayShopItemEvent;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopHeroList : PayShopBaseTabList<CreatureData, PayShopHeroComponent, PayShopHeroClassTabComponent, CreatureTagType>, IPayShopProductList
{
	public UIWidget NoHeroRoot;

	public override PayShopIndexType PayShopIndexType => PayShopIndexType.Hero;

	public override void CreateList()
	{
		CreateList(CreatureTagType.None);
	}

	public override List<CreatureData> GetDataList(CreatureTagType classTag = CreatureTagType.None)
	{
		return GetDatas(classTag);
	}

	public static List<CreatureData> GetDatas(CreatureTagType classTag = CreatureTagType.None)
	{
		List<CreatureData> creatureDataList = ((classTag != CreatureTagType.None) ? GameRoot.Instance.CreatureDataContainer.GetListByTagType(classTag) : GameRoot.Instance.CreatureDataContainer.GetOpenedHeroList());
		creatureDataList = FilteringData(creatureDataList);
		creatureDataList.Sort(SortData);
		return creatureDataList;
	}

	protected override PrefabAuto<PayShopHeroComponent> GetComponentPrefabAuto()
	{
		return PayShopHeroComponent.PrefabAuto;
	}

	protected override PrefabAuto<PayShopHeroClassTabComponent> GetTabPrefabAuto()
	{
		return PayShopHeroClassTabComponent.PrefabAuto;
	}

	protected override List<CreatureData> Filtering(List<CreatureData> creatureDataList)
	{
		return FilteringData(creatureDataList);
	}

	protected static List<CreatureData> FilteringData(List<CreatureData> creatureDataList)
	{
		List<CreatureData> list = new List<CreatureData>();
		foreach (CreatureData creatureData in creatureDataList)
		{
			if (creatureData.OpenType == CreatureOpenType.Opened && (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.HeroManager == null || GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(creatureData.Index) == null))
			{
				list.Add(creatureData);
			}
		}
		return list;
	}

	protected override int Sort(CreatureData a, CreatureData b)
	{
		return SortData(a, b);
	}

	protected static int SortData(CreatureData a, CreatureData b)
	{
		int num = 0;
		int openStatus = (int)a.OpenStatus;
		int openStatus2 = (int)b.OpenStatus;
		num = -openStatus.CompareTo(openStatus2);
		if (num != 0)
		{
			return num;
		}
		num = -a.OpenDate.CompareTo(b.OpenDate);
		if (num != 0)
		{
			return num;
		}
		return -a.Index.CompareTo(b.Index);
	}

	protected override CreatureData GetData(int index)
	{
		return GameRoot.Instance.CreatureDataContainer.GetData(index);
	}

	protected override void OnEndCreateList(bool isEmptyList)
	{
		UIHelper.EnableGameObject(NoHeroRoot, isEmptyList);
	}

	public static PayShopFlagType CheckCategoryFlag()
	{
		PayShopCategoryComponent.NewFlagCheckClass newFlagCheckClass = PayShopCategoryComponent.LoadNewFlagCheckClass(NewPayShopCategoryType.Hero.ToString());
		PayShopFlagType payShopFlagType = PayShopFlagType.None;
		foreach (CreatureData item in FilteringData(GameRoot.Instance.CreatureDataContainer.GetOpenedHeroList()))
		{
			if (item.OpenStatus == CreatureOpenStatusType.New)
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
			int num = 0;
			if (GlobalAutoBehaviour<PayShopItemEventManager>.isValidInstance)
			{
				num = GlobalAutoBehaviour<PayShopItemEventManager>.instance.GetDiscountPercentage(item);
			}
			if (num > 0)
			{
				payShopFlagType |= PayShopFlagType.Sale;
			}
		}
		PayShopCategoryComponent.SaveNewFlagCheckClass(NewPayShopCategoryType.Hero.ToString(), newFlagCheckClass);
		return payShopFlagType;
	}
}
