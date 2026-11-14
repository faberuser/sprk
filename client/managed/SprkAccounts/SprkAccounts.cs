using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NConfig;
using NGame2.NGuild;
using NGame2.NLogin;
using NGame2.NLogin.NState;
using NGame2.NOrdeal;
using NGame2.NUI;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NChatting;
using NGameAsset;
using NGameAsset.NBundleManager;
using NShared;
using NShared.Logout;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Networking;

public sealed class SprkAccounts : MonoBehaviour
{
	[Serializable]
	public class DeleteAccountRequest
	{
		public string session_key;

		public string current_password;

		public string confirmation;
	}

	[Serializable]
	public class ChatAtlasData
	{
		public UISpriteData[] sprites;
	}

	[Serializable]
	public class Request
	{
		public string username;

		public string password;
	}

	[Serializable]
	public class PasswordChangeRequest
	{
		public string session_key;

		public string current_password;

		public string new_password;
	}

	[Serializable]
	public class TokenReply
	{
		public string status;

		public string message;

		public string InternalErrorMessage;

		public GetTokenData data;
	}

	[Serializable]
	public class SessionReply
	{
		public string status;

		public string message;

		public string InternalErrorMessage;

		public GetSessionData data;
	}

	private bool deletingAccount;

	private static UIAtlas chatAtlas;

	private static LoginSuccData pending;

	private static GetTokenData pendingTokens;

	private static SprkSavedAccount activeAccount;

	private static bool switchingAccount;

	private static IDisposable accountLease;

	private float nextAccountRefresh;

	private string accountListState = "";

	private static SprkAccountStore store;

	private bool choosing;

	private int accountPage;

	private GameObject accountList;

	private UIButton back;

	private UIButton close;

	private const int PageSize = 4;

	private LoginSelect window;

	private LoginSelect.Listener listener;

	private AppServerInfo[] servers;

	private int serverIndex;

	private bool registering;

	private bool busy;

	private bool changingPassword;

	private bool passwordChanged;

	private GameOptionWindow settingsWindow;

	private Vector3 formOrigin;

	private UIButton currentVisibility;

	private UILabel currentVisibilityLabel;

	private GameObject root;

	private UITexture background;

	private UITexture border;

	private UILabel template;

	private UILabel title;

	private UILabel message;

	private UILabel switchLabel;

	private UILabel submitLabel;

	private UILabel serverLabel;

	private UILabel passwordVisibilityLabel;

	private UILabel confirmVisibilityLabel;

	private UIInput username;

	private UIInput password;

	private UIInput confirm;

	private GameObject confirmRow;

	private GameObject serverStatus;

	private UIButton submit;

	private UIButton toggle;

	private UIButton changeServerButton;

	private UIButton passwordVisibility;

	private UIButton confirmVisibility;

	private static readonly Color Accent = new Color(0.4f, 0.79f, 0.84f);

	public const float CloseX = 376f;

	public const float CloseY = 396f;

