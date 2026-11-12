using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NConfig;
using NGame2.NDevice;
using NGame2.NLogin.NHelper;
using NGame2.NLogin.NState;
using NGame2.NNotice;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset.NPatch;
using NShared;
using NShared.GetLoginId;
using NShared.Login;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Scripting;

namespace NGame2.NLogin;

public class LoginManager : SingletonUpdater<LoginManager>, StateBase.IStateOwner
{
	public KRConnectorInfo KRConnectorInfo = new KRConnectorInfo();

	public LocalStorageBool FirstLogined = new LocalStorageBool(i_default: false, "LoginData_FirstLogined");

	public LocalStorageBool ForceBackgroundDownload = new LocalStorageBool(i_default: false, "ForceBackgroundDownload");

	public string ForceLoginId = string.Empty;

	public string CountryCode = string.Empty;

	public CheckInAppBilling CheckInAppBilling = new CheckInAppBilling();

	public LocalStorageString PatchQueryHost = new LocalStorageString(string.Empty, "PatchQueryHost");

	public LocalStorageString ForcedServerIp = new LocalStorageString(string.Empty, "ForcedServerIp");

	public LocalStorageString ForceLoginServer = new LocalStorageString(string.Empty, "ForceLoignServer");

	public LocalStorageString BinaryVersionNumber = new LocalStorageString(string.Empty, "OverwriteBinaryVersionNumber");

	public LocalStorageInt ForceMarketType = new LocalStorageInt(0, "ForceMarketType");

	public LocalAccountBool AdultAgreementAccepted = new LocalAccountBool(i_default: false, "LoginData_AdultAgreementAccepted");

	public LocalStorageString LastPostfIx = new LocalStorageString(string.Empty, "LoginData_AgreementAccepted_LastPostfIx");

	private LocalStorageString _forcedAccountId = new LocalStorageString(string.Empty, typeof(LoginManager).FullName + "_ForcedAccountId");

	public LocalStorageString LastLoginVerificationInfo = new LocalStorageString(string.Empty, "LastLoginVerificationInfo");

	public bool IsSessionExpired;

	public bool ResetedLoginData;

	private StateBase _state;

	private StateBase _nextState;

	private StateBase _subState;

	private List<StateBase> _runningSubStates = new List<StateBase>();

	public static ClientConfig ClientConfig => SingletonAuto<Config>.instance.ClientConfig;

	public bool IsNewUser { get; set; }

	public bool IsFullDownload { get; set; }

	public bool IsLogined
	{
		get
		{
			if (_state != null)
			{
				return _state is StateBase_Logined;
			}
			return false;
		}
	}

	public string ServerName
	{
		get
		{
			if (ServerInfo == null)
			{
				return string.Empty;
			}
			return ServerInfo.Name;
		}
	}

	public VersionedHost Host { get; private set; }

	public AppVersionInfo VersionInfo { get; private set; }

	public AppServerInfo ServerInfo { get; private set; }

	public string ForcedAccountId
	{
		get
		{
			return _forcedAccountId.Value;
		}
		set
		{
			_forcedAccountId.Value = value;
		}
	}

	public bool IsReadyLogin
	{
		get
		{
			if (LoginData.LastLoginMethod != LoginMethodType.None && ServerInfo != null && SingletonAuto<Config>.instance.ClientConfig != null)
			{
				return true;
			}
			return false;
		}
	}

