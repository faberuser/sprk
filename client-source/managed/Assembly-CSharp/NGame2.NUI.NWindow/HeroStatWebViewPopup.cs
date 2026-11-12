using System.Text;
using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class HeroStatWebViewPopup : BaseWindow, IGlobalWindow, WebViewComponent.IListener
{
	public const string BaseUrl = "https://kr-apne2-web.masangsoft.com/stats/v1/html/";

	public const string PrefabPath = "Resources/Prefab2/Window/HeroStatWebviewPopup";

	private static GlobalWindowPrefab<HeroStatWebViewPopup> _windowPrefab = new GlobalWindowPrefab<HeroStatWebViewPopup>("Resources/Prefab2/Window/HeroStatWebviewPopup", GlobalUIRoot.ELayerType.GlobalUI);

	public WebViewComponent WebView;

	private string _url;

	public static bool IsOpenGlobal()
	{
		return _windowPrefab.IsOpen();
	}

	public static void CloseGlobal()
	{
		if (IsOpenGlobal())
		{
			_windowPrefab.CloseWindow();
		}
	}

	public static void OpenGlobal(string url = null)
	{
		_windowPrefab.instance.Open("index.html?" + GetQueryString());
	}

	private static string GetQueryString()
	{
		return GetOwnedHeroQueryString() + "&" + GetLanguageQueryString();
	}

	private static string GetOwnedHeroQueryString()
	{
		CachedHeroInfo[] heroInfos = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfos();
		StringBuilder stringBuilder = new StringBuilder("heroes=");
		CachedHeroInfo[] array = heroInfos;
		foreach (CachedHeroInfo cachedHeroInfo in array)
		{
			stringBuilder.Append(cachedHeroInfo.HeroIndex + ",");
		}
		stringBuilder.Length--;
		return stringBuilder.ToString();
	}

	private static string GetLanguageQueryString()
	{
		StringBuilder stringBuilder = new StringBuilder("lang=");
		string text = GameRoot.Instance.LocalizationValueDataContainer.GetCultureInfo().ToString().ToLower();
		switch (text)
		{
		case "en-us":
		case "pt-pt":
		case "zh-cn":
		case "zh-tw":
			stringBuilder.Append(text);
			break;
		default:
			stringBuilder.Append(text.Substring(0, 2));
			break;
		}
		return stringBuilder.ToString();
	}

	public static void OnSceneChanged()
	{
		if (!_windowPrefab.IsOpen())
		{
			_windowPrefab.ReleaseWindow();
			_windowPrefab.ReleasePrefab();
		}
	}

	public void OnClickContentsButton()
	{
		Open("select-content.html?" + GetQueryString());
	}

	public void OnClickHeroButton()
	{
		Open("select-hero.html?" + GetQueryString());
	}

	public void Open(string path)
	{
		OpenInternal("https://kr-apne2-web.masangsoft.com/stats/v1/html/" + path);
	}

	protected void OpenInternal(string url)
	{
		_url = url;
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

	public override void Reset()
	{
		base.Reset();
		if (WebView != null)
		{
			WebView.Close();
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	void WebViewComponent.IListener.OnWebViewClose()
	{
		OnClickClose();
	}
}
