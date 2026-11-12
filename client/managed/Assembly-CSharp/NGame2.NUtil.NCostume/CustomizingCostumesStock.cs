using System.Collections.Generic;
using NGame2.NUI.NHelper.NPayShop;
using NShared;

namespace NGame2.NUtil.NCostume;

public struct CustomizingCostumesStock
{
	public int HeroIndex;

	public int[] UnsetAccessoryCostumeIndices;

	public Dictionary<int, PositionInfo> AccessoryPositionPairs;

	public string AccessoryCostumePositionInfo;

	public int CostumeIndex;

	public int HairCostumeIndex;

	public int WeaponCostumeIndex;

	public byte HideUniqueWeapon;

	public int BuyReqGem;

	public int Mileage;

	public List<Tuple<int, string>> PurchaseProductList;

	public string PopupMessage;

	public string DebugLog;

	public string PacketLog;

	public List<IPayShopEventSearchInfo> PayShopEventSearchInfos;

	public List<CollectResult> CollectResultList;

	public void RefreshPrice()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(PayShopEventSearchInfos))
		{
			BuyReqGem = 0;
			Mileage = 0;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PayShopEventSearchInfos); i++)
			{
				int price = 0;
				int mileage = 0;
				CalChangeValues(PayShopEventSearchInfos[i], out price, out mileage);
				BuyReqGem += price;
				Mileage += mileage;
			}
			PayShopEventSearchInfos.Clear();
			PayShopEventSearchInfos = null;
		}
	}

	private void CalChangeValues(IPayShopEventSearchInfo info, out int price, out int mileage)
	{
		price = 0;
		mileage = 0;
		if (info is CreatureData)
		{
			CreatureData creatureData = info as CreatureData;
			price = PayShopHelper.GetPrice(creatureData);
			mileage = PayShopHelper.GetMilieage(creatureData);
		}
		else if (info is CostumeData)
		{
			CostumeData costumeData = info as CostumeData;
			price = HeroCostume.GetPriceGem(costumeData);
			mileage = HeroCostume.GetObtainMileage(costumeData);
		}
		else if (info is AccessoryCostumeData)
		{
			AccessoryCostumeData costumeData2 = info as AccessoryCostumeData;
			price = AccessoryCostume.GetPriceGem(costumeData2);
			mileage = AccessoryCostume.GetObtainMileage(costumeData2);
		}
		else if (info is HairCostumeData)
		{
			HairCostumeData costumeData3 = info as HairCostumeData;
			price = HairCostume.GetPriceGem(costumeData3);
			mileage = HairCostume.GetObtainMileage(costumeData3);
		}
		else if (info is WeaponCostumeData)
		{
			WeaponCostumeData costumeData4 = info as WeaponCostumeData;
			price = WeaponCostume.GetPriceGem(costumeData4);
			mileage = WeaponCostume.GetObtainMileage(costumeData4);
		}
	}
}
