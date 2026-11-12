using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetChallengeRaidRankInfo;
using NShared.GetChallengeRaidRankerList;
using NShared.GetMultPlayiChallengeRaidRankerList;
using NShared.NRaid;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager.NLobby;

public class ChallengeRaidWrapper
{
	private class AchievementInfo
	{
		public int ChallengeRaidIndex;

		public int RaidLevel;

		public bool RewardComplete;

		public int ClearCount;
	}

	private class ChalllengeRaidInfoWrapper
	{
		public ChallengeRaidInfo ChallengeRaidInfo;

		public ChallengeRaidData ChallengeRaidData;

		private DropBonusEvent _eventInfo;

		public ChalllengeRaidInfoWrapper()
		{
			ChallengeRaidInfo = new ChallengeRaidInfo();
		}

		public void Clear()
		{
			_eventInfo = null;
			ChallengeRaidData = null;
			ChallengeRaidInfo.ChallengeRaidIndex = 0;
			ChallengeRaidInfo.BeginTime = string.Empty;
			ChallengeRaidInfo.EndTime = string.Empty;
			ChallengeRaidInfo.RaidIndex = 0;
			ChallengeRaidInfo.raidRewardType = RaidRewardType.Bid;
		}

		public void SetChallengeRaidInfo(DropBonusEvent info)
		{
			_eventInfo = info;
			ChallengeRaidInfo.ChallengeRaidIndex = int.Parse(info.Condition);
			ChallengeRaidInfo.BeginTime = info.BeginTime;
			ChallengeRaidInfo.EndTime = info.EndTime;
			ChallengeRaidData = GameRoot.Instance.ChallengeRaidDataContainer.GetData(ChallengeRaidInfo.ChallengeRaidIndex);
			ChallengeRaidInfo.RaidIndex = ((ChallengeRaidData != null) ? ChallengeRaidData.RaidIndex : 0);
			ChallengeRaidInfo.raidRewardType = ((ChallengeRaidData == null) ? RaidRewardType.Normal : ChallengeRaidData.RewardType);
		}

		public bool IsAble()
		{
			if (ChallengeRaidData == null)
			{
				return false;
			}
			if (_eventInfo == null)
			{
				return false;
			}
			return IsAbleEvent(_eventInfo);
		}

		private bool IsAbleEvent(DropBonusEvent info)
		{
			if (info == null)
			{
				return false;
			}
			if (ServerTimeInBetweenTimes(info.BeginTime, info.EndTime))
			{
				return ServerTimeInBetweenHours(info.BeginHour, info.BeginMinute, info.EndHour, info.EndMinute);
			}
			return false;
		}
	}

	public LocalStorageInt LastStoragedRaidIndex = new LocalStorageInt(0, "LastChallengeRaidIndex");

	private const string CHALLENGE_RAID_TITLE = "CHALLENGERAID_TITLE";

	private const string CHALLENGE_RAID_SEASON_END = "CHALLENGERAID_SEASON_END";

	private const string CHALLENGE_RAID_BATTLE_END = "CHALLENGERAID_BATTLE_END";

	public static ChallengeRaidRankerInfo MyRankInfo;

	private ChallengeRaidInfo[] _challengeRaidInfos;

	private ChalllengeRaidInfoWrapper _noticeInfo = new ChalllengeRaidInfoWrapper();

	private ChalllengeRaidInfoWrapper _enterInfo = new ChalllengeRaidInfoWrapper();

	private ChalllengeRaidInfoWrapper _seasonInfo = new ChalllengeRaidInfoWrapper();

	private List<DropBonusEvent> _noticeInfoList;

	private List<DropBonusEvent> _seasonInfoList;

	private List<DropBonusEvent> _enterInfoList;

	private float _cachedRankerTime;

	public const int RankerPerPage = 10;

	private Dictionary<int, ChallengeRaidRankerInfo[]> _cachedRankerDic = new Dictionary<int, ChallengeRaidRankerInfo[]>();

	private MultiPlayChallengeRaidRankerInfo[] _cachedMultiRankerInfos;

