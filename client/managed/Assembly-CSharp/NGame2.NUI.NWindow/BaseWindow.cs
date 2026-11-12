using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NEventTrigger;
using NGame2.NPayShopItemEvent;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUtil;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BaseWindow : LinkFactoryComponent, IBaseWindow, IWindow, IGlobalWindow, ITouchDisabler, WindowAnimation.IAlphaAnimator, IUpdater, ILateUpdater
{
	public enum EState
	{
		Closed,
		OpenedHide,
		AnimateShow,
		OpenedShow,
		AnimateHide
	}

	[Flags]
	public enum ETopmostMenuFlag
	{
		None = 0,
		Ignore = 1,
		ShowBar = 2,
		ShowProfile = 4,
		ShowParty = 8,
		NoWindowExist = 0xC
	}

	public enum ETopmostMenuType
	{
		_undefined_,
		DontChange,
		HideAll,
		ResourceOnly,
		BackButtonTitle,
		NakedBackButtonTitle,
		BackButtonTitleShop,
		Tutorial,
		Tutorial_Guide,
		BackButtonOnly,
		SoulWeapon,
		MissionMain,
		Punishment
	}

	public enum EBackButtonType
	{
		_undefined_,
		Pass,
		Handle,
		HandleSkip
	}

	public static int PrevDepthScore = 0;

	[HideInInspector]
	public int BasePanelDepth;

	[HideInInspector]
	public bool DepthScoreDirty;

	public EDepthOrder DepthOrder = EDepthOrder.Normal;

	public int DepthOrder2;

	public EWindowSide Side;

	public bool BackRegister;

	public EBackButtonType BackButtonType;

	[HideInInspector]
	public bool HideBackTitle;

	public ETopmostMenuType TopmostMenuType;

	public LocalizeLinkData title;

	[HideInInspector]
	public bool HideTopMostMenu;

	[HideInInspector]
	public ETopmostMenuFlag TopmostMenuFlag;

	public bool HideWorldMap = true;

	public Transform WindowRoot;

	public float OpenTimeout = 10f;

	public bool DisableTouchWhileOpening;

	public GuildConstructionResourceTopmostMenuType GuildResourceType;

	public StaminaType Stamina;

	public UserManager.UserStatType UserStat;

	public int WindowGuideIndex;

	public bool IsDisableBuyShopUserStat;

	public bool UnUsedTouchDisabler;

	public bool UseTaskWhenOpen = true;

	public bool UseUI3DUILayer;

	public bool ChangeAbleLayer;

	private LobbyTopmostMenu.EventDungeonAssetInfo EventDungeonAssetInfo;

	private LobbyTopmostMenu.SubStatAssetInfo SubAssetInfo;

	protected EState _windowState;

	protected UIPanel[] _panels;

	protected WeakRef<IManagementWrapper> _ownerManagement = new WeakRef<IManagementWrapper>();

	protected IWindowShadowShader _iWindowShadowShader;

	private bool isTutorialWindow;

	private ETopmostMenuType Saved_TopmostMenuType;

	private bool _reservedOpenTask;

	private int _depthScore;

	private static int _lastToggleGroup = 10000;

	private static float _lastUnloadTime;

	private float _eventAlpha = 1f;

	private float _animationAlpha = 1f;

	private BaseWindowStaminaObserver _baseStaminaObserver;

	private BaseWindowUserStatObserver _baseUserStatObserver;

	private BaseWindowMiniGuideButtonStatObserver _baseMiniGuideButtonObserver;

	private BaseWindowObserver<DisableBuyShopUserStatObserverValue> _baseDisableBuyShopObserver;

	private BaseWindowObserver<GuildResourceObserverValue> _baseGuildResourceObserver;

	public int DepthScore => _depthScore + DepthOrder2 * 100000;

	public EState WindowState => _windowState;

	public bool IsShow
	{
		get
		{
			if (_windowState != EState.AnimateShow)
			{
				return _windowState == EState.OpenedShow;
			}
			return true;
		}
	}

	public bool ReservedOpenTask => _reservedOpenTask;

	protected WindowAnimation _windowAnimation
	{
		get
		{
			if (this == null || base.transform == null)
			{
				return null;
			}
			return SingletonAuto<LinkedTransform<WindowAnimation>>.instance?.Get(base.transform);
		}
	}

	EDepthOrder IWindow.DepthOrder => DepthOrder;

	public bool IsValid => base.gameObject.activeInHierarchy;

	public void SetEventDungeonAssetInfo(LobbyTopmostMenu.EventDungeonAssetInfo _EventDungeonAssetInfo)
	{
		EventDungeonAssetInfo = _EventDungeonAssetInfo;
	}

	public void SetSubAssetInfo(LobbyTopmostMenu.SubStatAssetInfo _SubAssetInfo)
	{
		SubAssetInfo = _SubAssetInfo;
	}

	public new virtual void Awake()
	{
		GlobalUpdateBehaviour.AddUpdater(this);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			RemoveTouchDisabler("Destroy");
			Reset();
		}
	}

	private void OnDisable()
	{
		SprkFreeCamera.InvalidateLayout(this);
		RemoveTouchDisabler("Disable");
	}

	public virtual void SetEventHide(float alpha)
	{
		if (!Mathf.Approximately(_eventAlpha, alpha))
		{
			_eventAlpha = alpha;
			RefreshAlpha();
		}
	}

	void WindowAnimation.IAlphaAnimator.SetAlpha(float alpha)
	{
		if (!Mathf.Approximately(_animationAlpha, alpha))
		{
			_animationAlpha = alpha;
			RefreshAlpha();
		}
	}

	private void RefreshAlpha()
	{
		if (_panels == null)
		{
			_panels = GetComponentsInChildren<UIPanel>(includeInactive: true);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_panels); i++)
		{
			_panels[i].alpha = _eventAlpha * _animationAlpha;
		}
	}

	public void InitFromManagement(IWindowPrefab prefab, IManagementWrapper management)
	{
		_ownerManagement.Value = management;
	}

	public void MakeDirty()
	{
		DepthScoreDirty = true;
		if (_ownerManagement.Value != null)
		{
			_ownerManagement.Value.MakeDirty();
		}
	}

	public virtual void Open()
	{
		Open(null, null);
	}

	public virtual void Open(Action onComplete)
	{
		Open(null, onComplete);
	}

	protected void Open(Action onOpen, Action onComplete, Func<bool> waiter = null)
	{
		if (ChangeAbleLayer)
		{
			bool flag = false;
			if (base.transform != null && base.transform.parent != null && GlobalUIRoot.instance.ContentsGroup.transform == base.transform.parent)
			{
				flag = true;
			}
			if (!flag)
			{
				if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
				{
					LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
					ChangeLayerRecursive(layerInfo);
				}
				else
				{
					LayerInfo layerInfo2 = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI);
					ChangeLayerRecursive(layerInfo2);
				}
			}
		}
		if (UseTaskWhenOpen)
		{
			_reservedOpenTask = true;
			new WindowOpenTask(this, OpenTimeout, Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(OpenAsync(new DelegateTask(onOpen), immediately: false, waiter)), onComplete).WrapperTouchControl().DisableTouch(DisableTouchWhileOpening)).Commit(GetType());
			return;
		}
		if (DisableTouchWhileOpening)
		{
			GlobalTopmostWindow.instance.AddTouchDisabler(this);
		}
		_reservedOpenTask = true;
		VespaCoroutine.Start(CoOpen(onOpen, onComplete, waiter));
	}

	public virtual void OpenParams(Action onOpen, Action onComplete, params object[] args)
	{
		Open(onOpen, onComplete);
	}

	public IEnumerator<TaskCurrentData> OpenAsync(bool immediately = false, Action<BaseWindow> onOpen = null)
	{
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		bool opened = false;
		Open(delegate
		{
			opened = true;
		});
		if (immediately)
		{
			CompleteWindowAnimation();
		}
		Vespa_System_Action_Extension_Method.fire(onOpen, this);
		while (!opened)
		{
			yield return TaskCurrentData.RUNNING;
		}
		yield return TaskCurrentData.SUCCESS;
	}

	public ITask OpenAsync(ITask initTask, bool immediately = false, Func<bool> waiter = null)
	{
		return Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask(), OpenHide).ADDTASK(new BranchTask().If(new DelegateConditionTask(() => initTask != null)).Then(initTask)).ADDTASK(ShowOnOpenAsync(immediately, waiter));
	}

	protected void OpenHide()
	{
		_reservedOpenTask = false;
		_windowState = EState.OpenedHide;
		base.gameObject.SetActive(value: true);
		Reset();
		MakeTopmostDepth();
		UIHelper.EnableGameObject(WindowRoot, enable: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_panels); i++)
		{
			_panels[i].alpha = 1f;
		}
		_animationAlpha = 1f;
		_eventAlpha = 1f;
		RemoveTouchDisabler("OpenHide");
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this as IUserObserver);
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SetLobbyManagementObserver_GuildResourceObserver();
			SetLobbyManagementObserver_CustomStaminaObserver();
			SetLobbyManagementObserver_UserStatObserver();
			SetLobbyManagementObserver_MiniGuideButtonObserver();
			SetLobbyManagementObserver_DisableBuyShopObserver();
		}
		GlobalAutoBehaviour<PayShopItemEventManager>.instance.ObserverManager.AddObserver(this as IUserObserver);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.AddObserver(this as IUserObserver);
		RegisterOpenedWindow();
		if (_ownerManagement.Value != null)
		{
			_ownerManagement.Value.OnWindowOpened(this);
			MakeDirty();
		}
		RaiseEventEnterMenu();
		OnChangeWindowState();
	}

	public void RegisterOpenedWindow()
	{
		if (BackButtonType == EBackButtonType.Handle || BackButtonType == EBackButtonType.HandleSkip)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.RegisterOpenedWindow(this);
		}
		else if (BackButtonType == EBackButtonType._undefined_ && BackRegister)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.RegisterOpenedWindow(this);
		}
	}

	private void SetLobbyManagementObserver_GuildResourceObserver()
	{
		SetLobbyManagemnetObserverNew(ref _baseGuildResourceObserver, GuildResourceType, LobbyTopmostMenu.PublicObserverType.ShowGUildResource);
	}

	private void SetLobbyManagementObserver_CustomStaminaObserver()
	{
		Action<LobbyTopmostMenu.ICustomStaminaObserver> onSetObserver = SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyTopmostMenu;
		SetLobbyManagemnetObserver(this as LobbyTopmostMenu.ICustomStaminaObserver, onSetObserver, ref _baseStaminaObserver, Stamina, () => new BaseWindowStaminaObserver
		{
			StaminaType = Stamina
		});
	}

	private void SetLobbyManagementObserver_UserStatObserver()
	{
		Action<LobbyTopmostMenu.IUserStatObserver> onSetObserver = OpenLobbyManagemnetPlayerAssetMenu;
		SetLobbyManagemnetObserver(this as LobbyTopmostMenu.IUserStatObserver, onSetObserver, ref _baseUserStatObserver, UserStat, () => new BaseWindowUserStatObserver
		{
			StatType = UserStat
		});
	}

	private void OpenLobbyManagemnetPlayerAssetMenu(LobbyTopmostMenu.IUserStatObserver userStatObserver)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.OpenPlayerAssetMenu(userStatObserver, EventDungeonAssetInfo, SubAssetInfo);
	}

	private void SetLobbyManagementObserver_MiniGuideButtonObserver()
	{
		Action<LobbyTopmostMenu.IMiniGuideButtonObserver> onSetObserver = SceneAutoBehaviour<LobbyManagement>.instance.OpenMiniGuideButton;
		SetLobbyManagemnetObserver(this as LobbyTopmostMenu.IMiniGuideButtonObserver, onSetObserver, ref _baseMiniGuideButtonObserver, WindowGuideIndex, () => new BaseWindowMiniGuideButtonStatObserver
		{
			WindowGuideIndex = WindowGuideIndex
		});
	}

	private void SetLobbyManagementObserver_DisableBuyShopObserver()
	{
		SetLobbyManagemnetObserverNew(ref _baseDisableBuyShopObserver, IsDisableBuyShopUserStat, LobbyTopmostMenu.PublicObserverType.DisableBuyShopUserStat);
	}

	private void SetLobbyManagemnetObserver<Observer, ObserverParent, ObserverWindow, WindowValue>(Observer observer, Action<ObserverParent> onSetObserver, ref ObserverWindow refWindow, WindowValue value, Func<ObserverWindow> funcNewWindow) where Observer : class, ObserverParent where ObserverParent : class where ObserverWindow : ObserverParent where WindowValue : IComparable
	{
		if (observer != null)
		{
			Vespa_System_Action_Extension_Method.fire(onSetObserver, (ObserverParent)observer);
		}
		else if (value.CompareTo(default(WindowValue)) != 0)
		{
			refWindow = Vespa_System_Action_Extension_Method.fire(funcNewWindow);
			Vespa_System_Action_Extension_Method.fire(onSetObserver, (ObserverParent)(object)refWindow);
		}
	}

	private void SetLobbyManagemnetObserverNew<ObserverValue, WindowValue>(ref BaseWindowObserver<ObserverValue> observerWindow, WindowValue windowValue, LobbyTopmostMenu.PublicObserverType type) where ObserverValue : ObserverBaseValue<WindowValue>, new() where WindowValue : struct, IComparable
	{
		Action<LobbyTopmostMenu.ILobbyTopmostObserver<ObserverValue>> onSetObserver = SceneAutoBehaviour<LobbyManagement>.instance.SetTopMostMenuObserver;
		SetLobbyManagemnetObserver(this as LobbyTopmostMenu.ILobbyTopmostObserver<ObserverValue>, onSetObserver, ref observerWindow, windowValue, () => new BaseWindowObserver<ObserverValue>
		{
			Type = type,
			BaseValue = new ObserverValue
			{
				Value = windowValue
			}
		});
	}

	private ITask ShowOnOpenAsync(bool immediately, Func<bool> waiter = null)
	{
		return new BranchTask().ConditionTask(new DelegateConditionTask(() => _windowAnimation != null && !_windowAnimation.ManualStart)).OnSuccessTask(ShowAsync(immediately)).OnFailTask(new BranchTask().ConditionTask(() => waiter != null).OnSuccessTask(new WaitTask(waiter)).OnFinalizeTask(new DelegateTask(delegate
		{
			UIHelper.EnableGameObject(WindowRoot, enable: true);
		})));
	}

	public virtual ITask ShowAsync(bool immediately = false)
	{
		return new DelegateTask(coShowAsync(immediately));
	}

	private IEnumerator<TaskCurrentData> coShowAsync(bool immediately)
	{
		UIHelper.EnableGameObject(WindowRoot, enable: true);
		_reservedOpenTask = false;
		_windowState = EState.AnimateShow;
		OnChangeWindowState();
		WindowAnimation windowAnimation = _windowAnimation;
		if (windowAnimation != null)
		{
			MakeDirty();
			bool completeAnimation = false;
			windowAnimation.PlayAnimation(reverse: false, delegate
			{
				completeAnimation = true;
			});
			if (immediately)
			{
				windowAnimation.Complete();
			}
			while (!completeAnimation)
			{
				if (windowAnimation == null || base.gameObject == null || !windowAnimation.IsAnimating(reverse: false) || !base.gameObject.activeInHierarchy)
				{
					yield return TaskCurrentData.FAIL;
					yield break;
				}
				yield return TaskCurrentData.RUNNING;
			}
		}
		MakeDirty();
		_windowState = EState.OpenedShow;
		OnChangeWindowState();
		yield return TaskCurrentData.SUCCESS;
	}

	private ITask HideOnCloseAsync()
	{
		return new BranchTask().ConditionTask(new DelegateConditionTask(() => _windowAnimation != null && !_windowAnimation.ManualStart)).OnSuccessTask(HideAsync()).OnFailTask(new DelegateTask(delegate
		{
			UIHelper.EnableGameObject(WindowRoot, enable: false);
		}));
	}

	public virtual ITask HideAsync()
	{
		return new DelegateTask(coHideAsync());
	}

	private IEnumerator<TaskCurrentData> coHideAsync()
	{
		_reservedOpenTask = false;
		_windowState = EState.AnimateHide;
		OnChangeWindowState();
		WindowAnimation windowAnimation = _windowAnimation;
		if (windowAnimation != null)
		{
			MakeDirty();
			bool completeAnimation = false;
			windowAnimation.PlayAnimation(reverse: true, delegate
			{
				completeAnimation = true;
			});
			while (!completeAnimation)
			{
				if (windowAnimation == null || base.gameObject == null || !windowAnimation.IsAnimating(reverse: true))
				{
					yield return TaskCurrentData.FAIL;
					yield break;
				}
				yield return TaskCurrentData.RUNNING;
			}
		}
		UIHelper.EnableGameObject(WindowRoot, enable: false);
		MakeDirty();
		_windowState = EState.OpenedHide;
		OnChangeWindowState();
		yield return TaskCurrentData.SUCCESS;
	}

	private IEnumerator CoOpen(Action onOpen, Action onComplete, Func<bool> waiter = null)
	{
		OpenHide();
		Vespa_System_Action_Extension_Method.fire(onOpen);
		DateTime startTime = DateTime.Now;
		if (waiter != null)
		{
			while (Vespa_System_Action_Extension_Method.fire(waiter) && !((DateTime.Now - startTime).TotalSeconds > (double)OpenTimeout))
			{
				yield return null;
			}
		}
		if (_windowAnimation != null && !_windowAnimation.ManualStart)
		{
			VespaCoroutine.Start(CoAnimationWinodw(immediately: false));
			while (_windowState != EState.OpenedShow && !((DateTime.Now - startTime).TotalSeconds > (double)OpenTimeout))
			{
				yield return null;
			}
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
		if (DisableTouchWhileOpening)
		{
			GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
		}
	}

	private IEnumerator CoAnimationWinodw(bool immediately)
	{
		UIHelper.EnableGameObject(WindowRoot, enable: true);
		_reservedOpenTask = false;
		_windowState = EState.AnimateShow;
		OnChangeWindowState();
		WindowAnimation windowAnimation = _windowAnimation;
		if (windowAnimation != null)
		{
			MakeDirty();
			bool completeAnimation = false;
			windowAnimation.PlayAnimation(reverse: false, delegate
			{
				completeAnimation = true;
			});
			if (immediately)
			{
				windowAnimation.Complete();
			}
			while (!completeAnimation)
			{
				if (windowAnimation == null || base.gameObject == null || !windowAnimation.IsAnimating(reverse: false) || !base.gameObject.activeInHierarchy)
				{
					completeAnimation = true;
				}
				yield return null;
			}
		}
		MakeDirty();
		_windowState = EState.OpenedShow;
		OnChangeWindowState();
	}

	protected virtual void OnChangeWindowState()
	{
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.EnableLobbyChapterReward();
			SceneAutoBehaviour<LobbyManagement>.instance.EnableLobbyShakmehBossGauge();
		}
	}

	public void MakeTopmostDepth()
	{
		_depthScore = ++PrevDepthScore;
		MakeDirty();
	}

	void IWindow.SetBasePanelDepth(int depth)
	{
		BasePanelDepth = depth;
	}

	public void ShowRoot(bool show)
	{
		if (!(WindowRoot == null) && show != IsShowing())
		{
			OnShowRoot(show);
			UIHelper.EnableGameObject(WindowRoot, show);
		}
	}

	protected virtual void OnShowRoot(bool isShow)
	{
	}

	public bool IsShowing()
	{
		return UIHelper.IsEnabledGameObjet(WindowRoot);
	}

	public void MakeTopmostToggleGroup(params UIToggle[] toggles)
	{
		_lastToggleGroup++;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(toggles); i++)
		{
			toggles[i].group = _lastToggleGroup;
		}
	}

	public virtual bool IsOpen()
	{
		return _windowState != EState.Closed;
	}

	public virtual void OnSystemBackButton()
	{
		Close();
	}

	public virtual void OnClosing()
	{
	}

	public virtual void OnOtherWindowOpened()
	{
	}

	public virtual void ReOpen()
	{
		RaiseEventEnterMenu();
	}

	public virtual void Close()
	{
		SetEventDungeonAssetInfo(null);
		SetSubAssetInfo(null);
		Close2(immediately: false);
	}

	public void Close2(bool immediately)
	{
		if (_windowState == EState.Closed)
		{
			if (base.gameObject != null && base.gameObject.activeInHierarchy)
			{
				base.gameObject.SetActive(value: false);
				MakeDirty();
			}
			return;
		}
		WindowAnimation windowAnimation = _windowAnimation;
		if (!immediately && windowAnimation != null)
		{
			if (windowAnimation.IsAnimating(reverse: true))
			{
				return;
			}
			if (!windowAnimation.ManualStart)
			{
				AddTouchDisabler("Close");
				windowAnimation.PlayAnimation(reverse: true, delegate
				{
					RemoveTouchDisabler("Close");
					CloseInternal();
				});
				MakeDirty();
				return;
			}
		}
		if ((bool)windowAnimation)
		{
			windowAnimation.CompleteWithoutCallback();
		}
		CloseInternal();
	}

	public virtual ITask CloseAsync()
	{
		return Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(HideOnCloseAsync()), CloseInternal);
	}

	protected void CloseInternal()
	{
		_reservedOpenTask = false;
		_windowState = EState.Closed;
		OnClosing();
		Reset();
		if (GlobalAutoBehaviour<PayShopItemEventManager>.isValidInstance)
		{
			GlobalAutoBehaviour<PayShopItemEventManager>.instance.ObserverManager.RemoveObserver(this as IUserObserver);
		}
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this as IUserObserver);
		}
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			RemoveLobbyManagementObserver_GuildResourceObserver();
			RemoveLobbyManagementObserver_CustomStaminaObserver();
			RemoveLobbyManagementObserver_UserStatObserver();
			RemoveLobbyManagementObserver_MiniGuideButtonObserver();
			RemoveLobbyManagementObserver_EnableBuyShopObserver();
		}
		if (SingletonAuto<EventTriggerManager>.isValidInstance && this != null)
		{
			string window = GetType().Name;
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new LeaveMenu
			{
				Window = window
			});
		}
		if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.UnregisterOpenedWindowStack(this);
		}
		ResetWindowOriginState();
		if (base.gameObject != null)
		{
			base.gameObject.SetActive(value: false);
		}
		MakeDirty();
	}

	private void RemoveLobbyManagementObserver_GuildResourceObserver()
	{
		RemoveLobbyManagementObserverNew(ref _baseGuildResourceObserver);
	}

	private void RemoveLobbyManagementObserver_CustomStaminaObserver()
	{
		Action<LobbyTopmostMenu.ICustomStaminaObserver> onRemoveObserver = SceneAutoBehaviour<LobbyManagement>.instance.CloseLobbyTopmostMenu;
		RemoveLobbyManagementObserver(onRemoveObserver, ref _baseStaminaObserver);
	}

	private void RemoveLobbyManagementObserver_UserStatObserver()
	{
		Action<LobbyTopmostMenu.IUserStatObserver> onRemoveObserver = SceneAutoBehaviour<LobbyManagement>.instance.ClosePlayerAssetMenu;
		RemoveLobbyManagementObserver(onRemoveObserver, ref _baseUserStatObserver);
	}

	private void RemoveLobbyManagementObserver_MiniGuideButtonObserver()
	{
		Action<LobbyTopmostMenu.IMiniGuideButtonObserver> onRemoveObserver = SceneAutoBehaviour<LobbyManagement>.instance.CloseMiniGuideButton;
		RemoveLobbyManagementObserver(onRemoveObserver, ref _baseMiniGuideButtonObserver);
	}

	private void RemoveLobbyManagementObserver_EnableBuyShopObserver()
	{
		RemoveLobbyManagementObserverNew(ref _baseDisableBuyShopObserver);
	}

	private void RemoveLobbyManagementObserver<Observer, ObserverWindow>(Action<Observer> onRemoveObserver, ref ObserverWindow observerWindow) where Observer : class where ObserverWindow : class, Observer
	{
		if (this is Observer i_argA)
		{
			Vespa_System_Action_Extension_Method.fire(onRemoveObserver, i_argA);
		}
		else if (observerWindow != null)
		{
			Vespa_System_Action_Extension_Method.fire(onRemoveObserver, (Observer)observerWindow);
			observerWindow = null;
		}
	}

	private void RemoveLobbyManagementObserverNew<ObserverValue>(ref BaseWindowObserver<ObserverValue> observerWindow) where ObserverValue : class
	{
		Action<LobbyTopmostMenu.ILobbyTopmostObserver<ObserverValue>> onRemoveObserver = SceneAutoBehaviour<LobbyManagement>.instance.RemoveTopMostMenuObserver;
		RemoveLobbyManagementObserver(onRemoveObserver, ref observerWindow);
	}

	public bool IsWindowAnimating()
	{
		if (!IsOpeningWindowAnimating())
		{
			return IsClosingWindowAnimating();
		}
		return true;
	}

	public bool IsOpeningWindowAnimating()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (_windowAnimation == null)
		{
			return false;
		}
		return _windowAnimation.IsAnimating(reverse: false);
	}

	public bool IsClosingWindowAnimating()
	{
		if (base.gameObject != null && !base.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (_windowAnimation == null)
		{
			return false;
		}
		return _windowAnimation.IsAnimating(reverse: true);
	}

	public void CompleteWindowAnimation()
	{
		if (!(_windowAnimation == null))
		{
			_windowAnimation.Complete();
		}
	}

	public void PlayWindowOpenAnimation(Action onPlayed = null)
	{
		if ((bool)_windowAnimation)
		{
			_windowAnimation.PlayAnimation(reverse: false, onPlayed);
		}
	}

	public void PlayWindowCloseAnimation(Action onPlayed = null)
	{
		if ((bool)_windowAnimation)
		{
			_windowAnimation.PlayAnimation(reverse: true, onPlayed);
		}
	}

	public void ResumeWindowAnimation()
	{
		if ((bool)_windowAnimation)
		{
			_windowAnimation.Resume();
		}
	}

	public void PauseWindowAnimation()
	{
		if ((bool)_windowAnimation)
		{
			_windowAnimation.Pause();
		}
	}

	public void CompleteAnimationWithoutCallback()
	{
		if ((bool)_windowAnimation)
		{
			_windowAnimation.CompleteWithoutCallback();
		}
	}

	public void AddTouchDisabler(string funcName)
	{
		if (!UnUsedTouchDisabler && GlobalTopmostWindow.IsValidInstance)
		{
			GlobalTopmostWindow.instance.AddTouchDisabler(this);
		}
	}

	public void RemoveTouchDisabler(string funcName)
	{
		if (!UnUsedTouchDisabler && GlobalTopmostWindow.IsValidInstance)
		{
			GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
		}
	}

	public virtual void Reset()
	{
	}

	public virtual void ReserveCache(UiPreloader preloader)
	{
	}

	void IUpdater.ChainUpdate()
	{
		if (base.isActiveAndEnabled)
		{
			UpdateDerived();
		}
	}

	public virtual void UpdateDerived()
	{
		SprkFreeCamera.RefreshLayout(this);
	}

	void ILateUpdater.ChainLateUpdate()
	{
		if (base.isActiveAndEnabled)
		{
			LateUpdateDerived();
		}
	}

	public virtual void LateUpdateDerived()
	{
	}

	public virtual void RaiseEventEnterMenu()
	{
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new EnterMenu
		{
			Window = GetType().Name,
			SceneTriggerSequance = SingletonAuto<EventTriggerManager>.instance.GetEnterMenuSequance(GetType().Name)
		});
	}

	public void SetTutorialWindow()
	{
		isTutorialWindow = true;
		Saved_TopmostMenuType = TopmostMenuType;
		TopmostMenuType = ETopmostMenuType.Tutorial;
	}

	public void SetTutorialGuideWindow()
	{
		isTutorialWindow = true;
		Saved_TopmostMenuType = TopmostMenuType;
		TopmostMenuType = ETopmostMenuType.Tutorial_Guide;
	}

	public void ResetWindowOriginState()
	{
		if (isTutorialWindow)
		{
			TopmostMenuType = Saved_TopmostMenuType;
			isTutorialWindow = false;
		}
	}

	public IWindowShadowShader GetWindowShadowShader()
	{
		return _iWindowShadowShader;
	}

	public void ChangeLayerRecursive(LayerInfo layer)
	{
		if (layer != null)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(base.gameObject, layer.Camera.gameObject.layer);
		}
	}
}
