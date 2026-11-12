using System.Collections.Generic;
using NShared;
using NVespa;

namespace NGame2.NBattleThread.NEventService;

public class RollbackManager
{
	public const long DurationTickMs = 3000L;

	private IncreaseDic<long, List<BaseBattleCallback>> _rollbackDic = new IncreaseDic<long, List<BaseBattleCallback>>();

	private IncreaseDic<long, List<BaseBattleCallback>> _recordDic = new IncreaseDic<long, List<BaseBattleCallback>>();

	public void OnCallback(BaseBattleCallback callback, out bool skipThis)
	{
		if (!_rollbackDic.IsNullOrEmpty())
		{
			if (SkipThisCallback(callback))
			{
				skipThis = true;
				return;
			}
			ClearAll();
			RecordThisCallback(callback, out var _);
			skipThis = false;
			return;
		}
		RemoveOldCallback(callback);
		RecordThisCallback(callback, out var isOldCallback2);
		if (!isOldCallback2)
		{
			skipThis = false;
			return;
		}
		if (Rollback(callback))
		{
			skipThis = true;
			return;
		}
		ClearAll();
		RecordThisCallback(callback, out isOldCallback2);
		skipThis = false;
	}

	private void RemoveOldCallback(BaseBattleCallback callback)
	{
		while (_recordDic.Count > 0)
		{
			long keyByIndex = _recordDic.GetKeyByIndex(0);
			if (callback.GlobalTimestamp - keyByIndex > 3000)
			{
				_recordDic.RemoveByIndex(0);
				continue;
			}
			break;
		}
	}

	private bool Rollback(BaseBattleCallback callback)
	{
		_rollbackDic.Clear();
		Math.Swap(ref _rollbackDic, ref _recordDic);
		while (_rollbackDic.Count > 0 && _rollbackDic.GetKeyByIndex(0) < callback.GlobalTimestamp)
		{
			_rollbackDic.RemoveByIndex(0);
		}
		return SkipThisCallback(callback);
	}

	private bool SkipThisCallback(BaseBattleCallback callback)
	{
		if (_rollbackDic.IsNullOrEmpty())
		{
			return false;
		}
		KeyValuePair<long, List<BaseBattleCallback>> keyValuePairByIndex = _rollbackDic.GetKeyValuePairByIndex(0);
		if (keyValuePairByIndex.Key != callback.GlobalTimestamp)
		{
			return false;
		}
		List<BaseBattleCallback> value = keyValuePairByIndex.Value;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(value))
		{
			return false;
		}
		BaseBattleCallback baseBattleCallback = value[0];
		if (callback.GetType() != baseBattleCallback.GetType())
		{
			return false;
		}
		if (!callback.IsEquals(baseBattleCallback))
		{
			return false;
		}
		value.RemoveAt(0);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(value))
		{
			_rollbackDic.RemoveByIndex(0);
		}
		return true;
	}

	private void RecordThisCallback(BaseBattleCallback callback, out bool isOldCallback)
	{
		if (_recordDic.Count == 0)
		{
			List<BaseBattleCallback> list = new List<BaseBattleCallback>();
			_recordDic.Add(callback.GlobalTimestamp, list);
			list.Add(callback);
			isOldCallback = false;
			return;
		}
		long keyByIndex = _recordDic.GetKeyByIndex(_recordDic.Count - 1);
		if (callback.GlobalTimestamp < keyByIndex)
		{
			isOldCallback = true;
			return;
		}
		if (callback.GlobalTimestamp == keyByIndex)
		{
			_recordDic.GetValueByIndex(_recordDic.Count - 1).Add(callback);
			isOldCallback = false;
			return;
		}
		List<BaseBattleCallback> list2 = new List<BaseBattleCallback>();
		_recordDic.Add(callback.GlobalTimestamp, list2);
		list2.Add(callback);
		isOldCallback = false;
	}

	private void ClearAll()
	{
		_rollbackDic.Clear();
		_recordDic.Clear();
	}
}
