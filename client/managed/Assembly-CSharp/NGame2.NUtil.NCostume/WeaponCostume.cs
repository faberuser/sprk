using NGame2.NAccount;
using NGame2.NUI.NComponent2.NCostume;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NCostume;

public class WeaponCostume : BaseCostume
{
	public static CostumeState GetCostumeState(HeroInfo heroInfo, WeaponCostumeComponent.OpenData openData, bool checkHeroOwn = true)
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
		if (openData.ComponentType == WeaponCostumeComponent.ComponentType.SelectUnique)
		{
			if (heroInfo.WeaponCostumeIndex == 0 && heroInfo.HideUniqueWeapon == 0)
			{
				return CostumeState.UnEquipSelect;
			}
			return CostumeState.Owned;
		}
		if (openData.ComponentType != WeaponCostumeComponent.ComponentType.SelectThema)
		{
			return GetCostumeState(heroInfo, openData.WeaponCostumeData, checkHeroOwn);
		}
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(heroInfo.CostumeIndex);
		if (data != null && data.CostumeType == EHeroCostumeType.Legend)
		{
			return CostumeState.UnEquipSelect;
		}
		if (heroInfo.WeaponCostumeIndex == 0 && heroInfo.HideUniqueWeapon == 1)
		{
			return CostumeState.Equipped;
		}
		return CostumeState.Owned;
	}

	public static CostumeState GetCostumeState(HeroInfo heroInfo, WeaponCostumeData costumeData, bool checkHeroOwn = true)
	{
		return GetCostumeState((heroInfo == null) ? null : GameRoot.Instance.WeaponCostumeDataContainer.GetData(heroInfo.WeaponCostumeIndex), costumeData, checkHeroOwn);
	}

	private static CostumeState GetCostumeState(WeaponCostumeData equippedCostumeData, WeaponCostumeData costumeData, bool checkHeroOwn = true)
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
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(costumeData.NeedCostume) && __Extension_Method_ICollection__.COUNT(costumeData.NeedCostume) > 0)
		{
			if (BaseCostume.IsOwnedCostume(costumeData.NeedCostume))
			{
				return CostumeState.Owned;
			}
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeData.NeedCostume[0]);
			return HeroCostume.GetCostumeState(cachedHeroInfo, data);
		}
		if (costumeData.ReqBuyGem > 0 && GlobalAutoBehaviour<AccountManager>.instance.WeaponCostumeManager.Get(costumeData.Index) != null)
		{
			return CostumeState.Owned;
		}
		return CostumeState.Buyable;
	}

	public static int GetPriceGem(WeaponCostumeData costumeData, bool checkDiscount = true)
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

	public static int GetObtainMileage(WeaponCostumeData costumeData)
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
