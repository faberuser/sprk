using NGame2.NUI.NComponent2;

namespace NGame2.NUI.NWindow;

public class OfficialChannelWebView : BaseWindow, WebViewComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Lobby/OfficialChannelWebView";

	private static GlobalWindowPrefab<OfficialChannelWebView> _windowPrefab = new GlobalWindowPrefab<OfficialChannelWebView>("Resources/Prefab2/Window/Lobby/OfficialChannelWebView", GlobalUIRoot.ELayerType.GlobalUI);

	public WebViewComponent WebView;

	private string _url;

	public static void CloseGlobal()
	{
		if (_windowPrefab.IsOpen())
		{
			_windowPrefab.CloseWindow();
		}
	}

	public static void OpenGlobal(string url = null)
	{
		_windowPrefab.instance.OpenInternal(url);
	}

	public static void OnSceneChanged()
	{
		if (!_windowPrefab.IsOpen())
		{
			_windowPrefab.ReleaseWindow();
			_windowPrefab.ReleasePrefab();
		}
	}

	protected void OpenInternal(string url)
	{
		_url = url;
		Open(null, OnOpenComplete);
	}

	public override void Reset()
	{
		base.Reset();
		if (WebView != null)
		{
			WebView.Close();
		}
	}

	private void OnOpenComplete()
	{
		if (WebView != null)
		{
			WebView.openLinksInWebview = true;
			WebView.OpenUrl(_url, this);
		}
	}

	void WebViewComponent.IListener.OnWebViewClose()
	{
		OnClickClose();
	}

	public void OnClickClose()
	{
		if (WebView != null)
		{
			WebView.Close();
		}
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}
}
