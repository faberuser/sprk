using System;
using System.IO;
using NGameAsset.NAsyncLoad;
using NGameAsset.NPatch;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset.NBundleManager;

public abstract class BundleLoadBase : IBundleLoad, IAsyncLoadBase
{
	public class LoadResult
	{
		public string Error;

		public AssetBundle AssetBundle;
	}

	public delegate void LoadAsync(string path, Action<LoadResult> onLoad);

	public enum PriorityDownloadState
	{
		None,
		Success,
		NotFoundList,
		Failed
	}

	public static int CreateCount;

	public static int StartLoadCount;

	public static int CompleteCount;

	public string PlatformName;

	public string FileName;

	public bool IsDone;

	public PriorityDownloadState _DownloadState;

	public string Error;

	public AssetBundle AssetBundle;

	public string SpecFolder = string.Empty;

	private LockCounter _lockCounter = new LockCounter();

	string IAsyncLoadBase.KeyDesc => FileName;

	bool IAsyncLoadBase.IsDone => IsDone;

	bool IAsyncLoadBase.IsValid
	{
		get
		{
			if (!IsDone)
			{
				return false;
			}
			if (!String_Extension_Method.IsNullOrEmpty(Error))
			{
				return false;
			}
			return AssetBundle != null;
		}
	}

	string IAsyncLoadBase.LoadMessage => Error;

	string IBundleLoad.BundleFileName => FileName;

	ILockCounter IBundleLoad.Lock => _lockCounter;

	public BundleLoadBase()
	{
		CreateCount++;
	}

	public void Open(string fileName)
	{
		Open(fileName, DefinePlatform.PlatformName);
	}

	public void Open(string fileName, string platformName)
	{
		FileName = fileName;
		PlatformName = platformName;
	}

	public void SetSpecFolder()
	{
		SpecFolder = string.Empty;
		if (SystemOptionData.Spec == ESpecType.Low)
		{
			if (Exists(splitLanguage: false, "/LowSpec/", checkExist: false))
			{
				SpecFolder = "/LowSpec/";
			}
			else if (Exists(splitLanguage: false, "/MidAndLowSpec/", checkExist: false))
			{
				SpecFolder = "/MidAndLowSpec/";
			}
			else if (Exists(splitLanguage: false, "/LowLargeSpec/", checkExist: false))
			{
				SpecFolder = "/LowLargeSpec/";
			}
		}
		else if (SystemOptionData.Spec == ESpecType.Mid)
		{
			if (Exists(splitLanguage: false, "/MidSpec/", checkExist: false))
			{
				SpecFolder = "/MidSpec/";
			}
			else if (Exists(splitLanguage: false, "/MidAndLowSpec/", checkExist: false))
			{
				SpecFolder = "/MidAndLowSpec/";
			}
			else if (Exists(splitLanguage: false, "/HighAndMidSpec/", checkExist: false))
			{
				SpecFolder = "/HighAndMidSpec/";
			}
		}
		else if (Exists(splitLanguage: false, "/OriginalSpec/", checkExist: false))
		{
			SpecFolder = "/MidSpec/";
		}
		else if (Exists(splitLanguage: false, "/HighAndMidSpec/", checkExist: false))
		{
			SpecFolder = "/MidAndLowSpec/";
		}
	}

	protected string GetBundlePath()
	{
		BundleInfoContainer infoContainer = SingletonAuto<BundleManager>.instance.GetInfoContainer();
		if (infoContainer == null)
		{
			return GetBundlePath(splitLanguage: false, SpecFolder, checkExist: false);
		}
		BundleFileInfo info = infoContainer.GetInfo(FileName);
		if (info == null)
		{
			return GetBundlePath(splitLanguage: false, SpecFolder, checkExist: false);
		}
		if (!info.ContainsSpec)
		{
			return GetBundlePath(info.SplitLanguage, SpecFolder, checkExist: false);
		}
		string bundlePath;
		if (SystemOptionData.Spec == ESpecType.Low)
		{
			bundlePath = GetBundlePath(info.SplitLanguage, "/LowSpec/", checkExist: true);
			if (!String_Extension_Method.IsNullOrEmpty(bundlePath))
			{
				return bundlePath;
			}
			bundlePath = GetBundlePath(info.SplitLanguage, "/MidAndLowSpec/", checkExist: true);
			if (!String_Extension_Method.IsNullOrEmpty(bundlePath))
			{
				return bundlePath;
			}
			bundlePath = GetBundlePath(info.SplitLanguage, "/LowLargeSpec/", checkExist: true);
			if (!String_Extension_Method.IsNullOrEmpty(bundlePath))
			{
				return bundlePath;
			}
		}
		else if (SystemOptionData.Spec == ESpecType.Mid)
		{
			bundlePath = GetBundlePath(info.SplitLanguage, "/MidSpec/", checkExist: true);
			if (!String_Extension_Method.IsNullOrEmpty(bundlePath))
			{
				return bundlePath;
			}
			bundlePath = GetBundlePath(info.SplitLanguage, "/MidAndLowSpec/", checkExist: true);
			if (!String_Extension_Method.IsNullOrEmpty(bundlePath))
			{
				return bundlePath;
			}
			bundlePath = GetBundlePath(info.SplitLanguage, "/HighAndMidSpec/", checkExist: true);
			if (!String_Extension_Method.IsNullOrEmpty(bundlePath))
			{
				return bundlePath;
			}
		}
		bundlePath = GetBundlePath(info.SplitLanguage, "/OriginalSpec/", checkExist: true);
		if (!String_Extension_Method.IsNullOrEmpty(bundlePath))
		{
			return bundlePath;
		}
		bundlePath = GetBundlePath(info.SplitLanguage, "/HighAndMidSpec/", checkExist: true);
		if (!String_Extension_Method.IsNullOrEmpty(bundlePath))
		{
			return bundlePath;
		}
		return GetBundlePath(info.SplitLanguage, SpecFolder, checkExist: false);
	}

