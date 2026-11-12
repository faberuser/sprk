using System;
using NGame2.NAccount;
using NGame2.NConfig;
using NGame2.NDevice;
using NGame2.NLogin;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OqupieWebViewPopup : BaseWindow, IGlobalWindow, WebViewComponent.IListener
{
	private static readonly string BaseUrl = "https://vespainc.oqupie.com/portals/finder?access_key=da8cad4a4a7be05c&secret_key=8cae2c470e9d3e79721db88ec7eb939d";

	private static readonly string PrefabPath = "Resources/Prefab2/Window/OqupieWebViewPopup";

	private static readonly string BrandKey1 = "brand_key1";

	private static GlobalWindowPrefab<OqupieWebViewPopup> _windowPrefab = new GlobalWindowPrefab<OqupieWebViewPopup>(PrefabPath, GlobalUIRoot.ELayerType.GlobalUI);

	public WebViewComponent WebView;

	private string _url;

	private string _urlParameter;

	private Action _closeAction;

	private static SystemLanguage[] _supportLanguageList = new SystemLanguage[5]
	{
		SystemLanguage.Korean,
		SystemLanguage.Japanese,
		SystemLanguage.Chinese,
		SystemLanguage.Vietnamese,
		SystemLanguage.Thai
	};

	public static void OpenPopup(Action closeAction = null)
	{
		_windowPrefab.instance.OpenOqupie(closeAction);
	}

	public static void ClosePopup()
	{
		if (_windowPrefab.IsOpen())
		{
			_windowPrefab.CloseWindow();
		}
	}

	public static void OnSceneChanged()
	{
		if (!_windowPrefab.IsOpen())
		{
			_windowPrefab.ReleaseWindow();
			_windowPrefab.ReleasePrefab();
		}
	}

	private void AddOqupieParameter(string name, string value)
	{
		if (String_Extension_Method.IsNullOrEmpty(value))
		{
			value = "None";
		}
		_urlParameter += $"&{name}={value}";
	}

	private bool IsSupportedLanauage(SystemLanguage language)
	{
		SystemLanguage[] supportLanguageList = _supportLanguageList;
		for (int i = 0; i < supportLanguageList.Length; i++)
		{
			if (supportLanguageList[i] == language)
			{
				return true;
			}
		}
		return false;
	}

	public void OpenOqupie(Action closeAction)
	{
		_closeAction = closeAction;
		SystemLanguage systemLanguage = (SingletonAuto<LocalizationManager>.isValidInstance ? SingletonAuto<LocalizationManager>.instance.Language : SystemLanguage.English);
		SystemLanguage systemLanguage2 = systemLanguage;
		if (systemLanguage == SystemLanguage.ChineseSimplified || systemLanguage == SystemLanguage.ChineseTraditional)
		{
			systemLanguage = SystemLanguage.Chinese;
		}
		if (!IsSupportedLanauage(systemLanguage))
		{
			systemLanguage = SystemLanguage.English;
		}
		_urlParameter = "";
		string text = systemLanguage.ToString();
		text = text.ToLower();
		AddOqupieParameter(BrandKey1, text);
		long num = 0L;
		if (AccountManager.IsLogined())
		{
			UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
			if (userManager != null)
			{
				num = userManager.Id;
			}
		}
		DeviceBuildInfo buildInfo = DeviceHelper.GetBuildInfo();
		AddOqupieParameter("userId", num.ToString());
		string nick = GlobalAutoBehaviour<AccountManager>.instance.Nick;
		AddOqupieParameter("userName", nick);
		AddOqupieParameter("deviceId", SprkTestPrefs.DeviceId());
		AddOqupieParameter("deviceModel", buildInfo.Model);
		AddOqupieParameter("systemMemorySize", SystemInfo.systemMemorySize.ToString());
		AddOqupieParameter("systemLanguage", Application.systemLanguage.ToString());
		AddOqupieParameter("operatingSystem", SystemInfo.operatingSystem);
		AddOqupieParameter("graphicsDeviceName", SystemInfo.graphicsDeviceName);
		AddOqupieParameter("graphicsShaderLevel", SystemInfo.graphicsShaderLevel.ToString());
		AddOqupieParameter("batteryLevel", SystemInfo.batteryLevel.ToString());
		AddOqupieParameter("graphicsMemorySize", SystemInfo.graphicsMemorySize.ToString());
		AddOqupieParameter("applicationLanguage", systemLanguage2.ToString());
		AddOqupieParameter("LoginPlatform", LoginData.LastLoginMethod.ToString());
		AddOqupieParameter("Server", GlobalAutoBehaviour<AccountManager>.instance.CountryServerName);
		AddOqupieParameter("TargetCPUArch", DeviceHelper.GetCpuFamily());
		AddOqupieParameter("CPUArch", SystemInfo.processorType);
		ClientConfig clientConfig = SingletonAuto<Config>.instance.ClientConfig;
		AddOqupieParameter("BuildVersion", clientConfig.Version);
		Debug.LogError(_urlParameter);
		OpenInternal(BaseUrl + _urlParameter);
		Debug.LogError(_url);
	}

	protected void OpenInternal(string url)
	{
		url = url.Replace(" ", "");
		_url = url + "/";
		Open(null, OnOpenComplete);
	}

	private void OnOpenComplete()
	{
		if (!(WebView != null))
		{
			return;
		}
		if (String_Extension_Method.IsNullOrEmpty(_url) && SingletonUpdater<WebServiceManager>.isValidInstance)
		{
			if (LoginData.WebEvent == null)
			{
				VespaDebug.LogError("WebEvent is Null");
				return;
			}
			_url = LoginData.WebEvent.LinkUrl;
		}
		WebView.openLinksInWebview = true;
		WebView.OpenUrl(_url, this);
	}

	public override void Close()
	{
		if (_closeAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(_closeAction);
		}
		base.Close();
	}

	public void OnCloseClick()
	{
		Close();
	}

	void WebViewComponent.IListener.OnWebViewClose()
	{
		Close();
	}

	public override void OnSystemBackButton()
	{
		Close();
	}

	public override void Reset()
	{
		base.Reset();
		if (WebView != null)
		{
			WebView.Close();
		}
	}
}
