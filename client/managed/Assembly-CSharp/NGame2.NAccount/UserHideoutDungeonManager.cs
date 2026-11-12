using System;
using System.Collections.Generic;
using NShared;
using NShared.GetDungeonInfo;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NAccount;

public class UserHideoutDungeonManager : NShared.UserHideoutDungeonManager
{
	private readonly UserObserverManager<HideoutDungeonInfo> _observerManager = new UserObserverManager<HideoutDungeonInfo>();

	private float UPDATE_PERIOD = 1f;

	private float REQUEST_WAITING_TIME = 10f;

	public float _requestedTime;

	public float _lastUpdatedTime;

	private DateTime _nextResetTime;

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(HideoutDungeonInfo info)
	{
		if (info != null)
		{
			Add(info);
			_observerManager.AnnounceToObservers(info);
			if (info.NextResetRemainTime >= 0)
			{
				_nextResetTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime().AddSeconds(info.NextResetRemainTime);
			}
		}
	}

	public void Apply(IList<HideoutDungeonInfo> info)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info); i++)
		{
			Apply(info[i]);
		}
	}

	protected override void OnAddData(int key, HideoutDungeonInfo value)
	{
		base.OnAddData(key, value);
		_observerManager.AnnounceToObservers(value);
	}

	private HideoutDungeonInfo GetCurrentHideoutDungeonInfo(int reqChpaterIndex = 0)
	{
		int key = UserCampaignManager.ChapterIndex;
		if (reqChpaterIndex != 0)
		{
			key = reqChpaterIndex;
		}
		return Get(key);
	}

	public bool GetChapterCount(int chapterIndex, out int remainCount, out int maxCount)
	{
		HideoutDungeonInfo hideoutDungeonInfo = Get(chapterIndex);
		maxCount = GetChapterMaxCount();
		remainCount = hideoutDungeonInfo?.DailyCompletedCount ?? maxCount;
		return true;
	}

	public bool GetChapterRemainCount(out long remainCount, out long maxCount, int reqChpaterIndex = 0)
	{
		int chapterMaxCount = GetChapterMaxCount();
		maxCount = chapterMaxCount;
		int chapterRemainCount = GetChapterRemainCount(reqChpaterIndex);
		remainCount = chapterRemainCount;
		return true;
	}

	public int GetChapterRemainCount(int reqChapterIndex = 0)
	{
		return GetCurrentHideoutDungeonInfo(reqChapterIndex)?.DailyCompletedCount ?? GetChapterMaxCount();
	}

	public int GetChapterMaxCount()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxHideoutDungeonTryCount");
	}

	public int GetCurrentChapterRechargeGem(int reqChapterIndex = 0)
	{
		int a = GetCurrentHideoutDungeonInfo(reqChapterIndex)?.ResetCount ?? 0;
		int[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("ResetHideoutDungeonGem");
		int num = Mathf.Min(a, __Extension_Method_ICollection__.COUNT(valueArray) - 1);
		return valueArray[num];
	}

	public void Update()
	{
		if (!(Time.time - _lastUpdatedTime < UPDATE_PERIOD))
		{
			_lastUpdatedTime = Time.time;
			UpdateStamina();
		}
	}

	private void UpdateStamina()
	{
		if ((!(_requestedTime > 0f) || !(Time.time - _requestedTime < REQUEST_WAITING_TIME)) && !(_nextResetTime == DateTime.MinValue))
		{
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			if (!((_nextResetTime - currentServerUTCTime).TotalSeconds > 0.0))
			{
				_requestedTime = Time.time;
				RequestGetStamina();
			}
		}
	}

	private void RequestGetStamina()
	{
		NShared.GetDungeonInfo.Request request = new NShared.GetDungeonInfo.Request
		{
			BattleType = BattleType.Hideout
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.GetDungeonInfo.Response response)
		{
			if (response != null)
			{
				_nextResetTime = DateTime.MinValue;
				Apply(response.DungeonInfos);
			}
		});
	}
}
