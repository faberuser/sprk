using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace NVespa;

public static class VespaDebugFile
{
	private class LogData
	{
		public string condition;

		public string stackTrace;

		public LogType type;
	}

	private static string _logFileName;

	private static List<LogData> _logList;

	static VespaDebugFile()
	{
		_logList = new List<LogData>();
		DateTime now = DateTime.Now;
		string rhs = $"Log_{now.Month}m_{now.Day}d_{now.Hour}h_{now.Minute}m_{now.Second}s.txt";
		_logFileName = String_Extension_Method.CombinePath(UnityCache.persistentDataPath, rhs);
	}

	public static void LogCallback(string condition, string stackTrace, LogType type)
	{
		if (type != LogType.Log)
		{
			LogData logData = new LogData();
			logData.condition = condition;
			logData.stackTrace = stackTrace;
			logData.type = type;
			_logList.Add(logData);
			switch (type)
			{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				FlushLog();
				break;
			case LogType.Warning:
				FlushLog();
				break;
			}
		}
	}

	private static void FlushLog()
	{
		if (_logList.Count != 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < _logList.Count; i++)
			{
				LogData logData = _logList[i];
				stringBuilder.AppendLine($"{Vespa_String_Extension_Method.GetEnumString(logData.type)}: {logData.condition}");
				stringBuilder.AppendLine(logData.stackTrace);
			}
			File.AppendAllText(_logFileName, stringBuilder.ToString());
			_logList.Clear();
		}
	}
}
