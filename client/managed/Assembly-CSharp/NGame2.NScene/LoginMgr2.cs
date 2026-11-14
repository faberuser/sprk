using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NChatting;
using NGame2.NConfig;
using NGame2.NDevice;
using NGame2.NLogin;
using NGame2.NLogin.NHelper;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.Login;
using NShared.Ping;
using NShared.SignOut;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NUtil;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NScene;

public class LoginMgr2 : SceneBehaviour<LoginMgr2>
{
	public Transform Root;

	public UILabel ServerNameLabel;

	public UILabel ServerAddressLabel;

	public UILabel LoginButtonLabel;

	public UILabel PingButtonLabel;

	public string ForceLoginId;

	public int ForceAccountId;

	public string ForceVersion;

	public string ForceServer;

	public string[] Configs;

	public LocalizeLinkData LoginLabel = new LocalizeLinkData();

	public UIInput TestPrefixInput;

	public UILabel[] TestAccountLabel;

	public UILabel ForceLoginIdLabel;

	private LocalEditorInt _currentServerIndex = new LocalEditorInt(-1, typeof(LoginMgr2).FullName + "_currentServerIndex");

	private LocalStorageString _testAccountPrefix = new LocalStorageString("test", "LoginMgr_TestAccountPrefix");

	private List<ClientConfig> _configList = new List<ClientConfig>();

	private bool _start;

	private string _cpuArch = "";

	private string[] _idfaStatus = new string[4] { "NotDetermined", "Restricted", "Denied", "Authorized" };

	private void Start()
	{
		if ((bool)Root)
		{
			Root.gameObject.SetActive(value: true);
		}
		_start = true;
		TestPrefixInput.value = _testAccountPrefix.Value;
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("LoginScene", this, DrawGui);
		if (Application.isEditor)
		{
			InitClientConfigList();
		}
		else
		{
			OpenLoginUi(null, null);
		}
		_cpuArch = DeviceHelper.GetCpuFamily();
	}

