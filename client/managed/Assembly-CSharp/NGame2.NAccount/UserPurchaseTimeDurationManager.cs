using System;
using NGame2.NUI.NWindow;
using NShared;

namespace NGame2.NAccount;

public class UserPurchaseTimeDurationManager : NShared.UserPurchaseTimeDurationManager
{
	private readonly UserObserverManager<PlayerPurchaseTimeDurationInfo> _observerManager = new UserObserverManager<PlayerPurchaseTimeDurationInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public PlayerPurchaseTimeDurationInfo Get(int ShopIndex, int Index)
	{
		return Get(new PurchaseInfoKey(ShopIndex, Index));
	}

	protected override void OnAddData(PurchaseInfoKey key, PlayerPurchaseTimeDurationInfo value)
	{
		base.OnAddData(key, value);
		_observerManager.AnnounceToObservers(value);
	}

	public double GetRemainDays(int ShopIndex, int Index)
	{
		return GetRemainDays(Get(ShopIndex, Index));
	}

	public static double GetRemainDays(PlayerPurchaseTimeDurationInfo timeInfo)
	{
		if (timeInfo == null)
		{
			return 0.0;
		}
		return GetRemainTimeSpan(timeInfo).TotalDays;
	}

	public static double GetRemainHours(PlayerPurchaseTimeDurationInfo timeInfo)
	{
		if (timeInfo == null)
		{
			return 0.0;
		}
		return GetRemainTimeSpan(timeInfo).TotalHours;
	}

	public static TimeSpan GetRemainTimeSpan(PlayerPurchaseTimeDurationInfo timeInfo)
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
}
