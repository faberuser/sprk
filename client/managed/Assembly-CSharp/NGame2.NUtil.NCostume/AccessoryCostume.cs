using NGame2.NAccount;
using NGame2.NUI.NComponent2.NCostume;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NCostume;

public class AccessoryCostume : BaseCostume
{
	public static CostumeState GetCostumeState(HeroInfo heroInfo, AccessoryCostumeComponent.OpenData openData, bool checkHeroOwn = true)
	{
		if (!openData.IsValid())
		{
			return CostumeState.None;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroInfo.HeroIndex);
		if (checkHeroOwn && cachedHeroInfo == null)
		{
			return CostumeState.NeedHero;
		}
		if (openData.ComponentType != AccessoryCostumeComponent.ComponentType.SelectUnset)
		{
			return GetCostumeState(heroInfo, openData.AccessoryCostumeData, checkHeroOwn);
		}
		if (heroInfo.GetAccessoryIndex(openData.AccessoryPartType) == 0)
		{
			return CostumeState.UnEquipSelect;
		}
		return CostumeState.Owned;
	}

	private static CostumeState GetCostumeState(HeroInfo heroInfo, AccessoryCostumeData accessoryCostumeData, bool checkHeroOwn)
	{
		return SprkAccessories.State(heroInfo, accessoryCostumeData, checkHeroOwn);
	}

	public static int GetPriceGem(AccessoryCostumeData costumeData, bool checkDiscount = true)
	{
		if (costumeData == null)
		{
			return -1;
		}
		int num = costumeData.ReqBuyGem;
		if (checkDiscount)
		{
			int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(costumeData);
			num = BaseCostume.CalculatePrice(num, priceDiscountPercentage);
		}
		return num;
	}

	public static int GetObtainMileage(AccessoryCostumeData costumeData)
	{
		if (costumeData == null)
		{
			return -1;
		}
		int mileage = costumeData.Mileage;
		int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(costumeData);
		return BaseCostume.GetMileage(mileage, priceDiscountPercentage);
	}
}