	private static SprkAccountStore Store
	{
		get
		{
			if (store == null)
			{
				string path = Application.persistentDataPath;
				string environmentVariable = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
				if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")) && !string.IsNullOrEmpty(environmentVariable) && Regex.IsMatch(environmentVariable, "\\A[A-Za-z0-9_-]{1,64}\\z"))
				{
					path = Path.Combine(path, "SprkAutomation", environmentVariable);
				}
				string environmentVariable2 = Environment.GetEnvironmentVariable("SPRK_HOST_URL");
				if (!string.IsNullOrEmpty(environmentVariable2))
				{
					string leftPart = new Uri(ResolveHostUrl(environmentVariable2)).GetLeftPart(UriPartial.Authority);
					string path2;
					using (SHA256 sHA = SHA256.Create())
					{
						path2 = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(leftPart))).Replace("-", "");
					}
					string text = Path.Combine(path, "sprk-accounts.dat");
					path = Path.Combine(path, "SprkServers", path2);
					string text2 = Path.Combine(path, "sprk-accounts.dat");
					bool flag = !File.Exists(text2);
					store = new SprkAccountStore(text2);
					if (flag && File.Exists(text))
					{
						foreach (SprkSavedAccount account in new SprkAccountStore(text).Accounts)
						{
							if (string.Equals(SprkAccountStore.Endpoint(account.Endpoint), leftPart, StringComparison.OrdinalIgnoreCase))
							{
								store.Upsert(account);
							}
						}
					}
				}
				else
				{
					store = new SprkAccountStore(Path.Combine(path, "sprk-accounts.dat"));
				}
			}
			return store;
		}
	}

	private float PickerAddY => 188 - System.Math.Min(4, Store.Accounts.Count - accountPage * 4) * 86 - ((Store.Accounts.Count > 4) ? 82 : 0);

	public static CachedHeroInfo CreateLobbyAvatarHeroInfo(CachedHeroInfo hero, PlayerAvatarHeroInfo avatar)
	{
		CachedHeroInfo cachedHeroInfo = hero.CreatePlayerAvatarHeroInfo(avatar);
		if (cachedHeroInfo == null)
		{
			return null;
		}
		return cachedHeroInfo;
	}

	public static bool ShouldRefreshLobbyAccessories(NGame2.NAccount.UserHeroManager.ChangeHeroInfo change)
	{
		if (change == null || change.ChangeType != NGame2.NAccount.UserHeroManager.HeroChangeType.AccessoryCostume)
		{
			return false;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (instance != null)
		{
			return new UIAvatarHelper.AvatarInfo(instance.UserManager.AvatarHeroIndex).HeroIndex == change.HeroIndex;
		}
		return false;
	}

	public static void HideGuestNickname(NickMessagePopup view)
	{
		Hide(view.Label_LoginMethod);
	}

	private static byte[] ChatResource(string name)
	{
		using Stream stream = typeof(SprkAccounts).Assembly.GetManifestResourceStream(name);
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public static bool UseFullChatLayout()
	{
		return !SceneAutoBehaviour<BattleManagement>.isValidInstance;
	}

	public static void RepairChatTabs(ChattingWindow view)
	{
		if (chatAtlas == null)
		{
			GameObject obj = new GameObject("SprkPreDoomsdayChatAtlas");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			chatAtlas = obj.AddComponent<UIAtlas>();
			Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
			if (!texture2D.LoadImage(ChatResource("ChatCommon.png")))
			{
				throw new InvalidOperationException("Cannot load archived chat atlas");
			}
			texture2D.name = "PreDoomsdayCommon";
			texture2D.wrapMode = TextureWrapMode.Clamp;
			Material material = new Material(Shader.Find("Unlit/Transparent Colored"));
			material.mainTexture = texture2D;
			chatAtlas.spriteMaterial = material;
			UISpriteData[] sprites = JsonUtility.FromJson<ChatAtlasData>(Encoding.UTF8.GetString(ChatResource("ChatCommon.json"))).sprites;
			foreach (UISpriteData sd in sprites)
			{
				chatAtlas.AddSprite(sd);
			}
		}
		UISprite[] componentsInChildren = view.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite uISprite in componentsInChildren)
		{
			if (uISprite.atlas != null && uISprite.atlas.name == "Common" && chatAtlas.GetSprite(uISprite.spriteName) != null)
			{
				uISprite.atlas = chatAtlas;
			}
		}
	}

	public static void OpenFreeNickname(MyInfoWindow view)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LoginCreateNick>().Open(view);
	}

	public static void LabelNickname(MyInfoWindow view)
	{
		UILabel[] componentsInChildren = view.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel uILabel in componentsInChildren)
		{
			if (!(uILabel == view.MyInfo.Label_Nick) && !((uILabel.text ?? "").Trim() != "ID"))
			{
				UILocalize[] components = uILabel.GetComponents<UILocalize>();
				foreach (UILocalize obj in components)
				{
					obj.enabled = false;
					UnityEngine.Object.Destroy(obj);
				}
				UILocalizeEx[] components2 = uILabel.GetComponents<UILocalizeEx>();
				foreach (UILocalizeEx obj2 in components2)
				{
					obj2.enabled = false;
					UnityEngine.Object.Destroy(obj2);
				}
				uILabel.text = SprkText.Get("Nickname");
				uILabel.overflowMethod = UILabel.Overflow.ShrinkContent;
			}
		}
	}

	public static string[] GetHostTokens(string host)
	{
		if (string.IsNullOrEmpty(host))
		{
			return null;
		}
		Uri uri = new Uri(host.Contains("://") ? host : ("http://" + host));
		return new string[2]
		{
			uri.Host,
			uri.Port.ToString(CultureInfo.InvariantCulture)
		};
	}

	public static string SecureHost(string host)
	{
		Uri uri = new Uri(host.Contains("://") ? host : ("https://" + host));
		if (uri.Scheme == "https")
		{
			return uri.AbsoluteUri.TrimEnd('/');
		}
		return new UriBuilder(uri)
		{
			Scheme = "https",
			Port = (uri.IsDefaultPort ? 443 : uri.Port)
		}.Uri.AbsoluteUri.TrimEnd('/');
	}

	private static void ReleaseAccount()
	{
		if (accountLease != null)
		{
			accountLease.Dispose();
			accountLease = null;
		}
	}

	private void OnApplicationQuit()
	{
		ReleaseAccount();
	}

	public static void Initialize()
	{
		LoginData.LastAccessToken = "";
		LoginData.LastRefreshToken = "";
		LoginData.LastLoginMethod = LoginMethodType.None;
	}

	public static string ResolveHostUrl(string original)
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_HOST_URL");
		if (string.IsNullOrEmpty(environmentVariable))
		{
			return original;
		}
		if (!Uri.TryCreate(environmentVariable, UriKind.Absolute, out var result) || (!(result.Scheme == "https") && (!(result.Scheme == "http") || !result.IsLoopback)) || !string.IsNullOrEmpty(result.UserInfo) || !string.IsNullOrEmpty(result.Fragment))
		{
			throw new InvalidOperationException("Invalid SPRK server profile URL");
		}
		return result.AbsoluteUri;
	}

	public static void Open(LoginSelect view, LoginSelect.Listener callback, LoginMethodType ignored, AppServerInfo[] choices, string lastServer)
	{
		view.Open();
		switchingAccount = false;
		SprkAccounts sprkAccounts = view.GetComponent<SprkAccounts>();
		if (sprkAccounts == null)
		{
			sprkAccounts = view.gameObject.AddComponent<SprkAccounts>();
		}
		sprkAccounts.window = view;
		sprkAccounts.listener = callback;
		sprkAccounts.servers = choices;
		Store.Reload();
		ReleaseAccount();
		activeAccount = null;
		sprkAccounts.serverIndex = 0;
		if (choices != null)
		{
			for (int i = 0; i < choices.Length; i++)
			{
				if (choices[i].Name == lastServer)
				{
					sprkAccounts.serverIndex = i;
				}
			}
		}
		if (sprkAccounts.root == null)
		{
			sprkAccounts.Build();
		}
		sprkAccounts.root.SetActive(value: true);
		sprkAccounts.busy = false;
		sprkAccounts.registering = false;
		sprkAccounts.choosing = Store.Accounts.Count > 0;
		sprkAccounts.accountPage = 0;
		sprkAccounts.Refresh();
		sprkAccounts.HidePasswords();
		sprkAccounts.SetMessage("");
		if (!sprkAccounts.choosing)
		{
			sprkAccounts.username.isSelected = true;
		}
		sprkAccounts.root.GetComponent<UIPanel>().alpha = 0f;
		TweenAlpha.Begin(sprkAccounts.root, 0.18f, 1f);
		pending = null;
		pendingTokens = null;
	}

	public static void CleanLoginBackground(LoginBackground view)
	{
		SprkLoginPresentation.Configure(view);
		HidePatchVersion(view);
		UILabel[] componentsInChildren = view.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel uILabel in componentsInChildren)
		{
			if (uILabel.name == "Label_Company")
			{
				uILabel.gameObject.SetActive(value: false);
			}
		}
		if (view.Label_Version != null)
		{
			view.Label_Version.gameObject.SetActive(value: false);
		}
	}

	public static void HidePatchVersion(LoginBackground view)
	{
		if (view.Label_PatchVersion != null)
		{
			view.Label_PatchVersion.gameObject.SetActive(value: false);
		}
	}

	private static string DisplayVersion(string version)
	{
		ClientConfig.ParseVersion(version, out var majorVersion, out var minorVersion, out var patchVersion);
		return $"{majorVersion}.{minorVersion:D2}.{patchVersion:D2}";
	}

	public static void CleanLoginInfo(LoginAccountInfo view)
	{
		if (view.LoginButtonInfoRoot != null)
		{
			view.LoginButtonInfoRoot.SetActive(value: false);
		}
		SprkLoginSettings.Configure(view);
	}

	public static void SyncRefreshToken()
	{
		string lastRefreshToken = LoginData.LastRefreshToken;
		if (activeAccount == null || string.IsNullOrEmpty(lastRefreshToken) || activeAccount.RefreshToken == lastRefreshToken)
		{
			return;
		}
		SprkSavedAccount account = new SprkSavedAccount
		{
			Username = activeAccount.Username,
			ServerName = activeAccount.ServerName,
			Endpoint = activeAccount.Endpoint,
			RefreshToken = lastRefreshToken
		};
		try
		{
			Store.Upsert(account);
			activeAccount = account;
		}
		catch (Exception)
		{
			Debug.LogWarning("SPRK could not remember the renewed login.");
		}
	}

	public static void ClearSession()
	{
		SyncRefreshToken();
		activeAccount = null;
		ReleaseAccount();
		Initialize();
		pending = null;
		pendingTokens = null;
	}

	public static void ForgetSession()
	{
		if (activeAccount != null)
		{
			try
			{
				Store.Remove(activeAccount);
			}
			catch (Exception)
			{
				Debug.LogWarning("SPRK could not remove the saved account.");
			}
			activeAccount = null;
		}
		ClearSession();
	}

	public static void FinishLogout(NShared.Logout.Response response)
	{
		if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
		{
			ForgetSession();
			SingletonUpdater<WebServiceManager>.instance.Requester.Reset();
		}
	}

	private static void Hide(Component component)
	{
		if (component != null)
		{
			component.gameObject.SetActive(value: false);
		}
	}

	public static void ContinueAccountSwitch(LoginTouchWait view)
	{
		SprkLoginSettings.ConfigureTouch(view);
		if (switchingAccount)
		{
			view.OnTouchScreen();
		}
	}

	private static void SwitchAccount()
	{
		if (!switchingAccount)
		{
			switchingAccount = true;
			SyncRefreshToken();
			activeAccount = null;
			SingletonUpdater<WebServiceManager>.instance.Requester.Reset();
			ClearSession();
			LoginData.ClearLastLoginData();
			SingletonUpdater<LoginManager>.instance.LastLoginVerificationInfo.Value = string.Empty;
			if (GlobalAutoBehaviour<ChattingManager>.isValidInstance)
			{
				GlobalAutoBehaviour<ChattingManager>.instance.ClearChatList(ChatType.None, 0L);
			}
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
	}

	private static void AddSwitchAccount(GameOptionWindow view)
	{
		GameOptionWindow.GameOptionEtc etcInfo = view.EtcInfo;
		if (etcInfo.Button_Logout == null || etcInfo.Button_Coupon == null)
		{
			return;
		}
		Transform parent = etcInfo.Button_Logout.parent;
		if (!(parent.Find("Button_SwitchAccount") != null))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(etcInfo.Button_Coupon.gameObject, parent, worldPositionStays: false);
			gameObject.name = "Button_SwitchAccount";
			UIWidget component = gameObject.GetComponent<UIWidget>();
			Vector3 localPosition = etcInfo.Button_Logout.localPosition;
			localPosition.x -= ((component != null) ? component.width : 350) + 24;
			gameObject.transform.localPosition = localPosition;
			UILocalize[] componentsInChildren = gameObject.GetComponentsInChildren<UILocalize>(includeInactive: true);
			foreach (UILocalize obj in componentsInChildren)
			{
				obj.enabled = false;
				UnityEngine.Object.Destroy(obj);
			}
			UILocalizeEx[] componentsInChildren2 = gameObject.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
			foreach (UILocalizeEx obj2 in componentsInChildren2)
			{
				obj2.enabled = false;
				UnityEngine.Object.Destroy(obj2);
			}
			UILabel[] componentsInChildren3 = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].text = SprkText.Get("Switch Account");
			}
			UIButton component2 = gameObject.GetComponent<UIButton>();
			component2.onClick.Clear();
			EventDelegate.Add(component2.onClick, SwitchAccount);
			gameObject.SetActive(value: true);
		}
	}

	public static void CleanSettings(GameOptionWindow view)
	{
		SprkLoginPresentation.Setup(view);
		AddSwitchAccount(view);
		AddChangePassword(view);
		AddDeleteAccount(view);
		ArrangeAccountButtons(view);
		GameOptionWindow.GameOptionEtc etcInfo = view.EtcInfo;
		ClientConfig clientConfig = SingletonAuto<Config>.instance.ClientConfig;
		if (clientConfig != null && etcInfo.Label_GameVersionNum != null)
		{
			etcInfo.Label_GameVersionNum.text = DisplayVersion(clientConfig.Version);
		}
		Hide(etcInfo.Button_CustomerCenter);
		Hide(etcInfo.Button_MemberLeave);
		Hide(etcInfo.Button_LocalizedLaw1);
		Hide(etcInfo.Button_LocalizedLaw2);
		Hide(etcInfo.Button_LocalizedLaw3);
		Transform transform = view.transform.Find("Panel/Root/EtcOption");
		if (transform != null)
		{
			Hide(transform.Find("Button_PrivacyPolicy"));
			Hide(transform.Find("Button_TermOfService"));
			Hide(transform.Find("Button_CustomerCenter (1)"));
		}
		Hide(etcInfo.Label_GuestLogin);
		Hide(etcInfo.Label_FacebookLogin);
		Hide(etcInfo.Label_GPGSLogin);
		Hide(etcInfo.Label_GameCenterLogin);
		Hide(etcInfo.Label_AppleLogin);
		Hide(etcInfo.Label_TwitterLogin);
		Hide(etcInfo.Label_LineLogin);
		GameOptionWindow.ConnectUiGroup[] array = new GameOptionWindow.ConnectUiGroup[6] { etcInfo.ConnectFacebook, etcInfo.ConnectGPGS, etcInfo.ConnectGameCenter, etcInfo.ConnectApple, etcInfo.ConnectTwitter, etcInfo.ConnectLine };
		foreach (GameOptionWindow.ConnectUiGroup connectUiGroup in array)
		{
			if (connectUiGroup != null)
			{
				Hide(connectUiGroup.Root);
			}
		}
		Hide(etcInfo.Btn_LinkedSteam);
		Hide(etcInfo.Btn_LinkedGoogle);
		Hide(etcInfo.Btn_LinkedApple);
		if (etcInfo.Obj_ConnectedSteam != null)
		{
			etcInfo.Obj_ConnectedSteam.SetActive(value: false);
		}
		if (etcInfo.Obj_ConnectedGoogle != null)
		{
			etcInfo.Obj_ConnectedGoogle.SetActive(value: false);
		}
		if (etcInfo.Obj_ConnectedApple != null)
		{
			etcInfo.Obj_ConnectedApple.SetActive(value: false);
		}
		if (etcInfo.Btn_LinkedSteam != null && etcInfo.Btn_LinkedGoogle != null && etcInfo.Btn_LinkedApple != null)
		{
			Transform parent = etcInfo.Btn_LinkedSteam.transform.parent;
			while (parent != null && parent != view.transform)
			{
				if (etcInfo.Btn_LinkedGoogle.transform.IsChildOf(parent) && etcInfo.Btn_LinkedApple.transform.IsChildOf(parent))
				{
					if (!(etcInfo.Label_AccountIdNum != null) || !etcInfo.Label_AccountIdNum.transform.IsChildOf(parent))
					{
						parent.gameObject.SetActive(value: false);
					}
					break;
				}
				parent = parent.parent;
			}
		}
		SprkLoginSettings.CleanSettings(view);
	}

	private static void ArrangeAccountButtons(GameOptionWindow view)
	{
		GameOptionWindow.GameOptionEtc etcInfo = view.EtcInfo;
		if (etcInfo.Button_Logout == null || etcInfo.Button_Coupon == null)
		{
			return;
		}
		Transform parent = etcInfo.Button_Logout.parent;
		Transform transform = parent.Find("Button_ChangeLanguage");
		Transform transform2 = parent.Find("Button_ChangePassword");
		Transform transform3 = parent.Find("Button_DeleteAccount");
		Transform transform4 = parent.Find("Button_SwitchAccount");
		UIWidget component = etcInfo.Button_Coupon.GetComponent<UIWidget>();
		float num = ((component != null) ? component.width : 350) + 24;
		Vector3 localPosition = etcInfo.Button_Logout.localPosition;
		if (transform != null)
		{
			transform.localPosition = localPosition - new Vector3(num * 3f, 0f, 0f);
		}
		etcInfo.Button_Coupon.localPosition = localPosition + new Vector3((0f - num) * 3f, 100f, 0f);
		if (transform4 != null)
		{
			transform4.localPosition = localPosition - new Vector3(num, 0f, 0f);
		}
		if (transform2 != null)
		{
			transform2.localPosition = localPosition + new Vector3(0f - num, 100f, 0f);
		}
		if (transform3 == null)
		{
			return;
		}
		transform3.localPosition = localPosition + new Vector3(0f, 100f, 0f);
		UIAtlas uIAtlas = null;
		UIAtlas[] array = Resources.FindObjectsOfTypeAll<UIAtlas>();
		foreach (UIAtlas uIAtlas2 in array)
		{
			if (uIAtlas2.GetSprite("Common_Button7_Cancel") != null)
			{
				uIAtlas = uIAtlas2;
				break;
			}
		}
		if (uIAtlas == null)
		{
			BundleLoadFromFile bundleLoadFromFile = new BundleLoadFromFile();
			bundleLoadFromFile.Open("NguiAtlas_UI_Prefab_Common.unity3d");
			((IBundleLoad)bundleLoadFromFile).Interface().StartLoad();
			if (bundleLoadFromFile.AssetBundle != null)
			{
				uIAtlas = bundleLoadFromFile.AssetBundle.mainAsset as UIAtlas;
			}
		}
		UIButton component2 = transform3.GetComponent<UIButton>();
		if (component2 == null || uIAtlas == null || uIAtlas.GetSprite("Common_Button7_Cancel") == null)
		{
			return;
		}
		UISprite uISprite = ((component2.tweenTarget == null) ? transform3.GetComponent<UISprite>() : component2.tweenTarget.GetComponent<UISprite>());
		if (uISprite == null)
		{
			return;
		}
		UISpriteAtlasAssigner[] componentsInChildren = transform3.GetComponentsInChildren<UISpriteAtlasAssigner>(includeInactive: true);
		foreach (UISpriteAtlasAssigner uISpriteAtlasAssigner in componentsInChildren)
		{
			if (uISpriteAtlasAssigner.Target == uISprite)
			{
				uISpriteAtlasAssigner.AtlasName = "Common";
				uISpriteAtlasAssigner.SpriteName = "Common_Button7_Cancel";
			}
		}
		uISprite.atlas = uIAtlas;
		uISprite.spriteName = "Common_Button7_Cancel";
		uISprite.color = Color.white;
		component2.defaultColor = Color.white;
		component2.hover = Color.white;
		component2.pressed = new Color(0.9f, 0.9f, 0.9f, 1f);
		component2.normalSprite = "Common_Button7_Cancel";
		component2.hoverSprite = "Common_Button7_Cancel";
		component2.pressedSprite = "Common_Button7_Cancel";
		component2.disabledSprite = "Common_Button7_Disable";
		component2.SetState(UIButtonColor.State.Normal, immediate: true);
	}

	private static void AddChangePassword(GameOptionWindow view)
	{
		Transform button_Coupon = view.EtcInfo.Button_Coupon;
		if (button_Coupon == null)
		{
			return;
		}
		Transform parent = button_Coupon.parent;
		if (!(parent.Find("Button_ChangePassword") != null))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(button_Coupon.gameObject, parent, worldPositionStays: false);
			gameObject.name = "Button_ChangePassword";
			if (view.EtcInfo.Button_CustomerCenter != null)
			{
				gameObject.transform.localPosition = view.EtcInfo.Button_CustomerCenter.localPosition;
			}
			else
			{
				gameObject.transform.localPosition = button_Coupon.localPosition + new Vector3(0f, 100f, 0f);
			}
			UILocalize[] componentsInChildren = gameObject.GetComponentsInChildren<UILocalize>(includeInactive: true);
			foreach (UILocalize obj in componentsInChildren)
			{
				obj.enabled = false;
				UnityEngine.Object.Destroy(obj);
			}
			UILocalizeEx[] componentsInChildren2 = gameObject.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
			foreach (UILocalizeEx obj2 in componentsInChildren2)
			{
				obj2.enabled = false;
				UnityEngine.Object.Destroy(obj2);
			}
			UILabel[] componentsInChildren3 = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].text = SprkText.Get("Change Password");
			}
			UIButton component = gameObject.GetComponent<UIButton>();
			component.onClick.Clear();
			EventDelegate.Add(component.onClick, delegate
			{
				OpenPasswordChange(view);
			});
			gameObject.SetActive(value: true);
		}
	}

	private static void OpenPasswordChange(GameOptionWindow view)
	{
		SprkAccounts sprkAccounts = view.GetComponent<SprkAccounts>();
		if (sprkAccounts == null)
		{
			sprkAccounts = view.gameObject.AddComponent<SprkAccounts>();
		}
		if (!sprkAccounts.busy)
		{
			sprkAccounts.deletingAccount = false;
			sprkAccounts.changingPassword = true;
			sprkAccounts.settingsWindow = view;
			sprkAccounts.registering = true;
			sprkAccounts.choosing = false;
			sprkAccounts.passwordChanged = false;
			sprkAccounts.template = view.EtcInfo.Label_GameVersionNum;
			if (sprkAccounts.root == null)
			{
				sprkAccounts.Build();
			}
			sprkAccounts.root.SetActive(value: true);
			sprkAccounts.username.value = "";
			sprkAccounts.password.value = "";
			sprkAccounts.confirm.value = "";
			sprkAccounts.HidePasswords();
			sprkAccounts.SetMessage("");
			sprkAccounts.Refresh();
			sprkAccounts.username.isSelected = true;
		}
	}

	private static void AddDeleteAccount(GameOptionWindow view)
	{
		Transform button_Coupon = view.EtcInfo.Button_Coupon;
		if (button_Coupon == null || button_Coupon.parent.Find("Button_DeleteAccount") != null)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(button_Coupon.gameObject, button_Coupon.parent, worldPositionStays: false);
		gameObject.name = "Button_DeleteAccount";
		Transform transform = button_Coupon.parent.Find("Button_ChangePassword");
		gameObject.transform.localPosition = ((transform != null) ? transform.localPosition : button_Coupon.localPosition) + new Vector3(374f, 0f, 0f);
		UILocalize[] componentsInChildren = gameObject.GetComponentsInChildren<UILocalize>(includeInactive: true);
		foreach (UILocalize obj in componentsInChildren)
		{
			obj.enabled = false;
			UnityEngine.Object.Destroy(obj);
		}
		UILocalizeEx[] componentsInChildren2 = gameObject.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
		foreach (UILocalizeEx obj2 in componentsInChildren2)
		{
			obj2.enabled = false;
			UnityEngine.Object.Destroy(obj2);
		}
		UILabel[] componentsInChildren3 = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].text = SprkText.Get("Delete Account");
		}
		UIButton component = gameObject.GetComponent<UIButton>();
		component.onClick.Clear();
		EventDelegate.Add(component.onClick, delegate
		{
			OpenPasswordChange(view);
			SprkAccounts component2 = view.GetComponent<SprkAccounts>();
			if (!component2.busy)
			{
				component2.deletingAccount = true;
				component2.Refresh();
				component2.SetMessage(SprkText.Get("Permanently deletes this account and its progress. This cannot be undone."));
			}
		});
		gameObject.SetActive(value: true);
	}

	private void ClosePasswordChange()
	{
		if (!busy)
		{
			username.value = "";
			password.value = "";
			confirm.value = "";
			HidePasswords();
			UICamera.selectedObject = null;
			root.SetActive(value: false);
		}
	}

	public static void Complete(MasangLoginManager.LoginResultCallback callback)
	{
		LoginSuccData loginSuccData = pending;
		GetTokenData getTokenData = pendingTokens;
		pending = null;
		pendingTokens = null;
		bool flag = loginSuccData != null && getTokenData != null && !string.IsNullOrEmpty(getTokenData.access_token);
		if (flag)
		{
			LoginData.LastAccessToken = getTokenData.access_token;
			LoginData.LastRefreshToken = getTokenData.refresh_token;
		}
		callback(flag, flag ? loginSuccData : null);
	}

	public static void Reject(MasangLoginManager.LoginResultCallback callback)
	{
		callback(issuccess: false, null);
	}

	private GameObject Child(string name, Transform parent, float x, float y)
	{
		GameObject obj = new GameObject(name);
		obj.layer = base.gameObject.layer;
		obj.transform.SetParent(parent, worldPositionStays: false);
		obj.transform.localPosition = new Vector3(x, y, 0f);
		return obj;
	}

	private UITexture Surface(GameObject go, int width, int height, Color color, int depth)
	{
		UITexture uITexture = go.AddComponent<UITexture>();
		uITexture.mainTexture = Texture2D.whiteTexture;
		uITexture.width = width;
		uITexture.height = height;
		uITexture.color = color;
		uITexture.depth = depth;
		return uITexture;
	}

	private UILabel Label(string name, Transform parent, string text, float x, float y, int width, int height, int size)
	{
		UILabel uILabel = Child(name, parent, x, y).AddComponent<UILabel>();
		uILabel.bitmapFont = template.bitmapFont;
		uILabel.trueTypeFont = template.trueTypeFont;
		uILabel.fontSize = size;
		uILabel.width = width;
		uILabel.height = height;
		uILabel.depth = 12;
		uILabel.supportEncoding = false;
		uILabel.overflowMethod = UILabel.Overflow.ShrinkContent;
		uILabel.alignment = NGUIText.Alignment.Center;
		uILabel.text = SprkText.Get(text);
		uILabel.color = Color.white;
		return uILabel;
	}

	private UIButton Button(string name, string text, float x, float y, int width, EventDelegate.Callback action, out UILabel label, Transform parent = null, bool primary = false, int height = 64)
	{
		GameObject gameObject = Child(name, (parent == null) ? root.transform : parent, x, y);
		Surface(gameObject, width, height, primary ? new Color(0.14f, 0.39f, 0.47f) : new Color(0.1f, 0.16f, 0.23f), 14);
		gameObject.AddComponent<BoxCollider>().size = new Vector3(width, height, 1f);
		UIButton uIButton = gameObject.AddComponent<UIButton>();
		uIButton.tweenTarget = gameObject;
		uIButton.hover = (primary ? new Color(0.2f, 0.54f, 0.61f) : new Color(0.17f, 0.27f, 0.36f));
		uIButton.pressed = (primary ? new Color(0.09f, 0.27f, 0.34f) : new Color(0.07f, 0.12f, 0.18f));
		uIButton.disabledColor = new Color(0.08f, 0.12f, 0.17f);
		uIButton.duration = 0.12f;
		EventDelegate.Add(uIButton.onClick, action);
		label = Label("Label", gameObject.transform, text, 0f, 0f, width - 16, height - 8, (width < 120) ? 18 : 25);
		label.depth = 16;
		return uIButton;
	}

	private UIInput Input(string name, string caption, float y, bool secret, out GameObject row)
	{
		row = Child(name + "Row", root.transform, 0f, y);
		UILabel uILabel = Label("Caption", row.transform, caption, 0f, 48f, 696, 32, 22);
		uILabel.alignment = NGUIText.Alignment.Left;
		uILabel.color = new Color(0.77f, 0.84f, 0.91f);
		UITexture frame = Surface(Child("FocusBorder", row.transform, 0f, 0f), 700, 68, new Color(0.22f, 0.3f, 0.39f), 9);
		GameObject gameObject = Child(name, row.transform, 0f, 0f);
		Surface(gameObject, 696, 64, new Color(0.07f, 0.1f, 0.16f), 10);
		gameObject.AddComponent<BoxCollider>().size = new Vector3(696f, 64f, 1f);
		UILabel uILabel2 = Label("Value", gameObject.transform, "", secret ? (-58) : 0, 0f, secret ? 548 : 664, 50, 26);
		uILabel2.alignment = NGUIText.Alignment.Left;
		UIInput uIInput = gameObject.AddComponent<UIInput>();
		uIInput.label = uILabel2;
		uIInput.characterLimit = (secret ? 128 : 64);
		uIInput.inputType = (secret ? UIInput.InputType.Password : UIInput.InputType.Standard);
		uIInput.onReturnKey = UIInput.OnReturnKey.Submit;
		uIInput.selectAllTextOnFocus = false;
		uIInput.validation = UIInput.Validation.None;
		uIInput.value = "";
		uIInput.caretColor = Accent;
		uIInput.selectionColor = new Color(0.2f, 0.48f, 0.6f, 0.5f);
		EventDelegate.Add(uIInput.onFocusIn, delegate
		{
			frame.color = Accent;
		});
		EventDelegate.Add(uIInput.onFocusOut, delegate
		{
			frame.color = new Color(0.22f, 0.3f, 0.39f);
		});
		EventDelegate.Add(uIInput.onSubmit, InputSubmitted);
		return uIInput;
	}

	private void Build()
	{
		if (!changingPassword)
		{
			template = window.Label_ServerName;
		}
		if (!changingPassword)
		{
			foreach (Transform item in window.transform)
			{
				item.gameObject.SetActive(value: false);
			}
		}
		root = Child(changingPassword ? "ChangePasswordForm" : "AccountForm", base.transform, 0f, 0f);
		if (changingPassword)
		{
			Camera camera = NGUITools.FindCameraForLayer(base.gameObject.layer);
			Vector3 position = camera.WorldToScreenPoint(base.transform.position);
			position.x = (float)Screen.width * 0.5f;
			position.y = (float)Screen.height * 0.5f;
			formOrigin = base.transform.InverseTransformPoint(camera.ScreenToWorldPoint(position));
		}
		UIPanel uIPanel = root.AddComponent<UIPanel>();
		uIPanel.depth = 100;
		if (changingPassword)
		{
			uIPanel.depth = 1000;
			Surface(Child("ModalShade", root.transform, 0f, 0f), 4000, 4000, new Color(0f, 0f, 0f, 0.6f), -2).gameObject.AddComponent<BoxCollider>().size = new Vector3(4000f, 4000f, 1f);
		}
		border = Surface(Child("Border", root.transform, 0f, 0f), 846, 0, new Color(0.24f, 0.34f, 0.43f, 0.96f), -1);
		background = Surface(Child("Background", root.transform, 0f, 0f), 840, 0, new Color(0.035f, 0.055f, 0.09f, 0.96f), 0);
		title = Label("Title", root.transform, SprkText.Get("Welcome Back"), 0f, 363f, 730, 60, 42);
		Label("Subtitle", root.transform, SprkText.Get("SPRK ACCOUNT"), 0f, 313f, 730, 30, 19).color = new Color(0.61f, 0.72f, 0.82f);
		Surface(Child("Accent", root.transform, 0f, 287f), 72, 4, Accent, 1);
		username = Input(changingPassword ? "CurrentPassword" : "Username", changingPassword ? SprkText.Get("Current Password") : "Username", 218f, changingPassword, out var row);
		username.characterLimit = (changingPassword ? 128 : 32);
		if (changingPassword)
		{
			currentVisibility = Button("ShowPassword", SprkText.Get("Show"), 290f, 0f, 96, ShowCurrentPassword, out currentVisibilityLabel, username.transform, primary: false, 48);
		}
		password = Input(changingPassword ? "NewPassword" : "Password", changingPassword ? SprkText.Get("New Password") : "Password", 110f, secret: true, out row);
		passwordVisibility = Button("ShowPassword", SprkText.Get("Show"), 290f, 0f, 96, ShowPassword, out passwordVisibilityLabel, password.transform, primary: false, 48);
		confirm = Input("ConfirmPassword", SprkText.Get("Confirm Password"), 2f, secret: true, out confirmRow);
		confirmVisibility = Button("ShowPassword", SprkText.Get("Show"), 290f, 0f, 96, ShowConfirmation, out confirmVisibilityLabel, confirm.transform, primary: false, 48);
		serverStatus = Child("ServerStatus", root.transform, 0f, 0f);
		serverLabel = Label("Label", serverStatus.transform, "", 0f, 0f, 696, 36, 23);
		serverLabel.color = new Color(0.66f, 0.77f, 0.86f);
		changeServerButton = Button("ChangeServer", "Change", 296f, 0f, 96, ChangeServer, out var _, serverStatus.transform, primary: false, 40);
		submit = Button("Submit", SprkText.Get("Log In"), 0f, 0f, 696, Submit, out submitLabel, null, primary: true, 52);
		toggle = Button("SwitchMode", SprkText.Get("Create an Account"), 0f, 0f, 696, SwitchMode, out switchLabel, null, primary: false, 52);
		message = Label("Message", root.transform, "", 0f, 0f, 730, 22, 22);
		message.color = new Color(1f, 0.79f, 0.48f);
		message.overflowMethod = UILabel.Overflow.ResizeHeight;
		back = Button(changingPassword ? "Cancel" : "BackToAccounts", changingPassword ? "Cancel" : SprkText.Get("Back to Accounts"), 0f, 0f, 696, BackToAccounts, out var _, null, primary: false, 52);
		accountList = Child("SavedAccounts", root.transform, 0f, 0f);
		if (!changingPassword)
		{
			BuildCloseButton();
		}
	}

	private void BuildCloseButton()
	{
		close = Button("Close", "", 376f, 396f, 56, CloseToTitle, out var label, null, primary: false, 56);
		label.gameObject.SetActive(value: false);
		float[] array = new float[2] { 45f, -45f };
		foreach (float z in array)
		{
			Surface(Child("Cross", close.transform, 0f, 0f), 30, 4, new Color(0.77f, 0.84f, 0.91f), 16).transform.localRotation = Quaternion.Euler(0f, 0f, z);
		}
	}

	private void CloseToTitle()
	{
		if (!busy && !changingPassword && !(window == null))
		{
			username.value = "";
			password.value = "";
			confirm.value = "";
			HidePasswords();
			SetMessage("");
			UICamera.selectedObject = null;
			window.Close();
			SingletonUpdater<LoginManager>.instance.SetNextState(new StateBase_WaitTouch());
		}
	}

	private void Refresh()
	{
		if (changingPassword)
		{
			RefreshPasswordChange();
			return;
		}
		username.transform.parent.gameObject.SetActive(!choosing);
		password.transform.parent.gameObject.SetActive(!choosing);
		serverStatus.SetActive(!choosing);
		submit.gameObject.SetActive(!choosing);
		toggle.gameObject.SetActive(!choosing);
		back.gameObject.SetActive(!choosing && Store.Accounts.Count > 0);
		back.isEnabled = !busy;
		back.transform.localPosition = new Vector3(0f, registering ? (-312) : (-204), 0f);
		if (close != null)
		{
			close.isEnabled = !busy;
		}
		accountList.SetActive(choosing);
		title.text = SprkText.Get((choosing ? "Choose an Account" : (registering ? "Create Your Account" : "Welcome Back")));
		if (choosing)
		{
			BuildAccountList();
		}
		submitLabel.text = SprkText.Get((busy ? "Connecting..." : (registering ? "Register" : "Log In")));
		switchLabel.text = SprkText.Get((registering ? "Already registered? Log in" : "Create an Account"));
		RefreshLayout();
		serverStatus.transform.localPosition = new Vector3(0f, registering ? (-90) : 18, 0f);
		submit.transform.localPosition = new Vector3(0f, registering ? (-178) : (-70), 0f);
		toggle.transform.localPosition = new Vector3(0f, registering ? (-245) : (-137), 0f);
		confirmRow.SetActive(!choosing && registering);
		submit.isEnabled = !busy;
		toggle.isEnabled = !busy;
		bool flag = servers != null && servers.Length > 1;
		changeServerButton.gameObject.SetActive(flag);
		changeServerButton.isEnabled = !busy;
		serverLabel.width = (flag ? 530 : 696);
		serverLabel.transform.localPosition = new Vector3(flag ? (-56) : 0, 0f, 0f);
		serverLabel.text = SprkText.Get(((servers != null && servers.Length != 0) ? (SprkText.Format("Server: {0}", servers[serverIndex].Name)) : "No server available"));
		username.enabled = !busy;
		password.enabled = !busy;
		confirm.enabled = !busy;
		username.GetComponent<BoxCollider>().enabled = !busy;
		password.GetComponent<BoxCollider>().enabled = !busy;
		confirm.GetComponent<BoxCollider>().enabled = !busy;
		passwordVisibility.isEnabled = !busy;
		confirmVisibility.isEnabled = !busy;
	}

	private void RefreshPasswordChange()
	{
		title.text = SprkText.Get("Change Password");
		serverStatus.SetActive(value: false);
		toggle.gameObject.SetActive(value: false);
		accountList.SetActive(value: false);
		confirmRow.SetActive(value: true);
		back.gameObject.SetActive(value: true);
		back.isEnabled = !busy;
		submitLabel.text = SprkText.Get((busy ? "Saving..." : "Change Password"));
		submit.gameObject.SetActive(!passwordChanged);
		submit.isEnabled = !busy;
		submit.transform.localPosition = new Vector3(0f, -100f, 0f);
		back.transform.localPosition = new Vector3(0f, -168f, 0f);
		back.GetComponentInChildren<UILabel>().text = SprkText.Get((passwordChanged ? "Close" : "Cancel"));
		UIInput[] array = new UIInput[3] { username, password, confirm };
		foreach (UIInput obj in array)
		{
			obj.enabled = !busy && !passwordChanged;
			obj.GetComponent<BoxCollider>().enabled = !busy && !passwordChanged;
		}
		UIButton uIButton = currentVisibility;
		UIButton uIButton2 = passwordVisibility;
		bool flag = (confirmVisibility.isEnabled = !busy && !passwordChanged);
		bool isEnabled = (uIButton2.isEnabled = flag);
		uIButton.isEnabled = isEnabled;
		RefreshLayout();
		username.transform.parent.Find("Caption").GetComponent<UILabel>().text = SprkText.Get("Current Password");
		password.transform.parent.Find("Caption").GetComponent<UILabel>().text = SprkText.Get((deletingAccount ? "Type DELETE to confirm" : "New Password"));
		password.inputType = ((!deletingAccount) ? UIInput.InputType.Password : UIInput.InputType.Standard);
		passwordVisibility.gameObject.SetActive(!deletingAccount);
		confirmRow.SetActive(!deletingAccount);
		if (deletingAccount)
		{
			title.text = SprkText.Get("Delete Account");
			submitLabel.text = SprkText.Get((busy ? "Deleting..." : "Permanently Delete"));
		}
	}

	private void RefreshLayout()
	{
		float num = Mathf.Max(message.fontSize, Mathf.Ceil(message.printedSize.y));
		float num2 = (changingPassword ? (-220f) : (choosing ? (PickerAddY - 53f) : ((float)((registering ? (-245) : (-137)) - ((Store.Accounts.Count > 0) ? 67 : 0) - 26 - 26))));
		message.transform.localPosition = new Vector3(0f, num2 - num * 0.5f, 0f);
		background.height = (changingPassword ? 744 : (choosing ? Mathf.CeilToInt(548f - PickerAddY) : ((registering ? 786 : 678) + ((Store.Accounts.Count > 0) ? 67 : 0))));
		if (!string.IsNullOrEmpty(message.text))
		{
			background.height = Mathf.Max(background.height, Mathf.CeilToInt(440f - (num2 - num - 26f)));
		}
		float num3 = 440f - (float)background.height * 0.5f;
		root.transform.localPosition = formOrigin + new Vector3(0f, 0f - num3, 0f);
		background.transform.localPosition = new Vector3(0f, num3, 0f);
		border.height = background.height + 6;
		border.transform.localPosition = background.transform.localPosition;
	}

	private void SetMessage(string text)
	{
		message.text = SprkText.Get(text);
		RefreshLayout();
	}

	private string AccountListState()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (SprkSavedAccount account in Store.Accounts)
		{
			stringBuilder.Append(account.Endpoint).Append('|').Append(account.ServerName)
				.Append('|')
				.Append(account.Username)
				.Append(Store.InUse(account))
				.Append(';');
		}
		return stringBuilder.ToString();
	}

	private void Update()
	{
		if (!changingPassword && !busy && root != null && root.activeInHierarchy && Time.unscaledTime >= nextAccountRefresh)
		{
			nextAccountRefresh = Time.unscaledTime + 1f;
			try
			{
				Store.Reload();
				string text = AccountListState();
				if (text != accountListState)
				{
					accountListState = text;
					if (choosing && Store.Accounts.Count == 0)
					{
						choosing = false;
					}
					Refresh();
				}
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
		if (changingPassword && !busy && root != null && root.activeInHierarchy && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			ClosePasswordChange();
		}
		else if (!busy && !passwordChanged && !choosing && !(root == null) && root.activeInHierarchy && UnityEngine.Input.GetKeyDown(KeyCode.Tab))
		{
			bool flag = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
			UIInput selection = UIInput.selection;
			if (selection == username)
			{
				((!flag) ? password : (registering ? confirm : password)).isSelected = true;
			}
			else if (selection == password)
			{
				(flag ? username : (registering ? confirm : username)).isSelected = true;
			}
			else if (selection == confirm && registering)
			{
				(flag ? password : username).isSelected = true;
			}
		}
	}

	private void BuildAccountList()
	{
		foreach (Transform item in accountList.transform)
		{
			item.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(item.gameObject);
		}
		accountPage = Mathf.Clamp(accountPage, 0, Mathf.Max(0, (Store.Accounts.Count - 1) / 4));
		for (int i = accountPage * 4; i < System.Math.Min(Store.Accounts.Count, (accountPage + 1) * 4); i++)
		{
			SprkSavedAccount account = Store.Accounts[i];
			float y = 218 - i % 4 * 86;
			bool flag = Store.InUse(account);
			Button("Account" + i % 4, account.Username + "  /  " + account.ServerName + (flag ? SprkText.Get(" (In use)") : ""), -76f, y, 544, delegate
			{
				UseAccount(account);
			}, out var label, accountList.transform).isEnabled = !busy && !flag;
			Button("Logout" + i % 4, SprkText.Get("Log Out"), 282f, y, 132, delegate
			{
				RemoveAccount(account);
			}, out label, accountList.transform).isEnabled = !busy && !flag;
		}
		Button("AddAccount", SprkText.Get("Add Account"), 0f, PickerAddY, 696, AddAccount, out var label2, accountList.transform, primary: true).isEnabled = !busy && Store.Accounts.Count < 4;
		if (Store.Accounts.Count <= 4)
		{
			return;
		}
		Button("Previous", "Previous", -250f, PickerAddY + 82f, 180, delegate
		{
			if (!busy)
			{
				accountPage--;
				Refresh();
			}
		}, out label2, accountList.transform, primary: false, 44).isEnabled = !busy && accountPage > 0;
		Label("Page", accountList.transform, accountPage + 1 + " / " + (Store.Accounts.Count + 4 - 1) / 4, 0f, PickerAddY + 82f, 200, 36, 22);
		Button("Next", "Next", 250f, PickerAddY + 82f, 180, delegate
		{
			if (!busy)
			{
				accountPage++;
				Refresh();
			}
		}, out label2, accountList.transform, primary: false, 44).isEnabled = !busy && (accountPage + 1) * 4 < Store.Accounts.Count;
	}

	private void AddAccount()
	{
		if (!busy)
		{
			Store.Reload();
			if (Store.Accounts.Count >= 4)
			{
				SetMessage(SprkText.Get("Four accounts are saved. Log out of one to add another."));
				Refresh();
				return;
			}
			choosing = false;
			registering = false;
			username.value = "";
			password.value = "";
			confirm.value = "";
			HidePasswords();
			SetMessage("");
			Refresh();
			username.isSelected = true;
		}
	}

	private void BackToAccounts()
	{
		if (changingPassword)
		{
			ClosePasswordChange();
		}
		else if (!busy)
		{
			choosing = Store.Accounts.Count > 0;
			password.value = "";
			confirm.value = "";
			HidePasswords();
			UICamera.selectedObject = null;
			SetMessage("");
			Refresh();
		}
	}

	private void RemoveAccount(SprkSavedAccount account)
	{
		if (busy)
		{
			return;
		}
		try
		{
			using IDisposable disposable = Store.TryAcquire(account);
			if (disposable == null)
			{
				Fail(SprkText.Get("This account is open in another window."));
				return;
			}
			Store.Remove(account);
		}
		catch (Exception)
		{
			Fail(SprkText.Get("Unable to save logout. Please check local storage and try again."));
			return;
		}
		choosing = Store.Accounts.Count > 0;
		SetMessage("");
		Refresh();
		if (!choosing)
		{
			username.value = "";
			username.isSelected = true;
		}
	}

	private void UseAccount(SprkSavedAccount account)
	{
		if (busy)
		{
			return;
		}
		int num = -1;
		if (servers != null)
		{
			for (int i = 0; i < servers.Length; i++)
			{
				if (SprkAccountStore.Endpoint(servers[i].LoginServer) == account.Endpoint && servers[i].Name == account.ServerName)
				{
					num = i;
					break;
				}
			}
		}
		if (num < 0)
		{
			Fail(SprkText.Get("This account's server is unavailable. Please try again later."));
			return;
		}
		Store.Reload();
		SprkSavedAccount sprkSavedAccount = Store.Accounts.Find((SprkSavedAccount entry) => SprkAccountStore.Same(entry, account));
		if (sprkSavedAccount == null)
		{
			Refresh();
			return;
		}
		accountLease = Store.TryAcquire(sprkSavedAccount);
		if (accountLease == null)
		{
			Fail(SprkText.Get("This account is open in another window."));
			return;
		}
		Store.Reload();
		sprkSavedAccount = Store.Accounts.Find((SprkSavedAccount entry) => SprkAccountStore.Same(entry, account));
		if (sprkSavedAccount == null)
		{
			ReleaseAccount();
			Refresh();
			return;
		}
		serverIndex = num;
		busy = true;
		SetMessage(SprkText.Get("Connecting..."));
		Refresh();
		StartCoroutine(Authenticate(sprkSavedAccount));
	}

	private void SetPasswordVisibility(UIInput input, UILabel label, bool visible)
	{
		input.inputType = ((!visible) ? UIInput.InputType.Password : UIInput.InputType.Standard);
		input.UpdateLabel();
		label.text = SprkText.Get((visible ? "Hide" : "Show"));
	}

	private void HidePasswords()
	{
		SetPasswordVisibility(password, passwordVisibilityLabel, visible: false);
		SetPasswordVisibility(confirm, confirmVisibilityLabel, visible: false);
		if (changingPassword)
		{
			SetPasswordVisibility(username, currentVisibilityLabel, visible: false);
		}
	}

	private void ShowCurrentPassword()
	{
		if (!busy)
		{
			SetPasswordVisibility(username, currentVisibilityLabel, username.inputType == UIInput.InputType.Password);
			username.isSelected = true;
		}
	}

	private void ShowPassword()
	{
		if (!busy)
		{
			SetPasswordVisibility(password, passwordVisibilityLabel, password.inputType == UIInput.InputType.Password);
			password.isSelected = true;
		}
	}

	private void ShowConfirmation()
	{
		if (!busy)
		{
			SetPasswordVisibility(confirm, confirmVisibilityLabel, confirm.inputType == UIInput.InputType.Password);
			confirm.isSelected = true;
		}
	}

	private void ChangeServer()
	{
		if (!busy && servers != null && servers.Length != 0)
		{
			serverIndex = (serverIndex + 1) % servers.Length;
			Refresh();
		}
	}

	private void SwitchMode()
	{
		if (!busy)
		{
			registering = !registering;
			password.value = "";
			confirm.value = "";
			HidePasswords();
			SetMessage("");
			Refresh();
			username.isSelected = true;
		}
	}

	private void OnDisable()
	{
		if (password != null)
		{
			password.value = "";
			confirm.value = "";
			if (changingPassword)
			{
				username.value = "";
			}
			HidePasswords();
			if (changingPassword && root != null)
			{
				root.SetActive(value: false);
			}
		}
	}

	private void InputSubmitted()
	{
		if (!busy)
		{
			Submit();
		}
	}

	private void Submit()
	{
		if (changingPassword)
		{
			SubmitPasswordChange();
		}
		else
		{
			if (busy || choosing)
			{
				return;
			}
			if (servers == null || servers.Length == 0)
			{
				Fail(SprkText.Get("No server available."));
				return;
			}
			if (username.value.Trim().Length < 3 || password.value.Length < 8)
			{
				Fail(SprkText.Get("Enter a username (3-32 characters) and password (at least 8 characters)."), (username.value.Trim().Length < 3) ? username : password);
				return;
			}
			if (registering && password.value != confirm.value)
			{
				Fail(SprkText.Get("Passwords do not match."), confirm);
				return;
			}
			Store.Reload();
			SprkSavedAccount account = new SprkSavedAccount
			{
				Username = username.value.Trim().ToLowerInvariant(),
				ServerName = servers[serverIndex].Name,
				Endpoint = SprkAccountStore.Endpoint(servers[serverIndex].LoginServer)
			};
			if (!Store.CanSave(account))
			{
				Fail(SprkText.Get("Four accounts are saved. Log out of one to add another."));
				return;
			}
			accountLease = Store.TryAcquire(account);
			if (accountLease == null)
			{
				Fail(SprkText.Get("This account is open in another window."));
				return;
			}
			busy = true;
			SetMessage("");
			HidePasswords();
			UICamera.selectedObject = null;
			Refresh();
			StartCoroutine(Authenticate());
		}
	}

	private void SubmitPasswordChange()
	{
		if (deletingAccount)
		{
			if (!busy)
			{
				if (string.IsNullOrEmpty(username.value) || password.value != "DELETE")
				{
					PasswordChangeFailed(SprkText.Get("Enter your current password and type DELETE."), username);
					return;
				}
				busy = true;
				SetMessage("");
				UICamera.selectedObject = null;
				Refresh();
				StartCoroutine(DeleteCurrentAccount());
			}
		}
		else if (!busy && !passwordChanged)
		{
			if (string.IsNullOrEmpty(username.value))
			{
				PasswordChangeFailed(SprkText.Get("Enter your current password."), username);
				return;
			}
			if (Encoding.UTF8.GetByteCount(password.value) < 8 || Encoding.UTF8.GetByteCount(password.value) > 128)
			{
				PasswordChangeFailed(SprkText.Get("New password must be 8-128 bytes."), password);
				return;
			}
			if (password.value != confirm.value)
			{
				PasswordChangeFailed(SprkText.Get("Passwords do not match."), confirm);
				return;
			}
			if (username.value == password.value)
			{
				PasswordChangeFailed(SprkText.Get("Choose a different new password."), password);
				return;
			}
			busy = true;
			HidePasswords();
			SetMessage("");
			UICamera.selectedObject = null;
			Refresh();
			StartCoroutine(ChangePassword());
		}
	}

	private IEnumerator ChangePassword()
	{
		string text = ((activeAccount != null) ? activeAccount.Endpoint : null);
		if (string.IsNullOrEmpty(text))
		{
			PasswordChangeFailed(SprkText.Get("Please log in again before changing your password."), username);
			yield break;
		}
		UserInfo userInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.UserInfo;
		string s = JsonUtility.ToJson(new PasswordChangeRequest
		{
			session_key = userInfo.SessionKey,
			current_password = username.value,
			new_password = password.value
		});
		using UnityWebRequest request = new UnityWebRequest(text.TrimEnd('/') + "/api/auth/change-password", "POST");
		request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
		request.downloadHandler = new DownloadHandlerBuffer();
		request.timeout = 25;
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		TokenReply tokenReply = null;
		try
		{
			tokenReply = JsonUtility.FromJson<TokenReply>(request.downloadHandler.text);
		}
		catch (Exception)
		{
		}
		if (request.result != UnityWebRequest.Result.Success || tokenReply == null || tokenReply.status != "success" || tokenReply.data == null || string.IsNullOrEmpty(tokenReply.data.refresh_token))
		{
			PasswordChangeFailed((tokenReply != null && !string.IsNullOrEmpty(tokenReply.InternalErrorMessage)) ? tokenReply.InternalErrorMessage : SprkText.Get("Unable to change password. Check the connection and try again."), username);
			yield break;
		}
		LoginData.LastAccessToken = tokenReply.data.access_token;
		LoginData.LastRefreshToken = tokenReply.data.refresh_token;
		SyncRefreshToken();
		busy = false;
		passwordChanged = true;
		username.value = "";
		password.value = "";
		confirm.value = "";
		SetMessage(SprkText.Get("Password changed successfully."));
		Refresh();
	}

	private IEnumerator DeleteCurrentAccount()
	{
		string text = ((activeAccount != null) ? activeAccount.Endpoint : null);
		if (string.IsNullOrEmpty(text))
		{
			PasswordChangeFailed(SprkText.Get("Please log in again first."), username);
			yield break;
		}
		UserInfo userInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.UserInfo;
		string s = JsonUtility.ToJson(new DeleteAccountRequest
		{
			session_key = userInfo.SessionKey,
			current_password = username.value,
			confirmation = password.value
		});
		using UnityWebRequest request = new UnityWebRequest(text.TrimEnd('/') + "/api/auth/delete-account", "POST");
		request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
		request.downloadHandler = new DownloadHandlerBuffer();
		request.timeout = 25;
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		TokenReply tokenReply = null;
		try
		{
			tokenReply = JsonUtility.FromJson<TokenReply>(request.downloadHandler.text);
		}
		catch (Exception)
		{
		}
		if (request.result != UnityWebRequest.Result.Success || tokenReply == null || tokenReply.status != "success")
		{
			PasswordChangeFailed((tokenReply != null && !string.IsNullOrEmpty(tokenReply.InternalErrorMessage)) ? tokenReply.InternalErrorMessage : SprkText.Get("Deletion could not be confirmed. Check the connection before retrying."), username);
			yield break;
		}
		busy = false;
		username.value = "";
		password.value = "";
		confirm.value = "";
		root.SetActive(value: false);
		ForgetSession();
		SwitchAccount();
	}

	private void PasswordChangeFailed(string error, UIInput field)
	{
		busy = false;
		SetMessage(error);
		Refresh();
		StartCoroutine(FocusForRetry(field, mode: true));
	}

	private IEnumerator Authenticate(SprkSavedAccount saved = null)
	{
		AppServerInfo server = servers[serverIndex];
		SingletonUpdater<LoginManager>.instance.SetServerName(server.Name);
		string endpoint = server.LoginServer.TrimEnd('/') + "/api/auth/";
		string s = ((saved == null) ? JsonUtility.ToJson(new Request
		{
			username = username.value.Trim(),
			password = password.value
		}) : "{}");
		using (UnityWebRequest request = new UnityWebRequest(endpoint + ((saved != null) ? "refresh-token" : (registering ? "register" : "login")), "POST"))
		{
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.timeout = 25;
			request.SetRequestHeader("Content-Type", "application/json");
			if (saved != null)
			{
				request.SetRequestHeader("Authorization", "Bearer " + saved.RefreshToken);
			}
			yield return request.SendWebRequest();
			TokenReply response = null;
			try
			{
				response = JsonUtility.FromJson<TokenReply>(request.downloadHandler.text);
			}
			catch (Exception)
			{
			}
			if (request.result != UnityWebRequest.Result.Success || response == null || response.data == null || response.status != "success" || string.IsNullOrEmpty(response.data.access_token) || string.IsNullOrEmpty(response.data.refresh_token))
			{
				if (saved != null && (request.responseCode == 401 || request.responseCode == 403))
				{
					try
					{
						Store.Remove(saved);
					}
					catch (Exception)
					{
						Fail(SprkText.Get("Unable to update saved accounts. Please check local storage."));
						yield break;
					}
					choosing = false;
					registering = false;
					username.value = saved.Username;
					password.value = "";
					confirm.value = "";
					Fail(SprkText.Get("Your saved login has expired. Please enter your password."));
				}
				else
				{
					Debug.LogWarning("SPRK account request failed: HTTP " + request.responseCode + ", " + request.error + ", " + endpoint);
					Fail((response != null && !string.IsNullOrEmpty(response.InternalErrorMessage)) ? response.InternalErrorMessage : SprkText.Get("Unable to sign in. Check the server connection and try again."));
				}
				yield break;
			}
			SprkSavedAccount remembered = new SprkSavedAccount
			{
				Username = ((saved == null) ? username.value.Trim().ToLowerInvariant() : saved.Username),
				ServerName = server.Name,
				Endpoint = SprkAccountStore.Endpoint(server.LoginServer),
				RefreshToken = response.data.refresh_token
			};
			try
			{
				Store.Upsert(remembered);
			}
			catch (InvalidOperationException ex3)
			{
				Fail(ex3.Message);
				yield break;
			}
			catch (Exception)
			{
				Fail(SprkText.Get("Unable to remember this login. Please check local storage and try again."));
				yield break;
			}
			using UnityWebRequest verify = new UnityWebRequest(endpoint + "token/verify", "POST");
			verify.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
			verify.downloadHandler = new DownloadHandlerBuffer();
			verify.timeout = 25;
			verify.SetRequestHeader("Content-Type", "application/json");
			verify.SetRequestHeader("Authorization", "Bearer " + response.data.access_token);
			yield return verify.SendWebRequest();
			SessionReply sessionReply = null;
			try
			{
				sessionReply = JsonUtility.FromJson<SessionReply>(verify.downloadHandler.text);
			}
			catch (Exception)
			{
			}
			if (verify.result != UnityWebRequest.Result.Success || sessionReply == null || sessionReply.status != "success" || sessionReply.data == null || string.IsNullOrEmpty(sessionReply.data.session))
			{
				Fail(SprkText.Get("Account authenticated, but the game connection failed. Please log in again."));
				yield break;
			}
			pending = new LoginSuccData
			{
				id = sessionReply.data.id,
				sessionid = sessionReply.data.session
			};
			pendingTokens = response.data;
			activeAccount = remembered;
		}
		busy = false;
		password.value = "";
		confirm.value = "";
		listener.OnSelect(window, LoginMethodType.Guest, server.Name, UserSelect: true);
	}

	private void Fail(string error, UIInput field = null)
	{
		ReleaseAccount();
		busy = false;
		SetMessage(error);
		Refresh();
		if (!choosing)
		{
			StartCoroutine(FocusForRetry((field == null) ? password : field, registering));
		}
	}

	private IEnumerator FocusForRetry(UIInput field, bool mode)
	{
		yield return null;
		if (!busy && !choosing && !(root == null) && root.activeInHierarchy && registering == mode && !(field == null) && field.isActiveAndEnabled)
		{
			field.selectAllTextOnFocus = true;
			field.isSelected = true;
			field.selectionStart = 0;
			field.selectionEnd = field.value.Length;
			yield return null;
			if (field != null)
			{
				field.selectAllTextOnFocus = false;
			}
		}
	}
}
