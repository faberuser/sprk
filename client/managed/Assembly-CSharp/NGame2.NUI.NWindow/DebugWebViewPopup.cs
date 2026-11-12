using NGame2.NLogin;
using NGame2.NUI.NComponent2;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow;

public class DebugWebViewPopup : BaseWindow, IGlobalWindow, WebViewComponent.IListener
{
	private const string PrefabPath = "Resources/Prefab2/Window/DebugWebViewPopup";

	private static GlobalWindowPrefab<DebugWebViewPopup> _windowPrefab = new GlobalWindowPrefab<DebugWebViewPopup>("Resources/Prefab2/Window/DebugWebViewPopup", GlobalUIRoot.ELayerType.GlobalUI);

	public UILabel addressBar;

	public UIInput input;

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

	public static void OnSceneChanged()
	{
		if (!_windowPrefab.IsOpen())
		{
			_windowPrefab.ReleaseWindow();
			_windowPrefab.ReleasePrefab();
		}
	}

	public void OpenUrl()
	{
		string text = addressBar.text;
		_windowPrefab.instance.OpenInternal(text);
	}

	public static void OpenGlobal(string url)
	{
		_windowPrefab.instance.OpenInternal(url);
	}

	private void OpenInternal(string url)
	{
		_url = url;
		addressBar.text = url;
		input.value = url;
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
		WebView.OpenUrl(_url, this);
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnWebViewClose()
	{
		OnClickClose();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
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
