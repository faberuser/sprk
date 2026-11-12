using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLogin.NState;

public class StateBase_LoginMethod_Steam : StateBase
{
	private enum LoginSuccessState
	{
		None,
		Success,
		Failed,
		DeleteID
	}

	private LoginSuccessState _loginSuccessState;

	private string _loginId;

	private string _loginVerificationInfo = string.Empty;

	private string _delete_dt = string.Empty;

	public StateBase_LoginMethod_Steam()
	{
		_ = 0;
	}

	public override void OnBegin()
	{
		base.OnBegin();
		StartCoroutine(coEntry());
	}

	public override void OnEnd()
	{
		base.OnEnd();
		WaitingPopup.Close(this);
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
		WaitingPopup.Close(this);
		if (_loginSuccessState == LoginSuccessState.Failed)
		{
			LoginData.LastLoginMethod = LoginMethodType.None;
			StateOwner.SetNextState(new StateBase_SelectLoginMethod());
		}
		else if (_loginSuccessState == LoginSuccessState.Success)
		{
			StateOwner.SetNextState(new StateBase_LoginGameServer(_loginId, LoginMethodType.Steam, _loginVerificationInfo));
		}
		else if (_loginSuccessState != LoginSuccessState.None)
		{
			ErrorAndQuit("Error Login Facebook");
		}
	}

	public override IEnumerator coRun()
	{
		WaitingPopup.Open(this);
		bool isloginfinish = false;
		GlobalAutoBehaviour<MasangLoginManager>.instance.SteamLogin(delegate(bool issuccess, LoginSuccData data)
		{
			if (issuccess)
			{
				_loginId = data.id;
				_loginVerificationInfo = data.sessionid;
				_loginSuccessState = LoginSuccessState.Success;
			}
			else if (data != null)
			{
				_loginId = data.id;
				_delete_dt = data.sessionid;
				_loginSuccessState = LoginSuccessState.DeleteID;
			}
			else
			{
				_loginSuccessState = LoginSuccessState.Failed;
			}
			isloginfinish = true;
		});
		while (!isloginfinish)
		{
			yield return null;
		}
		if (_loginSuccessState != LoginSuccessState.DeleteID)
		{
			yield break;
		}
		bool isdeletpopupfinish = false;
		NGame2.NUI.NWindow.SignOutAccountRecoveryPopup.OpenGlobal(delegate
		{
			GlobalAutoBehaviour<MasangLoginManager>.instance.AccountSignOutCancel(delegate(bool isSuccess)
			{
				if (isSuccess)
				{
					GlobalAutoBehaviour<MasangLoginManager>.instance.AccessTokenLogin(delegate(bool issuccess, LoginSuccData logindata)
					{
						if (issuccess)
						{
							_loginId = logindata.id;
							_loginVerificationInfo = logindata.sessionid;
							_loginSuccessState = LoginSuccessState.Success;
						}
						isdeletpopupfinish = true;
					});
				}
				else
				{
					_loginSuccessState = LoginSuccessState.Failed;
					isdeletpopupfinish = true;
				}
			});
		}, delegate
		{
			_loginSuccessState = LoginSuccessState.Failed;
			isdeletpopupfinish = true;
		}, delegate
		{
			_loginSuccessState = LoginSuccessState.Failed;
			isdeletpopupfinish = true;
		}, _delete_dt);
		while (!isdeletpopupfinish)
		{
			yield return null;
		}
	}
}