	public int GetSeason()
	{
		if (_challengeRaidInfos == null || __Extension_Method_ICollection__.COUNT(_challengeRaidInfos) == 0)
		{
			return 0;
		}
		return _challengeRaidInfos[0].SeasonIndex;
	}

	public bool SetChallengeRaidInfos(ChallengeRaidInfo[] infos)
	{
		if (__Extension_Method_ICollection__.COUNT(infos) > 0)
		{
			_challengeRaidInfos = infos;
			return true;
		}
		return false;
	}

	public ChallengeRaidInfo GetNoticeInfo()
	{
		return _noticeInfo.ChallengeRaidInfo;
	}

	public ChallengeRaidInfo GetSeasonInfo()
	{
		return _seasonInfo.ChallengeRaidInfo;
	}

	public ChallengeRaidInfo GetEnterInfo()
	{
		return _enterInfo.ChallengeRaidInfo;
	}

	public void SetChallengeRaidInfos()
	{
		_noticeInfo.Clear();
		_seasonInfo.Clear();
		_enterInfo.Clear();
		_noticeInfoList = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(EventBonusType.ChallengeRaidNotice);
		_seasonInfoList = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(EventBonusType.ChallengeRaidSeason);
		_enterInfoList = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(EventBonusType.ChallengeRaid);
		if (__Extension_Method_ICollection__.COUNT(_noticeInfoList) > 0)
		{
			_noticeInfo.SetChallengeRaidInfo(_noticeInfoList[0]);
		}
		if (__Extension_Method_ICollection__.COUNT(_seasonInfoList) > 0)
		{
			_seasonInfo.SetChallengeRaidInfo(_seasonInfoList[0]);
		}
		if (__Extension_Method_ICollection__.COUNT(_enterInfoList) > 0)
		{
			_enterInfo.SetChallengeRaidInfo(_enterInfoList[0]);
		}
	}

