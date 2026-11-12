using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NVespa;

namespace NShared;

public class BattleEventUtility
{
	private class BattleEventLineInfo
	{
		public string Line;

		public int LineNo;

		public string Command;

		public long GlobalTimeMs;
	}

	public class TracedEvent
	{
		public bool IsComment;

		public object EventObject;

		public string[] EventLogs;
	}

	public const char LogSplitter = '/';

	public const string CommentPrefix = ";";

	private static Func<object, string> _traceBattleEventSerializer;

	private static string _traceBattleEventDirectory = ".";

	private static int _traceFileUid = 0;

	private List<long> _traceAccountIds = new List<long>();

	private List<TracedEvent> _tracedEventList = new List<TracedEvent>();

	private bool _queueTracedEvent;

	private List<string> _logList = new List<string>();

	private Thread _logThread;

	private string _traceFileName = string.Empty;

	public static bool HasBattleEventTraceAccount(BattleInstance battleInstance)
	{
		return battleInstance?.IsTraceLogToFile ?? false;
	}

	public static void EnableTraceBattleEventToFile(Func<object, string> eventSerializer, string traceDirectory)
	{
		_traceBattleEventSerializer = eventSerializer;
		_traceBattleEventDirectory = traceDirectory;
	}

	public void SetTraceBattleEventAccountId(long accountId)
	{
		if (!_traceAccountIds.Contains(accountId))
		{
			_traceAccountIds.Add(accountId);
		}
	}

	public string GetFileName()
	{
		int num = Interlocked.Increment(ref _traceFileUid);
		string text = "";
		foreach (long traceAccountId in _traceAccountIds)
		{
			text = text + traceAccountId + "_";
		}
		return DateTime.Now.ToString("yyyyMMdd_HHmmss_" + text + num) + ".txt";
	}

	private void ClearTracedEventList()
	{
		lock (_tracedEventList)
		{
			_tracedEventList.Clear();
		}
	}

	public TracedEvent[] PopTracedEventList()
	{
		TracedEvent[] array = null;
		lock (_tracedEventList)
		{
			array = _tracedEventList.ToArray();
			_tracedEventList.Clear();
			return array;
		}
	}

	public void AddEvent<T>(BattleInstance battleInstance, bool isComment, T data, bool isQueuedEvent)
	{
		if (_queueTracedEvent)
		{
			TracedEvent tracedEvent = new TracedEvent();
			tracedEvent.IsComment = isComment;
			tracedEvent.EventObject = data;
			lock (_tracedEventList)
			{
				_tracedEventList.Add(tracedEvent);
			}
		}
		if (_traceBattleEventSerializer != null && HasBattleEventTraceAccount(battleInstance))
		{
			string text = typeof(T).Name;
			if (isQueuedEvent)
			{
				text += "(QUEUED)";
			}
			string text2 = _traceBattleEventSerializer(data);
			AppendLogToTraceFile(battleInstance, isComment, text, text2);
		}
	}

	public void AddEvent(BattleInstance battleInstance, bool isComment, string[] logs)
	{
		if (_queueTracedEvent)
		{
			TracedEvent tracedEvent = new TracedEvent();
			tracedEvent.IsComment = isComment;
			tracedEvent.EventLogs = logs;
			lock (_tracedEventList)
			{
				_tracedEventList.Add(tracedEvent);
			}
		}
		if (HasBattleEventTraceAccount(battleInstance))
		{
			AppendLogToTraceFile(battleInstance, isComment, logs);
		}
	}

