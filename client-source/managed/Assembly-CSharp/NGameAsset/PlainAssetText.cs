using System;
using NGameAsset.NPatch;
using NVespa;
using UnityEngine;

namespace NGameAsset;

public class PlainAssetText
{
	public enum EExtensionType
	{
		txt,
		json,
		csv
	}

	private UnityEngine.Object _owner;

	private Action<string> _onComplete;

	public string Text
	{
		get
		{
			if (AssetLoad == null || !AssetLoad.IsValid)
			{
				return null;
			}
			return AssetLoad.Text;
		}
	}

	public IPlainAssetLoad2 AssetLoad { get; private set; }

	public void SetCallback(UnityEngine.Object owner, Action<string> onComplete)
	{
		if (AssetLoad != null && AssetLoad.IsDone)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, Text);
			return;
		}
		_owner = owner;
		_onComplete = onComplete;
	}

	public void Open(string path, EExtensionType extType, bool reload = false)
	{
		string assetPath = GetAssetPath(path, extType);
		AssetLoad = SingletonUpdater<PlainAssetManager>.instance.LockAsset(assetPath, reload);
		AsyncManager.SingleEntry singleEntry = new AsyncManager.SingleEntry();
		singleEntry.AsyncLoad = AssetLoad;
		singleEntry.OnComplete = OnCompleteAsset;
		SingletonUpdater<AsyncManager>.instance.Add(singleEntry);
	}

	public void Close()
	{
		_owner = null;
		_onComplete = null;
		if (AssetLoad != null && SingletonUpdater<PlainAssetManager>.isValidInstance)
		{
			PlainAssetManager.ReleaseAsset(AssetLoad, destroyImmediately: false);
			AssetLoad = null;
		}
	}

	private void OnCompleteAsset()
	{
		if (_onComplete != null)
		{
			if (AssetLoad != null)
			{
				_ = AssetLoad.IsValid;
			}
			if (_owner != null && _onComplete != null)
			{
				Vespa_System_Action_Extension_Method.fire(_onComplete, Text);
			}
			_owner = null;
			_onComplete = null;
		}
	}

	public static bool IsExists(string path, EExtensionType extType)
	{
		string assetPath = GetAssetPath(path, extType);
		return SingletonAuto<AssetFileManager>.instance.ExistInAssetInfos(assetPath);
	}

	public static string GetAssetPath(string path, EExtensionType extType)
	{
		string rhs = String_Extension_Method.CombinePath("Text", path);
		return ValidateExtension(PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, String_Extension_Method.CombinePath("PlainAsset", rhs)), extType);
	}

	public static string ValidateExtension(string assetPath, EExtensionType extType)
	{
		if (assetPath.EndsWith(EnumTools.GetName(extType), StringComparison.OrdinalIgnoreCase))
		{
			return assetPath;
		}
		return assetPath + "." + EnumTools.GetName(extType);
	}

	public static bool IsValidExtension(string extension)
	{
		string[] array = new string[2] { ".txt", ".json" };
		for (int i = 0; i < array.Length; i++)
		{
			if (string.Equals(array[i], extension, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
