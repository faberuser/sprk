using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NGame2.NAccount;
using NGame2.NDevice;
using NGame2.NNotification;
using NGame2.NScene;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI3;
using NGameAsset;
using NGameAsset.NSound;
using NShared;
using NShared.GetCertificate;
using NShared.GetUserInfoRegion;
using NShared.InsertUserData;
using NShared.Login;
using NShared.NUtil;
using NShared.RestoreAccount;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NLogin.NHelper;

public class LoginProgress
{
	public enum RequestLoginStatus
	{
		NormalLogin,
		CancelSignOutLogin,
		ForceSignOutLogin
	}

	public enum EStatus
	{
		Progressing,
		Success,
		Error_GetCertificateFail,
		Error_Login,
		Error_AuthFail,
		Cancel_Login,
		Cancel_DisabledLogin
	}

	public const int MaxRetryCount = 5;

	public Action<string> OnMessage;

	public string ServerAddress = string.Empty;

	public string CookieKeyword = string.Empty;

	public bool UseHttps;

	public ClientConfig ClientConfig;

	public LoginMethodType LoginMethod;

	public string LoginVerificationInfo = string.Empty;

	private LoginUserId _loginUserId;

	private string _originalServerAddress = string.Empty;

	private string _certificateString = string.Empty;

	private long _encryptSalt;

	private bool _isAuthenticated;

	private const float _loginWaitTime = 10f;

	public bool isInSignOutProcessing;

	public RequestLoginStatus requestLoginStatus;

	public EStatus Status { get; private set; }

	public NShared.GetCertificate.Response GetCertificateResponse { get; private set; }

	public NShared.Login.Response LoginResponse { get; private set; }

	public int RetryCount { get; private set; }

	public LoginProgress(string serverAddress, string cookieKeyword, bool useHttps, ClientConfig clientConfig, LoginUserId loginUserId, LoginMethodType loginMethod, string loginVerificationInfo = "")
	{
		ServerAddress = serverAddress;
		CookieKeyword = cookieKeyword;
		UseHttps = useHttps;
		ClientConfig = clientConfig;
		_loginUserId = loginUserId;
		LoginMethod = loginMethod;
		LoginVerificationInfo = loginVerificationInfo;
	}

	public void Start()
	{
		_originalServerAddress = ServerAddress;
		Run();
	}

	private void Run()
	{
		Status = EStatus.Progressing;
		SingletonUpdater<WebServiceManager>.instance.Requester.AesKey = null;
		SingletonUpdater<WebServiceManager>.instance.Requester.SetHost(ServerAddress);
		Vespa_System_Action_Extension_Method.fire(OnMessage, "Get Certificate.");
		SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.GetCertificate.Request, NShared.GetCertificate.Response>(new NShared.GetCertificate.Request(), OnResponseCertificate);
	}

	private void OnResponseCertificate(NShared.GetCertificate.Response r)
	{
		GetCertificateResponse = r;
		if (r == null)
		{
			Status = EStatus.Error_GetCertificateFail;
			return;
		}
		if (r.BaseResult != BaseResultType.Success)
		{
			Status = EStatus.Error_GetCertificateFail;
			return;
		}
		if (r.Result != NShared.GetCertificate.ResultType.Success)
		{
			Status = EStatus.Error_GetCertificateFail;
			return;
		}
		_certificateString = r.Certificate;
		_encryptSalt = r.Salt;
		if (r.PublicIp != null)
		{
			ServerAddress = r.PublicIp;
			SingletonUpdater<WebServiceManager>.instance.Requester.SetHost(ServerAddress);
		}
		RequestLogin(requestLoginStatus);
	}