	private void Update()
	{
		if (_start)
		{
			_testAccountPrefix.Value = TestPrefixInput.value;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TestAccountLabel); i++)
		{
			if (!(TestAccountLabel[i] == null))
			{
				UIHelper.SetLabelText(TestAccountLabel[i], TestPrefixInput.value + (i + 1));
			}
		}
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_configList, _currentServerIndex.Value))
		{
			string text = _configList[_currentServerIndex.Value].Name;
			text = text.Replace("_Debug", string.Empty);
			string text2 = "(" + _configList[_currentServerIndex.Value].QueryHost + ")";
			UIHelper.SetLabelText(ServerNameLabel, text);
			UIHelper.SetLabelText(ServerAddressLabel, text2);
		}
		if (!string.IsNullOrEmpty(ForceLoginId))
		{
			UIHelper.SetLabelText(ForceLoginIdLabel, "L=" + ForceLoginId);
		}
		else if (ForceAccountId != 0)
		{
			UIHelper.SetLabelText(ForceLoginIdLabel, "A=" + ForceAccountId);
		}
		else
		{
			UIHelper.SetLabelText(ForceLoginIdLabel, "DEVICE");
		}
	}

	private void InitClientConfigList()
	{
	}

	private void UpdateHost()
	{
		GetMyServer(delegate(AppServerInfo server)
		{
			if (server != null)
			{
				SingletonUpdater<WebServiceManager>.instance.Requester.Reset();
				SingletonUpdater<WebServiceManager>.instance.Requester.SetHost(server.Host);
			}
		});
	}

	private void GetMyServer(Action<AppServerInfo> onMyServer)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_configList, _currentServerIndex.Value))
		{
			Vespa_System_Action_Extension_Method.fire(onMyServer, null);
			return;
		}
		ClientConfig config = _configList[_currentServerIndex.Value];
		string queryHost = config.QueryHost;
		WwwText www = new WwwText(queryHost);
		SingletonUpdater<AsyncManager>.instance.Add(www, delegate
		{
			if (!Helper.FindHostInfo(www.text, _configList[_currentServerIndex.Value], out var foundHostInfo, out var _))
			{
				Vespa_System_Action_Extension_Method.fire(onMyServer, null);
			}
			else if (__Extension_Method_ICollection__.COUNT(foundHostInfo.Server) == 0)
			{
				Vespa_System_Action_Extension_Method.fire(onMyServer, null);
			}
			else
			{
				if (!String_Extension_Method.IsNullOrEmpty(config.ForcedServerName))
				{
					AppServerInfo appServerInfo = foundHostInfo.Server.FirstOrDefault((AppServerInfo e) => string.Equals(e.Name, config.ForcedServerName, StringComparison.InvariantCultureIgnoreCase));
					if (appServerInfo != null)
					{
						Vespa_System_Action_Extension_Method.fire(onMyServer, appServerInfo);
						return;
					}
				}
				Vespa_System_Action_Extension_Method.fire(onMyServer, foundHostInfo.Server[0]);
			}
		});
	}

	public void OnClickResetTest1()
	{
		SignOut(_testAccountPrefix.Value + "1");
	}

	public void OnClickTest1()
	{
		ProcessLogin(_testAccountPrefix.Value + "1");
	}

	public void OnClickResetTest2()
	{
		SignOut(_testAccountPrefix.Value + "2");
	}

	public void OnClickTest2()
	{
		ProcessLogin(_testAccountPrefix.Value + "2");
	}

	public void OnClickResetTest3()
	{
		SignOut(_testAccountPrefix.Value + "3");
	}

	public void OnClickTest3()
	{
		ProcessLogin(_testAccountPrefix.Value + "3");
	}

	public void OnClickResetTest4()
	{
		SignOut(_testAccountPrefix.Value + "4");
	}

	public void OnClickTest4()
	{
		ProcessLogin(_testAccountPrefix.Value + "4");
	}

	public void OnClickResetTest5()
	{
		SignOut(_testAccountPrefix.Value + "5");
	}

	public void OnClickTest5()
	{
		ProcessLogin(_testAccountPrefix.Value + "5");
	}

	public void OnClickResetTest6()
	{
		SignOut(_testAccountPrefix.Value + "6");
	}

	public void OnClickTest6()
	{
		ProcessLogin(_testAccountPrefix.Value + "6");
	}

	public void OnClickResetTest7()
	{
		SignOut(_testAccountPrefix.Value + "7");
	}

	public void OnClickTest7()
	{
		ProcessLogin(_testAccountPrefix.Value + "7");
	}

	public void OnClickResetTest8()
	{
		SignOut(_testAccountPrefix.Value + "8");
	}

	public void OnClickTest8()
	{
		ProcessLogin(_testAccountPrefix.Value + "8");
	}

	public void OnClickResetTest9()
	{
		SignOut(_testAccountPrefix.Value + "9");
	}

	public void OnClickTest9()
	{
		ProcessLogin(_testAccountPrefix.Value + "9");
	}

	public void OnClickDeviceUid()
	{
		if (!string.IsNullOrEmpty(ForceLoginId))
		{
			ProcessLogin(ForceLoginId);
			return;
		}
		if (ForceAccountId != 0)
		{
			SingletonUpdater<LoginManager>.instance.RequestLoginIdByAccountId(ForceAccountId, ProcessLogin);
			return;
		}
		string deviceUid = Helper.GetDeviceUid();
		ProcessLogin(deviceUid);
	}

	public void OnClickNormal()
	{
		if (!string.IsNullOrEmpty(ForceLoginId))
		{
			ProcessLogin(ForceLoginId);
		}
		else if (ForceAccountId != 0)
		{
			SingletonUpdater<LoginManager>.instance.RequestLoginIdByAccountId(ForceAccountId, ProcessLogin);
		}
		else
		{
			ProcessLogin("");
		}
	}

	public void OnClickPing()
	{
		NShared.Ping.Request request = new NShared.Ping.Request();
		DateTime requestedTime = DateTime.UtcNow;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.None, delegate(NShared.Ping.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				GlobalMessagePopup.OpenGlobalError(response.BaseResult);
			}
			else
			{
				int num = (int)(DateTime.UtcNow - requestedTime).TotalMilliseconds;
				UIHelper.SetLabelText(PingButtonLabel, "RTT= " + num + " ms");
			}
		});
	}

	public void OnClickReset()
	{
		string deviceUid = Helper.GetDeviceUid();
		SignOut(deviceUid);
	}

	private void SignOut(string uid)
	{
		MessagePopup.Open("Reset", uid + "\n\n초기화 하시겠습니까??", delegate
		{
			VespaCoroutine.Start(coSignOut(uid));
		}, delegate
		{
		});
	}

	private IEnumerator coSignOut(string uid)
	{
		ClientConfig clientConfig = SingletonAuto<Config>.instance.ClientConfig;
		if (clientConfig == null)
		{
			VespaDebug.LogError("Client config is null.");
			yield break;
		}
		bool isComplete = false;
		AppServerInfo serverInfo = null;
		GetMyServer(delegate(AppServerInfo iServerInfo)
		{
			serverInfo = iServerInfo;
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
		if (serverInfo == null)
		{
			yield break;
		}
		LoginProgress loginProgress = new LoginProgress(serverInfo.Host, null, useHttps: false, clientConfig, new LoginUserId(uid, string.Empty, 0L), LoginMethodType.Guest);
		loginProgress.Start();
		while (loginProgress.Status == LoginProgress.EStatus.Progressing)
		{
			yield return null;
		}
		if (loginProgress.Status == LoginProgress.EStatus.Error_GetCertificateFail)
		{
			GlobalMessagePopup.OpenGlobalError(SprkText.Get("Invalid Certificate"));
			yield break;
		}
		if (loginProgress.Status == LoginProgress.EStatus.Error_Login)
		{
			Enum error = BaseResultType.HttpError;
			if (loginProgress.LoginResponse != null)
			{
				if (loginProgress.LoginResponse.BaseResult != BaseResultType.Success)
				{
					error = loginProgress.LoginResponse.BaseResult;
				}
				if (loginProgress.LoginResponse.Result != NShared.Login.ResultType.Success)
				{
					error = loginProgress.LoginResponse.Result;
				}
			}
			GlobalMessagePopup.OpenGlobalError(error);
			yield break;
		}
		NShared.SignOut.Request request = new NShared.SignOut.Request
		{
			ForceSignOut = true
		};
		isComplete = false;
		NShared.SignOut.Response response = null;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SignOut.Response iResponse)
		{
			isComplete = true;
			response = iResponse;
		});
		while (!isComplete)
		{
			yield return null;
		}
		if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.SignOut.ResultType.Success)
		{
			LoginData.LastLoginMethod = LoginMethodType.None;
			GlobalAutoBehaviour<ChattingManager>.instance.ClearChatList(ChatType.None, 0L);
			GlobalMessagePopup.OpenGlobal("SignOut!", "SignOut!");
		}
		else
		{
			GlobalMessagePopup.OpenGlobalError(SprkText.Get("SignOut Fail!"));
		}
	}

	public void OnClickServerAddress()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_configList))
		{
			_currentServerIndex.Value = (_currentServerIndex.Value + 1) % __Extension_Method_ICollection__.COUNT(_configList);
			UpdateHost();
		}
	}

	public void OnClickServerAddressPrev()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_configList))
		{
			_currentServerIndex.Value = (_currentServerIndex.Value + __Extension_Method_ICollection__.COUNT(_configList) - 1) % __Extension_Method_ICollection__.COUNT(_configList);
			UpdateHost();
		}
	}

	private void ProcessLogin(string loginId)
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_configList, _currentServerIndex.Value))
		{
			UIHelper.SetLabelText(LoginButtonLabel, SprkText.Get("Progressing.."));
			OpenLoginUi(loginId, _configList[_currentServerIndex.Value]);
		}
	}

	private void OpenLoginUi(string loginId, ClientConfig clientConfig)
	{
		base.gameObject.SetActive(value: false);
		if (clientConfig != null)
		{
			clientConfig.Version = (String_Extension_Method.IsNullOrEmpty(ForceVersion) ? clientConfig.Version : ForceVersion);
			SingletonAuto<Config>.instance.SetClientConfig(clientConfig);
		}
		SceneAutoBehaviour<LoginManagement>.instance.Open(loginId);
	}

	private void DrawGui()
	{
		using (new BlockManager().Vertical())
		{
			int num = 0;
			string arg = ((num > -1) ? _idfaStatus[num] : "Not Available!");
			GUILayout.Label($"Device Model={SystemInfo.deviceModel} ({_cpuArch})");
			GUILayout.Label($"[IOS_ATT_POPUP_LOG] AuthorizationStatus is {arg}");
			if (GUILayout.Button("Test Login"))
			{
				base.gameObject.SetActive(value: true);
			}
		}
	}
}