	protected string GetBundlePath(bool splitLanguage, string folder, bool checkExist)
	{
		if (splitLanguage)
		{
			folder = String_Extension_Method.CombinePath(Define.GetLanguageDir(SingletonAuto<LocalizationManager>.instance.Language), folder);
		}
		return BundleHelper.GetBundlePath(FileName, PlatformName, folder, checkExist);
	}

	public bool Exists(bool splitLanguage, string folder, bool checkExist)
	{
		string bundlePath = GetBundlePath(splitLanguage, folder, checkExist);
		bool result = false;
		if (File.Exists(bundlePath))
		{
			result = true;
		}
		return result;
	}

	bool IBundleLoad.Exists()
	{
		string bundlePath = GetBundlePath();
		bool result = false;
		if (File.Exists(bundlePath))
		{
			result = true;
		}
		return result;
	}

	bool IBundleLoad.IsDownloaded()
	{
		bool result = false;
		switch (_DownloadState)
		{
		case PriorityDownloadState.Success:
			if (this == null || ((IBundleLoad)this).Exists())
			{
				result = true;
			}
			break;
		case PriorityDownloadState.NotFoundList:
		case PriorityDownloadState.Failed:
			result = true;
			break;
		default:
			result = false;
			break;
		}
		return result;
	}

	void IBundleLoad.StartDownload()
	{
		_DownloadState = PriorityDownloadState.None;
		if (GlobalAutoBehaviour<PatchDownloader>.instance != null)
		{
			if (GlobalAutoBehaviour<PatchDownloader>.instance.IsPatchDownloading())
			{
				GlobalAutoBehaviour<PatchDownloader>.instance.PriorityBundle(FileName, delegate
				{
					_DownloadState = PriorityDownloadState.Success;
				});
			}
			else
			{
				_DownloadState = PriorityDownloadState.NotFoundList;
			}
		}
		else
		{
			_DownloadState = PriorityDownloadState.Failed;
		}
		PriorityDownloadState downloadState = _DownloadState;
		if ((uint)(downloadState - 2) <= 1u)
		{
			VespaDebug.LogError($"BundleLoadBase Failed BundleDownload, {_DownloadState.ToString()}");
		}
	}

	void IBundleLoad.StartLoad()
	{
		StartLoadInternal();
	}

	private void StartLoadInternal()
	{
		StartLoadCount++;
		string bundlePath = GetBundlePath();
		if (SingletonUpdater<AssetCacheService>.instance.GetCachedAssetBundle(FileName, out AssetBundle))
		{
			CompleteCount++;
			IsDone = true;
			return;
		}
		Action<LoadResult> onResult = delegate(LoadResult result)
		{
			AssetBundle = result.AssetBundle;
			Error = result.Error;
			if (!String_Extension_Method.IsNullOrEmpty(Error))
			{
				Debug.LogError("Bundle Load Fail: " + bundlePath + " error=" + Error);
			}
			CompleteCount++;
			IsDone = true;
			if (AssetBundle != null)
			{
				SingletonUpdater<AssetCacheService>.instance.CacheAssetBundle(FileName, AssetBundle);
			}
		};
		Load(bundlePath, delegate(LoadResult result)
		{
			Vespa_System_Action_Extension_Method.fire(onResult, result);
		});
	}

	UnityEngine.Object IBundleLoad.Load(string assetName, Type type)
	{
		return AssetBundle.LoadAsset(assetName, type);
	}

	IRequestWrapper IBundleLoad.LoadAsync(string assetName, Type type)
	{
		return new EnableAsyncWrapper(AssetBundle, assetName, type);
	}

	IRequestWrapper IBundleLoad.LoadMainAsset()
	{
		return MainAssetWrapper.LoadMainAsset(AssetBundle);
	}

	UnityEngine.Object[] IBundleLoad.LoadAll()
	{
		return AssetBundle.LoadAllAssets();
	}

	void IBundleLoad.Close(bool unloadAllLoadedObjects)
	{
		SingletonUpdater<AssetCacheService>.instance.MarkBundleToBeDisposed(FileName);
		OnClose();
	}

	protected virtual void OnClose()
	{
	}

	public abstract void Load(string bundlePath, Action<LoadResult> onLoad);
}