	private void RequestLogin(RequestLoginStatus requestLoginStatus = RequestLoginStatus.NormalLogin)
	{
		Vespa_System_Action_Extension_Method.fire(OnMessage, "Login to King's Raid.");
		SingletonUpdater<WebServiceManager>.instance.Requester.AesKey = null;
		string operatingSystem = SystemInfo.operatingSystem;
		string platformName = DefinePlatform.PlatformName;
		string deviceModel = SystemInfo.deviceModel;
		string deviceUid = Helper.GetDeviceUid();
		_ = (object)null;
		string adid = "";
		_ = (object)null;
		string googleAdid = "";
		_ = (object)null;
		string idfa = "";
		string empty = string.Empty;
		DateTime now = DateTime.Now;
		DateTime dateTime = NVespa.Math.CreateDateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
		DateTime utcNow = DateTime.UtcNow;
		DateTime dateTime2 = NVespa.Math.CreateDateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0);
		TimeSpan timeSpan = dateTime - dateTime2;
		string text = Aes128.CreateSalt(16);
		LoginUserId loginUserId = LoginEncrypter.CreateEncryptedLoginId(_certificateString, text, _encryptSalt, _loginUserId);
		bool flag = !string.IsNullOrEmpty(loginUserId.id);
		DeviceBuildInfo buildInfo = DeviceHelper.GetBuildInfo();
		UnityLogQueue.Enqueue("LoginId : " + _loginUserId.id, string.Empty, LogType.Log);
		UnityLogQueue.Enqueue("LoginId (Legacy) : " + _loginUserId.legacyId, string.Empty, LogType.Log);
		if (flag)
		{
			UnityLogQueue.Enqueue("encrypted LoginId : " + loginUserId.id, string.Empty, LogType.Log);
			UnityLogQueue.Enqueue("encrypted LoginId (Legacy) : " + loginUserId.legacyId, string.Empty, LogType.Log);
		}
		NShared.Login.Request request = new NShared.Login.Request
		{
			LoginMethod = LoginMethodType.Masang,
			LoginId = (flag ? loginUserId.id : _loginUserId.id),
			LegacyLoginId = (flag ? loginUserId.legacyId : _loginUserId.legacyId),
			SelectedAccountId = (flag ? loginUserId.accountId : _loginUserId.accountId),
			EncryptSalt = (flag ? _encryptSalt : 0),
			DeviceId = deviceUid,
			Version = ClientConfig.Version,
			UTCOffsetHour = (int)timeSpan.TotalHours,
			ForcedHost = (RetryCount >= 5),
			DeviceOs = operatingSystem,
			DevicePlatform = platformName,
			DeviceModel = deviceModel,
			DeviceMemorySize = SystemInfo.systemMemorySize,
			DeviceToken = SingletonAuto<NotificationManager>.instance.DeviceToken,
			BattleVersion = ClientConfig.BattleVersion,
			Language = Vespa_String_Extension_Method.GetEnumString(SingletonAuto<LocalizationManager>.instance.Language),
			CountryCode = SingletonUpdater<LoginManager>.instance.CountryCode,
			PatchNoteIndex = LoginData.LastPatchNoteIndex,
			EventPush = GameOptionData.Noti_EventNoti,
			GuildRaidPush = GameOptionData.Noti_GuildRaidOpenNoti,
			GuildSuppressPush = GameOptionData.Noti_GuildSuppressOpenNoti,
			NightPush = GameOptionData.Noti_NightNoti,
			GraphicsAPI = SystemInfo.graphicsDeviceType.ToString(),
			SupportTextureFormat = UIHelper.GetSupportTextureFormat(),
			Adid = adid,
			GoogleAdid = googleAdid,
			Idfa = idfa,
			Idfv = empty,
			SigningKey = DeviceHelper.GetSigningKey(),
			DeviceBuildFingerprint = buildInfo.Fingerprint,
			DeviceBuildModel = buildInfo.Model,
			DeviceBuildBrand = buildInfo.Brand,
			DeviceBuildDevice = buildInfo.Device,
			DeviceBuildProduct = buildInfo.Product,
			DeviceBuildType = buildInfo.Type,
			DeviceBuildSerial = buildInfo.Serial,
			DeviceBuildBoard = buildInfo.Board,
			CpuArch = DeviceHelper.GetCpuFamily(),
			LoginVerificationInfo = LoginVerificationInfo,
			AppPlayerBuild = (DeviceHelper.GetCpuFamily().Equals("x86") ? 1 : 0),
			CancelSignOut = (requestLoginStatus.Equals(RequestLoginStatus.CancelSignOutLogin) ? true : false),
			ForceSignOut = (requestLoginStatus.Equals(RequestLoginStatus.ForceSignOutLogin) ? true : false),
			PackageName = Application.identifier,
			EnableCharacterVoice = SystemOptionData.Sound_EnableCharacterVoice,
			CharacterVoiceLanguage = (int)SingletonAuto<LanguageSetting>.instance.GetBaseLanguage()
		};
		if (UseHttps)
		{
			SingletonUpdater<WebServiceManager>.instance.Requester.SecureRequest<NShared.Login.Request, NShared.Login.Response>(request, OnResponseLogin);
		}
		else
		{
			SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.Login.Request, NShared.Login.Response>(request, OnResponseLogin);
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.AesKey = (flag ? text : null);
	}

