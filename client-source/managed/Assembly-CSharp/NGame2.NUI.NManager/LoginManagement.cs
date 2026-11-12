using System;
using NGame2.NConfig;
using NGame2.NLogin;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NManager;

public class LoginManagement : BaseManagement<LoginManagement>
{
	private static bool _isNowSignOutProgressed = false;

	public static LocalStorageBool BIOncePlayed = new LocalStorageBool(i_default: false, "BIOncePlayed_0525");

	public static bool IsNowSignOutProgressed
	{
		get
		{
			return _isNowSignOutProgressed;
		}
		set
		{
			_isNowSignOutProgressed = value;
		}
	}

	public static ClientConfig ClientConfig => SingletonAuto<Config>.instance.ClientConfig;

	protected override void Awake()
	{
		base.Awake();
		SingletonAuto<LocalizationManager>.CreateInstance();
		bool flag = false;
		ConstantDataContainer constantDataContainer = SingletonAuto<TableDataManager>.instance.LoadConstantTableInResource();
		if (constantDataContainer != null)
		{
			flag = constantDataContainer.GetValueBool("UseLoginNightPref");
		}
		TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
		bool isNightTime = flag && (timeOfDay < TimeSpan.FromHours(6.0) || timeOfDay > TimeSpan.FromHours(18.0));
		AddInstance<LoginBackground>(GetDayOrNightPrefabPath("Resources/Prefab2/Window/Login/LoginBackground", isNightTime));
		AddInstance<LoginTouchWait>(GetDayOrNightPrefabPath("Resources/Prefab2/Window/Login/LoginTouchWait", isNightTime));
		AddInstance<LoginAccountInfo>(GetDayOrNightPrefabPath("Resources/Prefab2/Window/Login/LoginAccountInfo", isNightTime));
		AddInstance<LoginPatchDownloader>("Resources/Prefab2/Window/Login/LoginPatchDownloader");
		AddInstance<LoginAgreement>("Resources/Prefab2/Window/Login/LoginAgreement");
		AddInstance<LoginSelect>("Resources/Prefab2/Window/Login/LoginSelect");
		AddInstance<ServerSelect>("Resources/Prefab2/Window/Login/ServerSelect");
		AddInstance<LoginCreateNick>("Resources/Prefab2/Window/Login/LoginCreateNick");
		AddInstance<CountrySelect>("Resources/Prefab2/Window/Login/CountrySelect");
		AddInstance<LoginCouponSelect>("Resources/Prefab2/Window/Login/LoginCouponSelect");
		AddInstance<LoginUseCouponPopup>("Resources/Prefab2/Window/Login/LoginUseCouponPopup");
		AddInstance<LoginPatchNote>(GetDayOrNightPrefabPath("Resources/Prefab2/Window/Login/LoginPatchNote", isNightTime));
		AddInstance<LoginPatchNotePopup>("Resources/Prefab2/Window/Login/LoginPatchNotePopup");
		AddInstance<DownloadWarningPopup>("Resources/Prefab2/Window/Login/DownloadWarningPopup");
		AddInstance<MemoryWarningPopup>("Resources/Prefab2/Window/Login/MemoryWarningPopup");
		AddInstance<SelectLanguage>("Resources/Prefab2/Window/GameOption/SelectLanguage");
		AddInstance<SelectVoiceLanguage>("Resources/Prefab2/Window/GameOption/SelectVoiceLanguage");
		AddInstance<LoginIntroduceHero>("Resources/Prefab2/Window/Login/LoginIntroduceHero");
		AddInstance<LoginIntroduceHeroPopup>("Resources/Prefab2/Window/Login/LoginIntroduceheroPopup");
		AddInstance<LoginIntroduceWorldPopup>("Resources/Prefab2/Window/Login/LoginIntroduceWorldPopup");
		AddInstance<PatchValidation>("Resources/Prefab2/Window/Login/PatchValidation");
	}

	private string GetDayOrNightPrefabPath(string path, bool isNightTime)
	{
		if (!isNightTime)
		{
			return path;
		}
		return path + "_Night";
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public bool IsBIOncePlayed()
	{
		if (!BIOncePlayed.Value)
		{
			return LoginBI.IsLoginBINull;
		}
		return true;
	}

	public void Open(string forceLoginId)
	{
		SingletonUpdater<LoginManager>.instance.Login(forceLoginId);
	}

	public static string GetProviderId(LoginMethodType loginMethod)
	{
		return string.Empty;
	}

	public static bool IsUsedTokenSignIn(LoginMethodType loginMethod)
	{
		if ((uint)(loginMethod - 5) <= 2u)
		{
			return true;
		}
		return false;
	}
}