	public bool AgreementAccepted
	{
		get
		{
			try
			{
				string agreementAcceptedKey = GetAgreementAcceptedKey();
				if (!SprkTestPrefs.HasKey(agreementAcceptedKey))
				{
					return false;
				}
				return SprkTestPrefs.GetInt(agreementAcceptedKey, 0) > 0;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				return false;
			}
		}
		set
		{
			try
			{
				string agreementAcceptedKey = GetAgreementAcceptedKey(renewLastVersion: true);
				int value2 = (value ? 1 : 0);
				SprkTestPrefs.SetInt(agreementAcceptedKey, value2);
				SprkTestPrefs.Save();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
	}

	public void ResumeNextState()
	{
		StateBase_InAppBilling.IsReady = true;
		SetNextState(new StateBase_BackgroundDownload());
	}

	public LoginManager()
	{
		CountryCode = DeviceHelper.GetCountryCode();
		string kRConnectorPreferenceStr = SingletonUpdater<LoginManager>.instance.KRConnectorInfo.GetKRConnectorPreferenceStr("country");
		if (!string.IsNullOrEmpty(kRConnectorPreferenceStr))
		{
			CountryCode = kRConnectorPreferenceStr;
		}
		GlobalAutoBehaviour<GuiManager>.instance.Add("LoginManager", this, delegate
		{
			using (new BlockManager().Vertical(GUI.skin.box, GUILayout.Width(500f), GUILayout.Height(300f)).Scroll(this, "MessageLog", alwaysShowHorizontal: false, alwaysShowVertical: true))
			{
				using (new BlockManager().Horizontal())
				{
					if (GUILayout.Button("SignOut", GUILayout.Width(150f), GUILayout.Height(50f)))
					{
						SignOut();
					}
					ClientDebugNetwork.ForceSessionExpired = GUILayout.Toggle(ClientDebugNetwork.ForceSessionExpired, "Force SessionExpired");
					ForceBackgroundDownload.Value = GUILayout.Toggle(ForceBackgroundDownload.Value, "BackgroundDownload");
				}
				if (GUILayout.Button("Move to lobby", GUILayout.Width(150f)))
				{
					GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
				}
				if (GUILayout.Button("SessionExpired", GUILayout.Width(150f)))
				{
					SetNextState(new StateBase_SessionExpired());
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("Patch Query Host");
					PatchQueryHost.Value = GUILayout.TextField(PatchQueryHost.Value);
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("Server Ip");
					ForcedServerIp.Value = GUILayout.TextField(ForcedServerIp.Value);
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("LoginServer Ip");
					ForceLoginServer.Value = GUILayout.TextField(ForceLoginServer.Value);
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("Binary Ver.");
					BinaryVersionNumber.Value = GUILayout.TextField(BinaryVersionNumber.Value);
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("Account Id Login");
					_forcedAccountId.Value = GUILayout.TextField(_forcedAccountId.Value);
					if (SceneAutoBehaviour<LoginManagement>.isValidInstance && SceneAutoBehaviour<LoginManagement>.instance.IsOpened<LoginSelect>() && GUILayout.Button("Login"))
					{
						SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginSelect>().LoginByAccountId(ForcedAccountId);
					}
				}
				if (GlobalAutoBehaviour<PatchDownloader>.isValidInstance)
				{
					using (new BlockManager().Horizontal())
					{
						GUILayout.Label($"BackgroundDownload({GlobalAutoBehaviour<PatchDownloader>.instance.State})");
						if (GUILayout.Button("Pause"))
						{
							GlobalAutoBehaviour<PatchDownloader>.instance.Pause();
						}
						if (GUILayout.Button("Resume"))
						{
							GlobalAutoBehaviour<PatchDownloader>.instance.Resume();
						}
					}
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label($"Download File Write SleepTime: ({BackgroundDownloader.DownloadFileWriteSleepTime.Value}ms)");
					BackgroundDownloader.DownloadFileWriteSleepTime.Value = GUILayout.TextField(BackgroundDownloader.DownloadFileWriteSleepTime.Value);
				}
				if (SingletonUpdater<GCManager>.isValidInstance)
				{
					using (new BlockManager().Horizontal())
					{
						GUILayout.Label($"GCManager({GarbageCollector.GCMode})");
						if (GUILayout.Button("Disable"))
						{
							SingletonUpdater<GCManager>.instance.DisableGC();
						}
						if (GUILayout.Button("Enable"))
						{
							SingletonUpdater<GCManager>.instance.EnableGC();
						}
					}
				}
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StateBase.MessageLogList); i++)
				{
					StateBase.MessageTimeData messageTimeData = StateBase.MessageLogList[i];
					GUILayout.Label($"{messageTimeData.Message}: {messageTimeData.EndTime - messageTimeData.BeginTime}s");
				}
			}
		});
	}

