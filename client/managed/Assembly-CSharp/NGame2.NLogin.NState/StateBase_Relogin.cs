using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NConfig;
using NGame2.NLogin.NHelper;
using NGame2.NScene;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NShared.Login;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_Relogin : StateBase, ITouchDisabler
{
	private enum ReloginState
	{
		None,
		Error,
		Success
	}

	private ReloginState _reloginState;

	private Action<ResultType> _onLogin;

	private string _forwardedHost;

	private float _delay;

	private NShared.Login.Response _response;

	bool ITouchDisabler.IsValid => true;

	public StateBase_Relogin(Action<ResultType> onLogin, string forwardedHost, float delay)
	{
		_onLogin = onLogin;
		_forwardedHost = forwardedHost;
		_delay = delay;
	}

	public override void OnBegin()
	{
		base.OnBegin();
		StartCoroutine(coEntry());
	}

	public override void OnEnd()
	{
		base.OnEnd();
		GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
		GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
		if (_reloginState == ReloginState.Error)
		{
			LoginData.LastLoginMethod = LoginMethodType.None;
			SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value = string.Empty;
			LoginData.ClearLastLoginData();
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Login);
		}
		else if (_reloginState == ReloginState.Success)
		{
			GlobalAutoBehaviour<AccountManager>.instance.OnLoginResponse(_response);
			Vespa_System_Action_Extension_Method.fire(_onLogin, _response.Result);
			StateOwner.SetNextState(new StateBase_Logined());
		}
		else
		{
			ErrorAndQuit("Error Relogin");
		}
	}

	public override IEnumerator coRun()
	{
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		yield return new WaitForSeconds(_delay);
		LoginUserId lastLoginUserId = LoginData.LastLoginUserId;
		LoginMethodType lastLoginMethod = LoginData.LastLoginMethod;
		string text = LoginData.LastServerAddress;
		bool lastUseHttps = LoginData.LastUseHttps;
		string lastCookieKeyword = LoginData.LastCookieKeyword;
		string verificationInfo = string.Empty;
		if (!String_Extension_Method.IsNullOrEmpty(_forwardedHost))
		{
			text = _forwardedHost;
		}
		if (String_Extension_Method.IsNullOrEmpty(lastLoginUserId.id))
		{
			yield return new WaitErrorPopup("Invalid Login Id");
			_reloginState = ReloginState.Error;
			yield break;
		}
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			yield return new WaitErrorPopup("Server address is empty.");
			_reloginState = ReloginState.Error;
			yield break;
		}
		if (String_Extension_Method.IsNullOrEmpty(lastLoginUserId.id))
		{
			yield return new WaitErrorPopup("LoginId is empty.");
			_reloginState = ReloginState.Error;
			yield break;
		}
		ClientConfig clientConfig = SingletonAuto<Config>.instance.ClientConfig;
		if (clientConfig == null)
		{
			yield return new WaitErrorPopup("Client config is null.");
			_reloginState = ReloginState.Error;
			yield break;
		}
		if (!object.Equals(SingletonUpdater<LoginManager>.instance, null) && !String_Extension_Method.IsNullOrEmpty(SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value))
		{
			verificationInfo = SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value;
		}
		LoginProgress loginProgress = new LoginProgress(text, lastCookieKeyword, lastUseHttps, clientConfig, lastLoginUserId, lastLoginMethod, verificationInfo);
		loginProgress.OnMessage = OnLoginMessage;
		loginProgress.Start();
		while (loginProgress.Status == LoginProgress.EStatus.Progressing)
		{
			yield return null;
		}
		if (!object.Equals(loginProgress.LoginVerificationInfo, verificationInfo))
		{
			verificationInfo = loginProgress.LoginVerificationInfo;
		}
		if (loginProgress.Status == LoginProgress.EStatus.Error_GetCertificateFail)
		{
			yield return new WaitErrorPopup("Invalid Certificate");
			_reloginState = ReloginState.Error;
		}
		else if (loginProgress.Status == LoginProgress.EStatus.Error_Login)
		{
			if (loginProgress.LoginResponse == null)
			{
				yield return new WaitErrorPopup("Invalid Login Response");
				_reloginState = ReloginState.Error;
			}
			else if (loginProgress.LoginResponse.BaseResult != BaseResultType.Success)
			{
				yield return new WaitErrorPopup(loginProgress.LoginResponse.BaseResult);
				_reloginState = ReloginState.Error;
			}
			else
			{
				yield return new WaitErrorPopup(loginProgress.LoginResponse.Result);
				_reloginState = ReloginState.Error;
			}
		}
		else
		{
			SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value = verificationInfo;
			_response = loginProgress.LoginResponse;
			_reloginState = ReloginState.Success;
		}
	}

	private void OnLoginMessage(string message)
	{
	}
}
