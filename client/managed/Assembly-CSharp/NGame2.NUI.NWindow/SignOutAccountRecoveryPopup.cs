using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

internal class SignOutAccountRecoveryPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/SignOutAccountRecoveryPopup";

	private static GlobalWindowPrefab<NGame2.NUI.NWindow.SignOutAccountRecoveryPopup> _windowPrefab = new GlobalWindowPrefab<NGame2.NUI.NWindow.SignOutAccountRecoveryPopup>("Resources/Prefab2/Window/MessagePopup/SignOutAccountRecoveryPopup", GlobalUIRoot.ELayerType.GlobalUI);

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UIButton ButtonYes;

	public UIButton ButtonNo;

	public UILabel ButtonLabelYes;

	public UILabel ButtonLabelNo;

	private Action _onYes;

	private Action _onNo;

	private Action _onClose;

	private DateTime _signOutDoneTime;

	private float _lastCheckedRemainTime;

	private bool _isRequestLogin = true;

	private string _title;

	private string _message;

	public static void Open(Action onYes, Action onNo, Action onClose, string signOutDoneTime, bool isRequestLogin = true)
	{
		WindowPrefab<NGame2.NUI.NWindow.SignOutAccountRecoveryPopup> windowPrefab = new WindowPrefab<NGame2.NUI.NWindow.SignOutAccountRecoveryPopup>("Resources/Prefab2/Window/MessagePopup/SignOutAccountRecoveryPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(onYes, onNo, onClose, signOutDoneTime, isRequestLogin);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenGlobal(Action onYes, Action onNo, Action onClose, string signOutDoneTime, bool isRequestLogin = true)
	{
		_windowPrefab.instance.OpenInternal(onYes, onNo, onClose, signOutDoneTime, isRequestLogin);
	}

	protected void OpenInternal(Action onYes, Action onNo, Action onClose, string signOutDoneTime, bool isRequestLogin = true)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		_onClose = onClose;
		_signOutDoneTime = Convert.ToDateTime(signOutDoneTime);
		_lastCheckedRemainTime = Time.time;
		_isRequestLogin = isRequestLogin;
		if (_isRequestLogin)
		{
			_title = "ACCOUNT_RECOVERY_TITLE";
			_message = "ACCOUNT_RECOVERY_MESSAGE";
		}
		else
		{
			_title = "NEW_ACCOUNT_LINK_TITLE";
			_message = "NEW_ACCOUNT_LINK_MESSAGE";
		}
		UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect(_title));
		UpdateRemainTime();
		UIHelper.SetLabelText(ButtonLabelYes, LocalizationManager.GetStringDirect("COMMON_YES"));
		UIHelper.SetLabelText(ButtonLabelNo, LocalizationManager.GetStringDirect("COMMON_NO"));
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (Time.time - _lastCheckedRemainTime >= 1f)
		{
			UpdateRemainTime();
			_lastCheckedRemainTime = Time.time;
		}
	}

	private void UpdateRemainTime()
	{
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		int num = Mathf.Max((int)(_signOutDoneTime - currentServerUTCTime).TotalSeconds, 0);
		long num2 = num;
		if (num2 == 0L)
		{
			if (!_isRequestLogin)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("EXPIRE_WAIT_ACCOUNT_SIGNOUT_CENTERMSG"));
			}
			OnClickClose();
		}
		else
		{
			string remainTimeTextAll = UIHelper.GetRemainTimeTextAll(num2);
			string stringDirect = LocalizationManager.GetStringDirect(_message, remainTimeTextAll);
			UIHelper.SetLabelText(MessageLabel, stringDirect);
		}
	}

	public void OnClickNo()
	{
		Vespa_System_Action_Extension_Method.fire(_onNo);
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
	}

	public void OnClickYes()
	{
		Vespa_System_Action_Extension_Method.fire(_onYes);
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
	}

	public void OnClickClose()
	{
		Vespa_System_Action_Extension_Method.fire(_onClose);
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}
}
