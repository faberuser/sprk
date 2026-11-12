using NGame2.NLogin;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LawRelatedWebViewPopup : BaseWindow, WebViewComponent.IListener
{
	public enum WebViewType
	{
		None,
		Privacy,
		TOS
	}

	public const string _prefabPath = "Resources/Prefab2/Window/LawRelatedWebViewPopup";

	public UILabel _titleLabel;

	public WebViewComponent _webView;

	private string _connectURL = string.Empty;

	private WebViewType _webViewType;

	public static void Open(string title, WebViewType webViewType)
	{
		WindowPrefab<LawRelatedWebViewPopup> windowPrefab = new WindowPrefab<LawRelatedWebViewPopup>("Resources/Prefab2/Window/LawRelatedWebViewPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, webViewType);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(string title, WebViewType webViewType)
	{
		_webViewType = webViewType;
		Open(null, OnOpenComplete);
		UIHelper.SetLabelText(_titleLabel, title);
	}

	private void OnOpenComplete()
	{
		string text = string.Empty;
		CountryDataContainer countryDataContainer = GameRoot.Instance.CountryDataContainer;
		CountryData data = countryDataContainer.GetData("None");
		CountryData data2 = countryDataContainer.GetData(SingletonUpdater<LoginManager>.instance.CountryCode);
		if (data2 != null)
		{
			text = ((_webViewType == WebViewType.Privacy) ? data2.PrivacyURL : data2.TeamOfServiceURL);
		}
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			text = ((_webViewType == WebViewType.Privacy) ? data.PrivacyURL : data.TeamOfServiceURL);
		}
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			VespaDebug.LogError("[LawRelatedWebViewPopup] Input URL is Null!");
			return;
		}
		SingletonAuto<LocalizationManager>.instance.Language.ToString();
		if (!String_Extension_Method.IsNullOrEmpty(LoginManager.ClientConfig.BuildStep))
		{
			_ = LoginManager.ClientConfig.BuildStep;
		}
		_connectURL = text;
		_webView.OpenUrl(_connectURL, this);
		_webView.SetBackgroundColor(new Color(0f, 0f, 0f, 0f));
	}

	void WebViewComponent.IListener.OnWebViewClose()
	{
		Close();
	}

	public override void Close()
	{
		_webView.Close();
		base.Close();
	}

	public void OnClick()
	{
		Close();
	}
}
