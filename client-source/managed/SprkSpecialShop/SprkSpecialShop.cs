using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkSpecialShop
{
	private static int pendingIndex;

	private static int pendingQuantity = 1;

	private static float nextResetCheck;

	public static int Rank(bool free, int purchased, int limit)
	{
		if (free)
		{
			if (limit <= 0 || purchased < limit)
			{
				return 0;
			}
			return 2;
		}
		return 1;
	}

	public static int Maximum(long balance, long price, int purchased, int limit, int grantLimit)
	{
		long val = 999L;
		if (price > 0)
		{
			val = Math.Min(val, balance / price);
		}
		if (limit > 0)
		{
			val = Math.Min(val, (long)limit - (long)purchased);
		}
		val = Math.Min(val, grantLimit);
		return (int)Math.Max(0L, val);
	}

	public static long Balance(UserManager user, CurrencyType currency)
	{
		return currency switch
		{
			CurrencyType.Gold => user.Gold, 
			CurrencyType.Gem => user.TotalGem, 
			CurrencyType.Mileage => user.Mileage, 
			CurrencyType.LuaPoint => user.LuaPoint, 
			_ => user.GetUserStat(currency.ToString()), 
		};
	}

	public static bool CanSelectQuantity(NewPayShopProductInfo info)
	{
		if (info != null && (info.ProductType == ProductType.Item || info.ProductType == ProductType.Gem || info.ProductType == ProductType.Gold) && string.IsNullOrEmpty(info.InAppPurchaseId) && info.DurationDay == 0)
		{
			if (info.StepProductGroup != null)
			{
				return info.StepProductGroup.Length == 0;
			}
			return true;
		}
		return false;
	}

	public static int Purchased(NewPayShopProductInfo info)
	{
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(info.Index);
		if (playerProductPurchaseInfo == null)
		{
			return 0;
		}
		if (info.DailyReset && playerProductPurchaseInfo.PurchaseCountResetTime.TryParseNetworkTime2(out var o_dateTime) && o_dateTime <= Common.GetUTCNow())
		{
			return 0;
		}
		return playerProductPurchaseInfo.PurchasedCount;
	}

	public static void Sort(List<NewPayShopProductInfo> products)
	{
		NewPayShopProductInfo[] collection = products.OrderBy((NewPayShopProductInfo p) => Rank(p.Price == 0L && string.IsNullOrEmpty(p.InAppPurchaseId), Purchased(p), p.PurchasableCount)).ToArray();
		products.Clear();
		products.AddRange(collection);
	}

	private static void Rebuild(PayShop shop)
	{
		FieldInfo field = typeof(PayShop).GetField("_selectedPayShopList", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo field2 = typeof(PayShop).GetField("_selectedTag", BindingFlags.Instance | BindingFlags.NonPublic);
		PayShopBaseList payShopBaseList = field.GetValue(shop) as PayShopBaseList;
		if (!(payShopBaseList == null))
		{
			int num = (int)field2.GetValue(shop);
			if (num > 0)
			{
				payShopBaseList.CreateList(num);
			}
			else
			{
				payShopBaseList.ReCreateList();
			}
		}
	}

	public static void AfterPurchase(PayShop shop, NewPayShopProductInfo info)
	{
		if (info != null && info.Price == 0L)
		{
			Rebuild(shop);
		}
	}

	public static void Tick(PayShop shop)
	{
		if (Time.realtimeSinceStartup < nextResetCheck)
		{
			return;
		}
		nextResetCheck = Time.realtimeSinceStartup + 1f;
		bool flag = false;
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		NewPayShopProductInfo[] values = instance.NewPayShopProductManager.GetValues();
		foreach (NewPayShopProductInfo newPayShopProductInfo in values)
		{
			if (newPayShopProductInfo.Price == 0L && newPayShopProductInfo.DailyReset)
			{
				PlayerProductPurchaseInfo playerProductPurchaseInfo = instance.PlayerProductPurchaseManager.Get(newPayShopProductInfo.Index);
				if (playerProductPurchaseInfo != null && playerProductPurchaseInfo.PurchasedCount > 0 && Purchased(newPayShopProductInfo) == 0)
				{
					playerProductPurchaseInfo.PurchasedCount = 0;
					flag = true;
				}
			}
		}
		if (flag)
		{
			Rebuild(shop);
		}
	}

	public static void Init(PayShopProductInfoPopup popup, NewPayShopProductInfo info)
	{
		pendingIndex = 0;
		pendingQuantity = 1;
		SprkShopQuantity sprkShopQuantity = popup.GetComponent<SprkShopQuantity>();
		if (sprkShopQuantity == null)
		{
			sprkShopQuantity = popup.gameObject.AddComponent<SprkShopQuantity>();
		}
		sprkShopQuantity.Open(popup, info);
	}

	public static bool BeforeBuy(PayShopBaseInfoPopup popup)
	{
		SprkShopQuantity component = popup.GetComponent<SprkShopQuantity>();
		pendingIndex = 0;
		pendingQuantity = 1;
		if (component == null || !component.Enabled)
		{
			return true;
		}
		component.Refresh();
		if (component.MaxCount < 1)
		{
			return false;
		}
		pendingIndex = component.Info.Index;
		pendingQuantity = component.Count;
		return true;
	}

	public static int TakeQuantity(long index)
	{
		int result = ((index != pendingIndex) ? 1 : pendingQuantity);
		pendingIndex = 0;
		pendingQuantity = 1;
		return result;
	}

	public static Dictionary<string, object> Encode(Dictionary<string, object> data, int quantity)
	{
		data["Quantity"] = Math.Max(1, quantity).ToString(CultureInfo.InvariantCulture);
		return data;
	}
}
