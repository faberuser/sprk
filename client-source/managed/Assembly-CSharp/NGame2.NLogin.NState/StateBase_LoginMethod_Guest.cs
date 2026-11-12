using System.Collections;
using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NLogin.NState;

public class StateBase_LoginMethod_Guest : StateBase
{
	private enum LoginSuccessState
	{
		None,
		Success,
		Failed
	}

	private string _loginId;

	private string _loginVerificationInfo = string.Empty;

	private LoginSuccessState _loginSuccessState;

	public override void OnBegin()
	{
		base.OnBegin();
		StartCoroutine(coEntry());
	}

	public override void OnEnd()
	{
		base.OnEnd();
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
		Helper.GetDeviceUid();
		if (!String_Extension_Method.IsNullOrEmpty(SingletonUpdater<LoginManager>.instance.ForceLoginId))
		{
			_ = SingletonUpdater<LoginManager>.instance.ForceLoginId;
		}
		if (_loginSuccessState == LoginSuccessState.Failed)
		{
			LoginData.LastLoginMethod = LoginMethodType.None;
			StateOwner.SetNextState(new StateBase_SelectLoginMethod());
		}
		else if (_loginSuccessState == LoginSuccessState.Success)
		{
			StateOwner.SetNextState(new StateBase_LoginGameServer(_loginId, LoginMethodType.Guest, _loginVerificationInfo));
		}
	}

	public override IEnumerator coRun()
	{
		bool isComplete = false;
		GlobalAutoBehaviour<MasangLoginManager>.instance.GuestLogin(delegate(bool issuccess, LoginSuccData data)
		{
			if (issuccess)
			{
				_loginId = data.id;
				_loginVerificationInfo = data.sessionid;
				_loginSuccessState = LoginSuccessState.Success;
			}
			else
			{
				_loginSuccessState = LoginSuccessState.Failed;
			}
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
	}
}
