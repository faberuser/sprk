using System;
using System.Collections.Generic;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserFreeEquipGachaManager : NShared.UserFreeEquipGachaManager
{
	public enum ChangeType
	{
		None,
		FreeGachaChanged
	}

	public class ChangedGachaInfo
	{
		public ChangeType changeType;

		public int GachaIndex;
	}

	private readonly UserObserverManager<ChangedGachaInfo> _observerManager = new UserObserverManager<ChangedGachaInfo>();

	private Dictionary<int, DateTime> _DicUpdateFreeGachaResetTimes = new Dictionary<int, DateTime>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(FreeEquipGachaInfo[] infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Length; i++)
			{
				Set(infos[i]);
			}
		}
	}

	public void Set(FreeEquipGachaInfo info)
	{
		if (info != null)
		{
			Add(info);
			_observerManager.AnnounceToObservers(new ChangedGachaInfo
			{
				changeType = ChangeType.FreeGachaChanged,
				GachaIndex = info.GachaIndex
			});
		}
	}

	protected override void OnAddData(int key, FreeEquipGachaInfo value)
	{
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		if (!_DicUpdateFreeGachaResetTimes.ContainsKey(value.GachaIndex))
		{
			_DicUpdateFreeGachaResetTimes.Add(value.GachaIndex, currentServerLocalTime);
		}
		else
		{
			_DicUpdateFreeGachaResetTimes[value.GachaIndex] = currentServerLocalTime;
		}
		base.OnAddData(key, value);
	}

	public int GetRemainGachaChance(int GachaIndex)
	{
		FreeEquipGachaInfo freeEquipGachaInfo = Get(GachaIndex);
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(GachaIndex);
		if (data == null)
		{
			return 0;
		}
		if (freeEquipGachaInfo == null)
		{
			return data.DailyMaxFree;
		}
		if (freeEquipGachaInfo.ResetRemainTime > 0 && _DicUpdateFreeGachaResetTimes.ContainsKey(GachaIndex))
		{
			DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
			DateTime dateTime = _DicUpdateFreeGachaResetTimes[GachaIndex];
			if ((currentServerLocalTime - dateTime).TotalSeconds > (double)freeEquipGachaInfo.ResetRemainTime)
			{
				return data.DailyMaxFree;
			}
		}
		return freeEquipGachaInfo.RemainDayGachaChance;
	}

	public bool HasAnyFreeEquipGachaChance()
	{
		foreach (NewEquipGachaData allDatum in GameRoot.Instance.NewEquipGachaDataContainer.GetAllData())
		{
			if (allDatum.FreeCoolTime != 0 && Free(allDatum.Index))
			{
				return true;
			}
		}
		return false;
	}

	public bool Free(int gachaIndex)
	{
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(gachaIndex);
		if (data == null)
		{
			return false;
		}
		if (data.DailyMaxFree == 0)
		{
			return false;
		}
		if (Get(gachaIndex) == null)
		{
			return true;
		}
		bool result = false;
		int remainGachaChance = GetRemainGachaChance(gachaIndex);
		if (remainGachaChance > 0 && remainGachaChance < data.DailyMaxFree)
		{
			result = !GetRemainFreeGachaCoolTimeSpan(gachaIndex).HasValue;
		}
		if (remainGachaChance == data.DailyMaxFree)
		{
			result = true;
		}
		return result;
	}

	public TimeSpan? GetRemainFreeGachaCoolTimeSpan(int GachaIndex)
	{
		DateTime nextGachaFreeTime = GetNextGachaFreeTime(GachaIndex);
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (nextGachaFreeTime.CompareTo(DateTime.MinValue) == 0 || nextGachaFreeTime.CompareTo(currentServerUTCTime) < 0)
		{
			return null;
		}
		return nextGachaFreeTime - currentServerUTCTime;
	}

	private DateTime GetNextGachaFreeTime(int gachaIndex)
	{
		FreeEquipGachaInfo freeEquipGachaInfo = Get(gachaIndex);
		if (freeEquipGachaInfo == null || String_Extension_Method.IsNullOrEmpty(freeEquipGachaInfo.LastGachaTime))
		{
			return DateTime.MinValue;
		}
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(freeEquipGachaInfo.GachaIndex);
		if (data == null)
		{
			return DateTime.MinValue;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(freeEquipGachaInfo.LastGachaTime, out var o_dateTime))
		{
			return DateTime.MinValue;
		}
		if (data.DailyMaxFree == 1)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.GetUTCOffsetNextMidnightTime();
		}
		return o_dateTime.AddSeconds(data.FreeCoolTime);
	}
}
