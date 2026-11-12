using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NDispatchBattle;

public class DispatchInfoListUpdater
{
	private const int MAX_TRY_COUNT_FOR_UPDATE_LIST = 5;

	private const float TIME_MIN_UPDATE_TERM = 5f;

	private const float TIME_MAX_UPDATE_TERM = 30f;

	private List<WCL_DispatchBattleInfo> _infoList = new List<WCL_DispatchBattleInfo>(15);

	private WCL_DispatchBattleInfo _waitingResponseInfo;

	private float _tempTimeS;

	private bool _isNewTempTime;

	private bool _isLazyUpdate = true;

	private WCL_DispatchBattleInfo _targetInfo;

	private bool _isNewInfo;

	private Dictionary<int, int> _tryCntTable = new Dictionary<int, int>();

	private Coroutine _coCheckingDispatchEndTimeForUpdate;

	private Coroutine _coUpdatingDispatchListWithInfo;

	private static DispatchInfoListUpdater _inst;

	public static DispatchInfoListUpdater Inst
	{
		get
		{
			if (_inst == null)
			{
				_inst = new DispatchInfoListUpdater();
			}
			return _inst;
		}
	}

	private DispatchInfoListUpdater()
	{
		VespaCoroutine.StartGlobal(CoUpdatingDispatchList(), "DispatchListUpdater");
		VespaCoroutine.StartGlobal(CoCheckingDispatchEndTimeForUpdate(), "DispatchEndTimeChecker");
	}

	public void Regist(WCL_DispatchBattleInfo info)
	{
		int i = 0;
		for (int count = _infoList.Count; i < count; i++)
		{
			WCL_DispatchBattleInfo wCL_DispatchBattleInfo = _infoList[i];
			if (wCL_DispatchBattleInfo != null && wCL_DispatchBattleInfo.SlotIndex == info.SlotIndex)
			{
				_infoList.RemoveAt(i);
				break;
			}
		}
		_infoList.Add(info);
	}

	public void Remove(WCL_DispatchBattleInfo info)
	{
		int i = 0;
		for (int count = _infoList.Count; i < count; i++)
		{
			WCL_DispatchBattleInfo wCL_DispatchBattleInfo = _infoList[i];
			if (wCL_DispatchBattleInfo != null && wCL_DispatchBattleInfo.SlotIndex == info.SlotIndex)
			{
				_infoList.RemoveAt(i);
				break;
			}
		}
	}

	public void Clear()
	{
		_infoList.Clear();
		_waitingResponseInfo = null;
	}

	public void SetLazyUpdate(bool useLazyUpdate)
	{
		_isLazyUpdate = useLazyUpdate;
	}

	public void SetInfo(WCL_DispatchBattleInfo info)
	{
		if (info != null)
		{
			_targetInfo = info;
			if (_coUpdatingDispatchListWithInfo == null)
			{
				_coUpdatingDispatchListWithInfo = VespaCoroutine.StartGlobal(CoUpdatingDispatchListWithInfo(), "DispatchListUpdaterWithInfo");
			}
		}
	}

	public void ClearInfo()
	{
		_targetInfo = null;
		if (_coUpdatingDispatchListWithInfo != null)
		{
			VespaCoroutine.Stop(_coUpdatingDispatchListWithInfo);
			_coUpdatingDispatchListWithInfo = null;
		}
	}

	public void SetTempTime(float timeS)
	{
		timeS = Mathf.Clamp(timeS, 5f, 30f);
		if (timeS != _tempTimeS)
		{
			_isNewTempTime = true;
		}
		_tempTimeS = timeS;
		if (_coCheckingDispatchEndTimeForUpdate == null)
		{
			_coCheckingDispatchEndTimeForUpdate = VespaCoroutine.StartGlobal(CoUpdatingDispatchListWithTempTime(), "DispatchListUpdaterWithTempTimeS");
		}
	}

	public void ClearTempTime()
	{
		_tempTimeS = 0f;
		if (_coCheckingDispatchEndTimeForUpdate != null)
		{
			VespaCoroutine.Stop(_coCheckingDispatchEndTimeForUpdate);
			_coCheckingDispatchEndTimeForUpdate = null;
		}
	}

	private IEnumerator CoUpdatingDispatchList()
	{
		yield break;
	}

	private IEnumerator CoUpdatingDispatchListWithTempTime()
	{
		yield break;
	}

	private IEnumerator CoUpdatingDispatchListWithInfo()
	{
		yield break;
	}

	private IEnumerator CoCheckingDispatchEndTimeForUpdate()
	{
		yield break;
	}

	private bool TryUpdateState(WCL_DispatchBattleInfo info)
	{
		DispatchState state = info.State;
		if ((uint)(state - 1) <= 2u && info.IsTimeEnd())
		{
			if (_tryCntTable.ContainsKey(info.SlotIndex))
			{
				if (_tryCntTable[info.SlotIndex] > 5)
				{
					return false;
				}
				_tryCntTable[info.SlotIndex] = Mathf.Min(6, _tryCntTable[info.SlotIndex]++);
			}
			else
			{
				_tryCntTable[info.SlotIndex] = 1;
			}
			_waitingResponseInfo = info;
			DispatchHelper.SendRQGetDispatchList(delegate
			{
				_waitingResponseInfo = null;
				if (info != null)
				{
					WCL_DispatchBattleInfo wCL_DispatchBattleInfo = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Get(info.SlotIndex);
					if (wCL_DispatchBattleInfo != null)
					{
						DispatchState state2 = wCL_DispatchBattleInfo.State;
						if ((uint)(state2 - 1) > 3u && _tryCntTable.ContainsKey(wCL_DispatchBattleInfo.SlotIndex))
						{
							_tryCntTable.Remove(wCL_DispatchBattleInfo.SlotIndex);
						}
					}
					else if (_tryCntTable.ContainsKey(info.SlotIndex))
					{
						_tryCntTable.Remove(info.SlotIndex);
					}
				}
			});
			return true;
		}
		return false;
	}
}
