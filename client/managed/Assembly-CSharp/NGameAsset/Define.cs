using NVespa;
using UnityEngine;

namespace NGameAsset;

public static class Define
{
	public const bool Test_ForceLoadAll_JsonRead = false;

	public const int DestroyDelayTick = 10000;

	public const int LowSpecMemoryLimit = 2200;

	private static bool _patchDownloaded;

	public static LocalStorageBool AutoBanPick;

	public static LocalStorageBool TestMiniGame;

	public static LocalStorageBool PreloadBundle;

	public static LocalStorageBool ImproveLogin;

	public static LocalStorageBool FullDownloadUser;

	public static LocalStorageBool TableSingleLoad;

	public static LocalStorageBool ReuseAssetInstance;

	public static LocalStorageBool LocalAssetLoading;

	public const string BundlePackBinaryFileName = "BundlePack.bin";

	public const string ShaderBundleFileName = "Unity_2018/Shader.unity3d";

	public const string OriginalSpecFolder = "/OriginalSpec/";

	public const string MidSpecFolder = "/MidSpec/";

	public const string LowSpecFolder = "/LowSpec/";

	public const string MidAndLowSpecFolder = "/MidAndLowSpec/";

	public const string HighAndMidSpecFolder = "/HighAndMidSpec/";

	public const string LowLargeSpecFolder = "/LowLargeSpec/";

	public const string VideoFolder = "Video/";

	public const string VideoOriginSpecFolder = "Video/Origin/";

	public const string VideoHighSpecFolder = "Video/High/";

	public const string VideoMidSpecFolder = "Video/Mid/";

	public const string VideoLowSpecFolder = "Video/Low/";

	public const string MainAssetJson_Name = "MainAsset_Json";

	public const string ExportJson_Name = "ExportJson";

	public const string SoundTableJson_Name = "SoundTable_Json";

	public const string BgmTableJson_Name = "BgmTable_Json";

	public const string CameraDataJson_Name = "CameraData_Json";

	public const string MainAsset_Name = "MainAsset";

	public const string DefaultTableDataFolder = "DefaultTableData/";

	public const string AgreementsFolder = "Agreement/";

	public const string DefaultAgreementsFolder = "DefaultTableData/DefaultAgreements/";

	public const string DefaultPurchasePolicyFolder = "DefaultTableData/DefaultPurchasePolicy/";

	public static bool PatchDownloaded
	{
		get
		{
			return _patchDownloaded;
		}
		set
		{
			_patchDownloaded = value;
		}
	}

	static Define()
	{
		_patchDownloaded = false;
		AutoBanPick = new LocalStorageBool(i_default: false, typeof(Define).FullName + "AutoBanPick");
		TestMiniGame = new LocalStorageBool(i_default: false, typeof(Define).FullName + "TestMiniGame");
		PreloadBundle = new LocalStorageBool(i_default: true, typeof(Define).FullName + "PreloadBundle");
		ImproveLogin = new LocalStorageBool(i_default: false, typeof(Define).FullName + "ImproveLogin");
		FullDownloadUser = new LocalStorageBool(i_default: false, typeof(Define).FullName + "FullDownloadUser");
		TableSingleLoad = new LocalStorageBool(i_default: false, typeof(Define).FullName + "TableSingleLoad");
		ReuseAssetInstance = new LocalStorageBool(i_default: true, typeof(Define).FullName + "ReuseAssetInstance");
		LocalAssetLoading = new LocalStorageBool(i_default: false, typeof(Define).FullName + "_LocalAssetLoading2");
	}

	public static string GetLanguageDir(SystemLanguage language)
	{
		return $"[{Vespa_String_Extension_Method.GetEnumString(language)}]/";
	}

	public static bool PreloadBundleValue()
	{
		return true;
	}

	public static bool ImproveLoginValue()
	{
		return true;
	}

	public static bool IsLiveBuild()
	{
		return true;
	}
}
