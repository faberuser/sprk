using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NLogin.NState;

public class StateBase_Login : StateBase
{
	private enum LoginState
	{
		Login,
		LoginAccountID,
		NeedSelectServer,
		SocialLogin
	}

	private LoginUserId _loginUserId;

	private LoginMethodType _loginMethod;

	private LoginState _loginState;

	private bool _isUserSelect;

	private string _loginVerification = string.Empty;

	private string _delete_dt = string.Empty;

	private string _loginid = string.Empty;

	public StateBase_Login()
	{
	}

	public StateBase_Login(LoginMethodType methodType, bool UserSelect = false)
	{
		_loginMethod = methodType;
		_isUserSelect = UserSelect;
	}

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
		if (_loginState == LoginState.Login)
		{
			if (!String_Extension_Method.IsNullOrEmpty(_loginUserId.id) && _loginMethod != LoginMethodType.None && !String_Extension_Method.IsNullOrEmpty(_loginVerification) && !String_Extension_Method.IsNullOrEmpty(_loginid))
			{
				StateOwner.SetNextState(new StateBase_LoginGameServer(_loginid, _loginMethod, _loginVerification));
			}
			else
			{
				StateOwner.SetNextState(new StateBase_WaitTouch());
			}
		}
		else if (_loginState == LoginState.LoginAccountID)
		{
			StateOwner.SetNextState(new StateBase_LoginGameServer(_loginUserId, _loginMethod));
		}
		else if (_loginState == LoginState.NeedSelectServer)
		{
			LoginData.LastLoginMethod = LoginMethodType.None;
			StateOwner.SetNextState(new StateBase_SelectLoginMethod());
		}
		else if (_loginState == LoginState.SocialLogin)
		{
			if (_loginMethod == LoginMethodType.Guest)
			{
				StateOwner.SetNextState(new StateBase_LoginMethod_Guest());
			}
			else if (_loginMethod == LoginMethodType.Steam)
			{
				StateOwner.SetNextState(new StateBase_LoginMethod_Steam());
			}
			else if (_loginMethod == LoginMethodType.GooglePlus)
			{
				StateOwner.SetNextState(new StateBase_LoginMethod_GPGS());
			}
			else if (_loginMethod == LoginMethodType.GameCenter)
			{
				StateOwner.SetNextState(new StateBase_LoginMethod_AppleGameCenter());
			}
			else if (_loginMethod == LoginMethodType.Apple)
			{
				StateOwner.SetNextState(new StateBase_LoginMethod_Apple());
			}
			else
			{
				ErrorAndQuit($"Not supported Social Login Type({_loginMethod})");
			}
		}
	}

	public override IEnumerator coRun()
	{
		_ = string.Empty;
		string forcedAccountId = SingletonUpdater<LoginManager>.instance.ForcedAccountId;
		if (!String_Extension_Method.IsNullOrEmpty(forcedAccountId))
		{
			int accountId = Vespa_String_Extension_Method.TOINT(forcedAccountId, 0);
			if (accountId != 0)
			{
				bool isComplete = false;
				SingletonUpdater<LoginManager>.instance.RequestLoginIdByAccountId(accountId, delegate(string loginId)
				{
					isComplete = true;
					_loginUserId = new LoginUserId(loginId, string.Empty, accountId);
				});
				while (!isComplete)
				{
					yield return null;
				}
				_loginMethod = LoginMethodType.Guest;
				_loginState = LoginState.LoginAccountID;
				yield break;
			}
		}
		LoginMethodType lastLoginMethod = LoginData.LastLoginMethod;
		_ = SingletonUpdater<LoginManager>.instance.Host.Server;
		string lastServerName = LoginData.LastServerName;
		if (_isUserSelect)
		{
			LoginData.LastLoginUserId = new LoginUserId(string.Empty, string.Empty, 0L);
			LoginData.LastAccessToken = "";
			LoginData.LastRefreshToken = "";
			yield return Login(_loginMethod, lastServerName);
		}
		else if (SingletonUpdater<LoginManager>.instance.IsReadyLogin)
		{
			yield return Login(lastLoginMethod, lastServerName);
		}
		else
		{
			LoginData.LastAccessToken = "";
			LoginData.LastRefreshToken = "";
			_loginUserId = new LoginUserId(string.Empty, string.Empty, 0L);
			_loginMethod = LoginMethodType.None;
			_loginState = LoginState.Login;
		}
	}

	private IEnumerator Login(LoginMethodType loginMethod, string serverName)
	{
		SingletonUpdater<LoginManager>.instance.SetServerName(serverName);
		AppServerInfo serverInfo = SingletonUpdater<LoginManager>.instance.ServerInfo;
		if (serverInfo == null)
		{
			yield return new WaitQuitError("No available server to query status : " + serverName);
		}
		QueryServerDown queryServerDown = new QueryServerDown();
		bool isComplete = false;
		queryServerDown.Query(serverInfo.QueryServerStatusUrl, delegate
		{
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
		if (!queryServerDown.Verify())
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string resultMessage = queryServerDown.ResultMessage;
			isComplete = false;
			GlobalMessagePopup.OpenGlobal(title, resultMessage, delegate
			{
				isComplete = true;
			}, null);
			while (!isComplete)
			{
				yield return null;
			}
			_loginState = LoginState.NeedSelectServer;
		}
		else if (loginMethod != LoginMethodType.None)
		{
			_ = LoginData.LastLoginUserId;
			if (!String_Extension_Method.IsNullOrEmpty(LoginData.LastLoginUserId.id))
			{
				_loginUserId = LoginData.LastLoginUserId;
				_loginMethod = loginMethod;
				_loginState = LoginState.Login;
				if (!String_Extension_Method.IsNullOrEmpty(LoginData.LastAccessToken))
				{
					bool iscomplete = false;
					bool isdeleteid = false;
					string delete_dt = string.Empty;
					GlobalAutoBehaviour<MasangLoginManager>.instance.AccessTokenLogin(delegate(bool issuccess, LoginSuccData logindata)
					{
						if (issuccess)
						{
							_loginid = logindata.id;
							_loginVerification = logindata.sessionid;
						}
						else if (logindata != null)
						{
							isdeleteid = true;
							delete_dt = logindata.sessionid;
						}
						iscomplete = true;
					});
					while (!iscomplete)
					{
						yield return null;
					}
					if (isdeleteid)
					{
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
											_loginid = logindata.id;
											_loginVerification = logindata.sessionid;
										}
										isdeletpopupfinish = true;
									});
								}
								else
								{
									isdeletpopupfinish = true;
								}
							});
						}, delegate
						{
							isdeletpopupfinish = true;
						}, delegate
						{
							isdeletpopupfinish = true;
						}, delete_dt);
						while (!isdeletpopupfinish)
						{
							yield return null;
						}
					}
					if (!String_Extension_Method.IsNullOrEmpty(_loginid))
					{
						yield break;
					}
				}
				LoginData.LastAccessToken = "";
				LoginData.LastRefreshToken = "";
				_loginUserId = new LoginUserId(string.Empty, string.Empty, 0L);
				_loginMethod = LoginMethodType.None;
				_loginState = LoginState.Login;
			}
			else
			{
				LoginData.LastAccessToken = "";
				LoginData.LastRefreshToken = "";
				_loginUserId = new LoginUserId(string.Empty, string.Empty, 0L);
				_loginMethod = loginMethod;
				_loginState = LoginState.SocialLogin;
			}
		}
		else
		{
			yield return new WaitQuitError($"Not supported Login Type({loginMethod})");
		}
	}
}
