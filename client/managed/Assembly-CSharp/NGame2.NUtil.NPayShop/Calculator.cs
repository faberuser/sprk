using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NPayShop;

public class Calculator
{
	public static long GetStepUpMileage(NewEquipGachaData equipGachaData, long mileage)
	{
		int stepUpGachaDiscount = 0;
		if (Checker.IsDiscountStepupGacha(equipGachaData, out stepUpGachaDiscount))
		{
			int mileage2 = GetMileage(mileage, stepUpGachaDiscount);
			return mileage2;
		}
		return mileage;
	}

	public static int GetMileageApplyValidMileage(NewEquipGachaData equipGachaData)
	{
		EquipGachaInfo equipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Get(equipGachaData.Index);
		if (Checker.IsDiscount(equipGachaData, equipGachaInfo))
		{
			int mileage = equipGachaData.Mileage;
			int discount = equipGachaInfo.Discount;
			int num = mileage;
			return GetMileage(num, discount);
		}
		return equipGachaData.Mileage;
	}

	public static int GetStepUpDiscount(NewEquipGachaData equipGachaData, int price)
	{
		int stepUpGachaDiscount = 0;
		if (Checker.IsDiscountStepupGacha(equipGachaData, out stepUpGachaDiscount))
		{
			return GetPrice(price, stepUpGachaDiscount);
		}
		return price;
	}

	public static int GetGemApplyValidDiscounts(NewEquipGachaData equipGachaData)
	{
		EquipGachaInfo equipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Get(equipGachaData.Index);
		if (Checker.IsDiscount(equipGachaData, equipGachaInfo))
		{
			return GetPrice(equipGachaData.OpenGem, equipGachaInfo.Discount);
		}
		return equipGachaData.OpenGem;
	}

	public static int GetPrice(int priceGem, int discountPercentage)
	{
		int num = (int)((double)priceGem * (0.01 * (double)discountPercentage));
		priceGem -= num;
		return priceGem;
	}

	public static int GetMileage(long mileage, int discountPercentage)
	{
		double num = (double)mileage * (0.01 * (double)discountPercentage);
		return (int)((double)mileage - num);
	}
}
