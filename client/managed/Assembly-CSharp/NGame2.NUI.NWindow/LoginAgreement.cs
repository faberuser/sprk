using System;
using System.Collections;
using NGame2.NLogin;
using NGame2.NNotification;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginAgreement : BaseWindow, WebViewComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginAgreement";

	public string TestCountryCode;

	public UIButton _confirmButton;

	public GameObject _checkBoxLeft;

	public GameObject _checkBoxRight;

	public GameObject _checkBox_NightNotiConfirm;

	public WebViewComponent _teamOfServiceView;

	public WebViewComponent _privacyView;

	private string _privacyURL = string.Empty;

	private string _teamOfServiceURL = string.Empty;

	private bool _isNightNotiChecked;

	private bool _isNightNotiDisagreeShown;

	private Action _onConfirm;

	public void Open(CountryData countryData, Action onConfirm)
	{
		Open(delegate
		{
			Init(countryData, onConfirm);
		}, OnOpenComplete);
	}

	void WebViewComponent.IListener.OnWebViewClose()
	{
	}

	public override void Close()
	{
		_privacyView.Close();
		_teamOfServiceView.Close();
		base.Close();
	}

	public override void OnSystemBackButton()
	{
		OpenQuitPopup();
	}

	public void OpenQuitPopup()
	{
	}

	public void Init(CountryData countryData, Action onConfirm)
	{
		_onConfirm = onConfirm;
		if (countryData == null)
		{
			Vespa_System_Action_Extension_Method.fire(_onConfirm);
			VespaDebug.LogError("Country Data is Null!");
			return;
		}
		CountryData data = GameRoot.Instance.CountryDataContainer.GetData("None");
		string privacyURL = countryData.PrivacyURL;
		string teamOfServiceURL = countryData.TeamOfServiceURL;
		try
		{
			_privacyURL = (String_Extension_Method.IsNullOrEmpty(privacyURL) ? data.PrivacyURL : privacyURL);
			_teamOfServiceURL = (String_Extension_Method.IsNullOrEmpty(teamOfServiceURL) ? data.TeamOfServiceURL : teamOfServiceURL);
		}
		catch (Exception arg)
		{
			VespaDebug.LogError("[LoginAgreement] No found any URL from CountryData");
			VespaDebug.LogError($"[LoginAgreement] Exception : {arg}");
			_privacyURL = string.Empty;
			_teamOfServiceURL = string.Empty;
		}
		UIHelper.EnableButton(_confirmButton, enable: false);
		UIHelper.EnableGameObject(_checkBoxLeft, enable: false);
		UIHelper.EnableGameObject(_checkBoxRight, enable: false);
		UIHelper.EnableGameObject(_checkBox_NightNotiConfirm, enable: false);
	}

	public override void Reset()
	{
		base.Reset();
		_onConfirm = null;
	}

	private void OnOpenComplete()
	{
		if (_privacyView == null || _teamOfServiceView == null)
		{
			VespaDebug.LogError("Failed Load WebView..");
			return;
		}
		_privacyView.openLinksInWebview = true;
		_teamOfServiceView.openLinksInWebview = true;
		string text = SingletonAuto<LocalizationManager>.instance.Language.ToString();
		string arg = (String_Extension_Method.IsNullOrEmpty(LoginManager.ClientConfig.BuildStep) ? "dev" : LoginManager.ClientConfig.BuildStep);
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			text = "English";
		}
		string url = string.Format(_privacyURL, arg, text);
		string url2 = string.Format(_teamOfServiceURL, arg, text);
		_privacyView.OpenUrl(url, this);
		_teamOfServiceView.OpenUrl(url2, this);
		_privacyView.SetEnableBackButton(isEnabled: false);
		_teamOfServiceView.SetEnableBackButton(isEnabled: false);
		Color backgroundColor = new Color(0f, 0f, 0f, 0f);
		_privacyView.SetBackgroundColor(backgroundColor);
		_teamOfServiceView.SetBackgroundColor(backgroundColor);
	}

	public void OnClickConfirm()
	{
		VespaCoroutine.Start(coOnClickConfirm());
	}

	private IEnumerator coOnClickConfirm()
	{
		UIHelper.EnableButton(_confirmButton, enable: false);
		if (!_isNightNotiChecked && !_isNightNotiDisagreeShown)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_NIGHT_DISAGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).transform.localPosition = new Vector2(0f, -300f);
			_isNightNotiDisagreeShown = true;
			yield return new WaitForSeconds(2f);
		}
		Vespa_System_Action_Extension_Method.fire(_onConfirm);
		NotificationDefaultSetting();
	}

	public void ToggleCheckboxObject(GameObject senderObject)
	{
		if (!object.Equals(senderObject, _checkBoxLeft) && !object.Equals(senderObject, _checkBoxRight))
		{
			VespaDebug.LogError("[LoginAgreement] Invalid checkbox object. senderObject =" + senderObject);
			return;
		}
		bool flag = false;
		if (senderObject == _checkBoxRight)
		{
			flag = true;
		}
		if (flag)
		{
			CenterMessage centerMessage = null;
			centerMessage = ((!UIHelper.IsEnabledGameObjet(senderObject)) ? CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_AGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)) : CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_DISAGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)));
			if (!object.Equals(centerMessage, null))
			{
				centerMessage.transform.localPosition = new Vector2(0f, -300f);
			}
		}
		UIHelper.ToggleGameObject(senderObject);
		UpdateAllChecked();
	}

	private void UpdateAllChecked()
	{
		bool activeSelf = _checkBoxLeft.activeSelf;
		bool activeSelf2 = _checkBoxRight.activeSelf;
		bool enable = activeSelf && activeSelf2;
		UIHelper.EnableButton(_confirmButton, enable);
	}

	public void ToggleNightNotiConfirm(GameObject senderObject)
	{
		UIHelper.ToggleGameObject(senderObject);
		CenterMessage centerMessage;
		if (UIHelper.IsEnabledGameObjet(senderObject))
		{
			centerMessage = CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_NIGHT_AGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			_isNightNotiChecked = true;
		}
		else
		{
			centerMessage = CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_NIGHT_DISAGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			_isNightNotiChecked = false;
			_isNightNotiDisagreeShown = true;
		}
		centerMessage.transform.localPosition = new Vector2(0f, -300f);
	}

	public void NotificationDefaultSetting()
	{
		SingletonAuto<NotificationManager>.instance.SetNotificationDefaultValuesOnce();
		if (_isNightNotiChecked)
		{
			GameOptionData.Noti_NightNoti = true;
			GameOptionData.Noti_AllNoti = true;
		}
	}
}
