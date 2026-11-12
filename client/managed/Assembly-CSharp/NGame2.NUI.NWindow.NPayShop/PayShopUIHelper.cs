using System;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

public static class PayShopUIHelper
{
	public static void SetProductTitle(NewPayShopProductInfo productInfo, UILabel titleLabel)
	{
		if (productInfo != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(productInfo.ProductName);
			UIHelper.SetLabelText(titleLabel, stringDirect);
		}
	}

	public static void SetProductIcon(NewPayShopProductInfo productInfo, UIBundleLoader uiBundleTexture)
	{
		if (productInfo != null)
		{
			UIHelper.SetBundleTexture(uiBundleTexture, productInfo.IconPath);
		}
	}

	public static void SetLocalizedLaw(NewPayShopProductInfo productInfo, UIWidget japanLawRoot, UIWidget normalLawRoot, UILabel labelWidthdraw, UIButton normalLawDetail)
	{
		if (productInfo == null)
		{
			UIHelper.EnableGameObject(japanLawRoot, enable: false);
			UIHelper.EnableGameObject(normalLawRoot, enable: false);
			UIHelper.EnableGameObject(labelWidthdraw, enable: false);
			UIHelper.EnableGameObject(normalLawDetail, enable: false);
			return;
		}
		string countryServerName = GlobalAutoBehaviour<AccountManager>.instance.CountryServerName;
		bool flag = countryServerName.Equals("Korea");
		bool flag2 = countryServerName.Equals("Taiwan");
		if (flag || flag2)
		{
			UIHelper.EnableGameObject(japanLawRoot, enable: false);
			UIHelper.EnableGameObject(normalLawRoot, enable: true);
			if (productInfo != null)
			{
				string stringDirect = LocalizationManager.GetStringDirect(productInfo.WithdrawTextKey);
				UIHelper.SetLabelText(labelWidthdraw, stringDirect);
			}
			UIHelper.EnableGameObject(normalLawDetail, flag);
		}
		else
		{
			CountryServerData countryServerData = LobbyManagement.GetCountryServerData();
			bool enable = countryServerData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(countryServerData.Law);
			UIHelper.EnableGameObject(japanLawRoot, enable);
			UIHelper.EnableGameObject(normalLawRoot, enable: false);
		}
	}

	public static void SetPriceComponent(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo, SkuInfo skuInfo, PayShopPriceComponent priceComponent)
	{
		if (!(priceComponent == null))
		{
			priceComponent.Open(productInfo, purchaseInfo, skuInfo);
		}
	}

	public static void SetProbabilityButton(NewPayShopProductInfo productInfo, UIButton probabilityButton)
	{
		if (productInfo != null)
		{
			UIHelper.EnableGameObject(probabilityButton, productInfo.ShowProbability);
		}
	}

	public static void SetLawRoot()
	{
	}

	public static int GetCustomComparePayShop(Transform lhs, Transform rhs)
	{
		PayShopProductSimpleComponent payShopProductSimpleComponent = SingletonAuto<LinkedTransform<PayShopProductSimpleComponent>>.instance.Get(lhs);
		PayShopProductSimpleComponent payShopProductSimpleComponent2 = SingletonAuto<LinkedTransform<PayShopProductSimpleComponent>>.instance.Get(rhs);
		int result = 0;
		if (payShopProductSimpleComponent != null && payShopProductSimpleComponent2 != null)
		{
			result = ((payShopProductSimpleComponent.ObtainType > payShopProductSimpleComponent2.ObtainType) ? 1 : ((payShopProductSimpleComponent.ObtainType >= payShopProductSimpleComponent2.ObtainType) ? payShopProductSimpleComponent.SortIndex.CompareTo(payShopProductSimpleComponent2.SortIndex) : (-1)));
		}
		else if (payShopProductSimpleComponent == null && payShopProductSimpleComponent2 != null)
		{
			result = ((payShopProductSimpleComponent2.ObtainType < ObtainItemType.Bonus) ? 1 : (-1));
		}
		else if (payShopProductSimpleComponent != null && payShopProductSimpleComponent2 == null)
		{
			result = ((payShopProductSimpleComponent.ObtainType >= ObtainItemType.Bonus) ? 1 : (-1));
		}
		return result;
	}

