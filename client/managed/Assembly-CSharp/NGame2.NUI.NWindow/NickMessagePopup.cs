using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class NickMessagePopup : BaseWindow
{
	[Serializable]
	public class LoginMethodRoot
	{
		public LoginMethodType LoginMethod;

		public Transform Root;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/NickMessagePopup";

	public UILabel TitleLabel;

	public UILabel ContentLabel;

	public UILabel Label_LoginMethod;

	private Action _onYes;

	private Action _onNo;

	public static void Open(string title, string content, LoginMethodType loginMethod, Action onYes, Action onNo)
	{
		WindowPrefab<NickMessagePopup> windowPrefab = new WindowPrefab<NickMessagePopup>("Resources/Prefab2/Window/Login/NickMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, content, loginMethod, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(string title, string content, LoginMethodType loginMethod, Action onYes, Action onNo)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(ContentLabel, content);
		SetLoginMethodText(loginMethod);
		SprkAccounts.HideGuestNickname(this);
	}

	public override void Close()
	{
		Vespa_System_Action_Extension_Method.fire(_onNo);
		base.Close();
	}

	public override void Reset()
	{
		base.Reset();
		_onYes = null;
		_onNo = null;
	}

	private void SetLoginMethodText(LoginMethodType loginMethod)
	{
		string text = string.Empty;
		switch (loginMethod)
		{
		case LoginMethodType.Guest:
			text = LocalizationManager.GetStringDirect("GUEST_LOGIN_MESSAGE");
			break;
		case LoginMethodType.GooglePlus:
			text = LocalizationManager.GetStringDirect("GOOGLE_LOGIN_MESSAGE");
			break;
		case LoginMethodType.Steam:
			text = LocalizationManager.GetStringDirect("FACEBOOK_LOGIN_MESSAGE");
			break;
		case LoginMethodType.GameCenter:
			text = LocalizationManager.GetStringDirect("GAMECENTER_LOGIN_MESSAGE");
			break;
		case LoginMethodType.Apple:
			text = LocalizationManager.GetStringDirect("APPLE_LOGIN_MESSAGE");
			break;
		case LoginMethodType.Twitter:
			text = LocalizationManager.GetStringDirect("TWITTER_LOGIN_MESSAGE");
			break;
		case LoginMethodType.Line:
			text = LocalizationManager.GetStringDirect("LINE_LOGIN_MESSAGE");
			break;
		}
		UIHelper.SetLabelText(Label_LoginMethod, text);
	}

	public void OnClickYes()
	{
		Vespa_System_Action_Extension_Method.fire(_onYes);
		_onNo = null;
		Close();
	}

	public void OnClickNo()
	{
		Close();
	}
}
