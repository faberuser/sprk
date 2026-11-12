using System;
using System.Collections;
using NGame2.NAccount;
using NShared.Login;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RestoreAccountPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/RestoreAccountPopup";

	private static GlobalWindowPrefab<RestoreAccountPopup> _windowPrefab = new GlobalWindowPrefab<RestoreAccountPopup>("Resources/Prefab2/Window/MessagePopup/RestoreAccountPopup", GlobalUIRoot.ELayerType.GlobalUI);

	public UILabel LastLoginTimeLabel;

	public UILabel InActiveTimeLabel;

	public UILabel MessageLabel;

	public UILabel RestoreFailLabel;

	public UIButton RestoreButton;

	public UIButton ConfirmButton;

	public UIButton ServiceCenterButton;

	private Action _onRestore;

	private Action _onConfirm;

	private float _restoreWaitingTime = 10f;

	private bool _isSuccessRestore;

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

	public static void OpenGlobal(Response res, Action onRestore, Action onConfirm)
	{
		_windowPrefab.instance.OpenInternal(res, onRestore, onConfirm);
	}

	protected void OpenInternal(Response res, Action onRestore, Action onConfirm)
	{
		Open(delegate
		{
			SingletonAuto<LocalizationManager>.CreateInstance();
			_onRestore = onRestore;
			_onConfirm = onConfirm;
			_restoreWaitingTime = res.RestoreAccountTimeSec;
			Init(res);
		}, null);
	}

	private void Init(Response res)
	{
		_isSuccessRestore = false;
		string stringDirect = LocalizationManager.GetStringDirect("INACTIVE_POPUP_DESC_NEED_RESTORE");
		UIHelper.SetLabelText(MessageLabel, stringDirect);
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(res.LastLoginTime, out var o_dateTime) && Vespa_String_Extension_Method2.TryParseNetworkTime2(res.InactiveTime, out var o_dateTime2))
		{
			DateTime i_dateTime = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime);
			DateTime i_dateTime2 = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime2);
			string text = string.Format("{0} {1}", LocalizationManager.GetStringDirect("INACTIVE_POPUP_DESC_LAST_LOGIN"), Vespa_DateTime_Extension_Method.ToNetworkString2(i_dateTime));
			string text2 = string.Format("{0} {1}", LocalizationManager.GetStringDirect("INACTIVE_POPUP_DESC_TRANSITION"), Vespa_DateTime_Extension_Method.ToNetworkString2(i_dateTime2));
			UIHelper.SetLabelText(LastLoginTimeLabel, text);
			UIHelper.SetLabelText(InActiveTimeLabel, text2);
		}
		else
		{
			UIHelper.SetLabelText(LastLoginTimeLabel, "");
			UIHelper.SetLabelText(InActiveTimeLabel, "");
		}
		UIHelper.EnableGameObject(RestoreButton, enable: true);
		UIHelper.EnableGameObject(ConfirmButton, enable: false);
		UIHelper.EnableGameObject(ServiceCenterButton, enable: false);
	}

	public static void SetSuccess()
	{
		_windowPrefab.instance.SetSuccessRestore();
	}

	protected void SetSuccessRestore()
	{
		_isSuccessRestore = true;
	}

	private IEnumerator coRestoring()
	{
		string message = LocalizationManager.GetStringDirect("INACTIVE_POPUP_DESC_RESTORING");
		UIHelper.SetLabelText(MessageLabel, message);
		UIHelper.EnableGameObject(RestoreButton, enable: false);
		float waitingTime = _restoreWaitingTime / 3f;
		yield return new WaitForSeconds(waitingTime);
		UIHelper.SetLabelText(MessageLabel, $"{message}.");
		yield return new WaitForSeconds(waitingTime);
		UIHelper.SetLabelText(MessageLabel, $"{message}..");
		yield return new WaitForSeconds(waitingTime);
		UIHelper.SetLabelText(MessageLabel, $"{message}...");
		yield return new WaitUntil(() => _isSuccessRestore);
		SetSuccessUI();
	}

	private void SetSuccessUI()
	{
		string stringDirect = LocalizationManager.GetStringDirect("INACTIVE_POPUP_DESC_RESTORE_COMPLETE");
		UIHelper.SetLabelText(MessageLabel, stringDirect);
		UIHelper.EnableGameObject(ConfirmButton, enable: true);
	}

	public override void Reset()
	{
		base.Reset();
		_onRestore = null;
		_onConfirm = null;
		_isSuccessRestore = false;
	}

	public void OnClickRestore()
	{
		Vespa_System_Action_Extension_Method.fire(_onRestore);
		VespaCoroutine.Start(this, coRestoring());
	}

	public void OnClickConfirm()
	{
		Vespa_System_Action_Extension_Method.fire(_onConfirm);
		base.Close();
	}
}