	public void Login(string forceLoginId)
	{
		if (string.IsNullOrEmpty(LoginData.LastServerName))
		{
			LoginData.IsNewAccount = true;
		}
		IsSessionExpired = false;
		ClientDebugNetwork.ForceSessionExpired = false;
		StateBase.MessageLogList.Clear();
		ForceLoginId = forceLoginId;
		if (!string.IsNullOrEmpty(forceLoginId))
		{
			LoginData.LastLoginUserId = default(LoginUserId);
		}
		SetNextState(new StateBase_CheckClientExpired());
	}

	public void SetHostAndVersion(VersionedHost host, AppVersionInfo versionInfo)
	{
		Host = host;
		VersionInfo = versionInfo;
		string serverName = LoginData.LastServerName;
		if (!String_Extension_Method.IsNullOrEmpty(ClientConfig.ForcedServerName))
		{
			serverName = ClientConfig.ForcedServerName;
		}
		SetServerName(serverName);
		SingletonUpdater<WebServiceManager>.instance.Requester.OnBaseResultError = OnBaseResultError;
		SingletonUpdater<WebServiceManager>.instance.Requester.OnBaseResponse = OnBaseResponse;
		SingletonUpdater<WebServiceManager>.instance.Requester.OnRetryRequest = OnRetryRequest;
	}

	public void SetServerName(string serverName)
	{
		ServerInfo = Helper.GetServerSafely(Host, serverName);
		if (ServerInfo != null)
		{
			LoginData.LastServerName = ServerInfo.Name;
		}
		else
		{
			VespaDebug.LogError($"null server: Name={serverName}, Country={CountryCode}");
		}
	}

	public void ChangeLoginMethod(LoginMethodType loginMethod, Action<bool> onEnd)
	{
		if (LoginData.LastLoginMethod != loginMethod && AccountManager.IsLogined())
		{
			SetNextState(new StateBase_ChangeLoginMethod(loginMethod, onEnd));
		}
	}

	public void SetNextState(StateBase nextState)
	{
		_nextState = nextState;
	}

	public void Relogin(Action<NShared.Login.ResultType> onLogin, string forwardedHost = null, float delay = 0f)
	{
		SetNextState(new StateBase_Relogin(onLogin, forwardedHost, delay));
	}

	public void Logout()
	{
		if (IsLogined)
		{
			SetNextState(new StateBase_Logout());
		}
	}

	public void SignOut()
	{
		if (IsLogined)
		{
			LoginManagement.IsNowSignOutProgressed = true;
			SetNextState(new StateBase_SignOut());
		}
	}