	private void OnResponseLogin(NShared.Login.Response r)
	{
		LoginResponse = r;
		if (LoginResponse == null)
		{
			LoginResponse = new NShared.Login.Response();
			LoginResponse.BaseResult = BaseResultType.Timeout;
		}
		if (LoginResponse.BaseResult != BaseResultType.Success)
		{
			Status = EStatus.Error_Login;
		}
		else if (LoginResponse.Result == NShared.Login.ResultType.ContentsDisabled)
		{
			Status = EStatus.Cancel_DisabledLogin;
		}
		else if (LoginResponse.Result == NShared.Login.ResultType.AuthFail)
		{
			Status = EStatus.Error_AuthFail;
		}
		else if (LoginResponse.Result != NShared.Login.ResultType.Success)
		{
			if (LoginResponse.Result == NShared.Login.ResultType.Retry)
			{
				RetryCount++;
				if (!String_Extension_Method.IsNullOrEmpty(LoginResponse.AlternativeHost))
				{
					ServerAddress = LoginResponse.AlternativeHost;
				}
				else
				{
					Vespa_System_Action_Extension_Method.fire(OnMessage, "Retry Login");
				}
				RunLoginFlow();
			}
			else if (LoginResponse.Result == NShared.Login.ResultType.ServerClosed && LoginData.IsNewAccount)
			{
				GlobalMessagePopup.OpenGlobalError(SprkText.Get("Server is closed"), GlobalAutoBehaviour<LoadingManager>.instance.Quit);
				Status = EStatus.Error_Login;
			}
			else if (LoginResponse.Result == NShared.Login.ResultType.InactiveAccount)
			{
				GlobalAutoBehaviour<AccountManager>.instance.SetServerTime(LoginResponse.ServerLocalTime, LoginResponse.ServerUTCOffsetHour);
				OpenRestoreAcctounPopup(LoginResponse);
			}
			else if (LoginResponse.Result == NShared.Login.ResultType.InSignOutProcessing)
			{
				isInSignOutProcessing = true;
				GlobalAutoBehaviour<AccountManager>.instance.SetServerTime(LoginResponse.ServerLocalTime, LoginResponse.ServerUTCOffsetHour);
				OpenCancelSignOutLoginPopup();
			}
			else if (LoginResponse.Result == NShared.Login.ResultType.DuplicateUserInfos)
			{
				Open_SelectAccount_from_DuplicateUserInfos(LoginResponse.DuplicateUserInfos).Forget();
			}
			else
			{
				Status = EStatus.Error_Login;
			}
		}
		else
		{
			SingletonUpdater<WebServiceManager>.instance.Requester.SetHost(LoginResponse.StickyHost);
			GlobalAutoBehaviour<AccountManager>.instance.SetUniqueUserString(LoginResponse.UserInfo);
			SingletonUpdater<WebServiceManager>.instance.Requester.SessionKey = LoginResponse.UserInfo.SessionKey;
			LoginData.LastLoginMethod = LoginMethod;
			LoginData.LastLoginUserId = _loginUserId;
			LoginData.LastServerAddress = ServerAddress;
			LoginData.LastUseHttps = UseHttps;
			LoginData.LastCookieKeyword = CookieKeyword;
			if (!String_Extension_Method.IsNullOrEmpty(LoginVerificationInfo))
			{
				SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value = LoginVerificationInfo;
			}
			GameOptionData.Noti_EventNoti = r.EventPush;
			GameOptionData.Noti_NightNoti = r.NightPush;
			GameOptionData.Noti_GuildRaidOpenNoti = r.GuildRaidPush;
			GameOptionData.Noti_GuildSuppressOpenNoti = r.GuildSuppressPush;
			Status = EStatus.Success;
			ItemLinkButtonComponent.SetLastLinkedItem(null);
			SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginAccountInfo>().Refresh();
		}
	}

	private async void RunLoginFlow()
	{
		LoginVerificationInfo = await LoginAfterDelay();
		Run();
	}

	public async Task<string> LoginAfterDelay()
	{
		await Task.Delay(2000);
		TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
		GlobalAutoBehaviour<MasangLoginManager>.instance.AccessTokenLogin(delegate(bool issussce, LoginSuccData data)
		{
			tcs.TrySetResult(data.sessionid);
		});
		return await tcs.Task;
	}

