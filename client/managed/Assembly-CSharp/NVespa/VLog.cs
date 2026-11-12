using System;
using System.Diagnostics;
using UnityEngine;

namespace NVespa;

public static class VLog
{
	public enum LogLevel
	{
		Verbose,
		Debug,
		Info,
		Warn,
		Error,
		Fatal
	}

	public static bool IsReady;

	private static Action<string> _logDebug;

	private static Action<string> _logInfo;

	private static Action<string> _logWarn;

	private static Action<string> _logError;

	private static Action<string> _logFatal;

	public static LogLevel MinimumLevel;

	private static void EmptyLog(string message)
	{
	}

	static VLog()
	{
		IsReady = false;
		_logDebug = EmptyLog;
		_logInfo = EmptyLog;
		_logWarn = EmptyLog;
		_logError = EmptyLog;
		_logFatal = EmptyLog;
		MinimumLevel = LogLevel.Debug;
	}

	public static void BindDebug(Action<string> logDebug)
	{
		_logDebug = logDebug;
	}

	public static void BindInfo(Action<string> logInfo)
	{
		_logInfo = logInfo;
	}

	public static void BindWarn(Action<string> logWarn)
	{
		_logWarn = logWarn;
	}

	public static void BindError(Action<string> logError)
	{
		_logError = logError;
	}

	public static void BindFatal(Action<string> logFatal)
	{
		_logFatal = logFatal;
	}

	public static void BindForUnityLog()
	{
		if (!IsReady)
		{
			BindDebug(UnityEngine.Debug.Log);
			BindInfo(UnityEngine.Debug.Log);
			BindWarn(UnityEngine.Debug.LogWarning);
			BindError(UnityEngine.Debug.LogError);
			BindFatal(UnityEngine.Debug.LogError);
			IsReady = true;
		}
	}

	[MustUseReturnValue]
	public static VLogMessage.DebugMessage D(string category)
	{
		return VLogMessage.LogMessage<VLogMessage.DebugMessage>.Init(category);
	}

	[MustUseReturnValue]
	public static VLogMessage.InfoMessage I(string category)
	{
		return VLogMessage.LogMessage<VLogMessage.InfoMessage>.Init(category);
	}

	[MustUseReturnValue]
	public static VLogMessage.WarnMessage W(string category)
	{
		return VLogMessage.LogMessage<VLogMessage.WarnMessage>.Init(category);
	}

	[MustUseReturnValue]
	public static VLogMessage.ErrorMessage E(string category)
	{
		return VLogMessage.LogMessage<VLogMessage.ErrorMessage>.Init(category);
	}

	[MustUseReturnValue]
	public static VLogMessage.FatalMessage F(string category)
	{
		return VLogMessage.LogMessage<VLogMessage.FatalMessage>.Init(category);
	}

	[Conditional("LOG_DEBUG")]
	public static void DF(string category, string format, params object[] args)
	{
		_ = args?.LongLength;
	}

	[Conditional("LOG_INFO")]
	public static void IF(string category, string format, params object[] args)
	{
		_ = args?.LongLength;
	}

	[Conditional("LOG_WARN")]
	public static void WF(string category, string format, params object[] args)
	{
		_ = args?.LongLength;
	}

	[Conditional("LOG_ERROR")]
	public static void EF(string category, string format, params object[] args)
	{
		_ = args?.LongLength;
	}

	[Conditional("LOG_FATAL")]
	public static void FF(string category, string format, params object[] args)
	{
		_ = args?.LongLength;
	}

	public static void Print(VLogMessage.ILogMessage builder)
	{
		if (builder != null)
		{
			string obj = builder.ToString();
			switch (builder.LogLevel)
			{
			case LogLevel.Debug:
				_logDebug(obj);
				break;
			case LogLevel.Info:
				_logInfo(obj);
				break;
			case LogLevel.Warn:
				_logWarn(obj);
				break;
			case LogLevel.Error:
				_logError(obj);
				break;
			case LogLevel.Fatal:
				_logFatal(obj);
				break;
			}
		}
	}
}
