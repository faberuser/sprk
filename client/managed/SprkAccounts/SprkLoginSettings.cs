using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NClientData;
using NGame2.NLogin;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NJit;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkLoginSettings : MonoBehaviour, GameOptionWindow.Listener, GameOptionWindow.IDataProvider
{
	public sealed class LoginContext : MonoBehaviour
	{
		public readonly Dictionary<GameObject, bool> Hidden = new Dictionary<GameObject, bool>();

		public void Hide(GameObject go)
		{
			if (!(go == null))
			{
				if (!Hidden.ContainsKey(go))
				{
					Hidden.Add(go, go.activeSelf);
				}
				go.SetActive(value: false);
			}
		}

		public void Restore()
		{
			foreach (KeyValuePair<GameObject, bool> item in Hidden)
			{
				if (item.Key != null)
				{
					item.Key.SetActive(item.Value);
				}
			}
			Hidden.Clear();
		}
	}

	private LoginAccountInfo login;

	private GameOptionWindow settings;

	private static readonly Dictionary<string, UIAtlas> loginAtlases = new Dictionary<string, UIAtlas>();

	private static bool probeStarted;

	private static bool AtLogin
	{
		get
		{
			if (SceneAutoBehaviour<LoginManagement>.isValidInstance)
			{
				return !SceneAutoBehaviour<LobbyManagement>.isValidInstance;
			}
			return false;
		}
	}

	public static void Configure(LoginAccountInfo view)
	{
		if (view == null)
		{
			return;
		}
		if (view.PatchValidationRoot != null)
		{
			view.PatchValidationRoot.SetActive(value: false);
		}
		if (view.VoiceButtonRoot != null)
		{
			view.VoiceButtonRoot.SetActive(value: false);
		}
		if (view.LanguageButtonRoot == null)
		{
			return;
		}
		SprkLoginSettings sprkLoginSettings = view.GetComponent<SprkLoginSettings>();
		if (sprkLoginSettings == null)
		{
			sprkLoginSettings = view.gameObject.AddComponent<SprkLoginSettings>();
		}
		sprkLoginSettings.login = view;
		UILocalize[] componentsInChildren = view.LanguageButtonRoot.GetComponentsInChildren<UILocalize>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		UILocalizeEx[] componentsInChildren2 = view.LanguageButtonRoot.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = false;
		}
		UILabel[] componentsInChildren3 = view.LanguageButtonRoot.GetComponentsInChildren<UILabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].text = SprkText.Get("Settings");
		}
		UIButton componentInChildren = view.LanguageButtonRoot.GetComponentInChildren<UIButton>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.onClick.Clear();
			EventDelegate.Add(componentInChildren.onClick, sprkLoginSettings.OpenSettings);
		}
		if (view.LoginButtonRoot != null)
		{
			view.LoginButtonRoot.SetActive(value: false);
		}
		Transform transform = ((view.Label_ServerName != null) ? view.Label_ServerName.transform.parent : null);
		if (transform != null && transform.name == "ServerInfo")
		{
			transform.gameObject.SetActive(value: false);
			if (view.AccountInfoGrid != null)
			{
				view.AccountInfoGrid.Reposition();
			}
		}
		GameObject gameIntroduceButtonRoot = view.GameIntroduceButtonRoot;
		if (gameIntroduceButtonRoot != null && view.ButtonGrid != null)
		{
			componentsInChildren = gameIntroduceButtonRoot.GetComponentsInChildren<UILocalize>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			componentsInChildren2 = gameIntroduceButtonRoot.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = false;
			}
			componentsInChildren3 = gameIntroduceButtonRoot.GetComponentsInChildren<UILabel>(includeInactive: true);
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].text = SprkText.Get("Introduction");
			}
			if (gameIntroduceButtonRoot.transform.parent == view.ButtonGrid.transform)
			{
				gameIntroduceButtonRoot.transform.SetParent(view.ButtonGrid.transform.parent, worldPositionStays: false);
			}
		}
		if (view.ButtonGrid != null)
		{
			view.ButtonGrid.hideInactive = true;
			view.ButtonGrid.Reposition();
		}
		sprkLoginSettings.PlaceIntroduction();
	}

	public static void ConfigureTouch(LoginTouchWait view)
	{
		if (view != null && view.GetComponent<SprkTapToPlay>() == null)
		{
			view.gameObject.AddComponent<SprkTapToPlay>();
		}
	}

	private void LateUpdate()
	{
		PlaceIntroduction();
	}

	private void PlaceIntroduction()
	{
		if (!(login == null) && !(login.GameIntroduceButtonRoot == null) && !(login.LanguageButtonRoot == null) && !(login.ButtonGrid == null))
		{
			Transform transform = login.GameIntroduceButtonRoot.transform;
			Transform transform2 = login.transform.Find("Panel/Root");
			if (!(transform.parent != login.ButtonGrid.transform.parent) && !(transform2 == null))
			{
				Vector3 vector = transform2.InverseTransformPoint(login.LanguageButtonRoot.transform.position);
				transform.position = transform2.TransformPoint(new Vector3(0f - vector.x, vector.y, vector.z));
			}
		}
	}

	private static void PrepareOptions()
	{
		IDictionary dictionary = (IDictionary)typeof(TableRoot).GetField("_dic", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(TableRoot.Instance);
		lock (dictionary)
		{
			if (!dictionary.Contains(typeof(GameOptionDataContainer)))
			{
				GameOptionDataContainer gameOptionDataContainer = new GameOptionDataContainer();
				gameOptionDataContainer.Load();
				if (!gameOptionDataContainer.IsLoaded)
				{
					throw new InvalidOperationException("The installed settings table could not be loaded.");
				}
				dictionary.Add(typeof(GameOptionDataContainer), gameOptionDataContainer);
			}
		}
	}

	public void OpenSettings()
	{
		PrepareOptions();
		SingletonAuto<LocalizationLoader>.instance.SetEnableLoadFromPatch(enableLoad: true);
		LoginManagement instance = SceneAutoBehaviour<LoginManagement>.instance;
		settings = instance.GetInstance<GameOptionWindow>();
		if (settings == null)
		{
			instance.AddInstance<GameOptionWindow>("Resources/Prefab2/Window/GameOption/GameOptionWindow");
			settings = instance.GetInstance<GameOptionWindow>();
		}
		settings.Open(this, this, GameOptionTab.Game);
	}

	public static void PrepareSettings(GameOptionWindow view, GameOptionWindow.IDataProvider provider)
	{
		LoginContext component = view.GetComponent<LoginContext>();
		if (component != null)
		{
			component.Restore();
		}
		if (provider is SprkLoginSettings)
		{
			if (component == null)
			{
				view.gameObject.AddComponent<LoginContext>();
			}
		}
		else if (component != null)
		{
			UnityEngine.Object.DestroyImmediate(component);
		}
	}

	public static void CleanSettings(GameOptionWindow view)
	{
		LoginContext component = view.GetComponent<LoginContext>();
		if (component == null || !AtLogin)
		{
			return;
		}
		GameOptionWindow.GameOptionEtc etcInfo = view.EtcInfo;
		Transform[] array = new Transform[3] { etcInfo.Button_Coupon, etcInfo.Button_Logout, etcInfo.Button_MemberLeave };
		foreach (Transform transform in array)
		{
			if (transform != null)
			{
				component.Hide(transform.gameObject);
			}
		}
		string[] array2 = new string[3] { "Button_SwitchAccount", "Button_ChangePassword", "Button_DeleteAccount" };
		foreach (string text in array2)
		{
			array = view.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform2 in array)
			{
				if (transform2.name == text)
				{
					component.Hide(transform2.gameObject);
				}
			}
		}
		GameOptionBaseComponent[] componentsInChildren = view.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true);
		foreach (GameOptionBaseComponent gameOptionBaseComponent in componentsInChildren)
		{
			if (gameOptionBaseComponent.gameOptionData != null && (gameOptionBaseComponent.gameOptionData.PostProcessFunction == "OpenSkipRubyCountPopup" || gameOptionBaseComponent.gameOptionData.PostProcessFunction == "ManageBlockedUser"))
			{
				component.Hide(gameOptionBaseComponent.gameObject);
			}
		}
	}

	public static UIAtlas TryLoginAtlas(string name)
	{
		if ((name != "GameOption" && name != "PayShop") || !AtLogin)
		{
			return null;
		}
		return LoadLoginAtlas(name);
	}

	private static UIAtlas LoadLoginAtlas(string name)
	{
		string text = ((SystemOptionData.Spec == ESpecType.Low) ? "LowLargeSpec" : "HighAndMidSpec");
		string key = text + "/" + name;
		if (loginAtlases.TryGetValue(key, out var value) && value != null)
		{
			return value;
		}
		AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle", text, "NguiAtlas_UI_Prefab_" + name + ".unity3d"));
		if (assetBundle == null)
		{
			throw new InvalidOperationException("The installed settings artwork could not be loaded.");
		}
		value = assetBundle.mainAsset as UIAtlas;
		if (value == null)
		{
			throw new InvalidOperationException("The installed settings artwork has no atlas.");
		}
		Shader shader = Shader.Find("Unlit/Transparent Colored");
		if (shader == null)
		{
			throw new InvalidOperationException("The login UI shader is unavailable.");
		}
		Material material = new Material(shader)
		{
			mainTexture = value.texture
		};
		if (material.mainTexture == null)
		{
			throw new InvalidOperationException("The installed settings artwork has no texture.");
		}
		value.spriteMaterial = material;
		assetBundle.Unload(unloadAllLoadedObjects: false);
		loginAtlases[key] = value;
		return value;
	}

	public static bool SetTextureQuality(GameOptionBaseComponent row, object value)
	{
		if (!AtLogin)
		{
			return false;
		}
		SystemOptionData.Spec = (ESpecType)Enum.Parse(typeof(ESpecType), value.ToString());
		SprkTestPrefs.Save();
		return true;
	}

	public static bool SetPlayBGM(GameOptionBaseComponent row, object value)
	{
		if (!AtLogin)
		{
			return false;
		}
		SprkLoginPresentation.RefreshMusic();
		return true;
	}

	public LoginMethodType GetLastLoginMethod()
	{
		return LoginData.LastLoginMethod;
	}

	public CountryData GetCountryData()
	{
		return null;
	}

	public CountryServerData GetCountryServerData()
	{
		return null;
	}

	public void OnChangeLanguage()
	{
		login.OnClickChangeLanguage();
	}

	public void OnLogout()
	{
		login.OnClickLogout();
	}

	public void OnReset()
	{
	}

	public void OnUseCoupon()
	{
	}

	public void OnLeaveParty()
	{
	}

	public void ChangeLoginMethod(LoginMethodType type)
	{
	}

	public void OnCsMailPopup()
	{
	}

	public void OnOqupieWebView()
	{
	}

	public static void EnsureProbe()
	{
		SprkLoginPresentation.EnsureProbe();
		if (!probeStarted && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_LOGIN_SETTINGS_TEST")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")))
		{
			probeStarted = true;
			GameObject obj = new GameObject("LoginSettingsProbe");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.AddComponent<SprkLoginSettingsProbe>();
		}
	}
}
