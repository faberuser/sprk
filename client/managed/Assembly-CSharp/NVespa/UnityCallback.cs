using System.Reflection;
using UnityEngine;

namespace NVespa;

public static class UnityCallback
{
	public static void focusChanged(bool hasFocus)
	{
		UnityStartup.SystemLog($"UnityCallback.{MethodBase.GetCurrentMethod().Name}");
	}

	public static void lowMemory()
	{
		UnityStartup.SystemLog($"UnityCallback.{MethodBase.GetCurrentMethod().Name}");
	}

	public static bool wantsToQuit()
	{
		UnityStartup.SystemLog($"UnityCallback.{MethodBase.GetCurrentMethod().Name}");
		return true;
	}

	public static void quitting()
	{
		UnityStartup.SystemLog($"UnityCallback.{MethodBase.GetCurrentMethod().Name}");
		UnityCache.SetPlayMode(UnityCache.EPlayMode.Quitting);
	}

	public static void logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
	{
		UnityLogQueue.Enqueue(condition, stackTrace, type);
	}

	public static void RuntimeInit_BeforeSceneLoad()
	{
	}
}
