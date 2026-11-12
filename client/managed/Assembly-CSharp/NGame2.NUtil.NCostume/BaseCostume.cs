using NGame2.NAccount;
using NGame2.NPayShopItemEvent;
using NGame2.NUtil.NPayShop;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NCostume;

public class BaseCostume
{
	public static bool IsOwnedCostume(int[] needCostumes)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(needCostumes); i++)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(needCostumes[i]) != null)
			{
				return true;
			}
		}
		return false;
	}

	protected static int CalculatePrice(int priceGem, int discountPercentage)
	{
		return Calculator.GetPrice(priceGem, discountPercentage);
	}

	protected static int GetMileage(int mileage, int discountPercentage)
	{
		return Calculator.GetMileage(mileage, discountPercentage);
	}

	public static int GetPriceDiscountPercentage(IPayShopEventSearchInfo data)
	{
		return GlobalAutoBehaviour<PayShopItemEventManager>.instance.GetDiscountPercentage(data);
	}
}
