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
		// SPRK: parse the title background while the first scenes load.
		SprkLoginPresentation.Preload();
		SprkAndroidScreen();
		SystemLog($"UnityStartup.{MethodBase.GetCurrentMethod().Name}");
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void RuntimeInit_AfterSceneLoad()
	{
		SystemLog($"UnityStartup.{MethodBase.GetCurrentMethod().Name}");
	}

	// SPRK: on Android, follow the device's auto-rotate between both landscape sides,
	// and keep the screen on from the start: the game applies its own sleep option
	// (GameOptionData.ApplySleepMode) only from the first scene load, so the login screen
	// and its patch download fell asleep after the phone's screen timeout.
	// APKs built before SprkAndroidBuild allowed rotation request a fixed landscape, and
	// Unity keeps that for Screen.orientation = AutoRotation, so the activity is asked for
	// USER_LANDSCAPE directly (both sides; respects the rotation lock), again on every resume.
	private static void SprkAndroidScreen()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		RequestUserLandscape();
		// Unity applies its own orientation again when the app returns from the background.
		Application.focusChanged += delegate(bool focused)
		{
			if (focused)
			{
				RequestUserLandscape();
			}
		};
	}

	private static void RequestUserLandscape()
	{
		try
		{
			AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
			activity.Call("runOnUiThread", new AndroidJavaRunnable(delegate
			{
				activity.Call("setRequestedOrientation", 11);
			}));
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("SPRK: landscape rotation unavailable: " + ex.Message);
		}
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
