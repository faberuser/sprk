using System.IO;
using NGameAsset.NPatch;
using NVespa;

namespace NGameAsset.NBundleManager;

public static class BundleHelper
{
	public static string GetBundlePath(string fileName, string platformName, string folder, bool checkExist)
	{
		if (DefinePlatform.IsEditor && Define.LocalAssetLoading.Value)
		{
			return GetBundlePathFromLocal(fileName, platformName, folder, checkExist);
		}
		return GetBundlePathFromPatch(fileName, folder, checkExist);
	}

	public static string GetBundlePathFromLocal(string fileName, string platformName, string folder, bool checkExist)
	{
		string text = ((!String_Extension_Method.IsNullOrEmpty(folder)) ? DefinePlatform.GetLocalAssetBundlePath(String_Extension_Method.CombinePath(folder, fileName), platformName) : DefinePlatform.GetLocalAssetBundlePath(fileName, platformName));
		if (checkExist && !File.Exists(text))
		{
			return string.Empty;
		}
		return text;
	}

	public static void SetBundlePathModifier()
	{
	}

	public static string GetBundlePathFromPatch(string fileName, string folder, bool checkExist)
	{
		string text = "Assetbundle";
		if (!String_Extension_Method.IsNullOrEmpty(folder))
		{
			text = String_Extension_Method.CombinePath(text, folder);
		}
		text = String_Extension_Method.CombinePath(text, fileName);
		if (checkExist && !SingletonAuto<AssetFileManager>.instance.DownloadedExist(text))
		{
			return string.Empty;
		}
		return PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, text);
	}
}
