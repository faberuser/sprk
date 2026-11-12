using System;
using System.Collections.Generic;
using NGame2.NNotification;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserCraftSlotManager : NShared.UserCraftSlotManager
{
	private readonly UserObserverManager<CraftSlotInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserCraftSlotManager()
	{
		_observerManager = new UserObserverManager<CraftSlotInfo>();
	}

	public void Apply(CraftSlotResultInfo craftSlotResultInfo)
	{
		if (craftSlotResultInfo != null)
		{
			DateTime uTCNow = Common.GetUTCNow();
			CraftSlotInfo craftSlotInfo = Get(craftSlotResultInfo.SlotIndex);
			if (craftSlotInfo == null)
			{
				craftSlotInfo = new CraftSlotInfo
				{
					SlotIndex = craftSlotResultInfo.SlotIndex,
					CraftIndex = craftSlotResultInfo.CraftIndex,
					ItemIndex = craftSlotResultInfo.ItemIndex,
					CompleteTime = ((craftSlotResultInfo.RemainTime > 0) ? Vespa_DateTime_Extension_Method.ToNetworkString2(uTCNow.AddSeconds(craftSlotResultInfo.RemainTime)) : Vespa_DateTime_Extension_Method.ToNetworkString2(uTCNow)),
					ItemCount = craftSlotResultInfo.ItemCount
				};
			}
			else
			{
				craftSlotInfo.CraftIndex = craftSlotResultInfo.CraftIndex;
				craftSlotInfo.ItemIndex = craftSlotResultInfo.ItemIndex;
				craftSlotInfo.CompleteTime = ((craftSlotResultInfo.RemainTime > 0) ? Vespa_DateTime_Extension_Method.ToNetworkString2(uTCNow.AddSeconds(craftSlotResultInfo.RemainTime)) : Vespa_DateTime_Extension_Method.ToNetworkString2(uTCNow));
				craftSlotInfo.ItemCount = craftSlotResultInfo.ItemCount;
			}
			Apply(craftSlotInfo);
		}
	}

	public void Apply(CraftSlotResultInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public void Apply(CraftSlotInfo info)
	{
		Add(info);
		_observerManager.AnnounceToObservers(info);
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(info.CompleteTime, out var o_dateTime))
		{
			int slotIndex = info.SlotIndex;
			TimeSpan timeSpan = o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			if (timeSpan.TotalMilliseconds > 0.0)
			{
				SingletonAuto<NotificationManager>.instance.RegisteItemCraftCompletelNotification(slotIndex, (long)timeSpan.TotalMilliseconds);
			}
		}
	}

	public void Apply(CraftSlotInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public bool HasAnyCompletedCraftSlot()
	{
		using (IEnumerator<CraftSlotInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				CraftSlotInfo current = enumerator.Current;
				if (current.ItemIndex != 0 && CompletedCraft(current.SlotIndex))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEmptyCraftSlot()
	{
		using (IEnumerator<CraftSlotInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.ItemIndex == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetEmptyCraftSlot()
	{
		int num = 1;
		using (IEnumerator<CraftSlotInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.ItemIndex == 0)
				{
					return num;
				}
				num++;
			}
		}
		return 0;
	}

	public bool CompletedCraft(int slotIndex)
	{
		if (Get(slotIndex) == null)
		{
			return true;
		}
		return !GetRemainTimeSpan(slotIndex).HasValue;
	}

	public TimeSpan? GetRemainTimeSpan(int slotIndex)
	{
		DateTime completeTime = GetCompleteTime(slotIndex);
		DateTime uTCNow = Common.GetUTCNow();
		if (completeTime.CompareTo(DateTime.MinValue) == 0 || completeTime.CompareTo(uTCNow) < 0)
		{
			return null;
		}
		return completeTime - uTCNow;
	}

	private DateTime GetCompleteTime(int slotIndex)
	{
		CraftSlotInfo craftSlotInfo = Get(slotIndex);
		if (craftSlotInfo == null || String_Extension_Method.IsNullOrEmpty(craftSlotInfo.CompleteTime))
		{
			return DateTime.MinValue;
		}
		Vespa_String_Extension_Method2.TryParseNetworkTime2(craftSlotInfo.CompleteTime, out var o_dateTime);
		return o_dateTime;
	}
}