	public void ShowTimeOutPopup(bool isSeasonEnd, Action okFunc = null)
	{
		string stringDirect = LocalizationManager.GetStringDirect("CHALLENGERAID_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("CHALLENGERAID_BATTLE_END");
		if (isSeasonEnd)
		{
			stringDirect2 = LocalizationManager.GetStringDirect("CHALLENGERAID_SEASON_END");
		}
		MessagePopup.Open(stringDirect, stringDirect2, okFunc);
	}

	public bool IsAbleNotice()
	{
		return _noticeInfo.IsAble();
	}

	public bool IsAbleSeason()
	{
		return _seasonInfo.IsAble();
	}

	public bool IsAbleEnter()
	{
		return _enterInfo.IsAble();
	}

	public void GetRankerList(int page, Action<ChallengeRaidRankerInfo[]> onList)
	{
		if (GetSeasonInfo() == null)
		{
			Vespa_System_Action_Extension_Method.fire(onList, null);
			return;
		}
		_ = page * 10;
		if (_cachedRankerDic != null && Time.time - _cachedRankerTime > 10f)
		{
			_cachedRankerDic = null;
			_cachedRankerTime = Time.time;
		}
		if (_cachedRankerDic != null && _cachedRankerDic.ContainsKey(page))
		{
			Vespa_System_Action_Extension_Method.fire(onList, _cachedRankerDic[page]);
		}
		else
		{
			RequestChallengeRaidRankList(page, onList);
		}
	}

	private void RequestChallengeRaidRankList(int page, Action<ChallengeRaidRankerInfo[]> onList)
	{
		NShared.GetChallengeRaidRankerList.Request request = new NShared.GetChallengeRaidRankerList.Request();
		request.RaidIndex = GetSeasonInfo().ChallengeRaidIndex;
		request.PageNo = page;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetChallengeRaidRankerList.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.Result != NShared.GetChallengeRaidRankerList.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else
			{
				ChallengeRaidRankerInfo[] rankerInfos = response.RankerInfos;
				_cachedRankerDic = _cachedRankerDic ?? new Dictionary<int, ChallengeRaidRankerInfo[]>();
				_cachedRankerDic[page] = rankerInfos;
				Vespa_System_Action_Extension_Method.fire(onList, rankerInfos);
			}
		});
	}

	public void RequestChallengeRaidRankInfo(Action<bool> onResponse)
	{
		if (!_seasonInfo.IsAble())
		{
			VespaDebug.LogError("CurChallengeRaidInfo == null");
			Vespa_System_Action_Extension_Method.fire(onResponse, i_argA: false);
			return;
		}
		NShared.GetChallengeRaidRankInfo.Request request = new NShared.GetChallengeRaidRankInfo.Request();
		request.RaidIndex = _seasonInfo.ChallengeRaidInfo.ChallengeRaidIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetChallengeRaidRankInfo.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onResponse, i_argA: false);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onResponse, i_argA: false);
			}
			else if (response.Result != NShared.GetChallengeRaidRankInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onResponse, i_argA: false);
			}
			else
			{
				MyRankInfo = response.RankInfo;
				GlobalAutoBehaviour<AccountManager>.instance.ApplyChallengeRankerResult(response.RankInfo);
				Vespa_System_Action_Extension_Method.fire(onResponse, i_argA: true);
			}
		});
	}

	public void GetMultiRankerList(Action<MultiPlayChallengeRaidRankerInfo[]> onList)
	{
		if (GetSeasonInfo() == null)
		{
			Vespa_System_Action_Extension_Method.fire(onList, null);
			return;
		}
		if (_cachedMultiRankerInfos != null && Time.time - _cachedRankerTime > 10f)
		{
			_cachedMultiRankerInfos = null;
			_cachedRankerTime = Time.time;
		}
		if (_cachedMultiRankerInfos != null)
		{
			Vespa_System_Action_Extension_Method.fire(onList, _cachedMultiRankerInfos);
		}
		else
		{
			RequrestMultiChallengeRaidRankerList(onList);
		}
	}

	public void RequrestMultiChallengeRaidRankerList(Action<MultiPlayChallengeRaidRankerInfo[]> onList)
	{
		NShared.GetMultPlayiChallengeRaidRankerList.Request request = new NShared.GetMultPlayiChallengeRaidRankerList.Request();
		request.RaidIndex = GetSeasonInfo().ChallengeRaidIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetMultPlayiChallengeRaidRankerList.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.Result != NShared.GetMultPlayiChallengeRaidRankerList.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else
			{
				MultiPlayChallengeRaidRankerInfo[] rankerInfos = response.RankerInfos;
				_cachedMultiRankerInfos = rankerInfos;
				Vespa_System_Action_Extension_Method.fire(onList, rankerInfos);
			}
		});
	}

	public static bool ServerTimeInBetweenTimes(string beginTime, string endTime)
	{
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		DateTime o_dateTime;
		bool num = Vespa_String_Extension_Method2.TryParseNetworkTime2(beginTime, out o_dateTime);
		DateTime o_dateTime2;
		bool flag = Vespa_String_Extension_Method2.TryParseNetworkTime2(endTime, out o_dateTime2);
		if (!num || !flag)
		{
			return false;
		}
		if (DateTime.Compare(currentServerUTCTime, o_dateTime2) >= 0 || DateTime.Compare(currentServerUTCTime, o_dateTime) <= 0)
		{
			return false;
		}
		return true;
	}

	public static bool ServerTimeInBetweenHours(int beginHour, int beginMinute, int endHour, int endMinute)
	{
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		DateTime dateTime = NVespa.Math.CreateDateTime(currentServerUTCTime.Year, currentServerUTCTime.Month, currentServerUTCTime.Day, beginHour, beginMinute, 0);
		DateTime t = NVespa.Math.CreateDateTime(currentServerUTCTime.Year, currentServerUTCTime.Month, currentServerUTCTime.Day, endHour, endMinute, 0);
		if (DateTime.Compare(dateTime, t) >= 0)
		{
			return true;
		}
		if (DateTime.Compare(currentServerUTCTime, t) >= 0 || DateTime.Compare(currentServerUTCTime, dateTime) <= 0)
		{
			return false;
		}
		return true;
	}
}
