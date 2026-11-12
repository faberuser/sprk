using System;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserPlayerProductPurchaseManager : NShared.UserPlayerProductPurchaseManager
{
	private readonly UserObserverManager<PlayerProductPurchaseInfo> _observerManager = new UserObserverManager<PlayerProductPurchaseInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(PlayerProductPurchaseInfo info)
	{
		if (info != null)
		{
			Add(info);
		}
	}

	public void Apply(PlayerProductPurchaseInfo[] infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
			{
				Add(infos[i]);
			}
		}
	}

	protected override void OnAddData(int index, PlayerProductPurchaseInfo value)
	{
		base.OnAddData(index, value);
		_observerManager.AnnounceToObservers(value);
	}

	public static double GetRemainDays(PlayerProductPurchaseInfo info)
	{
		if (info == null)
		{
			return 0.0;
		}
		return GetRemainTimeSpan(info).TotalDays;
	}

	public static double GetRemainHours(PlayerProductPurchaseInfo timeInfo)
	{
		if (timeInfo == null)
		{
			return 0.0;
		}
		return GetRemainTimeSpan(timeInfo).TotalHours;
	}

	public static TimeSpan GetRemainTimeSpan(PlayerProductPurchaseInfo timeInfo)
	{
		if (timeInfo == null)
		{
			return default(TimeSpan);
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(timeInfo.EndTime, out var o_dateTime))
		{
			o_dateTime = Common.GetUTCNow();
		}
		return o_dateTime - Common.GetUTCNow();
	}

	public static bool IsPurchasable(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo, out double remainDayUntilMonthlyEnd)
	{
		remainDayUntilMonthlyEnd = GetRemainDays(purchaseInfo);
		bool flag = remainDayUntilMonthlyEnd <= (double)productInfo.BuyableDay;
		if (flag && productInfo.BuyGroup > 0)
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
						remainDayUntilMonthlyEnd = GetRemainDays(playerProductPurchaseInfo);
						flag = remainDayUntilMonthlyEnd <= (double)productInfo.BuyableDay;
						if (!flag)
						{
							break;
						}
					}
				}
			}
		}
		return flag;
	}
}
