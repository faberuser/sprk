using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NConfig;
using NGame2.NLogin.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_LoginGameServer : StateBase
{
	private enum LoginGameServerState
	{
		Normal,
		NullLoginInfo,
		FailedLogin,
		CancelLogin
	}

	public static LocalStorageBool TEST_Force_DuplicateUserInfo = new LocalStorageBool(i_default: false, "StateBase_LoginGameServerTEST_Force_DuplicateUserInfo");

	public static bool TEST_Force_DuplicateUserInfo_Used = false;

	private LoginUserId _loginUserId;

	private LoginMethodType _loginMethod;

	private LoginGameServerState _loginState;

	private string _loginVerificationInfo = string.Empty;

	private bool isShowUnsuitableMsg;

	private bool isShowGuestLoginMsg;

	private bool _isLoginMethod;

	private bool _isShowGuestLoginPopup;

	private bool _isChangeVerification;

	private float _loginWaitTime = 10f;

	private const float LoginWaitTimeSeconds = 10f;

	public StateBase_LoginGameServer(string loginId, LoginMethodType loginMethod, string loginVerificationInfo = "", bool isShowGuestLoginPopup = false)
	{
		_loginUserId = new LoginUserId(loginId, string.Empty, 0L);
		_loginMethod = loginMethod;
		_loginVerificationInfo = loginVerificationInfo;
		_isShowGuestLoginPopup = isShowGuestLoginPopup;
	}

	public StateBase_LoginGameServer(LoginUserId loginUserId, LoginMethodType loginMethod, string loginVerificationInfo = "", bool isShowGuestLoginPopup = false)
	{
		_loginUserId = loginUserId;
		_loginMethod = loginMethod;
		_loginVerificationInfo = loginVerificationInfo;
		_isShowGuestLoginPopup = isShowGuestLoginPopup;
	}

	public override void OnBegin()
	{
		base.OnBegin();
		_ = (object)null;
		_ = 2;
		_ = 1;
		_ = 1;
		Array.Empty<object>();
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
		if (_loginState == LoginGameServerState.Normal)
		{
			if (StateBase.SetNecessaryState(new string[1] { typeof(StateBase_WaitTouch).Name }))
			{
				StateOwner.SetNextState(new StateBase_PatchDownload());
				return;
			}
			SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginBackground>().ShowMessage(isShowUnsuitableMsg, isShowGuestLoginMsg);
			StateOwner.SetNextState(new StateBase_WaitTouch());
		}
		else if (_loginState == LoginGameServerState.NullLoginInfo)
		{
			StateOwner.SetNextState(new StateBase_SelectLoginMethod());
		}
		else if (_loginState == LoginGameServerState.FailedLogin)
		{
			StateOwner.SetNextState(new StateBase_SelectLoginMethod());
		}
		else if (_loginState == LoginGameServerState.CancelLogin)
		{
			StateOwner.SetNextState(new StateBase_WaitTouch());
		}
		else
		{
			ErrorAndQuit("Error. Login GameServer.");
		}
	}

	public override IEnumerator coRun()
	{
		if (String_Extension_Method.IsNullOrEmpty(_loginUserId.id))
		{
			yield return new WaitPopup("Error", "LoginId is empty");
			_loginState = LoginGameServerState.NullLoginInfo;
			yield break;
		}
		AppServerInfo serverInfo = SingletonUpdater<LoginManager>.instance.ServerInfo;
		if (serverInfo == null)
		{
			yield return new WaitPopup("Error", "Server data not found. name=" + LoginData.LastServerName);
			_loginState = LoginGameServerState.NullLoginInfo;
			yield break;
		}
		if (String_Extension_Method.IsNullOrEmpty(serverInfo.Host))
		{
			yield return new WaitPopup("Error", "Server host is empty. name=" + LoginData.LastServerName);
			_loginState = LoginGameServerState.NullLoginInfo;
			yield break;
		}
		int num = serverInfo.Host.IndexOf("://");
		if (num >= 0)
		{
			serverInfo.Host = serverInfo.Host.Substring(num + 3);
		}
		string host = serverInfo.Host;
		bool https = serverInfo.Https;
		string cookieKeyword = serverInfo.CookieKeyword;
		UnityLogQueue.Enqueue("LoginId : " + _loginUserId.id, string.Empty, LogType.Log);
		UnityLogQueue.Enqueue("(Legacy) LoginId : " + _loginUserId.legacyId, string.Empty, LogType.Log);
		if (String_Extension_Method.IsNullOrEmpty(host))
		{
			yield return new WaitPopup("Error", "Server address is empty");
			_loginState = LoginGameServerState.NullLoginInfo;
			yield break;
		}
		ClientConfig clientConfig = SingletonAuto<Config>.instance.ClientConfig;
		if (clientConfig == null)
		{
			yield return new WaitPopup("Error", "Client config is null");
			_loginState = LoginGameServerState.NullLoginInfo;
			yield break;
		}
		LoginProgress loginProgress = new LoginProgress(host, cookieKeyword, https, clientConfig, _loginUserId, _loginMethod, _loginVerificationInfo);
		loginProgress.OnMessage = delegate(string message2)
		{
			StateBase.SetBgMessage(message2, animate: true);
		};
		loginProgress.Start();
		while (loginProgress.Status == LoginProgress.EStatus.Progressing)
		{
			yield return null;
		}
		if (loginProgress.Status == LoginProgress.EStatus.Error_AuthFail)
		{
			string stringDirect = LocalizationManager.GetStringDirect("RESULT_TYPE.AUTH_FAIL");
			yield return new WaitPopup("Error", stringDirect);
			LoginData.LastLoginMethod = LoginMethodType.None;
			SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value = string.Empty;
			_loginState = LoginGameServerState.FailedLogin;
			yield break;
		}
		if (loginProgress.Status == LoginProgress.EStatus.Cancel_DisabledLogin)
		{
			yield return new WaitPopup("Error", "Tried LoginMethod has Disabled..");
			_loginState = LoginGameServerState.FailedLogin;
			yield break;
		}
		if (loginProgress.Status == LoginProgress.EStatus.Error_GetCertificateFail)
		{
			yield return new WaitPopup("Error", "Invalid Certificate");
			_loginState = LoginGameServerState.FailedLogin;
			yield break;
		}
		if (loginProgress.Status == LoginProgress.EStatus.Error_Login)
		{
			if (loginProgress.LoginResponse == null)
			{
				yield return new WaitPopup("Error", "Invalid Login Response");
				_loginState = LoginGameServerState.FailedLogin;
			}
			else if (loginProgress.LoginResponse.BaseResult != BaseResultType.Success)
			{
				yield return new WaitPopup("Login", loginProgress.LoginResponse.BaseResult.ToString());
				_loginState = LoginGameServerState.FailedLogin;
			}
			else
			{
				yield return new WaitPopup("Login", loginProgress.LoginResponse.Result.ToString());
				_loginState = LoginGameServerState.FailedLogin;
			}
			yield break;
		}
		if (loginProgress.Status == LoginProgress.EStatus.Cancel_Login)
		{
			_loginState = LoginGameServerState.CancelLogin;
			yield break;
		}
		if (_isShowGuestLoginPopup && !loginProgress.isInSignOutProcessing)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
			string message = LocalizationManager.GetString(NShared.NLocalization.System.LOGIN_GUEST_WARNING);
			GlobalMessagePopup.OpenGlobal(title, message, null, null);
		}
		ESpecType eSpecType = ((SystemInfo.systemMemorySize < 1500) ? ESpecType.Low : ESpecType.Mid);
		isShowUnsuitableMsg = false;
		if (eSpecType == ESpecType.Low && SystemOptionData.Spec != eSpecType)
		{
			isShowUnsuitableMsg = true;
		}
		isShowGuestLoginMsg = _loginMethod == LoginMethodType.Guest;
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			GlobalAutoBehaviour<AccountManager>.CreateInstance();
		}
		if (object.Equals(loginProgress, null))
		{
			VespaDebug.LogError("LoginProgress Is Null!!");
			GlobalAutoBehaviour<AccountManager>.instance.OnLoginResponse(null);
		}
		else
		{
			GlobalAutoBehaviour<AccountManager>.instance.OnLoginResponse(loginProgress.LoginResponse);
		}
		_ = _loginMethod;
		_loginState = LoginGameServerState.Normal;
	}
}