	public static bool SetPurchasePeriod(NewPayShopProductInfo productInfo, UILabel label)
	{
		if (productInfo.IsPurchaseMarketing)
		{
			PurchaseMarketingInfo purchaseMarketingInfo = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.GetPurchaseMarketingInfo(productInfo.Index);
			if (purchaseMarketingInfo == null)
			{
				UIHelper.EnableGameObject(label, enable: false);
				return false;
			}
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseMarketingInfo.EndTime, out var o_dateTime))
			{
				DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
				long num = (long)(o_dateTime - uTCNow).TotalSeconds;
				long val = num;
				int num2 = 0;
				num = System.Math.Max(val, num2);
				UIHelper.EnableGameObject(label, enable: true);
				UIHelper.SetLabelText(label, UIHelper.GetRemainTimeLabelText(num));
				return true;
			}
			UIHelper.EnableGameObject(label, enable: false);
			return false;
		}
		return WriteProductPeriod(productInfo.BeginTime, productInfo.EndTime, label);
	}

	public static bool WriteProductPeriod(string beginTime, string endTime, UILabel label)
	{
		bool flag = !String_Extension_Method.IsNullOrEmpty(beginTime) && !String_Extension_Method.IsNullOrEmpty(endTime);
		UIHelper.EnableGameObject(label, flag);
		if (flag)
		{
			DateTime clientTime = UIHelper.GetClientTime(beginTime);
			DateTime clientTime2 = UIHelper.GetClientTime(endTime);
			string text = LocalizationManager.GetString(NShared.NLocalization.PayShop.PRODUCT_SELL_PERIOD_DATE, clientTime.Month, clientTime.Day);
			string text2 = LocalizationManager.GetString(NShared.NLocalization.PayShop.PRODUCT_SELL_PERIOD_DATE, clientTime2.Month, clientTime2.Day);
			string text3 = LocalizationManager.GetString(NShared.NLocalization.PayShop.PRODUCT_SELL_PERIOD, text, text2);
			UIHelper.SetLabelText(label, text3);
		}
		return flag;
	}

	public static void SetPurchseableCount(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo, UILabel label)
	{
		int num = productInfo.PurchasableCount;
		if (purchaseInfo != null)
		{
			num = productInfo.PurchasableCount - purchaseInfo.PurchasedCount;
		}
		int purchasableCount = productInfo.PurchasableCount;
		if (purchasableCount <= 0)
		{
			if (productInfo.ProductType == ProductType.DailyGem || productInfo.ProductType == ProductType.DailyItem)
			{
				SetRemainDailyCount(productInfo, purchaseInfo, label);
			}
			else
			{
				UIHelper.EnableGameObject(label, enable: false);
			}
			return;
		}
		if (productInfo.FirstPurchase == 1)
		{
			UIHelper.EnableGameObject(label, enable: false);
			return;
		}
		string text = "";
		if (num == 0)
		{
			if (CanResetPurchase(productInfo))
			{
				text = GetNextBuyableRemainTime(purchaseInfo);
			}
		}
		else
		{
			text = LocalizationManager.GetString(NShared.NLocalization.PayShop.PACKAGE_BUY_COUNT, num, purchasableCount);
		}
		bool flag = !String_Extension_Method.IsNullOrEmpty(text);
		UIHelper.EnableGameObject(label, flag);
		if (flag)
		{
			UIHelper.SetLabelText(label, text);
		}
	}

	private static string GetNextBuyableRemainTime(PlayerProductPurchaseInfo purchaseInfo)
	{
		if (purchaseInfo == null)
		{
			return "";
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseInfo.PurchaseCountResetTime, out var o_dateTime))
		{
			return "";
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (o_dateTime < currentServerUTCTime)
		{
			return "";
		}
		return GetRemainTimeUntilPurchasable(o_dateTime - currentServerUTCTime);
	}

	private static bool CanResetPurchase(NewPayShopProductInfo productInfo)
	{
		bool result = false;
		if (productInfo.ResetType == ResetType.Daily)
		{
			result = productInfo.DailyReset;
		}
		else if (productInfo.ResetType == ResetType.Weekly)
		{
			result = !__Extension_Method_ICollection__.IsNullOrEmpty(productInfo.WeeklyReset);
		}
		else if (productInfo.ResetType == ResetType.Monthly)
		{
			result = !__Extension_Method_ICollection__.IsNullOrEmpty(productInfo.MonthlyResetDate);
		}
		return result;
	}

	public static string GetRemainTimeUntilPurchasable(TimeSpan ts)
	{
		int num = Mathf.Max((int)ts.TotalDays, 0);
		if (num > 0)
		{
			return LocalizationManager.GetString(NShared.NLocalization.PayShop.RESET_COOLTIME_DAY, num);
		}
		if ((int)ts.TotalHours > 0 && (int)ts.TotalHours < 24)
		{
			return LocalizationManager.GetString(NShared.NLocalization.PayShop.RESET_COOLTIME_HOUR, ts.Hours);
		}
		if ((int)ts.TotalMinutes > 0 && (int)ts.TotalMinutes < 60)
		{
			return LocalizationManager.GetString(NShared.NLocalization.PayShop.RESET_COOLTIME_MINUTE, ts.Minutes);
		}
		if ((int)ts.TotalSeconds > 0 && (int)ts.TotalSeconds < 60)
		{
			return LocalizationManager.GetString(NShared.NLocalization.PayShop.RESET_COOLTIME_MINUTE, 1);
		}
		return string.Empty;
	}

	public static void SetRemainDailyCount(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo, UILabel label)
	{
		TimeSpan remainTimeSpan = NGame2.NAccount.UserPlayerProductPurchaseManager.GetRemainTimeSpan(purchaseInfo);
		if (!(remainTimeSpan.TotalDays <= (double)productInfo.BuyableDay))
		{
			string remainTimeUntilPurchasable = GetRemainTimeUntilPurchasable(remainTimeSpan);
			UIHelper.EnableGameObject(label, enable: true);
			UIHelper.SetLabelText(label, remainTimeUntilPurchasable);
			return;
		}
		if (productInfo.BuyGroup > 0)
		{
			NewPayShopProductInfo[] products = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetProducts(productInfo.BuyGroup);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(products))
			{
				NewPayShopProductInfo[] array = products;
				foreach (NewPayShopProductInfo newPayShopProductInfo in array)
				{
					if (newPayShopProductInfo == productInfo)
					{
						continue;
					}
					PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(newPayShopProductInfo.Index);
					if (playerProductPurchaseInfo != null)
					{
						remainTimeSpan = NGame2.NAccount.UserPlayerProductPurchaseManager.GetRemainTimeSpan(playerProductPurchaseInfo);
						if (!(remainTimeSpan.TotalDays <= (double)productInfo.BuyableDay))
						{
							string remainTimeUntilPurchasable2 = GetRemainTimeUntilPurchasable(remainTimeSpan);
							UIHelper.EnableGameObject(label, enable: true);
							UIHelper.SetLabelText(label, remainTimeUntilPurchasable2);
							return;
						}
					}
				}
			}
		}
		UIHelper.EnableGameObject(label, enable: false);
	}

	public static string GetResetDesc(NewPayShopProductInfo productInfo)
	{
		string result = "";
		if (productInfo.ResetType == ResetType.Daily)
		{
			result = LocalizationManager.GetString(NShared.NLocalization.PayShop.DAILY_RESET_PURCHASE_COUNT);
		}
		else if (productInfo.ResetType == ResetType.Weekly)
		{
			string text = string.Empty;
			Array.Sort(productInfo.WeeklyReset);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(productInfo.WeeklyReset); i++)
			{
				Enum dayOfWeekKey = NGame2.NUI.Common.GetDayOfWeekKey((DayOfWeek)productInfo.WeeklyReset[i]);
				text += LocalizationManager.GetString(dayOfWeekKey);
				text += ",";
			}
			text = text.TrimEnd(',');
			result = LocalizationManager.GetString(NShared.NLocalization.PayShop.WEEKLY_RESET_PURCHASE_COUNT, text);
		}
		else if (productInfo.ResetType == ResetType.Monthly)
		{
			string text2 = string.Empty;
			Array.Sort(productInfo.MonthlyResetDate);
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(productInfo.MonthlyResetDate); j++)
			{
				text2 += NGame2.NUI.Common.GetOrdinalizeDay(productInfo.MonthlyResetDate[j]);
				text2 += ",";
			}
			text2 = text2.TrimEnd(',');
			result = LocalizationManager.GetString(NShared.NLocalization.PayShop.RESET_DAYOFMONTH_DESC, text2);
		}
		return result;
	}
}
