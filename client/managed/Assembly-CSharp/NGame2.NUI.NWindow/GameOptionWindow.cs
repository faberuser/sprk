using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NConfig;
using NGame2.NLogin;
using NGame2.NUI.NComponent2;
using NGameAsset.NPatch;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GameOptionWindow : BaseWindow, GameOptionBaseComponent.Listener
{
	[Serializable]
	public class TabUIInfo
	{
		public GameOptionTab GameOptionTab;

		public UISimpleGrid GameOptionGrid;

		public UIToggle Toggle;

		public UIWidget WidgetRoot;
	}

	[Serializable]
	public class ConnectUiGroup
	{
		public UIWidget Root;

		public UIButton ConnectButton;

		public UIWidget ConnectedWidget;

		public void Set(bool isEnable, bool isConnected)
		{
			UIHelper.EnableGameObject(Root, isEnable);
			if (isEnable)
			{
				UIHelper.EnableGameObject(ConnectButton, !isConnected);
				UIHelper.EnableGameObject(ConnectedWidget, isConnected);
			}
		}
	}

	[Serializable]
	public class GameOptionEtc
	{
		public UILabel Label_GameVersionNum;

		public UILabel Label_AccountIdNum;

		public Transform Button_Coupon;

		public Transform Button_CustomerCenter;

		public Transform Button_LocalizedLaw1;

		public Transform Button_LocalizedLaw2;

		public Transform Button_LocalizedLaw3;

		public Transform Button_MemberLeave;

		public Transform Button_Logout;

		public ConnectUiGroup ConnectFacebook;

		public ConnectUiGroup ConnectGPGS;

		public ConnectUiGroup ConnectGameCenter;

		public ConnectUiGroup ConnectApple;

		public ConnectUiGroup ConnectTwitter;

		public ConnectUiGroup ConnectLine;

		public UILabel Label_GuestLogin;

		public UILabel Label_FacebookLogin;

		public UILabel Label_GPGSLogin;

		public UILabel Label_GameCenterLogin;

		public UILabel Label_AppleLogin;

		public UILabel Label_TwitterLogin;

		public UILabel Label_LineLogin;

		public UIButton Btn_LinkedGoogle;

		public UIButton Btn_LinkedApple;

		public UIButton Btn_LinkedSteam;

		public GameObject Obj_ConnectedGoogle;

		public GameObject Obj_ConnectedApple;

		public GameObject Obj_ConnectedSteam;

		public void SetLoginType(LoginMethodType type)
		{
			UIHelper.EnableGameObject(Label_GuestLogin, type == LoginMethodType.Guest);
			UIHelper.EnableGameObject(Label_FacebookLogin, type == LoginMethodType.Steam);
			UIHelper.EnableGameObject(Label_GPGSLogin, type == LoginMethodType.GooglePlus);
			UIHelper.EnableGameObject(Label_GameCenterLogin, type == LoginMethodType.GameCenter);
			UIHelper.EnableGameObject(Label_AppleLogin, type == LoginMethodType.Apple);
			UIHelper.EnableGameObject(Label_TwitterLogin, type == LoginMethodType.Twitter);
			UIHelper.EnableGameObject(Label_LineLogin, type == LoginMethodType.Line);
		}
	}

	public interface Listener
	{
		void OnChangeLanguage();

		void OnLogout();

		void OnReset();

		void OnUseCoupon();

		void OnLeaveParty();

		void ChangeLoginMethod(LoginMethodType methodType);

		void OnCsMailPopup();

		void OnOqupieWebView();
	}

	public interface IDataProvider
	{
		LoginMethodType GetLastLoginMethod();

		CountryData GetCountryData();

		CountryServerData GetCountryServerData();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GameOption/GameOptionWindow";

	public TabUIInfo[] MainTabs;

	private TabUIInfo CurrentTab;

	public GameOptionEtc EtcInfo = new GameOptionEtc();

	private Dictionary<GameOptionTab, List<GameOptionBaseComponent>> gameOptionDic;

	private Listener _listener;

	private IDataProvider _dataProvider;

	public bool _lastFullScreenState;

	private CountryData _countryData;

	private CountryServerData _countryServerData;

	private const int COMMON_LAW_1_INDEX = 0;

	private const int COMMON_LAW_2_INDEX = 1;

	private const int LOCALIZED_LAW_1_INDEX = 0;

	private const int LOCALIZED_LAW_2_INDEX = 1;

	private const int LOCALIZED_LAW_3_INDEX = 2;

	private const string GAME_FRAME_OPTION_NAME = "OPTION_GAME_FRAME_RATE";

	private bool IsCheckRefreshRate;

	private bool islinkbtn;

	private bool isfinish = true;

	public override void ReOpen()
	{
		base.ReOpen();
		SetupOptionGrids();
		SetupEtc();
		SetWidgetRoot();
	}

	public override void Awake()
	{
		base.Awake();
		UIHelper.EnableGameObject(EtcInfo.Button_CustomerCenter, enable: true);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_lastFullScreenState != Screen.fullScreen)
		{
			_lastFullScreenState = Screen.fullScreen;
			SyncFullScreenUI();
		}
	}

	public void Open(Listener listener, IDataProvider dataProvider, GameOptionTab tab)
	{
		SprkLoginSettings.PrepareSettings(this, dataProvider);
		base.Open();
		PreSetup();
		_lastFullScreenState = Screen.fullScreen;
		_listener = listener;
		_dataProvider = dataProvider;
		_countryData = dataProvider.GetCountryData();
		_countryServerData = dataProvider.GetCountryServerData();
		CurrentTab = GetTabInfo(tab);
		SetCurrentTabToggle();
		SetupOptionGrids();
		SetupEtc();
		SetWidgetRoot();
	}

	private void PreSetup()
	{
		CheckDeviceRefreshRate();
	}

	public void SetupOptionGrids()
	{
		bool wasInitializing = GameOptionBaseComponent.InitializingOptions;
		GameOptionBaseComponent.InitializingOptions = true;
		try
		{
			SetupOptionGridsCore();
		}
		finally
		{
			GameOptionBaseComponent.InitializingOptions = wasInitializing;
		}
	}

	private void SetupOptionGridsCore()
	{
		TabUIInfo[] mainTabs;
		if (gameOptionDic == null)
		{
			gameOptionDic = new Dictionary<GameOptionTab, List<GameOptionBaseComponent>>();
			mainTabs = MainTabs;
			foreach (TabUIInfo tabUIInfo in mainTabs)
			{
				List<GameOptionBaseComponent> value = null;
				if (!gameOptionDic.TryGetValue(tabUIInfo.GameOptionTab, out value))
				{
					value = new List<GameOptionBaseComponent>();
					gameOptionDic.Add(tabUIInfo.GameOptionTab, value);
				}
				IList<NShared.GameOptionData> gameOptionDatas = GameRoot.Instance.GameOptionDataContainer.GetGameOptionDatas(tabUIInfo.GameOptionTab);
				if (gameOptionDatas == null)
				{
					continue;
				}
				foreach (NShared.GameOptionData item in gameOptionDatas)
				{
					switch (item.GameOptionComponent)
					{
					case GameOptionComponent.Button:
					{
						GameOptionButtonComponent gameOptionButtonComponent = UIHelper.AddGridItem(tabUIInfo.GameOptionGrid, GameOptionButtonComponent.PrefabAuto);
						gameOptionButtonComponent.Open(this, item);
						value?.Add(gameOptionButtonComponent);
						break;
					}
					case GameOptionComponent.Button_Text:
					{
						GameOptionButtonTextComponent gameOptionButtonTextComponent = UIHelper.AddGridItem(tabUIInfo.GameOptionGrid, GameOptionButtonTextComponent.PrefabAuto);
						gameOptionButtonTextComponent.Open(this, item);
						value?.Add(gameOptionButtonTextComponent);
						break;
					}
					case GameOptionComponent.Slider:
					{
						GameOptionSliderComponent gameOptionSliderComponent = UIHelper.AddGridItem(tabUIInfo.GameOptionGrid, GameOptionSliderComponent.PrefabAuto);
						gameOptionSliderComponent.Open(this, item);
						value?.Add(gameOptionSliderComponent);
						break;
					}
					case GameOptionComponent.Toggle3:
					{
						GameOptionToggle3Component gameOptionToggle3Component = UIHelper.AddGridItem(tabUIInfo.GameOptionGrid, GameOptionToggle3Component.PrefabAuto);
						gameOptionToggle3Component.Open(this, item);
						value?.Add(gameOptionToggle3Component);
						break;
					}
					case GameOptionComponent.Toggle4:
					{
						GameOptionToggle4Component gameOptionToggle4Component = UIHelper.AddGridItem(tabUIInfo.GameOptionGrid, GameOptionToggle4Component.PrefabAuto);
						gameOptionToggle4Component.Open(this, item);
						value?.Add(gameOptionToggle4Component);
						break;
					}
					}
				}
				tabUIInfo.GameOptionGrid.RepositionGrid();
			}
		}
		mainTabs = MainTabs;
		foreach (TabUIInfo tabUIInfo2 in mainTabs)
		{
			List<GameOptionBaseComponent> value2 = null;
			if (!gameOptionDic.TryGetValue(tabUIInfo2.GameOptionTab, out value2))
			{
				continue;
			}
			foreach (GameOptionBaseComponent item2 in value2)
			{
				if (item2.IsParent() && !item2.GetChildState())
				{
					item2.SetChildValue(item2.CurrentValue, item2.GetCurrentOptionValueInfo());
				}
				if (IsDisable(item2.gameOptionData))
				{
					item2.SetGrayScale(enable: false);
				}
			}
		}
		SprkGraphicsOptions.Setup(this);
	}

	public void RefreshComponent(int index)
	{
		TabUIInfo[] mainTabs = MainTabs;
		foreach (TabUIInfo tabUIInfo in mainTabs)
		{
			List<GameOptionBaseComponent> value = null;
			if (!gameOptionDic.TryGetValue(tabUIInfo.GameOptionTab, out value))
			{
				continue;
			}
			foreach (GameOptionBaseComponent item in value)
			{
				if (item.GetIndex() == index)
				{
					item.Refresh();
				}
			}
		}
	}

	public void SetCurrentTabToggle()
	{
		TabUIInfo[] mainTabs = MainTabs;
		foreach (TabUIInfo tabUIInfo in mainTabs)
		{
			if (tabUIInfo == CurrentTab)
			{
				tabUIInfo.Toggle.value = true;
			}
			else
			{
				tabUIInfo.Toggle.value = false;
			}
		}
	}

	private TabUIInfo GetTabInfo(GameOptionTab gameOptionTab)
	{
		TabUIInfo[] mainTabs = MainTabs;
		foreach (TabUIInfo tabUIInfo in mainTabs)
		{
			if (tabUIInfo.GameOptionTab == gameOptionTab)
			{
				return tabUIInfo;
			}
		}
		return MainTabs[0];
	}

	public void OnToggleMainTab()
	{
		TabUIInfo[] mainTabs = MainTabs;
		foreach (TabUIInfo tabUIInfo in mainTabs)
		{
			if (tabUIInfo.Toggle.value)
			{
				CurrentTab = tabUIInfo;
				if (tabUIInfo.GameOptionTab == GameOptionTab.Etc)
				{
					_ = (object)null;
					_ = 59;
					_ = 1;
					_ = 0;
					Array.Empty<object>();
				}
				break;
			}
		}
		SetWidgetRoot();
	}

	public void SetWidgetRoot()
	{
		TabUIInfo[] mainTabs = MainTabs;
		foreach (TabUIInfo tabUIInfo in mainTabs)
		{
			if (tabUIInfo == CurrentTab)
			{
				UIHelper.EnableGameObject(tabUIInfo.WidgetRoot, enable: true);
			}
			else
			{
				UIHelper.EnableGameObject(tabUIInfo.WidgetRoot, enable: false);
			}
		}
	}

	private void SetupEtc()
	{
		islinkbtn = false;
		isfinish = true;
		if (LoginData.LastLoginMethod == LoginMethodType.Guest)
		{
			EtcInfo.Button_MemberLeave.gameObject.SetActive(value: false);
		}
		else
		{
			EtcInfo.Button_MemberLeave.gameObject.SetActive(value: true);
		}
		if (AccountManager.IsLogined())
		{
			UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
			if (userManager != null)
			{
				UIHelper.SetLabelText(EtcInfo.Label_AccountIdNum, userManager.Id);
			}
		}
		if (SingletonAuto<Config>.instance.ClientConfig != null)
		{
			UIHelper.SetLabelText(EtcInfo.Label_GameVersionNum, SingletonAuto<Config>.instance.ClientConfig.Version);
		}
		else
		{
			UIHelper.SetLabelText(EtcInfo.Label_GameVersionNum, "(Error)");
		}
		EtcInfo.ConnectLine.Set(isEnable: false, isConnected: false);
		EtcInfo.ConnectTwitter.Set(isEnable: false, isConnected: false);
		EtcInfo.SetLoginType(_dataProvider.GetLastLoginMethod());
		SprkAccounts.CleanSettings(this);
	}

	private bool IsDisable(NShared.GameOptionData optionData)
	{
		if (optionData == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<PatchDownloader>.instance.IsPatchDownloading() && optionData.OptionName == "OPTION_GAME_QUALITY_TEXTURE")
		{
			return true;
		}
		if (!UIHelper.IsATSCSupported() && optionData.OptionName == "OPTION_GAME_QUALITY_TEXTURE")
		{
			return true;
		}
		return false;
	}

	public void OnClickCopyAccountId()
	{
		UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
		if (userManager != null)
		{
			UniClipboard.SetText(userManager.Id.ToString());
			CenterMessage.Open(LocalizationManager.GetString(GameOption.COPY_ACCOUNT_ID));
		}
	}

	public void OnClickCustomerCenter()
	{
	}

	public void OnClickChangeLanguage()
	{
		if (_listener != null)
		{
			_listener.OnChangeLanguage();
		}
	}

	public void OnClickLogout()
	{
		if (_listener != null)
		{
			_listener.OnLogout();
		}
	}

	public void OnClickReset()
	{
		if (_listener != null)
		{
			_listener.OnReset();
		}
	}

	public void OnClickUseCoupon()
	{
		if (_listener != null)
		{
			_listener.OnUseCoupon();
		}
	}

	public void OnClickLeaveParty()
	{
		if (_listener != null)
		{
			_listener.OnLeaveParty();
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}

	public void OnClickChangeToFaceBook()
	{
		ConnectToSocialId(LoginMethodType.Steam);
	}

	public void OnClickChangeToGPGS()
	{
		ConnectToSocialId(LoginMethodType.GooglePlus);
	}

	public void OnClickChangeToGameCenter()
	{
		ConnectToSocialId(LoginMethodType.GameCenter);
	}

	public void OnClickChangeToApple()
	{
		ConnectToSocialId(LoginMethodType.Apple);
	}

	public void OnClickChangeToTwitter()
	{
	}

	public void OnClickChangeToLine()
	{
	}

	private void ConnectToSocialId(LoginMethodType methodType)
	{
	}

	private void CheckDeviceRefreshRate()
	{
	}

	private IEnumerator OpenLinkedButton()
	{
		bool isComplete = false;
		ConnectedData connectedData = new ConnectedData();
		isfinish = false;
		bool isSuccess = false;
		GlobalAutoBehaviour<MasangLoginManager>.instance.CheckLinkedPlatform(delegate(bool issuccess, ConnectedData data)
		{
			connectedData = data;
			isSuccess = issuccess;
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
		if (!isSuccess)
		{
			isfinish = true;
			yield break;
		}
		if (connectedData.isGoogle)
		{
			EtcInfo.Obj_ConnectedGoogle.SetActive(value: true);
		}
		else
		{
			EtcInfo.Obj_ConnectedGoogle.SetActive(value: false);
		}
		if (connectedData.isApple)
		{
			EtcInfo.Obj_ConnectedApple.SetActive(value: true);
		}
		else
		{
			EtcInfo.Obj_ConnectedApple.SetActive(value: false);
		}
		if (connectedData.isSteam)
		{
			EtcInfo.Obj_ConnectedSteam.SetActive(value: true);
		}
		else
		{
			EtcInfo.Obj_ConnectedSteam.SetActive(value: false);
		}
		isfinish = true;
		islinkbtn = true;
	}

	private void SyncFullScreenUI()
	{
		SystemOptionData.SyncWindowOptionFromSystem();
		if (gameOptionDic == null)
		{
			return;
		}
		TabUIInfo[] mainTabs = MainTabs;
		foreach (TabUIInfo tabUIInfo in mainTabs)
		{
			List<GameOptionBaseComponent> value = null;
			if (!gameOptionDic.TryGetValue(tabUIInfo.GameOptionTab, out value))
			{
				continue;
			}
			foreach (GameOptionBaseComponent item in value)
			{
				if (item == null || item.gameOptionData == null)
				{
					continue;
				}
				if (item.gameOptionData.OptionName == "OPTION_GAME_RESOULUTION")
				{
					GameOptionToggle4Component[] componentsInChildren = item.GetComponentsInChildren<GameOptionToggle4Component>(includeInactive: true);
					foreach (GameOptionToggle4Component gameOptionToggle4Component in componentsInChildren)
					{
						if (!(gameOptionToggle4Component != null))
						{
							continue;
						}
						UIToggle[] componentsInChildren2 = gameOptionToggle4Component.GetComponentsInChildren<UIToggle>(includeInactive: true);
						for (int k = 0; k < componentsInChildren2.Length; k++)
						{
							_ = componentsInChildren2[k];
							if (Screen.fullScreen)
							{
								UIHelper.SetWidgetGrayScale(item.GetComponent<UIWidget>(), grayScale: true, setColliders: true);
							}
							else
							{
								UIHelper.SetWidgetGrayScale(item.GetComponent<UIWidget>(), grayScale: false, setColliders: true);
							}
						}
					}
				}
				item.Refresh();
			}
		}
	}

	public void OnClickPrivacyPolicy()
	{
	}

	public void OnClickTermsOfService()
	{
	}

	public void OnClickLocalizedLaw1()
	{
		if (_countryServerData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("MENU_SPECIFIC_COMMERCIAL_TRANSACTIONS_LAW");
			LawRelatedPopup.Open(_countryServerData, stringDirect, 0);
		}
	}

	public void OnClickLocalizedLaw2()
	{
		if (_countryServerData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("MENU_MONEY_SETTLEMENT_LAW");
			LawRelatedPopup.Open(_countryServerData, stringDirect, 1);
		}
	}

	public void OnClickLocalizedLaw3()
	{
		if (_countryServerData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("MENU_SERVICE_GUIDELINE");
			LawRelatedPopup.Open(_countryServerData, stringDirect, 2);
		}
	}

	private GameOptionBaseComponent GetGameOptionComponent(int index)
	{
		TabUIInfo[] mainTabs = MainTabs;
		foreach (TabUIInfo tabUIInfo in mainTabs)
		{
			List<GameOptionBaseComponent> value = null;
			if (!gameOptionDic.TryGetValue(tabUIInfo.GameOptionTab, out value))
			{
				continue;
			}
			foreach (GameOptionBaseComponent item in value)
			{
				if (item.GetIndex() == index)
				{
					return item;
				}
			}
		}
		return null;
	}

	void GameOptionBaseComponent.Listener.SetChildsValue(int[] childIndices, object value, bool enable, GameOptionValueInfo valueInfo)
	{
		if (childIndices == null || __Extension_Method_ICollection__.COUNT(childIndices) <= 0)
		{
			return;
		}
		foreach (int index in childIndices)
		{
			GameOptionBaseComponent gameOptionComponent = GetGameOptionComponent(index);
			if (gameOptionComponent != null)
			{
				gameOptionComponent.SetValueByParent(value, valueInfo);
				gameOptionComponent.SetGrayScale(enable);
				gameOptionComponent.Refresh();
			}
		}
	}

	void GameOptionBaseComponent.Listener.CheckAllChildValue(int parentIndex, object value, GameOptionValueInfo valueInfo)
	{
		IList<int> childList = GameRoot.Instance.GameOptionDataContainer.GetChildList(parentIndex);
		int[] disableGroup;
		foreach (int item in childList)
		{
			GameOptionBaseComponent gameOptionComponent = GetGameOptionComponent(item);
			if (!(gameOptionComponent != null))
			{
				continue;
			}
			GameOptionValueInfo currentOptionValueInfo = gameOptionComponent.GetCurrentOptionValueInfo();
			if (currentOptionValueInfo == null || currentOptionValueInfo.DisableGroup == null)
			{
				return;
			}
			if (__Extension_Method_ICollection__.COUNT(currentOptionValueInfo.DisableGroup) <= 0 || __Extension_Method_ICollection__.COUNT(valueInfo.DisableGroup) <= 0)
			{
				continue;
			}
			disableGroup = valueInfo.DisableGroup;
			int num = 0;
			if (num < disableGroup.Length)
			{
				int value2 = disableGroup[num];
				if (!TypeIterator.Contains(currentOptionValueInfo.DisableGroup, value2))
				{
					return;
				}
			}
		}
		foreach (int item2 in childList)
		{
			GameOptionBaseComponent gameOptionComponent2 = GetGameOptionComponent(item2);
			if (gameOptionComponent2 != null)
			{
				gameOptionComponent2.SetGrayScale(enable: false);
			}
		}
		GameOptionBaseComponent gameOptionComponent3 = GetGameOptionComponent(parentIndex);
		if (!(gameOptionComponent3 != null))
		{
			return;
		}
		disableGroup = valueInfo.DisableGroup;
		foreach (int groupIndex in disableGroup)
		{
			GameOptionValueInfo disableGroupOptionValueInfo = gameOptionComponent3.GetDisableGroupOptionValueInfo(groupIndex);
			object objectValue = gameOptionComponent3.GetObjectValue(disableGroupOptionValueInfo);
			if (disableGroupOptionValueInfo != null && objectValue != null)
			{
				gameOptionComponent3.SetValueByChilds(objectValue, disableGroupOptionValueInfo);
			}
		}
	}
}
