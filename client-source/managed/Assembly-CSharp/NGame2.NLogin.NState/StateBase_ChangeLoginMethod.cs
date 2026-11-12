using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.LinkAccount;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NLogin.NState;

public class StateBase_ChangeLoginMethod : StateBase
{
	public enum RequestLinkStatus
	{
		NormalLink,
		ForceLink
	}

	private enum LoginMethodState
	{
		None,
		Failed,
		FacebookSuccess,
		GPGSuccess,
		GameCenterSuccess,
		AppleSuccess,
		TwitterSuccess,
		LineSuccess,
		Cancel
	}

	private LoginMethodState _loginMethodState;

	private LoginMethodType _loginMethod;

	private Action<bool> _onEnd;

	private bool _changeResult;

	private int _connectRetryCount;

	private float _loginFailTime = 90f;

	private float _loginWaitTime = 5f;

	private bool _isSuccess;

	private bool _isComplete;

	private string _loginId = string.Empty;

	private string _verifyInfo = string.Empty;

	private const int _maxConncetRetryCount = 5;

	public StateBase_ChangeLoginMethod(LoginMethodType loginMethod, Action<bool> onEnd)
	{
		_loginMethod = loginMethod;
		_onEnd = onEnd;
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
		Vespa_System_Action_Extension_Method.fire(_onEnd, _changeResult);
		_onEnd = null;
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
		WaitingPopup.Close(this);
		if (_loginMethodState == LoginMethodState.FacebookSuccess)
		{
			_ = (object)null;
			_ = 43;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
			StateOwner.SetNextState(new StateBase_Logined());
		}
		else if (_loginMethodState == LoginMethodState.GPGSuccess)
		{
			_ = (object)null;
			_ = 42;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
			StateOwner.SetNextState(new StateBase_Logined());
		}
		else if (_loginMethodState == LoginMethodState.GameCenterSuccess)
		{
			_ = (object)null;
			_ = 44;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
			StateOwner.SetNextState(new StateBase_Logined());
		}
		else if (_loginMethodState == LoginMethodState.AppleSuccess)
		{
			_ = (object)null;
			_ = 45;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
			StateOwner.SetNextState(new StateBase_Logined());
		}
		else if (_loginMethodState == LoginMethodState.Failed || _loginMethodState == LoginMethodState.None || _loginMethodState == LoginMethodState.Cancel)
		{
			StateOwner.SetNextState(new StateBase_Logined());
		}
		else
		{
			ErrorAndQuit("Error ChangeLoginMethod");
		}
	}

	public override IEnumerator coRun()
	{
		WaitingPopup.Open(this);
		bool isComplete = false;
		string failmsg = "";
		if (_loginMethod == LoginMethodType.Steam)
		{
			GlobalAutoBehaviour<MasangLoginManager>.instance.LinkedSteam(delegate(MasangLoginManager.LoginSuccessState loginstate, string msg)
			{
				switch (loginstate)
				{
				case MasangLoginManager.LoginSuccessState.Success:
					_loginMethodState = LoginMethodState.FacebookSuccess;
					LoginData.LastLoginMethod = LoginMethodType.Steam;
					SceneAutoBehaviour<LobbyManagement>.instance.OpenGameOptionWindow(GameOptionTab.Etc);
					break;
				case MasangLoginManager.LoginSuccessState.Failed:
					_loginMethodState = LoginMethodState.Failed;
					failmsg = msg;
					break;
				case MasangLoginManager.LoginSuccessState.Cancel:
					_loginMethodState = LoginMethodState.Cancel;
					break;
				}
				isComplete = true;
			});
		}
		else if (_loginMethod == LoginMethodType.Apple)
		{
			GlobalAutoBehaviour<MasangLoginManager>.instance.LinkedApple(delegate(MasangLoginManager.LoginSuccessState loginstate, string msg)
			{
				switch (loginstate)
				{
				case MasangLoginManager.LoginSuccessState.Success:
					_loginMethodState = LoginMethodState.AppleSuccess;
					LoginData.LastLoginMethod = LoginMethodType.Apple;
					SceneAutoBehaviour<LobbyManagement>.instance.OpenGameOptionWindow(GameOptionTab.Etc);
					break;
				case MasangLoginManager.LoginSuccessState.Failed:
					_loginMethodState = LoginMethodState.Failed;
					failmsg = msg;
					break;
				case MasangLoginManager.LoginSuccessState.Cancel:
					_loginMethodState = LoginMethodState.Cancel;
					break;
				}
				isComplete = true;
			});
		}
		else if (_loginMethod == LoginMethodType.GooglePlus)
		{
			GlobalAutoBehaviour<MasangLoginManager>.instance.LinkedGoogle(delegate(MasangLoginManager.LoginSuccessState loginstate, string msg)
			{
				switch (loginstate)
				{
				case MasangLoginManager.LoginSuccessState.Success:
					_loginMethodState = LoginMethodState.GPGSuccess;
					LoginData.LastLoginMethod = LoginMethodType.GooglePlus;
					SceneAutoBehaviour<LobbyManagement>.instance.OpenGameOptionWindow(GameOptionTab.Etc);
					break;
				case MasangLoginManager.LoginSuccessState.Failed:
					_loginMethodState = LoginMethodState.Failed;
					failmsg = msg;
					break;
				case MasangLoginManager.LoginSuccessState.Cancel:
					_loginMethodState = LoginMethodState.Cancel;
					break;
				}
				isComplete = true;
			});
		}
		while (!isComplete)
		{
			yield return null;
		}
		if (_loginMethodState == LoginMethodState.Failed)
		{
			bool isFinish = false;
			if (String_Extension_Method.IsNullOrEmpty(failmsg))
			{
				failmsg = "Link Error";
			}
			GlobalMessagePopup.OpenYesOnly("error", failmsg, delegate
			{
				isFinish = true;
			}, ignoreOnClickClose: true);
			while (isFinish)
			{
				yield return null;
			}
		}
	}

