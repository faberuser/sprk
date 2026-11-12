using System.IO;
using System.Threading;
using NGameAsset;
using NShared;
using UnityEngine;

namespace NVespa;

public static class UnityCache
{
	public enum EPlayMode
	{
		EditMode,
		WillPlay,
		Playing,
		Quitting
	}

	public enum EMemorySpec
	{
		Low,
		Mid,
		High
	}

	private static int _mainThreadId;

	public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

	public static EPlayMode PlayMode { get; private set; }

	public static bool isGamePlaying { get; private set; }

	public static bool canCreateObject { get; private set; }

	public static bool isQuiting { get; private set; }

	public static string temporaryCachePath { get; private set; }

	public static string persistentDataPath { get; private set; }

	public static string identifier { get; private set; }

	public static string streamingAssetsPath { get; private set; }

	public static string dataPath { get; private set; }

	public static SystemLanguage systemLanguage { get; private set; }

	public static RuntimePlatform platform { get; private set; }

	public static bool isEditor { get; private set; }

	public static bool Application_isPlaying { get; private set; }

	public static bool isPlayingOrWillChangePlaymode { get; private set; }

	public static bool isCompiling { get; private set; }

	public static int SystemMemorySize { get; private set; }

	public static EMemorySpec MemorySpec => SpecByMemorySize(SystemMemorySize);

	public static string Clipboard
	{
		get
		{
			return GUIUtility.systemCopyBuffer;
		}
		set
		{
			GUIUtility.systemCopyBuffer = value;
		}
	}

	public static EMemorySpec SpecByMemorySize(int sizeInMb)
	{
		if (sizeInMb < 1500)
		{
			return EMemorySpec.Low;
		}
		if (sizeInMb < 2500)
		{
			return EMemorySpec.Mid;
		}
		return EMemorySpec.High;
	}

	public static void SetPlayMode(EPlayMode mode)
	{
		UnityStartup.SystemLog($"SetPlayMode: {PlayMode} => {mode}");
		RefreshApplicationData();
		PlayMode = mode;
		switch (PlayMode)
		{
		case EPlayMode.EditMode:
			isGamePlaying = false;
			canCreateObject = true;
			isQuiting = false;
			break;
		case EPlayMode.WillPlay:
			isGamePlaying = true;
			canCreateObject = true;
			isQuiting = false;
			break;
		case EPlayMode.Playing:
			isGamePlaying = true;
			canCreateObject = true;
			isQuiting = false;
			break;
		case EPlayMode.Quitting:
			isGamePlaying = false;
			canCreateObject = false;
			isQuiting = true;
			break;
		default:
			isGamePlaying = false;
			canCreateObject = false;
			isQuiting = false;
			break;
		}
	}

	public static void RefreshApplicationData()
	{
		_mainThreadId = Thread.CurrentThread.ManagedThreadId;
		temporaryCachePath = Application.temporaryCachePath;
		persistentDataPath = Application.dataPath;
		identifier = Application.identifier;
		streamingAssetsPath = Application.streamingAssetsPath;
		dataPath = Application.dataPath;
		systemLanguage = Application.systemLanguage;
		platform = Application.platform;
		isEditor = Application.isEditor;
		Application_isPlaying = true;
		isPlayingOrWillChangePlaymode = true;
		isCompiling = false;
		SystemMemorySize = SystemInfo.systemMemorySize;
		MessagePackInitializer.RegisterResolver();
		SetupSharedGlobalPathDefine();
	}

	private static void SetupSharedGlobalPathDefine()
	{
		GlobalPathDefine.Unity_Assets = dataPath;
		GlobalPathDefine.Unity_Assets_StreamingAssets = streamingAssetsPath;
		SetupSharedGlobalPathDefine_in_Mobile();
	}

	private static void SetupSharedGlobalPathDefine_in_Editor()
	{
		string fullPath = Path.GetFullPath(String_Extension_Method.CombinePath(dataPath, "../../../../../Table"));
		GlobalPathDefine.TableData = String_Extension_Method.CombinePath(fullPath, "TableData");
		GlobalPathDefine.TableJit = String_Extension_Method.CombinePath(fullPath, "TableJit");
		GlobalPathDefine.TableData_ContentsDefine = String_Extension_Method.CombinePath(fullPath, "TableData/ContentsDefine");
		string text = (GlobalPathDefine.LocalizationRepository_in_Editor = Path.GetFullPath(String_Extension_Method.CombinePath(dataPath, "../../../../../Localization")));
		GlobalPathDefine.Localization_TableJit = String_Extension_Method.CombinePath(text, "TableJit");
		GlobalPathDefine.Localization_BasicJit = String_Extension_Method.CombinePath(text, "BasicJit");
		SetupSharedGlobalPathDefine_in_Mobile();
	}

	private static void SetupSharedGlobalPathDefine_in_Mobile()
	{
		GlobalPathDefine.TableData = GetDownloadedAssetPath("TableData");
		GlobalPathDefine.TableJit = GetDownloadedAssetPath("TableJit");
		GlobalPathDefine.TableData_ContentsDefine = GetDownloadedAssetPath("ContentsDefine");
		GlobalPathDefine.Localization_TableJit = GetDownloadedAssetPath("LocalizationJit");
		GlobalPathDefine.Localization_BasicJit = GetDownloadedAssetPath("LocalizationBasicJit");
	}

	public static string GetDownloadedAssetPath(string subPath)
	{
		string text = String_Extension_Method.CombinePath(persistentDataPath, "Documents");
		return String_Extension_Method.CombinePath(String_Extension_Method.CombinePath(rhs: DefinePlatform.PlatformName, This: String_Extension_Method.CombinePath(text, "Patch")), subPath).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
	}

	public static bool CanCreateSingletonBehaviour<T>()
	{
		if (!canCreateObject)
		{
			VespaDebug.LogError("instance 생성 금지 타이밍: " + typeof(T).FullName);
			return false;
		}
		if (!IsMainThread)
		{
			VespaDebug.LogError("Unity Main Thread에서만 생성 가능: " + typeof(T).FullName);
			return false;
		}
		return true;
	}
}
