using System.Collections;
using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NLogin.NState;

public class StateBase_SignOut : StateBase
{
	private enum SignOutState
	{
		None,
		UserCancel,
		Success,
		Failed
	}

	private SignOutState _signOutState;

	private float _loginFailTime = 60f;

	public override void OnBegin()
	{
		base.OnBegin();
		StartCoroutine(coEntry());
	}

	public override void OnEnd()
	{
		LoginManagement.IsNowSignOutProgressed = false;
		base.OnEnd();
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
		if (_signOutState == SignOutState.UserCancel)
		{
			StateOwner.SetNextState(new StateBase_Logined());
		}
		else if (_signOutState == SignOutState.Failed)
		{
			StateOwner.SetNextState(new StateBase_Logined());
		}
		else if (_signOutState == SignOutState.Success)
		{
			SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value = string.Empty;
			LoginData.ClearLastLoginData();
			GlobalAutoBehaviour<ChattingManager>.instance.ClearChatList(ChatType.None, 0L);
			AccountManager.Logout();
			SingletonUpdater<LoginManager>.instance.ReturnToLogin();
		}
		else
		{
			ErrorAndQuit("Error SignOut");
		}
	}

	public override IEnumerator coRun()
	{
		bool isPopupClosed = false;
		bool popupSelectYes = false;
		SignOutPopup.OpenGlobal(delegate
		{
			isPopupClosed = true;
			popupSelectYes = true;
		}, delegate
		{
			isPopupClosed = true;
			popupSelectYes = false;
		});
		while (!isPopupClosed)
		{
			yield return null;
		}
		if (!popupSelectYes)
		{
			_signOutState = SignOutState.UserCancel;
			yield break;
		}
		bool isComplete = false;
		GlobalAutoBehaviour<MasangLoginManager>.instance.AccountSignOut(delegate(bool isSuccess)
		{
			if (isSuccess)
			{
				_signOutState = SignOutState.Success;
			}
			else
			{
				_signOutState = SignOutState.Failed;
			}
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
		if (_signOutState == SignOutState.Failed)
		{
			yield return new WaitErrorPopup("SignOut Fail");
			yield break;
		}
		isPopupClosed = false;
		string stringDirect = LocalizationManager.GetStringDirect("APPLY_SIGN_OUT_COMPLETE_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("SIGN_OUT_COMPLETE_MESSAGE");
		GlobalMessagePopup.OpenGlobal(stringDirect, stringDirect2, delegate
		{
			isPopupClosed = true;
		}, retryButton: false);
		while (!isPopupClosed)
		{
			yield return null;
		}
	}
}