	private IEnumerator coAppleGameCenterLogin(Action complete)
	{
		yield return null;
	}

	private IEnumerator coAppleLogin(Action complete)
	{
		yield return null;
	}

	private IEnumerator coSendLinkAccount(Action<bool> onComplete = null, RequestLinkStatus requestLinkStatus = RequestLinkStatus.NormalLink)
	{
		WaitingPopup.Close(this);
		if (String_Extension_Method.IsNullOrEmpty(_loginId))
		{
			VespaDebug.LogError("coSendLinkAccount loginId is null");
			Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
			yield break;
		}
		NShared.LinkAccount.Request request = new NShared.LinkAccount.Request();
		request.LoginMethod = _loginMethod;
		request.LinkedId = _loginId;
		request.LoginVerificationInfo = _verifyInfo;
		request.ForceLink = requestLinkStatus == RequestLinkStatus.ForceLink;
		request.AccountId = (AccountManager.IsLogined() ? GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId : 0);
		bool isComplete = false;
		NShared.LinkAccount.Response response = null;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.LinkAccount.Response iResponse)
		{
			isComplete = true;
			response = iResponse;
		});
		while (!isComplete)
		{
			yield return null;
		}
		if (response == null)
		{
			isComplete = false;
			GlobalMessagePopup.OpenGlobalError(BaseResultType.HttpError, delegate
			{
				isComplete = true;
			});
			while (!isComplete)
			{
				yield return null;
			}
			Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
		}
		else if (response.Result == ResultType.Fail && LoginManagement.IsUsedTokenSignIn(_loginMethod))
		{
			VespaDebug.LogError("[ChangeLoginMethod] Link Authorization Failed!");
			Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
		}
		else if (response.BaseResult != BaseResultType.Success)
		{
			isComplete = false;
			GlobalMessagePopup.OpenGlobalError(response.BaseResult, delegate
			{
				isComplete = true;
			});
			while (!isComplete)
			{
				yield return null;
			}
			Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
		}
		else if (response.Result != ResultType.Success)
		{
			if (response.Result == ResultType.InSignOutProcessing)
			{
				OpenForceLinkPopup(response.SignOutDoneTime, onComplete);
				yield break;
			}
			isComplete = false;
			GlobalMessagePopup.OpenGlobalError(response.Result, delegate
			{
				isComplete = true;
			});
			while (!isComplete)
			{
				yield return null;
			}
			Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
		}
		else
		{
			LoginData.LastLoginUserId = new LoginUserId(_loginId, string.Empty, 0L);
			LoginData.LastLoginMethod = _loginMethod;
			if (!String_Extension_Method.IsNullOrEmpty(_verifyInfo))
			{
				SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value = _verifyInfo;
			}
			GlobalAutoBehaviour<AccountManager>.instance.SetConnected(_loginMethod, _loginId);
			_changeResult = true;
			Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: true);
		}
	}

	private void OpenForceLinkPopup(string signOutDoneTime, Action<bool> onCompleteLinkAccount = null)
	{
		NGame2.NUI.NWindow.SignOutAccountRecoveryPopup.OpenGlobal(delegate
		{
			StartCoroutine(coSendLinkAccount(onCompleteLinkAccount, RequestLinkStatus.ForceLink));
		}, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLinkAccount, i_argA: false);
		}, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLinkAccount, i_argA: false);
		}, signOutDoneTime, isRequestLogin: false);
	}

	private void OpenFailedConnectPopup()
	{
		if (_loginMethodState == LoginMethodState.Failed)
		{
			if (_connectRetryCount >= 5)
			{
				MessagePopup.OpenError($"Timed Out! {_loginMethod.ToString()}, Retry to this.");
			}
			else
			{
				MessagePopup.OpenError($"Failed to Connect with {_loginMethod.ToString()}, Retry to this.");
			}
		}
	}
}
