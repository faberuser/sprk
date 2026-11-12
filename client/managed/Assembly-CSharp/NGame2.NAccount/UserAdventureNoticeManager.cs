using System;
using System.Collections.Generic;
using NShared;
using NShared.GetAdeventureNotice;
using NShared.NWorldBoss;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserAdventureNoticeManager
{
	public class AdventureNoticeInfo
	{
		public BattleType BattleType;

		public List<DateTime> EndTimeList = new List<DateTime>();

		private AdventureNoticeData _adventureNoticeData;

		public int GetRemainTime(bool useLocalTime = false)
		{
			int num = __Extension_Method_ICollection__.COUNT(EndTimeList);
			if (num == 0)
			{
				return 0;
			}
			DateTime dateTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			if (useLocalTime)
			{
				dateTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
			}
			_ = DateTime.MinValue;
			int num2 = 0;
			int result = 0;
			for (int i = 0; i < num; i++)
			{
				num2 = (int)(EndTimeList[i] - dateTime).TotalSeconds;
				if (num2 > 0)
				{
					result = Math.Max(num2, 0);
					break;
				}
			}
			return result;
		}
	}

	public Dictionary<BattleType, AdventureNoticeInfo> _adventureNoticeDic = new Dictionary<BattleType, AdventureNoticeInfo>();

	private bool _isRefresh;

	public bool IsRefreshInfo()
	{
		if (!_isRefresh)
		{
			return true;
		}
		return __Extension_Method_ICollection__.COUNT(_adventureNoticeDic) == 0;
	}

	public void ForeceRefrsh()
	{
		_isRefresh = false;
	}

	public void Apply(NShared.GetAdeventureNotice.Response response)
	{
		if (response != null)
		{
			_isRefresh = true;
			Apply(BattleType.Arena, response.MatchSeasonData);
			Apply(BattleType.ArenaBanPick, response.BanPickSeasonData);
			Apply(response.WorldBossInfos);
			Apply(response.GuildSuppressSessionInfo);
		}
	}

	public void Apply(BattleType battleType, MatchSeasonData data)
	{
		if (data != null)
		{
			DateTime endTime = Convert.ToDateTime(data.End);
			Set(battleType, endTime);
		}
	}

	public void Apply(WorldBossInfo[] infos)
	{
		WorldBossInfo worldBossInfo = (Vespa_Generic_IList_Extension_Method.IsValidIndex(infos, 0) ? infos[0] : null);
		if (worldBossInfo == null)
		{
			return;
		}
		BattleType battleType = BattleType.WorldBoss;
		DateTime endTime = DateTime.MinValue;
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(worldBossInfo.CreatedTime, out var o_dateTime))
		{
			WorldBossData data = GameRoot.Instance.WorldBossDataContainer.GetData(worldBossInfo.Index);
			if (data == null)
			{
				return;
			}
			int num = data.DealBattleDuration + data.NormalBattleDuration;
			endTime = o_dateTime.AddSeconds(num);
		}
		Set(battleType, endTime);
	}

	public void Apply(GuildSuppressSessionInfo info)
	{
		if (info != null)
		{
			BattleType battleType = BattleType.GuildSuppress;
			_ = info.State;
			DateTime o_dateTime = DateTime.MinValue;
			DateTime o_dateTime2 = DateTime.MinValue;
			Vespa_String_Extension_Method2.TryParseNetworkTime2(info.ApplyEndTime, out o_dateTime);
			Vespa_String_Extension_Method2.TryParseNetworkTime2(info.BattleEndTime, out o_dateTime2);
			Set(battleType, o_dateTime);
			AddEndTime(battleType, o_dateTime2);
		}
	}

	private void Set(BattleType battleType, DateTime endTime)
	{
		if (!_adventureNoticeDic.ContainsKey(battleType))
		{
			AdventureNoticeInfo adventureNoticeInfo = new AdventureNoticeInfo();
			adventureNoticeInfo.BattleType = battleType;
			adventureNoticeInfo.EndTimeList.Add(endTime);
			_adventureNoticeDic.Add(battleType, adventureNoticeInfo);
		}
		else
		{
			_adventureNoticeDic[battleType].EndTimeList.Clear();
			_adventureNoticeDic[battleType].EndTimeList.Add(endTime);
		}
	}

	private void AddEndTime(BattleType battleType, DateTime endTime)
	{
		if (!_adventureNoticeDic.ContainsKey(battleType))
		{
			AdventureNoticeInfo adventureNoticeInfo = new AdventureNoticeInfo();
			adventureNoticeInfo.BattleType = battleType;
			adventureNoticeInfo.EndTimeList.Add(endTime);
			_adventureNoticeDic.Add(battleType, adventureNoticeInfo);
		}
		else
		{
			_adventureNoticeDic[battleType].EndTimeList.Add(endTime);
		}
		_adventureNoticeDic[battleType].EndTimeList.Sort();
	}

	public void RemoveEndTime(BattleType battleType)
	{
		if (_adventureNoticeDic.ContainsKey(battleType))
		{
			_adventureNoticeDic[battleType].EndTimeList.Clear();
		}
	}

	public AdventureNoticeInfo GetAdventureNoticeInfo(BattleType battleType)
	{
		if (!_adventureNoticeDic.ContainsKey(battleType))
		{
			return null;
		}
		return _adventureNoticeDic[battleType];
	}
}
