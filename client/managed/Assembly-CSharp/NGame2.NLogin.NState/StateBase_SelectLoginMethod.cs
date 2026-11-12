using System;
using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NLogin.NState;

public class StateBase_SelectLoginMethod : StateBase, LoginSelect.Listener, ServerSelect.Listener
{
	private enum SelectState
	{
		None,
		Selected,
		AccountID
	}

	private bool _isUserSelect;

	private LoginMethodType _methodType;

	private SelectState _slectState;

	public override void OnBegin()
	{
		base.OnBegin();
		_ = (object)null;
		_ = 1;
		_ = 1;
		_ = 1;
		Array.Empty<object>();
		_isUserSelect = false;
		_methodType = LoginMethodType.None;
		_slectState = SelectState.None;
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
		if (_slectState == SelectState.Selected)
		{
			StateOwner.SetNextState(new StateBase_Login(_methodType, _isUserSelect));
		}
		else if (_slectState == SelectState.AccountID)
		{
			StateOwner.SetNextState(new StateBase_Login());
		}
		else
		{
			ErrorAndQuit("Error Select Login Method");
		}
	}

	public override IEnumerator coRun()
	{
		LoginMethodType lastLoginMethod = LoginData.LastLoginMethod;
		AppServerInfo[] server = SingletonUpdater<LoginManager>.instance.Host.Server;
		string lastServerName = LoginData.LastServerName;
		SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginSelect>().Open(this, lastLoginMethod, server, lastServerName);
		SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginTouchWait>().Close();
		SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginAccountInfo>().Close();
		while (_slectState == SelectState.None)
		{
			yield return null;
		}
	}

	void ServerSelect.Listener.OnSelect(ServerSelect caller, string serverName)
	{
		SingletonUpdater<LoginManager>.instance.SetServerName(serverName);
		SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginSelect>().SelectServer(serverName);
		caller.Close();
	}

	void LoginSelect.Listener.OnServerList(LoginSelect caller)
	{
		AppServerInfo[] server = SingletonUpdater<LoginManager>.instance.Host.Server;
		SceneAutoBehaviour<LoginManagement>.instance.GetInstance<ServerSelect>().Open(this, server);
	}

	void LoginSelect.Listener.OnSelect(LoginSelect caller, LoginMethodType methodType, string serverName, bool UserSelect)
	{
		_methodType = methodType;
		_isUserSelect = UserSelect;
		_slectState = SelectState.Selected;
		caller.Close();
	}

	void LoginSelect.Listener.OnLoginByAccountId(LoginSelect caller, int accountId)
	{
		SingletonUpdater<LoginManager>.instance.ForcedAccountId = accountId.ToString();
		_slectState = SelectState.AccountID;
		caller.Close();
	}
}
