using System.IO;
using UnityEngine;

namespace NGameAsset;

public static class DefinePlatform
{
	public const string AndroidPlatformName = "Android";

	public const string iOSPlatformName = "iOS";

	public const string UnknownPlatformName = "Unknown";

	public static bool IsEditor;

	public static string PlatformName;

	public static string AssetBundleRoot;

	public static string PlainAssetRoot;

	public static string VideoRoot;

	public static string StandalonePlatformName;

	static DefinePlatform()
	{
		StandalonePlatformName = "StandaloneWindows";
		IsEditor = false;
		PlatformName = StandalonePlatformName;
		PlatformName = StandalonePlatformName;
		AssetBundleRoot = String_Extension_Method.CombinePath(Application.dataPath, "../../../../../r1_asset/98. AssetBundle/Data");
		PlainAssetRoot = String_Extension_Method.CombinePath(Application.dataPath, "../../../../../r1_asset/97. PlainAsset");
		VideoRoot = String_Extension_Method.CombinePath(Application.dataPath, "../../../../../r1_asset/00. Video/Mid");
	}

	public static string GetLocalVideoPath(string name)
	{
		return String_Extension_Method.CombinePath(VideoRoot, name);
	}

	public static string GetLocalAssetBundlePath(string name)
	{
		return GetLocalAssetBundlePath(name, PlatformName);
	}

	public static string GetLocalAssetBundlePath(string name, string platformName)
	{
		return String_Extension_Method.CombinePath(String_Extension_Method.CombinePath(AssetBundleRoot, platformName), name);
	}

	public static string GetLocalPlainAssetPath(string name)
	{
		return GetLocalPlainAssetPath(name, PlatformName);
	}

	public static string GetLocalPlainAssetPath(string name, string platformName)
	{
		return String_Extension_Method.CombinePath(PlainAssetRoot, name);
	}

	public static string GetLocalDataPath(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		if (name.Contains(":\\") || name.Contains(":/"))
		{
			return name.Replace('/', Path.DirectorySeparatorChar);
		}
		string path = ("../../" + name).Replace('/', Path.DirectorySeparatorChar);
		return Path.Combine(Directory.GetCurrentDirectory(), path);
	}

	public static string GetTableRootPath(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		if (name.Contains(":\\") || name.Contains(":/"))
		{
			return name.Replace('/', Path.DirectorySeparatorChar);
		}
		string path = ("../../../../Table/" + name).Replace('/', Path.DirectorySeparatorChar);
		return Path.Combine(Directory.GetCurrentDirectory(), path);
	}
}
