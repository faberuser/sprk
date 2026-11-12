using System.Collections;
using NGame2.NUI.NWindow;
using NShared;

namespace NGame2.NLogin.NState;

public class StateBase_LoginMethod_AppleGameCenter : StateBase
{
	private enum LoginSuccessState
	{
		None,
		Success,
		Failed
	}

	private LoginSuccessState _loginSuccessState;

	private float _loginFailTime = 90f;

	private bool _isSuccess;

	private bool _isComplete;

	private LoginUserId _loginUserId;

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
			StateOwner.SetNextState(new StateBase_LoginGameServer(_loginUserId, LoginMethodType.GameCenter));
		}
		else if (_loginSuccessState != LoginSuccessState.None)
		{
			ErrorAndQuit("Error GameCenter Login");
		}
	}

	public override IEnumerator coRun()
	{
		WaitingPopup.Open(this);
		yield return new WaitQuitError("Invalid Platform For GameCenter");
	}
}
