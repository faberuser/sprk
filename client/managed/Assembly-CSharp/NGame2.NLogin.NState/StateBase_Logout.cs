using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NGuild;
using NGame2.NOrdeal;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.Logout;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NLogin.NState;

public class StateBase_Logout : StateBase
{
	private enum LogoutState
	{
		None,
		Success,
		Failed,
		Canceled
	}

	private Func<StateBase> _cancelStateCreator;

	private string _errorLog = string.Empty;

	private LogoutState _logoutState;

	public StateBase_Logout(Func<StateBase> cancelStateCreator = null)
	{
		_cancelStateCreator = cancelStateCreator;
	}

	public override void OnBegin()
	{
		base.OnBegin();
		string[] array = new string[1] { typeof(StateBase_LoginGameServer).Name };
		if (StateBase.SetNecessaryState(array))
		{
			_logoutState = LogoutState.None;
			StartCoroutine(coEntry());
		}
		else
		{
			ErrorAndQuit($"[{typeof(StateBase_Logout).Name}] must do first {Vespa_String_Extension_Method.ArrayToString(array)}");
		}
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
		if (_logoutState == LogoutState.Canceled)
		{
			if (_cancelStateCreator != null)
			{
				StateOwner.SetNextState(Vespa_System_Action_Extension_Method.fire(_cancelStateCreator));
			}
			else
			{
				StateOwner.SetNextState(new StateBase_Logined());
			}
		}
		else if (_logoutState == LogoutState.Success)
		{
			LoginData.LastLoginMethod = LoginMethodType.None;
			LoginData.ClearLastLoginData();
			SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value = string.Empty;
			GlobalAutoBehaviour<ChattingManager>.instance.ClearChatList(ChatType.None, 0L);
			AccountManager.Logout();
			if (GlobalAutoBehaviour<GuildManager>.isValidInstance)
			{
				GlobalAutoBehaviour<GuildManager>.instance.RemoveMyGuild();
			}
			if (GlobalAutoBehaviour<OrdealArenaManager>.isValidInstance)
			{
				GlobalAutoBehaviour<OrdealArenaManager>.instance.ResetAll();
			}
			if (SceneAutoBehaviour<MailManagement>.isValidInstance)
			{
				SceneAutoBehaviour<MailManagement>.instance.Reset();
			}
			StateBase.Reset();
			SingletonUpdater<LoginManager>.instance.ReturnToLogin();
		}
		else if (_logoutState == LogoutState.Failed)
		{
			ErrorAndQuit(_errorLog);
		}
		else
		{
			ErrorAndQuit("Error Logout");
		}
	}

	public override IEnumerator coRun()
	{
		string title = LocalizationManager.GetString(Account.LOGOUT_TITLE);
		string message = LocalizationManager.GetString(Account.LOGOUT_MESSAGE);
		bool isPopupClosed = false;
		bool popupSelectYes = false;
		GlobalMessagePopup.OpenGlobal(title, message, delegate
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
		if (popupSelectYes)
		{
			bool isComplete = false;
			NShared.Logout.Request request = new NShared.Logout.Request();
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.None, delegate(NShared.Logout.Response response)
			{
				SprkAccounts.FinishLogout(response);
				if (response.BaseResult == BaseResultType.Success)
				{
					if (response.Result == ResultType.Success)
					{
						_logoutState = LogoutState.Success;
					}
					else
					{
						_errorLog = response.Result.ToString();
						_logoutState = LogoutState.Failed;
					}
				}
				else
				{
					_errorLog = response.BaseResult.ToString();
					_logoutState = LogoutState.Failed;
				}
				isComplete = true;
			});
			while (!isComplete)
			{
				yield return null;
				if (_logoutState != LogoutState.None)
				{
					break;
				}
			}
		}
		else
		{
			_logoutState = LogoutState.Canceled;
		}
	}
}
