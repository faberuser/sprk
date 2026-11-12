using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NPayShopItemEvent;

public class PayShopItemEventManager : GlobalAutoBehaviour<PayShopItemEventManager>
{
	public enum EChangeType
	{
		Added,
		Removed,
		Changed
	}

	public class EventChangedInfo
	{
		public EChangeType ChangeType;

		public PayShopCategory PayShopCategory;

		public long EventId;

		public int ItemIndex;
	}

	private List<PayShopItemEventInfo> _list = new List<PayShopItemEventInfo>();

	private readonly UserObserverManager<EventChangedInfo> _observerManager = new UserObserverManager<EventChangedInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Add(PayShopItemEventInfo eventInfo, bool isListSort)
	{
		if (_list.Find((PayShopItemEventInfo info) => info.Id == eventInfo.Id) == null)
		{
			_list.Add(eventInfo);
			if (isListSort)
			{
				ListSortDesc();
			}
			_observerManager.AnnounceToObservers(new EventChangedInfo
			{
				ChangeType = EChangeType.Added,
				PayShopCategory = eventInfo.PayShopCategory,
				EventId = eventInfo.Id,
				ItemIndex = eventInfo.Index
			});
		}
	}

	private void ListSortDesc()
	{
		_list.Sort((PayShopItemEventInfo l, PayShopItemEventInfo r) => -l.Id.CompareTo(r.Id));
	}

	public void Remove(long Id)
	{
		PayShopItemEventInfo payShopItemEventInfo = _list.Find((PayShopItemEventInfo info) => info.Id == Id);
		if (payShopItemEventInfo != null)
		{
			int index = payShopItemEventInfo.Index;
			_list.Remove(payShopItemEventInfo);
			_observerManager.AnnounceToObservers(new EventChangedInfo
			{
				ChangeType = EChangeType.Removed,
				PayShopCategory = payShopItemEventInfo.PayShopCategory,
				EventId = Id,
				ItemIndex = index
			});
		}
	}

	public void Set(PayShopItemEventInfo[] infos)
	{
		if (infos != null)
		{
			_list.Clear();
			for (int i = 0; i < infos.Length; i++)
			{
				Add(infos[i], isListSort: false);
			}
			ListSortDesc();
		}
	}

	private void TempAddList(PayShopCategory category)
	{
		PayShopItemEventInfo item = TempCreateEventInfo(category);
		_list.Add(item);
	}

	private PayShopItemEventInfo TempCreateEventInfo(PayShopCategory category, int index = 0, string group = "All")
	{
		long id = __Extension_Method_ICollection__.COUNT(_list) + 1;
		DateTime i_dateTime = new DateTime(2019, 10, 30);
		return new PayShopItemEventInfo
		{
			Id = id,
			PayShopCategory = category,
			Group = group,
			Index = index,
			DiscountPercentage = 50,
			EndTime = Vespa_DateTime_Extension_Method.ToNetworkString2(i_dateTime)
		};
	}

	public PayShopItemEventInfo GetLatestEventInfo(Func<PayShopItemEventInfo, bool> func)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_list))
		{
			return null;
		}
		List<PayShopItemEventInfo> list = new List<PayShopItemEventInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
		{
			PayShopItemEventInfo payShopItemEventInfo = _list[i];
			if (func(payShopItemEventInfo))
			{
				list.Add(payShopItemEventInfo);
			}
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return null;
		}
		return list[0];
	}

	public List<PayShopItemEventInfo> GetPayShopItemEventInfoList()
	{
		return _list;
	}

	public static bool IsEndEvent(PayShopItemEventInfo eventInfo)
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(eventInfo.EndTime, out var o_dateTime))
		{
			return true;
		}
		return Common.GetUTCNow() >= o_dateTime;
	}

	public static double CalcRemainTime(PayShopItemEventInfo eventInfo)
	{
		DateTime uTCNow = Common.GetUTCNow();
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(eventInfo.EndTime, out var o_dateTime))
		{
			return 0.0;
		}
		double totalSeconds = (o_dateTime - uTCNow).TotalSeconds;
		if (totalSeconds > 0.0)
		{
			return totalSeconds;
		}
		return 0.0;
	}

	public PayShopItemEventInfo GetEffectiveEventInfo(IPayShopEventSearchInfo data)
	{
		return GetLatestEventInfo(delegate(PayShopItemEventInfo info)
		{
			if (info.PayShopCategory != data.PayShopCategory)
			{
				return false;
			}
			if (IsEndEvent(info))
			{
				return false;
			}
			if (info.Index == data.Index)
			{
				return true;
			}
			if (info.Group == "All")
			{
				return true;
			}
			return (info.Group == data.Group) ? true : false;
		});
	}

	public int GetDiscountPercentage(IPayShopEventSearchInfo data)
	{
		if (data == null)
		{
			return 0;
		}
		return GetEffectiveEventInfo(data)?.DiscountPercentage ?? 0;
	}

	public bool IsPayShopEventOnByCategory(PayShopCategory category)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
		{
			PayShopItemEventInfo payShopItemEventInfo = _list[i];
			if (payShopItemEventInfo.PayShopCategory == category && !IsEndEvent(payShopItemEventInfo))
			{
				return true;
			}
		}
		return false;
	}
}
