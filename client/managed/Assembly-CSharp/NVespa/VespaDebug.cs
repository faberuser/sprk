using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using NVespa.NGlobal.NGuiManager;
using NVespa.NUtil;
using UnityEngine;

namespace NVespa;

public static class VespaDebug
{
	public class CallStackData
	{
		public string MethodName;

		public string FileName;

		public int LineNumber;

		public string GetDescription(bool shortDescription)
		{
			string arg = (shortDescription ? Path.GetFileName(FileName) : FileName);
			return $"{MethodName}: {arg}({LineNumber})";
		}
	}

	private static string ThreadLogPrefix;

	private static Action<object> PlatformLog => NativeLog;

	private static Action<object> PlatformLogWarning => NativeLogWarning;

	private static Action<object> PlatformLogError => NativeLogError;

	private static Action<object> PlatformLogCritical => NativeLogError;

	public static Action<object> LogError => SelectLog(PlatformLogError, ThreadLogError);

	public static Action<object, UnityEngine.Object> LogError2 => UnityEngine.Debug.LogError;

	public static Action<object> LogCritical => SelectLog(PlatformLogCritical, ThreadLogCritical);

	public static Action<object, UnityEngine.Object> LogCritical2 => UnityEngine.Debug.LogError;

	[Conditional("VESPADEBUG_ENABLELOG_NETWORK")]
	public static void LogNetwork(object obj)
	{
	}

	[Conditional("VESPADEBUG_ENABLELOG_NETWORK")]
	public static void LogNetwork2(object obj, UnityEngine.Object context)
	{
	}

	[Conditional("VESPADEBUG_ENABLELOG_LOAD")]
	public static void LogLoad(object obj)
	{
	}

	[Conditional("VESPADEBUG_ENABLELOG_NETWORK")]
	public static void LogLoad2(object obj, UnityEngine.Object context)
	{
	}

	[Conditional("VESPADEBUG_ENABLELOG_NORMAL")]
	public static void Log(object obj)
	{
	}

	[Conditional("VESPADEBUG_ENABLELOG_NORMAL")]
	public static void Log2(object obj, UnityEngine.Object context)
	{
	}

	[Conditional("VESPADEBUG_ENABLELOG_WARNING")]
	public static void LogWarning(object obj)
	{
	}

	[Conditional("VESPADEBUG_ENABLELOG_WARNING")]
	public static void LogWarning2(object obj, UnityEngine.Object context)
	{
	}

	[Conditional("VESPADEBUG_ENABLE_DEBUG_CREATE_AND_DESTROY")]
	public static void NotifyOnDestroy(UnityEngine.Object obj)
	{
	}

	static VespaDebug()
	{
		ThreadLogPrefix = "\n --> [Thread Log] ";
		UnityEngine.Debug.unityLogger.filterLogType = LogType.Assert;
	}

	public static void InitUnityThreadId()
	{
		Application.logMessageReceived -= internalLogCallback;
		Application.logMessageReceived += internalLogCallback;
	}

	public static bool IsUnityMainThread()
	{
		return UnityCache.IsMainThread;
	}

	[Conditional("UNITY_EDITOR")]
	public static void VerifyUnityMainThread()
	{
	}

	public static void ErrorWithDialog(string errorMsg)
	{
		LogError(errorMsg);
	}

	public static void ActionOnUnityMainThread(Action action)
	{
		if (action != null)
		{
			if (IsUnityMainThread())
			{
				action();
			}
			else
			{
				SingletonUpdater<Executer>.instance.NextRun(action);
			}
		}
	}

	public static void Update()
	{
	}

	private static Action<object> SelectLog(Action<object> mainLog, Action<object> threadLog)
	{
		if (!IsUnityMainThread())
		{
			return threadLog;
		}
		return mainLog;
	}

	private static void NativeLog(object message)
	{
		UnityEngine.Debug.Log("\n--> " + message);
	}

	private static void NativeLogWarning(object message)
	{
		UnityEngine.Debug.LogWarning("\n--> " + message);
	}

	private static void NativeLogError(object message)
	{
		UnityEngine.Debug.LogError("\n--> " + message);
	}

	private static void ThreadLog(object message)
	{
		string callStackDump = GetCallStackDump(ThreadLogPrefix + message);
		UnityEngine.Debug.Log(callStackDump);
		internalLogCallback(callStackDump, null, LogType.Log);
	}

	private static void ThreadLog(object message, UnityEngine.Object context)
	{
		string callStackDump = GetCallStackDump(ThreadLogPrefix + message);
		UnityEngine.Debug.Log(callStackDump, context);
		internalLogCallback(callStackDump, null, LogType.Log);
	}

	private static void ThreadLogWarning(object message)
	{
		string callStackDump = GetCallStackDump(ThreadLogPrefix + message);
		UnityEngine.Debug.LogWarning(callStackDump);
		internalLogCallback(callStackDump, null, LogType.Warning);
	}

	private static void ThreadLogError(object message)
	{
		string callStackDump = GetCallStackDump(ThreadLogPrefix + message);
		UnityEngine.Debug.LogError(callStackDump);
		internalLogCallback(callStackDump, null, LogType.Error);
	}

	private static void ThreadLogCritical(object message)
	{
		string callStackDump = GetCallStackDump(ThreadLogPrefix + message);
		UnityEngine.Debug.LogError(callStackDump);
		internalLogCallback(callStackDump, null, LogType.Error);
	}

	private static void internalLogCallback(string condition, string stackTrace, LogType type)
	{
		GameConsole.LogCallback(condition, type);
	}

	[DebuggerHidden]
	public static string GetCallStackDump(object message)
	{
		List<CallStackData> callStack = GetCallStack();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(message.ToString());
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(callStack); i++)
		{
			CallStackData callStackData = callStack[i];
			stringBuilder.AppendLine($"{callStackData.MethodName} (at {callStackData.FileName}:{callStackData.LineNumber})");
		}
		return stringBuilder.ToString();
	}

	[DebuggerHidden]
	public static List<CallStackData> GetCallStack()
	{
		List<CallStackData> list = new List<CallStackData>();
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			StackFrame frame = stackTrace.GetFrame(i);
			MethodBase method = frame.GetMethod();
			if (__Extension_Method_ICollection__.COUNT(method.GetCustomAttributes(typeof(DebuggerHiddenAttribute), inherit: true)) > 0)
			{
				continue;
			}
			string fileName = frame.GetFileName();
			if (!String_Extension_Method.IsNullOrEmpty(fileName))
			{
				int num = fileName.IndexOf("/Assets/");
				if (num > 0)
				{
					fileName = fileName.Substring(num + "/Assets/".Length);
				}
				CallStackData callStackData = new CallStackData();
				callStackData.MethodName = $"{method.ReflectedType.Namespace}.{method.ReflectedType.Name}.{method.Name}";
				callStackData.FileName = frame.GetFileName();
				callStackData.LineNumber = frame.GetFileLineNumber();
				list.Add(callStackData);
			}
		}
		return list;
	}
}