	private async UniTaskVoid Open_SelectAccount_from_DuplicateUserInfos(UserInfo[] duplicateUserInfos)
	{
		long num = await NGame2.NUI3.SelectWindow_from_DuplicateUserInfo.OpenGlobalAsync(duplicateUserInfos);
		if (num == 0L)
		{
			Status = EStatus.Cancel_Login;
			return;
		}
		_loginUserId = new LoginUserId(_loginUserId.id, _loginUserId.legacyId, num);
		Run();
	}

	private void OpenCancelSignOutLoginPopup()
	{
		NGame2.NUI.NWindow.SignOutAccountRecoveryPopup.OpenGlobal(delegate
		{
			requestLoginStatus = RequestLoginStatus.CancelSignOutLogin;
			Run();
		}, delegate
		{
			OpenForceSignOutLoginPopup();
		}, delegate
		{
			Status = EStatus.Cancel_Login;
		}, LoginResponse.SignOutDoneTime);
	}

	private void OpenForceSignOutLoginPopup()
	{
		NGame2.NUI.NWindow.SignOutCreateNewAccountPopup.OpenGlobal(delegate
		{
			requestLoginStatus = RequestLoginStatus.ForceSignOutLogin;
			Run();
		}, delegate
		{
			OpenCancelSignOutLoginPopup();
		}, delegate
		{
			OpenCancelSignOutLoginPopup();
		});
	}

	private void OpenRestoreAcctounPopup(NShared.Login.Response res)
	{
		RestoreAccountPopup.OpenGlobal(res, delegate
		{
			string clientAesKey = Aes128.CreateSalt(16);
			LoginUserId loginUserId = LoginEncrypter.CreateEncryptedLoginId(_certificateString, clientAesKey, _encryptSalt, _loginUserId);
			bool flag = !string.IsNullOrEmpty(loginUserId.id);
			NShared.RestoreAccount.Request request = new NShared.RestoreAccount.Request
			{
				LoginMethod = LoginMethod,
				LoginId = (flag ? loginUserId.id : _loginUserId.id),
				LegacyLoginId = (flag ? loginUserId.legacyId : _loginUserId.legacyId),
				EncryptSalt = (flag ? _encryptSalt : 0)
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.AesKey = null;
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RestoreAccount.Response response)
			{
				if (response.BaseResult != BaseResultType.Success || response.Result != NShared.RestoreAccount.ResultType.Success)
				{
					RestoreAccountPopup.CloseGlobal();
					MessagePopup.OpenYesOnly(LocalizationManager.GetStringDirect("INACTIVE_POPUP_TITLE_ACCOUNT"), LocalizationManager.GetStringDirect("INACTIVE_POPUP_DESC_RESTORE_FAILED"), yesLabel: LocalizationManager.GetStringDirect("INACTIVE_POPUP_CUSTOMER_CENTER"), onYes: delegate
					{
						OqupieWebViewPopup.OpenPopup(delegate
						{
							SingletonUpdater<LoginManager>.instance.ReturnToLogin(LoadingManager.ELoadingType.Black);
						});
						Status = EStatus.Cancel_Login;
					}, ignoreBackButton: true, ignoreOnClickClose: true);
				}
				else
				{
					RestoreAccountPopup.SetSuccess();
				}
			});
		}, delegate
		{
			Run();
		});
	}

	private void OpenPtsUserSelectPopup(NShared.GetUserInfoRegion.Response res)
	{
		if (res.BaseResult != BaseResultType.Success || res.Result != NShared.GetUserInfoRegion.ResultType.Success)
		{
			GlobalMessagePopup.OpenGlobalError(LocalizationManager.GetStringDirect("PTS_ERROR"));
			return;
		}
		LoginPtsSelectUser.Open(res, delegate(string region, int accountId)
		{
			LoginPtsSelectUser.Close();
			NShared.InsertUserData.Request request = new NShared.InsertUserData.Request
			{
				Region = region,
				AccountId = accountId
			};
			WebRequestFlag flag = WebRequestFlag.DisableEncrypt;
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, flag, delegate(NShared.InsertUserData.Response response)
			{
				if (response.BaseResult != BaseResultType.Success || response.Result != NShared.InsertUserData.ResultType.Success)
				{
					GlobalMessagePopup.OpenGlobalError(LocalizationManager.GetStringDirect("PTS_ERROR"));
				}
				else
				{
					Run();
				}
			});
		});
	}
}
