using NGame2.NAccount;
using NGame2.NUI.NComponent2.NCostume;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NCostume;

public class HairCostume : BaseCostume
{
	public static CostumeState GetCostumeState(HeroInfo heroInfo, HairCostumeComponent.OpenData openData, bool checkHeroOwn = true)
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
		if (openData.ComponentType != HairCostumeComponent.ComponentType.SelectThema)
		{
			return GetCostumeState(heroInfo, openData.HairCostumeData, checkHeroOwn);
		}
		if (heroInfo.HairCostumeIndex == 0)
		{
			return CostumeState.UnEquipSelect;
		}
		return CostumeState.Owned;
	}

	private static CostumeState GetCostumeState(HeroInfo heroInfo, HairCostumeData costumeData, bool checkHeroOwn)
	{
		return GetCostumeState(GameRoot.Instance.HairCostumeDataContainer.GetData(heroInfo.HairCostumeIndex), costumeData, checkHeroOwn);
	}

	private static CostumeState GetCostumeState(HairCostumeData equippedCostumeData, HairCostumeData costumeData, bool checkHeroOwn)
	{
		if (costumeData == null)
		{
			return CostumeState.None;
		}
		if (equippedCostumeData != null && equippedCostumeData.Index == costumeData.Index)
		{
			return CostumeState.Equipped;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(costumeData.HeroIndex);
		if (checkHeroOwn && cachedHeroInfo == null)
		{
			return CostumeState.NeedHero;
		}
		if (cachedHeroInfo == null)
		{
			return CostumeState.NeedAwaken;
		}
		if (costumeData.IsDefault)
		{
			if (cachedHeroInfo.Star < costumeData.Star)
			{
				return CostumeState.NeedAwaken;
			}
			return CostumeState.Owned;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(costumeData.NeedCostume))
		{
			if (BaseCostume.IsOwnedCostume(costumeData.NeedCostume))
			{
				return CostumeState.Owned;
			}
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeData.NeedCostume[0]);
			return HeroCostume.GetCostumeState(cachedHeroInfo, data);
		}
		if (costumeData.ReqBuyGem > 0 && GlobalAutoBehaviour<AccountManager>.instance.HairCostumeManager.Get(costumeData.Index) != null)
		{
			return CostumeState.Owned;
		}
		return CostumeState.Buyable;
	}

	public static int GetPriceGem(HairCostumeData costumeData, bool CheckDiscount = true)
	{
		if (costumeData == null)
		{
			return -1;
		}
		int num = costumeData.ReqBuyGem;
		if (CheckDiscount)
		{
			int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(costumeData);
			num = BaseCostume.CalculatePrice(num, priceDiscountPercentage);
		}
		return num;
	}

	public static int GetObtainMileage(HairCostumeData costumeData)
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
