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

public class UserConquestDungeonManager : NShared.UserConquestDungeonManager
{
	private readonly UserObserverManager<ConquestDungeonInfo> _observerManager = new UserObserverManager<ConquestDungeonInfo>();

	private const float UPDATE_PERIOD = 1f;

	private const float REQUEST_WAITING_TIME = 10f;

	private float _requestedTime;

	private float _lastUpdatedTime;

	private DateTime _nextResetTime;

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(ConquestDungeonInfo info)
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

	public void Apply(IList<ConquestDungeonInfo> info)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info); i++)
		{
			Apply(info[i]);
		}
	}

	protected override void OnAddData(int key, ConquestDungeonInfo value)
	{
		base.OnAddData(key, value);
		_observerManager.AnnounceToObservers(value);
	}

	private ConquestDungeonInfo GetCurrentConquestDungeonInfo(int reqChpaterIndex = 0)
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
		ConquestDungeonInfo conquestDungeonInfo = Get(chapterIndex);
		maxCount = GetChapterMaxCount();
		remainCount = conquestDungeonInfo?.DailyCompletedCount ?? maxCount;
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
		return GetCurrentConquestDungeonInfo(reqChapterIndex)?.DailyCompletedCount ?? GetChapterMaxCount();
	}

	public int GetChapterMaxCount()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxConquestDungeonTryCount");
	}

	public int GetCurrentChapterRechargeGem(int reqChapterIndex = 0)
	{
		int a = GetCurrentConquestDungeonInfo(reqChapterIndex)?.ResetCount ?? 0;
		int[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("ResetConquestDungeonGem");
		int num = Mathf.Min(a, __Extension_Method_ICollection__.COUNT(valueArray) - 1);
		return valueArray[num];
	}

	public void Update()
	{
		if (!(Time.time - _lastUpdatedTime < 1f))
		{
			_lastUpdatedTime = Time.time;
			UpdateStamina();
		}
	}

	private void UpdateStamina()
	{
		if ((!(_requestedTime > 0f) || !(Time.time - _requestedTime < 10f)) && !(_nextResetTime == DateTime.MinValue))
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
			BattleType = BattleType.Conquest
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.GetDungeonInfo.Response response)
		{
			if (response != null)
			{
				_nextResetTime = DateTime.MinValue;
				Apply(response.ConquestDungeonInfo);
			}
		});
	}
}
