using System;
using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LinkAccountPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/LinkAccountPopup";

	public const string PrefabPath_New = "Resources/Prefab2/Window/PayShop/PayShopLinkAccountPopup";

	public UILabel Label_Message;

	public UIButton Button_GPGS;

	public UIButton Button_GameCenter;

	public UIButton Button_Apple;

	public UIButton Button_Twitter;

	public UIButton Button_Line;

	public UISimpleGrid ButtonGrid;

	private Action<bool> _onLinkedAccount;

	private LoginMethodType _loginMethod;

	public static void Open(Action<bool> onLinkedAccount)
	{
		Open(onLinkedAccount, "Resources/Prefab2/Window/MessagePopup/LinkAccountPopup");
	}

	public static void NewOpen(Action<bool> onLinkedAccount)
	{
		Open(onLinkedAccount, "Resources/Prefab2/Window/PayShop/PayShopLinkAccountPopup");
	}

	public static void Open(Action<bool> onLinkedAccount, string prefabPath)
	{
		string stringDirect = LocalizationManager.GetStringDirect("SHOP_PAY_GUEST_MESSAGE");
		WindowPrefab<LinkAccountPopup> windowPrefab = new WindowPrefab<LinkAccountPopup>(prefabPath, GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(stringDirect, onLinkedAccount);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string message, Action<bool> onLinkedAccount)
	{
		base.Open();
		UIHelper.SetLabelText(Label_Message, message);
		_onLinkedAccount = onLinkedAccount;
		UIHelper.EnableGameObject(Button_GPGS, Application.platform == RuntimePlatform.Android);
		UIHelper.EnableGameObject(Button_GameCenter, Application.platform == RuntimePlatform.IPhonePlayer);
		UIHelper.EnableGameObject(Button_Line, enable: false);
		UIHelper.EnableGameObject(Button_Twitter, enable: false);
		bool flag = AppleManager.IsEnableIOSVersion();
		UIHelper.EnableGameObject(Button_Apple, flag && Application.platform == RuntimePlatform.IPhonePlayer);
		UIHelper.RepositionGrid(ButtonGrid);
	}

	public override void Reset()
	{
		base.Reset();
		_onLinkedAccount = null;
	}

	public override void Close()
	{
		Vespa_System_Action_Extension_Method.fire(_onLinkedAccount, i_argA: false);
		_onLinkedAccount = null;
		base.Close();
	}

	public void OnClickFacebook()
	{
		ChangeLoginMethod(LoginMethodType.Steam);
	}

	public void OnClickGooglePlus()
	{
		ChangeLoginMethod(LoginMethodType.GooglePlus);
	}

	public void OnClickGameCenter()
	{
		ChangeLoginMethod(LoginMethodType.GameCenter);
	}

	public void OnClickApple()
	{
		ChangeLoginMethod(LoginMethodType.Apple);
	}

	public void OnClickTwitter()
	{
	}

	public void OnClickLine()
	{
	}

	public void OnClickClose()
	{
		Close();
	}

	private void ChangeLoginMethod(LoginMethodType loginMethod)
	{
		SingletonUpdater<LoginManager>.instance.ChangeLoginMethod(loginMethod, delegate(bool isSuccess)
		{
			if (isSuccess)
			{
				_loginMethod = loginMethod;
				Vespa_System_Action_Extension_Method.fire(_onLinkedAccount, i_argA: true);
				_onLinkedAccount = null;
				Close();
			}
		});
	}
}
