using System;
using System.Collections.Generic;
using System.Threading;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class BattleLogContainer
{
	private class DelayedBattleLog
	{
		public int DelayMs { get; set; }

		public BattleLog BattleLog { get; set; }
	}

	private readonly IndexableDictionay<int, BattleLog> _battleLogDictionary;

	private readonly BattleInstance _battleInstance;

	private int _lastAddedLogNo;

	private IBattleLogListener _listener;

	private Func<int> _battleLogDelayer;

	public long RestoreTimeMs;

	private List<DelayedBattleLog> _delayedBattleLogList = new List<DelayedBattleLog>();

	public BattleLogContainer(BattleInstance battleInstance)
	{
		_battleLogDictionary = new IndexableDictionay<int, BattleLog>(64);
		_battleInstance = battleInstance;
		RestoreTimeMs = -1L;
	}

	public void SetListener(IBattleLogListener listener)
	{
		_listener = listener;
	}

	public void SetBattleLogDelayerForTest(Func<int> battleLogDelayer)
	{
		_battleLogDelayer = battleLogDelayer;
	}

	public void Clear()
	{
		_battleLogDictionary.Clear();
		_delayedBattleLogList.Clear();
		_lastAddedLogNo = 0;
	}

	public void AddReceivedLog(BaseBattleLog baseBattleLog, Action onProcessed = null)
	{
		if (baseBattleLog != null)
		{
			BattleLog battleLog = new BattleLog();
			battleLog.BaseBattleLog = baseBattleLog;
			if (onProcessed != null)
			{
				battleLog.OnProcessed = onProcessed;
			}
			AddLog(battleLog);
		}
	}

	public BattleLog AddLog(IBattleLogRuntime runtime)
	{
		long currentTimeMs = _battleInstance.GlobalTimer.GetCurrentTimeMs();
		if (_battleInstance.IsBattleEnabled())
		{
			long globalTimeMs = currentTimeMs + _battleInstance.DeltaTimeMs;
			BattleLog battleLog = CreateBattleLog(globalTimeMs, runtime);
			if (_battleLogDelayer != null)
			{
				int delayMs = _battleLogDelayer();
				_delayedBattleLogList.Add(new DelayedBattleLog
				{
					DelayMs = delayMs,
					BattleLog = battleLog
				});
			}
			else
			{
				AddLog(battleLog);
			}
			return battleLog;
		}
		BattleLog battleLog2 = CreateBattleLog(currentTimeMs, runtime);
		AddLog(battleLog2);
		return battleLog2;
	}

	private BattleLog CreateBattleLog(long globalTimeMs, IBattleLogRuntime runtime)
	{
		if (runtime == null)
		{
			return null;
		}
		BattleLog battleLog = new BattleLog();
		battleLog.BaseBattleLog = new BaseBattleLog();
		battleLog.BaseBattleLog.Type = runtime.GetBattleLogType();
		battleLog.BaseBattleLog.GlobalTimeMs = globalTimeMs;
		battleLog.BaseBattleLog.Data = GameRoot.Instance.BattleLogFactory.SerializeBattleLog(runtime);
		int no = Interlocked.Increment(ref _lastAddedLogNo);
		battleLog.BaseBattleLog.No = no;
		return battleLog;
	}

	public void AddLog(BattleLog battleLog)
	{
		if (battleLog != null)
		{
			_battleLogDictionary.Remove(battleLog.No);
			_battleLogDictionary.TryAdd(battleLog.No, battleLog);
		}
	}

	public void AddLog(IList<BattleLog> battleLogs)
	{
		if (battleLogs == null)
		{
			return;
		}
		foreach (BattleLog battleLog in battleLogs)
		{
			AddLog(battleLog);
		}
	}

	public void AddReceivedLog(IList<BaseBattleLog> battleLogs)
	{
		if (battleLogs == null)
		{
			return;
		}
		foreach (BaseBattleLog battleLog in battleLogs)
		{
			AddReceivedLog(battleLog);
		}
	}

	public BattleLog GetNextBattleLog()
	{
		int no = _battleInstance.Snapshot.ProcessedBattleLogNo + 1;
		return GetBattleLog(no);
	}

	public BattleLog GetBattleLog(int no)
	{
		BattleLog value = null;
		_battleLogDictionary.TryGetValue(no, out value);
		return value;
	}

	public int ProcessLog(long globalTimeMs)
	{
		int num = 0;
		while (true)
		{
			BattleLog nextBattleLog = GetNextBattleLog();
			if (nextBattleLog == null)
			{
				break;
			}
			if (globalTimeMs < nextBattleLog.GlobalTimeMs)
			{
				if (_battleInstance.IsBattleEnabled())
				{
					break;
				}
				VespaDebug.LogError("Invalid battle log time : current=" + globalTimeMs + " < log=" + nextBattleLog.GlobalTimeMs);
			}
			_ = nextBattleLog.GlobalTimeMs;
			nextBattleLog.BaseBattleLog.GlobalTimeMs = globalTimeMs;
			nextBattleLog.Processed = true;
			_battleInstance.TraceBattleEvent(false, "BATTLELOG", "NO=" + nextBattleLog.No, "TYPE=" + nextBattleLog.Type, "DATA=" + nextBattleLog.Data);
			if (_listener != null)
			{
				_listener.OnProcessBattleLog(globalTimeMs, nextBattleLog);
			}
			IBattleLogRuntime battleLogRuntime = GameRoot.Instance.BattleLogFactory.Create(nextBattleLog.Type, nextBattleLog.Data);
			if (battleLogRuntime != null)
			{
				battleLogRuntime.Run(_battleInstance, nextBattleLog);
			}
			else
			{
				VespaDebug.LogError("Failed to create battle log : " + nextBattleLog.Type);
			}
			if (nextBattleLog.OnProcessed != null)
			{
				nextBattleLog.OnProcessed();
			}
			_battleInstance.Snapshot.ProcessedBattleLogNo++;
			num++;
		}
		ProcessDelayedBattleLog();
		return num;
	}

	private void ProcessDelayedBattleLog()
	{
		List<DelayedBattleLog> list = new List<DelayedBattleLog>();
		for (int i = 0; i < _delayedBattleLogList.Count; i++)
		{
			DelayedBattleLog delayedBattleLog = _delayedBattleLogList[i];
			delayedBattleLog.DelayMs -= _battleInstance.DeltaTimeMs;
			if (delayedBattleLog.DelayMs <= 0)
			{
				list.Add(delayedBattleLog);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			DelayedBattleLog delayedBattleLog2 = list[j];
			_delayedBattleLogList.Remove(delayedBattleLog2);
			AddLog(delayedBattleLog2.BattleLog);
		}
	}

	public List<BaseBattleLog> GetAllBattleLogList()
	{
		if (_battleLogDictionary.Count == 0)
		{
			return null;
		}
		List<BaseBattleLog> list = new List<BaseBattleLog>();
		for (int i = 0; i < _battleLogDictionary.Count; i++)
		{
			list.Add(_battleLogDictionary.GetValueByIndex(i).BaseBattleLog);
		}
		list.Sort((BaseBattleLog l, BaseBattleLog r) => l.No.CompareTo(r.No));
		return list;
	}

	public void GetEclipseBattleLogList(ref List<BaseBattleLog> battleLogList, int noStart, int noEnd = -1)
	{
		if (_battleLogDictionary.Count == 0)
		{
			return;
		}
		if (battleLogList == null)
		{
			battleLogList = new List<BaseBattleLog>();
		}
		battleLogList.Clear();
		for (int i = 0; i < _battleLogDictionary.Count; i++)
		{
			BaseBattleLog baseBattleLog = _battleLogDictionary.GetValueByIndex(i).BaseBattleLog;
			if (baseBattleLog.No > noStart && (noEnd == -1 || baseBattleLog.No <= noEnd))
			{
				battleLogList.Add(baseBattleLog);
			}
		}
		battleLogList.Sort((BaseBattleLog l, BaseBattleLog r) => l.No.CompareTo(r.No));
	}

	public List<BaseBattleLog> GetRange(int startIndex, int endIndex = -1)
	{
		if (_battleLogDictionary.Count == 0)
		{
			return null;
		}
		List<BaseBattleLog> list = new List<BaseBattleLog>();
		for (int i = 0; i < _battleLogDictionary.Count; i++)
		{
			BaseBattleLog baseBattleLog = _battleLogDictionary.GetValueByIndex(i).BaseBattleLog;
			if (baseBattleLog.No > startIndex && (endIndex == -1 || baseBattleLog.No <= endIndex))
			{
				list.Add(baseBattleLog);
			}
		}
		list.Sort((BaseBattleLog l, BaseBattleLog r) => l.No.CompareTo(r.No));
		return list;
	}

	public int GetNewestLogNo()
	{
		int num = -1;
		for (int i = 0; i < _battleLogDictionary.Count; i++)
		{
			int no = _battleLogDictionary.GetValueByIndex(i).BaseBattleLog.No;
			num = ((num < no) ? no : num);
		}
		return num;
	}

	public void RemoveFutureBattleLogs(long restoreTimeMs)
	{
		int num = _battleLogDictionary.Count - 1;
		while (num >= 0 && _battleLogDictionary.GetValueByIndex(num).GlobalTimeMs > restoreTimeMs)
		{
			_battleLogDictionary.Remove(_battleLogDictionary.GetKeyByIndex(num));
			num--;
		}
	}
}