	private void AppendLogToTraceFile(BattleInstance battleInstance, bool isComment, params string[] logs)
	{
		if (_traceBattleEventSerializer == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(isComment ? ";" : "");
		stringBuilder.Append(battleInstance.GlobalTimer.GetCurrentTimeMs());
		stringBuilder.Append('/');
		stringBuilder.Append(battleInstance.MainTimer.GetCurrentTimeMs());
		if (logs != null)
		{
			for (int i = 0; i < logs.Length; i++)
			{
				stringBuilder.Append('/');
				stringBuilder.Append(logs[i]);
			}
		}
		stringBuilder.AppendLine();
		lock (_logList)
		{
			_logList.Add(stringBuilder.ToString());
		}
		if (_logThread == null)
		{
			_logThread = new Thread(FlushLogToTraceFile);
			_logThread.Name = "BattleEventLogThread";
			_logThread.Start();
		}
	}

	private void FlushLogToTraceFile()
	{
		try
		{
			if (string.IsNullOrEmpty(_traceFileName))
			{
				_traceFileName = GetFileName();
			}
			string path = Path.Combine(_traceBattleEventDirectory, _traceFileName);
			if (!Directory.Exists(_traceBattleEventDirectory))
			{
				Directory.CreateDirectory(_traceBattleEventDirectory);
			}
			DateTime utcNow = DateTime.UtcNow;
			while (true)
			{
				Thread.Sleep(1000);
				if ((DateTime.UtcNow - utcNow).TotalSeconds > 10.0)
				{
					break;
				}
				if (_logList.Count == 0)
				{
					continue;
				}
				string text = null;
				lock (_logList)
				{
					if (_logList.Count == 0)
					{
						continue;
					}
					text = "";
					foreach (string log in _logList)
					{
						text += log;
					}
					_logList.Clear();
					goto IL_00fd;
				}
				IL_00fd:
				File.AppendAllText(path, text);
				utcNow = DateTime.UtcNow;
			}
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
		_logThread = null;
	}

	private static List<BattleEventLineInfo> GetOptimizedBattleEventLineInfoList(string[] lines)
	{
		List<BattleEventLineInfo> list = new List<BattleEventLineInfo>();
		for (int i = 0; i < lines.Length; i++)
		{
			string text = lines[i];
			if (!text.StartsWith(";"))
			{
				string[] array = text.Split(new char[1] { '/' });
				int num = int.Parse(array[0]);
				string command = array[2];
				BattleEventLineInfo item = new BattleEventLineInfo
				{
					Line = text,
					LineNo = i,
					Command = command,
					GlobalTimeMs = num
				};
				list.Add(item);
			}
		}
		List<BattleEventLineInfo> list2 = new List<BattleEventLineInfo>();
		for (int j = 0; j < list.Count; j++)
		{
			BattleEventLineInfo battleEventLineInfo = list[j];
			if (list2.Count == 0)
			{
				list2.Add(battleEventLineInfo);
			}
			else if (battleEventLineInfo.Command == "ROLLBACK_END")
			{
				while (list2.Count > 0)
				{
					int index = list2.Count - 1;
					if (list2[index].Command != "ROLLBACK_BEGIN")
					{
						list2.RemoveAt(index);
						continue;
					}
					list2.RemoveAt(index);
					break;
				}
				while (list2.Count > 0)
				{
					int index2 = list2.Count - 1;
					if (list2[index2].GlobalTimeMs < battleEventLineInfo.GlobalTimeMs)
					{
						break;
					}
					list2.RemoveAt(index2);
				}
			}
			else
			{
				list2.Add(battleEventLineInfo);
			}
		}
		return list2;
	}

	public static bool CheckBattleEventIntegrity(string[] lines1, string[] lines2, out string ret)
	{
		List<BattleEventLineInfo> optimizedBattleEventLineInfoList = GetOptimizedBattleEventLineInfoList(lines1);
		List<BattleEventLineInfo> optimizedBattleEventLineInfoList2 = GetOptimizedBattleEventLineInfoList(lines2);
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		for (num = 0; num < System.Math.Min(optimizedBattleEventLineInfoList.Count, optimizedBattleEventLineInfoList2.Count); num++)
		{
			BattleEventLineInfo battleEventLineInfo = optimizedBattleEventLineInfoList[num];
			BattleEventLineInfo battleEventLineInfo2 = optimizedBattleEventLineInfoList2[num];
			if (battleEventLineInfo.Line != battleEventLineInfo2.Line)
			{
				stringBuilder.AppendLine("Event mismatch : lineno1=" + battleEventLineInfo.LineNo + " lineno2=" + battleEventLineInfo2.LineNo);
				stringBuilder.AppendLine("line1 = " + battleEventLineInfo.Line);
				stringBuilder.AppendLine("line2 = " + battleEventLineInfo2.Line);
				ret = stringBuilder.ToString();
				return false;
			}
		}
		if (optimizedBattleEventLineInfoList.Count > num)
		{
			BattleEventLineInfo battleEventLineInfo3 = optimizedBattleEventLineInfoList[num];
			stringBuilder.AppendLine("Line1 count mismatch : unhandled from lineno1=" + battleEventLineInfo3.LineNo);
			stringBuilder.AppendLine("line1 = " + battleEventLineInfo3.Line);
			ret = stringBuilder.ToString();
			return false;
		}
		if (optimizedBattleEventLineInfoList2.Count > num)
		{
			BattleEventLineInfo battleEventLineInfo4 = optimizedBattleEventLineInfoList2[num];
			stringBuilder.AppendLine("Line2 count mismatch : unhandled from lineno2=" + battleEventLineInfo4.LineNo);
			stringBuilder.AppendLine("line2 = " + battleEventLineInfo4.Line);
			ret = stringBuilder.ToString();
			return false;
		}
		ret = "Same battle events.";
		return true;
	}
}
