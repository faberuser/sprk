using System;
using NGame2.NConfig;
using NGame2.NDevice;
using NGame2.NScene;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NShared.QueryMarketUrl;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUtil;

public class QueryMarketInfo
{
	private static bool _isAvailableDevConsole;

	public bool IsDone { get; private set; }

	public bool NeedUpgrade { get; private set; }

	public NShared.QueryMarketUrl.Request Request { get; private set; }

	public NShared.QueryMarketUrl.Response Response { get; private set; }

	public static bool IsAvailableDevConsole => _isAvailableDevConsole;

	public static MarketType GetMarketType()
	{
		return MarketType.StandaloneWindows;
	}

	public static string GetMarketTypeName()
	{
		return "StandaloneWindows";
	}

	public void Query(Action onComplete)
	{
		if (!SprkNetworkPerformance.AnswerMarketQuery(this, onComplete))
		{
			Action onComplete2 = onComplete;
			_isAvailableDevConsole = false;
			MarketType marketType = GetMarketType();
			int appPlayerBuild = (DeviceHelper.GetCpuFamily().Equals("x86") ? 1 : 0);
			SystemLanguage systemLanguage = (SingletonAuto<LocalizationManager>.isValidInstance ? SingletonAuto<LocalizationManager>.instance.Language : SystemLanguage.English);
			Request = new NShared.QueryMarketUrl.Request
			{
				MarketType = marketType,
				AppPlayerBuild = appPlayerBuild,
				Language = systemLanguage.ToString()
			};
			WebRequestFlag flag = WebRequestFlag.ShowLoadingGauge | WebRequestFlag.DisableEncrypt | WebRequestFlag.DisableRetry;
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(Request, flag, delegate(NShared.QueryMarketUrl.Response response)
			{
				IsDone = true;
				Response = response;
				Vespa_System_Action_Extension_Method.fire(onComplete2);
			});
		}
	}

	public bool Verify()
	{
		if (!IsDone)
		{
			return false;
		}
		if (Response == null)
		{
			VespaDebug.LogError("QueryMarketInfo response is null");
			return true;
		}
		if (Response.BaseResult == BaseResultType.Disabled)
		{
			return true;
		}
		if (Response.BaseResult != BaseResultType.Success)
		{
			VespaDebug.LogError("QueryMarketInfo failed : BaseResult=" + Response.BaseResult);
			return true;
		}
		if (Response.Result != ResultType.Success)
		{
			VespaDebug.LogError("QueryMarketInfo failed : Result=" + Response.Result);
			return true;
		}
		string version = Response.Version;
		string marketUrl = Response.MarketUrl;
		PopupTypes popupType = Response.PopupType;
		if (!String_Extension_Method.IsNullOrEmpty(Response.ReservedStr))
		{
			_isAvailableDevConsole = string.Equals(Response.ReservedStr.ToLower(), "vespa");
		}
		if (string.IsNullOrEmpty(marketUrl))
		{
			return true;
		}
		string version2 = SingletonAuto<Config>.instance.ClientConfig.Version;
		Debug.LogFormat("[QueryMarketInfo] newVersion={0}", version);
		Debug.LogFormat("[QueryMarketInfo] myVersion={0}", version2);
		Debug.LogFormat("[QueryMarketInfo] marketUrl={0}", marketUrl);
		bool flag = false;
		switch (popupType)
		{
		case PopupTypes.MarketUpdated:
			flag = NeedUpgradeVersion(version);
			break;
		default:
			VespaDebug.LogError($"[QueryMarketInfo] Unknown PopupType : {popupType}");
			return true;
		case PopupTypes.EmulatorWarningUpdated:
			break;
		}
		if (!flag)
		{
			return true;
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (popupType == PopupTypes.MarketUpdated)
		{
			empty = LocalizationManager.GetString(NShared.NLocalization.System.NEW_APP_VERSION_TITLE);
			empty2 = LocalizationManager.GetString(NShared.NLocalization.System.NEW_APP_VERSION_MESSAGE);
		}
		else
		{
			empty = LocalizationManager.GetStringDirect("APP_PLAYER_APP_NOTICE_TITLE");
			empty2 = LocalizationManager.GetStringDirect("APP_PLAYER_APP_NOTICE_MESSAGE");
		}
		GlobalMessagePopup.OpenGlobal(empty, empty2, delegate
		{
			Application.OpenURL(marketUrl);
			GlobalAutoBehaviour<LoadingManager>.instance.Quit();
		}, retryButton: false);
		return false;
	}

	private bool NeedUpgradeVersion(string newVersion)
	{
		if (string.IsNullOrEmpty(newVersion))
		{
			return false;
		}
		string version = SingletonAuto<Config>.instance.ClientConfig.Version;
		if (string.IsNullOrEmpty(version))
		{
			return false;
		}
		ClientConfig.ParseVersion(newVersion, out var majorVersion, out var minorVersion, out var patchVersion);
		ClientConfig.ParseVersion(version, out var majorVersion2, out var minorVersion2, out var patchVersion2);
		Debug.LogFormat("[QueryMarketInfo] newVersion Parsed={0}.{1}.{2}", majorVersion, minorVersion, patchVersion);
		Debug.LogFormat("[QueryMarketInfo] myVersion Parsed={0}.{1}.{2}", majorVersion2, minorVersion2, patchVersion2);
		long normalizedVersion = ClientConfig.GetNormalizedVersion(majorVersion, minorVersion, patchVersion);
		long normalizedVersion2 = ClientConfig.GetNormalizedVersion(majorVersion2, minorVersion2, patchVersion2);
		return normalizedVersion > normalizedVersion2;
	}
}
