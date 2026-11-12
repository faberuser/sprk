using System;
using System.Diagnostics;

namespace NGame2.NNodeTree;

public static class NodeTreeDebug
{
	public class LogEnable : IDisposable
	{
		public LogEnable()
		{
			EnableLogCount++;
		}

		void IDisposable.Dispose()
		{
			EnableLogCount--;
		}
	}

	public static int EnableLogCount;

	public static bool EnableLog => EnableLogCount > 0;

	[Conditional("NODETREEDEBUG_ENABLELOG_NORMAL")]
	public static void Log(object obj)
	{
	}

	[Conditional("NODETREEDEBUG_ENABLELOG_WARNING")]
	public static void LogWarning(object obj)
	{
	}

	[Conditional("NODETREEDEBUG_ENABLELOG_ERROR")]
	public static void LogError(object obj)
	{
	}

	static NodeTreeDebug()
	{
	}

	private static void SkipLog(object message)
	{
	}
}
