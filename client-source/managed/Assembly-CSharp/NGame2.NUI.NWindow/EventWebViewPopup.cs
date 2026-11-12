using System;
using NGame2.NLogin;
using NGame2.NUI.NComponent2;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow;

public class EventWebViewPopup : BaseWindow, IGlobalWindow, WebViewComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/EventWebViewPopup";

	private static GlobalWindowPrefab<EventWebViewPopup> _windowPrefab = new GlobalWindowPrefab<EventWebViewPopup>("Resources/Prefab2/Window/EventWebViewPopup", GlobalUIRoot.ELayerType.GlobalUI);

	private static LocalStorageDateTime _lastEnterLobbyOpen = new LocalStorageDateTime(DateTime.MinValue, typeof(EventWebViewPopup).FullName + "_LastEnterLobbyOpen");

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

	private void OnOpenComplete()
	{
		if (!(WebView != null))
		{
			return;
		}
		WebView.openLinksInWebview = true;
		if (String_Extension_Method.IsNullOrEmpty(_url) && SingletonUpdater<WebServiceManager>.isValidInstance)
		{
			if (LoginData.WebEvent == null)
			{
				VespaDebug.LogError("WebEvent is Null");
				return;
			}
			_url = $"{LoginData.WebEvent.LinkUrl}{LoginData.WebEvent.FirstEventId}.html{LoginData.WebEvent.QueryString}&id={LoginData.WebEvent.FirstEventId}";
		}
		WebView.OpenUrl(_url, this);
		_lastEnterLobbyOpen.Value = DateTime.Now;
	}

	void WebViewComponent.IListener.OnWebViewClose()
	{
		OnClickClose();
	}

	public static bool CanAutoOpen()
	{
		DateTime now = DateTime.Now;
		DateTime value = _lastEnterLobbyOpen.Value;
		return now.DayOfYear != value.DayOfYear;
	}

	public static void ClearLastEnterLobbyOpen()
	{
		_lastEnterLobbyOpen.Value = DateTime.MinValue;
	}
}
