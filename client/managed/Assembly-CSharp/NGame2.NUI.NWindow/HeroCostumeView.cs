using System;
using System.Collections.Generic;
using System.Text;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NPayShopItemEvent;
using NGame2.NUI.NComponent2.NCostume;
using NGame2.NUI.NManager;
using NGame2.NUI.NRender3D.NRenderTargetCinema;
using NGame2.NUI.NWindow.NCustomizing;
using NGame2.NUtil.NCostume;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroCostumeView : HeroCostumeSceneView, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>, IUserObserver<PayShopItemEventManager.EventChangedInfo>, IUserObserver<CostumeStorageSlotInfo>, IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>, IRenderFacadeController, IWidgetUiController
{
	public new interface Listener : HeroFullSceneViewBase.Listener
	{
		void OnClickPlusGold();

		void OnClickPlusGem();

		void OnClickViewMotion(HeroCostumeView caller);

		void ProcessSelectedCustomizingCostumes(HeroCustomizingDataGetter dataGetter, Action onComplete);

		void ResetHeroCostumes(int heroIndex, Action onComplete);

		void OpenLegendCostumeReservationPooup(int costumeIndex);
	}

	[Serializable]
	public class CostumeInfoUI
	{
		public UILabel Label_CostumeName;

		public UILabel Label_CostumeNameOutline;

		public UIWidget WidgetObtainDesc;

		public UIWidget WidgetPriceDesc;

		public UIWidget WidgetMileageDesc;

		public UIWidget WidgetOwned;

		public UIWidget WidgetEquipped;

		public UIWidget WidgetNeedCostume;

		public UILabel Label_Requirement;

		public UILabel Label_Price;

		public UILabel Label_DiscountPrice;

		public UILabel Label_MileageValue;

		public UILabel Label_AbilityDesc;
	}

	[Serializable]
	public class CostumeButtonInfo
	{
		public CostumeState State;

		public UIButton Button;
	}

	public class CameraOffsetUpdater
	{
		public Vector3 AddtionalOffset;

		public Vector3 TargetOffset;

		public float ZoomRate;

		public float Speed = 2f;

		public void Update(ref Vector3 currentOffset, ref float currentZoomRate)
		{
			float t = Time.deltaTime * Speed;
			Vector3 vector = currentOffset;
			Vector3 vector2 = TargetOffset + AddtionalOffset;
			if (!Mathf.Approximately(vector.x, vector2.x))
			{
				currentOffset.x = Mathf.Lerp(vector.x, vector2.x, t);
			}
			if (!Mathf.Approximately(vector.y, vector2.y))
			{
				currentOffset.y = Mathf.Lerp(vector.y, vector2.y, t);
			}
			if (!Mathf.Approximately(currentZoomRate, ZoomRate))
			{
				currentZoomRate = Mathf.Lerp(currentZoomRate, ZoomRate, t);
			}
		}
	}

	[Serializable]
	public class ToggleInfo
	{
		public ToggleType ToggleType;

		public UIToggle Toggle;

		public MonoBehaviour Widget;

		public Transform ButtonRoot;

		public void Open(bool isSelected)
		{
			bool flag = false;
			WidgetBase widgetBase = Widget as WidgetBase;
			if (!(widgetBase == null))
			{
				if (UIHelper.IsEnabledGameObjet(Widget) == !isSelected)
				{
					UIHelper.EnableGameObject(Widget, isSelected);
					flag = true;
				}
				if (UIHelper.IsEnabledGameObjet(ButtonRoot) == !isSelected)
				{
					UIHelper.EnableGameObject(ButtonRoot, isSelected);
				}
				if (isSelected)
				{
					widgetBase.Open(ToggleType);
				}
				else if (flag)
				{
					widgetBase.OnClose();
				}
			}
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/FullScene/HeroCostumeView";

	public CostumeInfoUI CostumeInfoUIs;

	public CostumeButtonInfo[] CostumeButtonInfos;

	public Transform MenuRoot;

	public UILabel Label_Mileage;

	public UILabel Label_Gold;

	public UILabel Label_Gem;

	public UIWidget WidgetViewMotion;

	public UILabel Label_AbilityDesc;

	public ToggleInfo[] ToggleList;

	private ToggleType _currentToggleType = ToggleType.Costume;

	private List<WidgetBase> _widgetContainer = new List<WidgetBase>();

	public MySetCostumeViewWidget MySetCostumeViewWidget;

	public HairCostumeViewWidget HairCostumeViewWidget;

	public HeroCostumeViewWidget HeroCostumeViewWidget;

	public WeaponCostumeViewWidget WeaponCostumeViewWidget;

	public AccessoryCostumeViewWidget[] AccessoryCostumeViewWidget;

	public HeroListWidget HeroListWidget;

	public DetailViewWidget DetailViewWidget;

	public UIButton ButtonResetCostumes;

	public UIButton ButtonDetailView;

	public UIButton ButtonHide;

	private const string LocaleKeyBuyAll = "CONTENT_CUSTOMIZE_BUY_ALL";

	private const string LocaleKeyEquipAll = "CONTENT_CUSTOMIZE_EQUIP_ALL";

	public UILabel LabelButtonBuyCostumes;

	public UIButton ButtonBuyCostumes;

	private CameraOffsetUpdater _cameraOffsetUpdater = new CameraOffsetUpdater();

	private bool _enableUpdateCaramara;

	private bool _needRefreshCostumesButton;

	protected new Listener _listener;

	private CachedHeroInfoCostumeStock _lastChangedHeroInfoCostumeStock;

	private Func<CostumeData> _heroCostumeDataGetter;

	private Func<HairCostumeComponent.OpenData> _hairCostumeOpenDataGetter;

	private Func<WeaponCostumeComponent.OpenData> _weaponCostumeOpenDataGetter;

	private Func<AccessoryCostumeComponent.OpenData>[] _accessoryCostumeOpenDataGetterArray;

	public CachedHeroInfo CurrentLocalInfo => LocalHeroInfo;

	protected override string _nodeTreeName => "HeroCostumeView";

	public CachedHeroInfo LocalHeroInfo { get; private set; }

	public void OnChangeToggleValue()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleList); i++)
		{
			if (ToggleList[i].Toggle.value)
			{
				_currentToggleType = ToggleList[i].ToggleType;
				UpdateToggle();
				break;
			}
		}
	}

	protected override void SetCameraController()
	{
		_cameraController = new CustomizingCameraController(_cinema, _heroUid);
		_cameraController.SetCameraInfo(base.HeroInfo.HeroIndex, base.HeroInfo.CostumeIndex);
		_cameraController.ZoomOutRate = 0.5f;
	}

	public void Open(CachedHeroInfo heroInfo, Listener listener, ToggleType toggleType = ToggleType.Costume, Action onEnd = null)
	{
		_currentToggleType = toggleType;
		_listener = listener;
		Open(heroInfo, listener, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
			InitPlayerAssetInfo();
			CloseHeroList();
		});
		if (ClientValueManager.CheckCustomizingCostumeValid)
		{
			CheckValidHeroManagerCostumesValid();
		}
		RegistObserver();
		InitContainer();
	}

	private void ResetLocalHeroInfo()
	{
		LocalHeroInfo = JsonMarshaler.Clone(base.HeroInfo);
		_lastChangedHeroInfoCostumeStock = CachedHeroInfoCostumeStock.NullStock;
	}

	private void SetWidgetDataGetter()
	{
		_heroCostumeDataGetter = () => HeroCostumeViewWidget.CurrentCostumeData;
		_hairCostumeOpenDataGetter = () => HairCostumeViewWidget.CurrentComponentOpenData;
		_weaponCostumeOpenDataGetter = () => WeaponCostumeViewWidget.CurrentComponentOpenData;
		_accessoryCostumeOpenDataGetterArray = new Func<AccessoryCostumeComponent.OpenData>[__Extension_Method_ICollection__.COUNT(AccessoryCostumeViewWidget)];
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(AccessoryCostumeViewWidget); num++)
		{
			AccessoryCostumeViewWidget widget = AccessoryCostumeViewWidget[num];
			_accessoryCostumeOpenDataGetterArray[num] = () => widget.CurrentComponentOpenData;
		}
	}

	private void InitContainer()
	{
		SetWidgetDataGetter();
		SetWidgetListener();
		_widgetContainer.Add(MySetCostumeViewWidget.SetRenderFacadeController(this).SetUiProvider(this));
		_widgetContainer.Add(HairCostumeViewWidget.SetRenderFacadeController(this).SetUiProvider(this));
		_widgetContainer.Add(HeroCostumeViewWidget.SetRenderFacadeController(this).SetUiProvider(this));
		_widgetContainer.Add(WeaponCostumeViewWidget.SetRenderFacadeController(this).SetUiProvider(this));
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AccessoryCostumeViewWidget); i++)
		{
			_widgetContainer.Add(AccessoryCostumeViewWidget[i].SetRenderFacadeController(this).SetUiProvider(this));
		}
		_widgetContainer.Add(HeroListWidget.SetRenderFacadeController(this).SetUiProvider(this));
	}

	private void SetWidgetListener()
	{
		MySetCostumeViewWidget.SetListener(SceneAutoBehaviour<CostumeManagement>.instance.MySetCostumeRequester);
		HairCostumeViewWidget.SetListener(SceneAutoBehaviour<CostumeManagement>.instance.HairCostumeRequester);
		HeroCostumeViewWidget.SetListener(SceneAutoBehaviour<CostumeManagement>.instance.HeroCostumeRequester);
		WeaponCostumeViewWidget.SetListener(SceneAutoBehaviour<CostumeManagement>.instance.WeaponCostumeRequester);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AccessoryCostumeViewWidget); i++)
		{
			AccessoryCostumeViewWidget[i].SetListener(SceneAutoBehaviour<CostumeManagement>.instance.AccessoryCostumeRequester);
		}
	}

	private void ForEachContainer(Action<WidgetBase> action)
	{
		_widgetContainer.ForEach(action);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateCamera();
		CheckButtonRefresh();
	}

	private void UpdateCamera()
	{
		if (__Extension_Method_ICollection__.COUNT(_touchList) > 0)
		{
			_enableUpdateCaramara = false;
		}
		if (_enableUpdateCaramara && base.CameraController != null)
		{
			float currentZoomRate = base.CameraController.ZoomOutRate;
			_cameraOffsetUpdater.AddtionalOffset = (DetailViewWidget.isActiveAndEnabled ? DetailViewWidget.CameraOffset : Vector3.zero);
			_cameraOffsetUpdater.Update(ref NormalModeExtraOffset, ref currentZoomRate);
			base.CameraController.ZoomOutRate = currentZoomRate;
		}
	}

	private void StartCameraZoom(AccessoryPartType cameraSettingType)
	{
		ClientCustomizingCameraData data = GameRoot.Instance.ClientCustomizingCameraDataContainer.GetData(cameraSettingType);
		if (data != null)
		{
			_cameraOffsetUpdater.ZoomRate = data.ZoomRate;
			_cameraOffsetUpdater.TargetOffset = NVespa.Math.Vec3FromArray(data.ExtraOffset);
			_enableUpdateCaramara = true;
		}
	}

	protected override void InitInfo()
	{
		base.InitInfo();
		ForEachContainer(delegate(WidgetBase widget)
		{
			widget.Reset();
		});
		ResetLocalHeroInfo();
		UpdateToggle();
	}

	protected override void FirstAction()
	{
		base.FirstAction();
		if (_currentToggleType == ToggleType.Costume)
		{
			SetRenderFacedeAnimation(new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, "Signature"));
		}
		else
		{
			SetRenderFacedeAnimation(new AnimationKeyData(DefineCreature.EMotion.Idle, "Idle"));
		}
	}

	public override void OnTouch()
	{
		UpdatePauseButtonState();
	}

	private void UpdateToggle()
	{
		ToggleInfo selectedToggle = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleList); i++)
		{
			ToggleInfo toggleInfo = ToggleList[i];
			if (toggleInfo.ToggleType == _currentToggleType)
			{
				selectedToggle = toggleInfo;
				continue;
			}
			toggleInfo.Toggle.Set(state: false);
			toggleInfo.Open(isSelected: false);
		}
		OnSelect(selectedToggle);
		FirstAction();
	}

	private void OnSelect(ToggleInfo selectedToggle)
	{
		if (selectedToggle != null)
		{
			selectedToggle.Toggle.Set(state: true);
			selectedToggle.Open(isSelected: true);
			OnOffDetailViewButton();
			SetEnableDetailViewButton();
			SetEnableResetCostumesButton();
			StartCameraZoom(selectedToggle.ToggleType.GetCustomizingCameraType());
		}
	}

	private void SetEnableResetCostumesButton()
	{
		bool enable = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(base.HeroInfo.HeroIndex) != null;
		UIHelper.EnableButton(ButtonResetCostumes, enable, grayscaleIfDisable: true);
	}

	private void OnOffDetailViewButton()
	{
		bool enable = _currentToggleType.GetAccessoryType() != AccessoryPartType.None;
		UIHelper.EnableGameObject(ButtonDetailView, enable);
	}

	private void SetEnableDetailViewButton()
	{
		AccessoryCostumeViewWidget accessoryCostumeViewWidget = GetCurrentWidget() as AccessoryCostumeViewWidget;
		bool enable = accessoryCostumeViewWidget != null && accessoryCostumeViewWidget.CurrentComponentOpenData.ComponentType == AccessoryCostumeComponent.ComponentType.SelectCostume;
		UIHelper.EnableButton(ButtonDetailView, enable, grayscaleIfDisable: true);
	}

	private void SetVisibleHideButton()
	{
		HeroCustomizingDataGetter currentCostumeGetter = GetCurrentCostumeGetter();
		CustomizingCostumesStock customizingCostumesStock = CustomizingHelper.CollectCostumes(currentCostumeGetter);
		CostumeData costumeData = Vespa_System_Action_Extension_Method.fire(currentCostumeGetter.HeroCostumeDataGetter);
		HairCostumeComponent.OpenData openData = Vespa_System_Action_Extension_Method.fire(currentCostumeGetter.HairCostumeOpenDataGetter);
		WeaponCostumeComponent.OpenData openData2 = Vespa_System_Action_Extension_Method.fire(currentCostumeGetter.WeaponCostumeOpenDataGetter);
		bool flag = true;
		if (costumeData != null)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeData.CostumeIndex);
			if (data != null && !data.IsShow && GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(data.CostumeIndex) == null)
			{
				flag = false;
			}
		}
		HairCostumeData hairCostumeData = openData.HairCostumeData;
		if (hairCostumeData != null && !hairCostumeData.IsShow && GlobalAutoBehaviour<AccountManager>.instance.HairCostumeManager.Get(hairCostumeData.Index) == null)
		{
			flag = false;
		}
		WeaponCostumeData weaponCostumeData = openData2.WeaponCostumeData;
		if (weaponCostumeData != null && !weaponCostumeData.IsShow && GlobalAutoBehaviour<AccountManager>.instance.WeaponCostumeManager.Get(weaponCostumeData.Index) == null)
		{
			flag = false;
		}
		if (customizingCostumesStock.AccessoryPositionPairs != null)
		{
			foreach (Func<AccessoryCostumeComponent.OpenData> accessoryCostumeOpenDataGetter in currentCostumeGetter.AccessoryCostumeOpenDataGetterList)
			{
				AccessoryCostumeData accessoryCostumeData = Vespa_System_Action_Extension_Method.fire(accessoryCostumeOpenDataGetter).AccessoryCostumeData;
				if (accessoryCostumeData != null && !accessoryCostumeData.IsShow && GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(base.HeroInfo.HeroIndex, accessoryCostumeData.Index)) == null)
				{
					flag = false;
				}
			}
		}
		UIHelper.EnableGameObject(ButtonHide, enable: true);
	}

	private void CurrentWidget(Action<WidgetBase> action)
	{
		Vespa_System_Action_Extension_Method.fire(action, GetCurrentWidget());
	}

	private WidgetBase GetCurrentWidget()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleList); i++)
		{
			if (ToggleList[i].ToggleType == _currentToggleType)
			{
				return ToggleList[i].Widget as WidgetBase;
			}
		}
		return null;
	}

	private WidgetBase GetWidgetbyToggleType(ToggleType toggleType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleList); i++)
		{
			if (ToggleList[i].ToggleType == toggleType)
			{
				return ToggleList[i].Widget as WidgetBase;
			}
		}
		return null;
	}

	private void InitPlayerAssetInfo()
	{
		UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
		if (userManager != null)
		{
			SetMileage(userManager.Mileage);
			SetGold(userManager.Gold);
			SetGem(userManager.TotalGem);
		}
	}

	public override void OnClickCloseNormal()
	{
		if (DetailViewWidget.isActiveAndEnabled)
		{
			OnCloseDetailView();
			return;
		}
		if (SceneAutoBehaviour<MyHeroManagement>.isValidInstance)
		{
			HeroList instance = SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<HeroList>();
			if ((bool)instance && instance.IsOpen() && instance.Hero3DViewPanel != null)
			{
				instance.Hero3DViewPanel.SetupCameraZoom();
			}
		}
		base.OnClickCloseNormal();
		RemoveObserver();
	}

	private void OnCloseDetailView()
	{
		DetailViewWidget.Close();
		MenuRoot.gameObject.SetActive(value: true);
		StartCameraZoom(_currentToggleType.GetCustomizingCameraType());
		_needRefreshCostumesButton = true;
	}

	private void CheckValidHeroManagerCostumesValid()
	{
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		stringBuilder.Append($"AccountId ({accountId})");
		foreach (CachedHeroInfo item in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
		{
			CheckResultInfo checkResultInfo = HeroCostumeValidChecker.Check(item);
			if (!checkResultInfo.IsValid)
			{
				flag = false;
				stringBuilder.AppendLine();
				stringBuilder.Append($"HeroIndex ({checkResultInfo.HeroIndex}) - ({checkResultInfo.CreatureName}) Costume Valid Check Fail");
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(checkResultInfo.InValidList); i++)
				{
					stringBuilder.Append(checkResultInfo.InValidList[i]);
				}
			}
		}
		string text = stringBuilder.ToString();
		if (!flag)
		{
			VespaDebug.LogError(text);
			MessagePopup.OpenLog("customizing valid check falid", text, "");
		}
	}

	private void RegistObserver()
	{
		GlobalAutoBehaviour<AccountManager>.instance.CostumeStorageManager.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.CostumeStorageManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.AddObserver(this);
	}

	private void RemoveObserver()
	{
		GlobalAutoBehaviour<AccountManager>.instance.CostumeStorageManager.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.RemoveObserver(this);
	}

	public override void Reset()
	{
		base.Reset();
		ResetUI();
		ForEachContainer(delegate(WidgetBase widget)
		{
			widget.Reset();
		});
	}

	private void ResetUI()
	{
		DetailViewWidget.gameObject.SetActive(value: false);
		MenuRoot.gameObject.SetActive(value: true);
	}

	private void SetMileage(long Mileage)
	{
		UIHelper.SetLabelText(Label_Mileage, Vespa_Int_Extension_Method.ToCommaString(Mileage));
	}

	private void SetGold(long Gold)
	{
		UIHelper.SetLabelText(Label_Gold, Vespa_Long_Extension_Method.ToCommaString(Gold));
	}

	private void SetGem(long Gem)
	{
		UIHelper.SetLabelText(Label_Gem, Vespa_Int_Extension_Method.ToCommaString(Gem));
	}

	private void OpenHeroList()
	{
		HeroListWidget.Open();
	}

	private void CloseHeroList()
	{
		HeroListWidget.Close();
	}

	public void SelectHero(int heroIndex)
	{
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
		if (cachedHeroInfo == null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			if (data == null)
			{
				VespaDebug.LogError("CretureData is null : heroIndex=" + heroIndex);
				return;
			}
			cachedHeroInfo = new CachedHeroInfo(data);
		}
		new DelegateTask(SetupHeroWithSceneChange(cachedHeroInfo)).Commit();
	}

	public void SelectCostume(ToggleType toggleType, int costumeIndex)
	{
		if (base.HeroInfo == null)
		{
			return;
		}
		CurrentWidget(delegate(WidgetBase widgetBase)
		{
			if (toggleType == ToggleType.Costume)
			{
				HeroCostumeViewWidget heroCostumeViewWidget = widgetBase as HeroCostumeViewWidget;
				if (heroCostumeViewWidget != null)
				{
					heroCostumeViewWidget.SelectCostume(GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex));
				}
			}
			else if (toggleType.GetAccessoryType() != AccessoryPartType.None)
			{
				AccessoryCostumeViewWidget accessoryCostumeViewWidget = widgetBase as AccessoryCostumeViewWidget;
				if (accessoryCostumeViewWidget != null)
				{
					AccessoryCostumeData data = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(costumeIndex);
					if (data != null)
					{
						accessoryCostumeViewWidget.SelectCostume(AccessoryCostumeComponent.OpenData.CreateCostumeType(data));
					}
				}
			}
		});
	}

	public void OnClickPlusGem()
	{
		_listener?.OnClickPlusGem();
	}

	public void OnClickPlusGold()
	{
		_listener?.OnClickPlusGold();
	}

	public void OnClickAllHero()
	{
		OpenHeroList();
	}

	public void OnClickExitHeroList()
	{
		CloseHeroList();
	}

	public void OnClickResetCostume()
	{
		if (_listener == null)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("CONTENTS_CUSTOMIZE_RESET_ALL_POPUP_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("CONTENTS_CUSTOMIZE_RESET_ALL_POPUP_DESC");
		int heroIndex = base.HeroInfo.HeroIndex;
		Action OnReset = delegate
		{
			ForEachContainer(delegate(WidgetBase widget)
			{
				widget.Reset();
			});
			ResetLocalHeroInfo();
			Vespa_Common_Class_Extension_Method.Interface((IRenderFacadeController)this).ChangeRenderFacadeOnly(LocalHeroInfo);
			UpdateToggle();
		};
		bool needRequestReset = !base.HeroInfo.GetCostumeStock().IsDefaultCostume();
		MessagePopup.Open(stringDirect, stringDirect2, delegate
		{
			if (needRequestReset)
			{
				_listener.ResetHeroCostumes(heroIndex, OnReset);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(OnReset);
			}
		}, null);
	}

	public void OnClickViewMotion()
	{
		_listener?.OnClickViewMotion(this);
	}

	public override void OnClickClosePhoto()
	{
		_photoMode = false;
		UIHelper.EnableGameObject(NormalMenuWidget, enable: true);
		UIHelper.EnableGameObject(WidgetViewMotion, enable: true, isForce: true);
	}

	public void OnClickDetailViewButton()
	{
		OpenDetailView();
	}

	private void OpenDetailView()
	{
		AccessoryCostumeViewWidget accessoryCostumeViewWidget = GetCurrentWidget() as AccessoryCostumeViewWidget;
		if (!(accessoryCostumeViewWidget == null))
		{
			AccessoryCostumeData currentComponentAccesoryCostumeData = accessoryCostumeViewWidget.CurrentComponentAccesoryCostumeData;
			ITransformInstnace accessoryInstance = Vespa_Common_Class_Extension_Method.Interface((IRenderFacadeController)this).GetAccessoryInstance(currentComponentAccesoryCostumeData);
			if (DetailViewWidget.Open(accessoryCostumeViewWidget, accessoryInstance))
			{
				StartCameraZoom(_currentToggleType.GetCustomizingCameraType());
				MenuRoot.gameObject.SetActive(value: false);
			}
		}
	}

	protected override void UpdatePauseButtonState()
	{
		if (_photoMode)
		{
			base.UpdatePauseButtonState();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo info)
	{
		switch (info.UserStatType)
		{
		case UserManager.UserStatType.Gem:
		case UserManager.UserStatType.PayGem:
		{
			long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
			SetGem(totalGem);
			break;
		}
		case UserManager.UserStatType.Gold:
			SetGold(info.AfterValue);
			break;
		case UserManager.UserStatType.Mileage:
			SetMileage(info.AfterValue);
			break;
		}
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changeHeroInfo)
	{
		CurrentWidget(delegate(WidgetBase widget)
		{
			widget.OnChanged(changeHeroInfo);
		});
		OnChangeHeroInfo();
	}

	void IUserObserver<PayShopItemEventManager.EventChangedInfo>.OnChanged(PayShopItemEventManager.EventChangedInfo info)
	{
		HeroCostumeViewWidget.OnChanged(info);
		HairCostumeViewWidget.OnChanged(info);
		WeaponCostumeViewWidget.OnChanged(info);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AccessoryCostumeViewWidget); i++)
		{
			AccessoryCostumeViewWidget accessoryCostumeViewWidget = AccessoryCostumeViewWidget[i];
			if (!(accessoryCostumeViewWidget == null))
			{
				accessoryCostumeViewWidget.OnChanged(info);
			}
		}
		CurrentWidget(delegate(WidgetBase widget)
		{
			widget.OnChanged(info);
		});
	}

	void IUserObserver<CostumeStorageSlotInfo>.OnChanged(CostumeStorageSlotInfo changedInfo)
	{
		CurrentWidget(delegate(WidgetBase widget)
		{
			widget.OnChanged(changedInfo);
		});
	}

	void IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>.OnChanged(DropBonusEventManager.DropBonusEventChangeInfo changedInfo)
	{
		DropBonusEvent bonusEvent = changedInfo.bonusEvent;
		if (bonusEvent.BonusType != EventBonusType.PayShopDiscount || changedInfo.changeType != DropBonusEventManager.DropBonusEventChangeType.EventRemoved)
		{
			return;
		}
		int result = 0;
		if (!int.TryParse(bonusEvent.Condition, out result) || result == 0)
		{
			return;
		}
		int num = -1;
		foreach (CostumeData allDatum in GameRoot.Instance.CostumeDataContainer.GetAllData())
		{
			if (allDatum.ProductIndex == result)
			{
				num = allDatum.HeroIndex;
				break;
			}
		}
		if (num != -1 && base.HeroInfo.HeroIndex == num)
		{
			GlobalAutoBehaviour<DropBonusEventManager>.instance.ClearAllEffectCacheTime();
			CurrentWidget(delegate(WidgetBase widget)
			{
				widget.OnChanged(changedInfo);
			});
		}
	}

	void IRenderFacadeController.SetupHero(int selectedHeroIndex)
	{
		SelectHero(selectedHeroIndex);
	}

	CachedHeroInfo IRenderFacadeController.GetHeroInfo()
	{
		return base.HeroInfo;
	}

	CachedHeroInfo IRenderFacadeController.GetLocalHeroInfo()
	{
		return LocalHeroInfo;
	}

	void IRenderFacadeController.ResetWidgets(WidgetBase butThis)
	{
		ForEachContainer(delegate(WidgetBase widget)
		{
			if (butThis != widget)
			{
				widget.Reset();
			}
		});
		ResetLocalHeroInfo();
		UpdateToggle();
	}

	void IRenderFacadeController.ChangeRenderFacadeOnly(CachedHeroInfo heroInfo)
	{
		ChangeRenderFacadeOnly(heroInfo);
	}

	void IRenderFacadeController.ChangeRenderFacadeWeapon(string weaponPrefix, int weaponStar, bool isEquipSoulWeapon, int soulWeaponGrade)
	{
		ChangeRenderFacadeWeapon(weaponPrefix, weaponStar, isEquipSoulWeapon, soulWeaponGrade);
	}

	void IRenderFacadeController.ChangeRenderFacadeHair(string hairPrefix)
	{
		ChangeRenderFacadeHair(hairPrefix);
	}

	void IRenderFacadeController.ChangeAccessoryCostume(AccessoryPartType partType, AccessoryCostumeData accessoryCostumeData, PositionInfo positionInfo)
	{
		ChangeRanderFacadeAccessory(partType, accessoryCostumeData, positionInfo);
	}

	ITransformInstnace IRenderFacadeController.GetAccessoryInstance(AccessoryCostumeData accessoryData)
	{
		CustomizeStock stock = default(CustomizeStock);
		if (!RenderFacadeBuilder.TryAssignCustomizeStock(ref stock, base.HeroInfo.HeroIndex, accessoryData, null))
		{
			return null;
		}
		return GetCustomizeItemInstance(stock);
	}

	void IRenderFacadeController.OnSelectCostume()
	{
		_needRefreshCostumesButton = true;
		SetEnableDetailViewButton();
		SetVisibleHideButton();
	}

	void IRenderFacadeController.OnSignatureMotion()
	{
		SetRenderFacedeAnimation(new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, "Signature"));
	}

	void IRenderFacadeController.OnCloseDetailView()
	{
		OnCloseDetailView();
	}

	private void OnChangeHeroInfo()
	{
		_needRefreshCostumesButton = true;
	}

	private void CheckButtonRefresh()
	{
		if (_needRefreshCostumesButton)
		{
			_needRefreshCostumesButton = false;
			SetEnableBuyCostumesButton();
			SetVisibleHideButton();
		}
	}

	private void SetEnableBuyCostumesButton()
	{
		CustomizingCostumesStock customizingCostumesStock = CustomizingHelper.CollectCostumes(GetCurrentCostumeGetter());
		bool existValidCostume = false;
		customizingCostumesStock.CollectResultList.ForEach(delegate(CollectResult result)
		{
			existValidCostume |= result.CostumeNeedUpdate;
		});
		if (CustomizingHelper.GetFailResult(customizingCostumesStock.CollectResultList, out var _) == CollectCostumeFailReason.NeedOwnLegend && customizingCostumesStock.BuyReqGem == 0)
		{
			existValidCostume = false;
		}
		string key = ((customizingCostumesStock.BuyReqGem > 0) ? "CONTENT_CUSTOMIZE_BUY_ALL" : "CONTENT_CUSTOMIZE_EQUIP_ALL");
		UIHelper.SetLabelText(LabelButtonBuyCostumes, LocalizationManager.GetStringDirect(key));
		UIHelper.EnableButton(ButtonBuyCostumes, existValidCostume, grayscaleIfDisable: true);
	}

	public void OnClickSaveMySet()
	{
		WidgetBase currentWidget = GetCurrentWidget();
		if (currentWidget == null)
		{
			return;
		}
		CustomizingCostumesStock customizingCostumesStock = CustomizingHelper.CollectCostumes(GetCurrentCostumeGetter());
		bool existChangedCostume = false;
		customizingCostumesStock.CollectResultList.ForEach(delegate(CollectResult result)
		{
			existChangedCostume |= result.CostumeNeedUpdate;
		});
		string failReason;
		CollectCostumeFailReason failResult = CustomizingHelper.GetFailResult(customizingCostumesStock.CollectResultList, out failReason);
		if (existChangedCostume || failResult != CollectCostumeFailReason.None)
		{
			if (failResult == CollectCostumeFailReason.NeedOwnLegend)
			{
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string stringDirect = LocalizationManager.GetStringDirect("COSTUME_LEGEND_PRESET_SAVE_UNABLE");
				MessagePopup.Open(text, stringDirect);
			}
			else
			{
				_listener.ProcessSelectedCustomizingCostumes(GetCurrentCostumeGetter(), delegate
				{
					currentWidget.OnButtonEvent(ButtonClickEvent.SetSave);
				});
			}
		}
		else
		{
			currentWidget.OnButtonEvent(ButtonClickEvent.SetSave);
		}
	}

	public void OnClickEquipMySet()
	{
		WidgetBase currentWidget = GetCurrentWidget();
		if (!(currentWidget == null))
		{
			currentWidget.OnButtonEvent(ButtonClickEvent.SetEquip);
		}
	}

	private HeroCustomizingDataGetter GetCurrentCostumeGetter()
	{
		HeroCustomizingDataGetter result = default(HeroCustomizingDataGetter);
		result.HeroInfoGetter = () => base.HeroInfo;
		result.HeroCostumeDataGetter = _heroCostumeDataGetter;
		result.HairCostumeOpenDataGetter = _hairCostumeOpenDataGetter;
		result.WeaponCostumeOpenDataGetter = _weaponCostumeOpenDataGetter;
		result.AccessoryCostumeOpenDataGetterList = new List<Func<AccessoryCostumeComponent.OpenData>>();
		result.AccessoryCostumeOpenDataGetterList.AddRange(_accessoryCostumeOpenDataGetterArray);
		result.AccessoryPositionInfoGetter = (AccessoryCostumeData accessoryData) => LocalHeroInfo.GetAccessoryPositionInfo(accessoryData.PartType);
		return result;
	}

	public void OnClickBuyCostumes()
	{
		if (_listener != null)
		{
			_listener.ProcessSelectedCustomizingCostumes(GetCurrentCostumeGetter(), null);
		}
	}

	private ITransformInstnace GetAccessoryInstance(AccessoryCostumeData accessoryCostumeData)
	{
		return Vespa_Common_Class_Extension_Method.Interface((IRenderFacadeController)this).GetAccessoryInstance(accessoryCostumeData);
	}

	public void OnClickUnequipCostume()
	{
		WidgetBase currentWidget = GetCurrentWidget();
		if (!(currentWidget == null))
		{
			currentWidget.OnButtonEvent(ButtonClickEvent.UnEquip);
		}
	}

	public void OnClickEquipCostume()
	{
		WidgetBase currentWidget = GetCurrentWidget();
		if (!(currentWidget == null))
		{
			currentWidget.OnButtonEvent(ButtonClickEvent.Equip);
		}
	}

	public void OnClickBuyHero()
	{
		WidgetBase currentWidget = GetCurrentWidget();
		if (!(currentWidget == null))
		{
			currentWidget.OnButtonEvent(ButtonClickEvent.BuyHero);
		}
	}

	public void OnClickBuyCostume()
	{
		WidgetBase currentWidget = GetCurrentWidget();
		if (!(currentWidget == null))
		{
			currentWidget.OnButtonEvent(ButtonClickEvent.BuyCostume);
		}
	}

	public void OnClickBuyLegendCostume()
	{
		if (_listener == null)
		{
			return;
		}
		HeroCostumeViewWidget heroCostumeViewWidget = GetCurrentWidget() as HeroCostumeViewWidget;
		if (!(heroCostumeViewWidget == null))
		{
			CostumeData currentCostumeData = heroCostumeViewWidget.CurrentCostumeData;
			if (currentCostumeData != null && currentCostumeData.CostumeType == EHeroCostumeType.Legend)
			{
				_listener.OpenLegendCostumeReservationPooup(currentCostumeData.CostumeIndex);
			}
		}
	}

	public void OnClickSubStory()
	{
		WidgetBase currentWidget = GetCurrentWidget();
		if (!(currentWidget == null))
		{
			currentWidget.OnButtonEvent(ButtonClickEvent.GoSubstory);
		}
	}

	CostumeInfoUI IWidgetUiController.GetCostumeInfoUI()
	{
		return CostumeInfoUIs;
	}

	CostumeButtonInfo[] IWidgetUiController.GetCostumeButtons()
	{
		return CostumeButtonInfos;
	}

	void IWidgetUiController.SwitchToggle(ToggleType toggleType)
	{
		_currentToggleType = toggleType;
		CurrentWidget(delegate(WidgetBase widget)
		{
			widget.Reset();
		});
		UpdateToggle();
	}

	WidgetBase IWidgetUiController.GetCostumeViewWidget(ToggleType toggleType)
	{
		return GetWidgetbyToggleType(toggleType);
	}
}