	public void ReturnToLogin(LoadingManager.ELoadingType loadingType = LoadingManager.ELoadingType.Default)
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance)
		{
			SingletonUpdater<TutorialManager>.instance.StopTutorial();
		}
		if (GlobalAutoBehaviour<PatchDownloader>.instance != null)
		{
			GlobalAutoBehaviour<PatchDownloader>.instance.ReturnToLogin();
			BackgroundDownloader.Close();
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.Reset();
		SingletonUpdater<NVespa.NTask.TaskManager>.instance.CancelAllTask();
		GlobalTopmostWindow.instance.Reset();
		StateBase_PatchDownload.IsReady = false;
		StateBase_LoadResource.IsReady = false;
		AccountManager.Reset();
		ForceEndSubState();
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Login, loadingType);
	}

	public void ForceEndSubState()
	{
		if (_runningSubStates == null)
		{
			return;
		}
		foreach (StateBase runningSubState in _runningSubStates)
		{
			if (runningSubState.IsRunning())
			{
				runningSubState.OnEnd();
			}
		}
		_runningSubStates.Clear();
	}

	public override void ChainUpdate()
	{
		if (_subState != null)
		{
			_subState.StateOwner = this;
			_subState.OnBegin();
			SetCrashReportLog("SubState_OnBegin", _subState.GetType().Name);
			if (!_runningSubStates.Contains(_subState))
			{
				_runningSubStates.Add(_subState);
			}
			_subState = null;
		}
		if (_nextState != null)
		{
			if (_state != null)
			{
				_state.OnEnd();
				SetCrashReportLog("MainState_OnEnd", _state.GetType().Name);
			}
			_state = _nextState;
			_nextState = null;
			StateBase.SetBgMessage((_state != null) ? $"[State]{_state.GetType().Name}" : string.Empty, animate: false);
			if (_state != null)
			{
				_state.StateOwner = this;
				_state.OnBegin();
				SetCrashReportLog("MainState_OnBegin", _state.GetType().Name);
			}
		}
		if (_state != null)
		{
			_state.ChainUpdate();
		}
	}

	private void SetCrashReportLog(string type, string state)
	{
	}

	private bool OnBaseResultError(BaseResultType resultType, NShared.Response response)
	{
		switch (resultType)
		{
		case BaseResultType.Offline:
			GlobalMessagePopup.OpenGlobalError(resultType);
			return false;
		case BaseResultType.SessionError:
			OnSessionExpired();
			return true;
		case BaseResultType.InternalServerError:
			OnInternalServerError((response != null) ? response.InternalErrorMessage : string.Empty);
			return false;
		case BaseResultType.InvalidAppError:
			OnInvalidAppError((response != null) ? response.InternalErrorMessage : string.Empty);
			return false;
		default:
			return false;
		}
	}

	public void OnSessionExpired()
	{
		IsSessionExpired = true;
		if (AccountManager.IsLogined() && (_state == null || !(_state is StateBase_SessionExpired)))
		{
			if (LoginManagement.IsNowSignOutProgressed)
			{
				SetNextState(new StateBase_SignOut());
			}
			else
			{
				SetNextState(new StateBase_SessionExpired());
			}
		}
	}

	private void OnInternalServerError(string error)
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.System.HTTP_INTERNAL_SERVER_ERROR);
		if (!String_Extension_Method.IsNullOrEmpty(error))
		{
			text = text + "\n" + error;
		}
		GlobalMessagePopup.OpenGlobalError(text, delegate
		{
		});
	}

	private void OnInvalidAppError(string error)
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.System.INVALID_APP);
		if (!String_Extension_Method.IsNullOrEmpty(error))
		{
			text = text + "\n" + error;
		}
		GlobalMessagePopup.OpenGlobalError(text, delegate
		{
		});
	}

	private void OnBaseResponse(NShared.Response response)
	{
		if (response == null || !AccountManager.IsLogined())
		{
			return;
		}
		AchievementInfo[] achievementInfos = response.AchievementInfos;
		if (__Extension_Method_ICollection__.COUNT(achievementInfos) > 0 && GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			GlobalAutoBehaviour<AccountManager>.instance.AchievementManager?.Apply(achievementInfos);
		}
		GlobalAutoBehaviour<AccountManager>.instance.QuestManager.Apply(response.ReservedMainQuestInfo);
		GlobalAutoBehaviour<AccountManager>.instance.QuestManager.Apply(response.ReservedSubQuestInfos, isComplete: false);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.ReservedStaminaResults);
		GlobalAutoBehaviour<AccountManager>.instance.PlayRecordManager.Apply(response.ReservedPlayRecordInfos);
		GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.Apply(response.ReservedPlayerArchiveInfos);
		GlobalAutoBehaviour<AccountManager>.instance.ClearMissionManager.Apply(response.ReservedClearMissionInfos);
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyLevel(response.ReservedUpdatedEquipItemInfos);
		GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Apply(response.ReservedPurchaseMarketingInfos);
		GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Set(response.ReservePlayerKingsPassExpInfo);
		GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Apply(response.ReservedKingsPassMissionInfos);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.ReservedCurrencyResults);
		GlobalAutoBehaviour<AccountManager>.instance.TeamLevelBuffManager.Apply(response.ReserveTeamLevelBuffInfos);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(response.ReservedNoticeInfos))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(response.ReservedNoticeInfos); i++)
			{
				SingletonAuto<NoticeManager>.instance.AddNotice(response.ReservedNoticeInfos[i]);
			}
		}
	}

	private IEnumerator OnRetryRequest(string error, bool retry, bool showError = true)
	{
		NShared.NLocalization.System system = (string.IsNullOrEmpty(error) ? NShared.NLocalization.System.HTTP_RETRY : NShared.NLocalization.System.HTTP_ERROR_RETRY);
		if (error != null)
		{
			if (error.ToLower().Contains("connection refused"))
			{
				system = NShared.NLocalization.System.HTTP_CONNECTION_REFUSED;
			}
			else if (error.ToLower().Contains("failed to connect to"))
			{
				system = NShared.NLocalization.System.HTTP_CONNECTION_REFUSED;
			}
			else if (error.StartsWith("500 Internal Server Error"))
			{
				system = NShared.NLocalization.System.HTTP_INTERNAL_SERVER_ERROR;
			}
			else if (error.IndexOf("UnknownHostException", StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				system = NShared.NLocalization.System.HTTP_UNKNOWN_HOST_ERROR;
			}
		}
		bool pressed = false;
		string message = LocalizationManager.GetString(system, error);
		if (showError)
		{
			GlobalMessagePopup.OpenGlobalError(message, delegate
			{
				pressed = true;
			}, retry);
		}
		else
		{
			pressed = true;
		}
		while (!pressed)
		{
			yield return null;
		}
	}

	public void BattleTest(Action onEnd, Func<bool> stopFunc)
	{
		DateTime battleStartTime = DateTime.UtcNow;
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			InitBattlePreAllocations();
			RunBattleTest(stopFunc);
		}, (Action)delegate
		{
			_ = (DateTime.UtcNow - battleStartTime).TotalSeconds;
			Vespa_System_Action_Extension_Method.fire(onEnd);
		});
	}

	private void InitBattlePreAllocations()
	{
		SnapshotObjectPoolOption.PoolEnabled = true;
		SnapshotObjectPoolContainer.Reset();
	}

	private void RunBattleTest(Func<bool> stopFunc)
	{
		SingletonAuto<BattleFacade>.CreateInstance();
		NShared.BattleTest.Run<BattleInstance>(1, 1, 0, 3000, null, stopFunc);
	}

	public void RequestLoginIdByAccountId(long accountId, Action<string> onLoginId)
	{
		NShared.GetLoginId.Request request = new NShared.GetLoginId.Request();
		request.AccountId = accountId;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.None, delegate(NShared.GetLoginId.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				GlobalMessagePopup.OpenGlobalError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onLoginId, null);
			}
			else if (response.Result != NShared.GetLoginId.ResultType.Success)
			{
				GlobalMessagePopup.OpenGlobalError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onLoginId, null);
			}
			else
			{
				string loginId = response.LoginId;
				if (String_Extension_Method.IsNullOrEmpty(loginId))
				{
					GlobalMessagePopup.OpenGlobalError("Failed to get LoginId of account=" + accountId);
					Vespa_System_Action_Extension_Method.fire(onLoginId, null);
				}
				else
				{
					Vespa_System_Action_Extension_Method.fire(onLoginId, loginId);
				}
			}
		});
	}

	private string GetPostfix()
	{
		string result = LastPostfIx.Value;
		if (GameRoot.Instance.CountryDataContainer != null && GameRoot.Instance.CountryDataContainer.GetAllData().Count > 0)
		{
			CountryCode = DeviceHelper.GetCountryCode();
			string kRConnectorPreferenceStr = SingletonUpdater<LoginManager>.instance.KRConnectorInfo.GetKRConnectorPreferenceStr("country");
			if (!string.IsNullOrEmpty(kRConnectorPreferenceStr))
			{
				CountryCode = kRConnectorPreferenceStr;
			}
			CountryData countryData = GameRoot.Instance.CountryDataContainer.GetData(CountryCode);
			if (countryData == null)
			{
				countryData = GameRoot.Instance.CountryDataContainer.GetFirstData();
				CountryCode = "US";
			}
			result = ((countryData.Version > 0) ? $"{CountryCode}_{countryData.Version}" : "");
		}
		return result;
	}

	private string GetAgreementAcceptedKey(bool renewLastVersion = false)
	{
		string postfix = GetPostfix();
		if (renewLastVersion)
		{
			LastPostfIx.Value = postfix;
		}
		return "LoginData_AgreementAccepted_new" + postfix;
	}

	public void SetSubState(StateBase subState)
	{
		if (subState != null)
		{
			subState.isSubState = true;
		}
		_subState = subState;
	}
}
