using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace NVespa;

public class UnityStartup
{
	private static int Number;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void RuntimeInit_BeforeSceneLoad()
	{
		Application.focusChanged += UnityCallback.focusChanged;
		Application.lowMemory += UnityCallback.lowMemory;
		Application.wantsToQuit += UnityCallback.wantsToQuit;
		Application.quitting += UnityCallback.quitting;
		Application.logMessageReceivedThreaded += UnityCallback.logMessageReceivedThreaded;
		UnityCache.SetPlayMode(UnityCache.EPlayMode.Playing);
		UnityCallback.RuntimeInit_BeforeSceneLoad();
		SystemLog($"UnityStartup.{MethodBase.GetCurrentMethod().Name}");
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void RuntimeInit_AfterSceneLoad()
	{
		SystemLog($"UnityStartup.{MethodBase.GetCurrentMethod().Name}");
	}

	public static void SystemLog(string msg)
	{
	}

	public static void TimingLog(string msg)
	{
	}

	public static string GetTimeString()
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
		return $"{timeSpan:hh\\:mm\\:ss\\.ff}";
	}

	[Conditional("ENABLE_UNITYSTARTUP_LOG")]
	private static void logInternal(string msg)
	{
		UnityEngine.Debug.LogError($"#UnitySystem# ({GetTimeString()}) {msg}\n" + $"\t\t-({Number,3}) isPlaying={Application.isPlaying,5}, ");
		Number++;
	}
}
