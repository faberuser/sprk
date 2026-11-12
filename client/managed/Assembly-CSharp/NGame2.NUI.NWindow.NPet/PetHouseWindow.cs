using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2.NPet;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NManager.NPet;
using NGame2.NUI.NPet;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWorldMap.NMapLayout;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetHouseWindow : RenewResourceWindow, PetHouseManager.IListener, PetHouseObjectManager.IListener, PetHouseCameraController.IListener, PetEggSupplyButton.IListener, IPetHouseProvider, IPetHouseObjectProvider, IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>, IUserObserver
{
	public interface IListener
	{
		IEnumerator RequestGetPetHouseInfo();

		void OpenPetInteraction(PetAvatar petAvatar);

		void OpenPetHouseObject(EPetHouseObjectType type);

		void OpenSelectPetPlacePopup();

		void OpenPetSoulInven();

		void OpenPetListBookWindow();

		void RequestPetEggGacha();

		void OpenPetExploreWindow();
	}

	[Serializable]
	public class NodeTreeInfo
	{
		public UITexture CinemaTexture;

		public string NodeTreePackCode;

		public string NodeTreeName;
	}

	[Serializable]
	public class PetHouseObjectInfo
	{
		public EPetHouseObjectType EPetHouseObjectType;

		public bool IsPossibleFocus;

		public float FocusDelayTime;

		public bool IsPlayTouchAnimation;

		public Vector3 TargetPositionOffset;

		public Vector3 TargetDirection;

		public float TargetDistance;

		[Header("[PetHouseObjectNameComponent]")]
		public Vector3 NameComponentLocalPositionOffset;

		public float NameComponetScale = 2.5f;

		public string LocalizationKey;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetHouseWindow";

	public NodeTreeInfo NodeTreeInfo_;

	public PetHouseCameraController CameraController;

	public List<PetHouseObjectInfo> PetHouseObjectInfoList = new List<PetHouseObjectInfo>();

	public List<Transform> HideRootTransformList = new List<Transform>();

	public Transform ShowUIButtonTransform;

	public FloatRange MoveXRange;

	public FloatRange MoveZRange;

	public Vector3 SlidePetStartPosition;

	public int PetPlaceCount;

	public Transform TutorialTransform;

	public Transform PlacePetNewPetRootTransform;

	public UISprite SpriteNoticeSoulInven;

	public PetEggSupplyButton PetEggSupplyButton;

	public GameObject PetExploreNoticeGo;

	private IPetHouseManager _iPetHouseManager;

	private IPetHouseObjectManager _iPetHouseObjectManager;

	private IListener _iListener;

	private RenderTargetCinema _cinema;

	private bool _isShowPetHouseUI = true;

	private bool _endFocus;

	private bool _isLoadingNodeTree;

	private Queue<Tuple<string, Action>> _nodeTreeNameQueue = new Queue<Tuple<string, Action>>();

	private SceneChanger _sceneChanger;

	private List<Coroutine> _coroutineList = new List<Coroutine>();

	private NGame2.NUI.NWindow.IntervalUpdater _petExploreNoticeUpdater;

	private bool _cachedPetExploreNotice;

	FloatRange IPetHouseProvider.MoveXRange => MoveXRange;

	FloatRange IPetHouseProvider.MoveZRange => MoveZRange;

	Vector3 IPetHouseProvider.SlidePetStartPosition => SlidePetStartPosition;

	bool IPetHouseProvider.IsShowPetHouseUI => _isShowPetHouseUI;

	int IPetHouseProvider.PetPlaceCount => PetPlaceCount;

	public void Open(IListener iListener, MapComponent mapComponent, EPetHouseObjectType ePetHouseObjectType = EPetHouseObjectType.None, Action onEnd = null)
	{
		ShowHideAllUI(isShow: false);
		Open(delegate
		{
			_coroutineList.Add(VespaCoroutine.Start(this, CoOpen(iListener, mapComponent, ePetHouseObjectType, onEnd)));
		}, null);
		PetEggSupplyButton.Open(this);
	}

	private IEnumerator CoOpen(IListener iListener, MapComponent mapComponent, EPetHouseObjectType ePetHouseObjectType, Action onEnd)
	{
		yield return null;
		_sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(10f, GlobalTopmostWindow.ESceneChangeType.Cloud);
		while (!_sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (iListener != null)
		{
			Coroutine coroutine = VespaCoroutine.Start(this, iListener.RequestGetPetHouseInfo());
			_coroutineList.Add(coroutine);
			yield return coroutine;
		}
		if (NodeTreeInfo_.CinemaTexture == null)
		{
			yield break;
		}
		Coroutine coroutine2 = VespaCoroutine.Start(this, CoLoadNodeTree(NodeTreeInfo_.NodeTreePackCode, NodeTreeInfo_.NodeTreeName, delegate
		{
			CameraController.Init(new PetHouseCameraController.CameraControllerInfo
			{
				Cinema = _cinema,
				IPetHouseProvider = this,
				IListener = this
			});
			Open(delegate
			{
				Init(iListener, mapComponent);
			});
		}));
		_coroutineList.Add(coroutine2);
		yield return coroutine2;
		yield return null;
		_sceneChanger.Close();
		while (!_sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (_iPetHouseObjectManager != null)
		{
			_iPetHouseObjectManager.FocusPetHouseObject(ePetHouseObjectType, onEnd);
		}
	}

	private IEnumerator CoLoadNodeTree(string packCode, string name, Action endAction)
	{
		if (_cinema != null)
		{
			_cinema.LoadNodeTree(packCode, name);
		}
		if (_cinema == null)
		{
			RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
			SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
			stock.NodeTreePackCode = packCode;
			stock.NodeTreeName = name;
			_cinema = RenderTargetCinema.CreateInstance(stock);
			_cinema.OrthographicMode(isOn: false);
			_cinema.AlowHDR(isOn: true);
		}
		if (_cinema == null)
		{
			yield break;
		}
		do
		{
			yield return null;
		}
		while (!_cinema.IsCompleteLoad());
		if (_cinema.StartNodeTree(endAction) && _cinema.CinemaCamera != null)
		{
			if (NodeTreeInfo_.CinemaTexture != null && _cinema != null)
			{
				NodeTreeInfo_.CinemaTexture.mainTexture = _cinema.RenderTarget;
				NodeTreeInfo_.CinemaTexture.alpha = 1f;
			}
			yield return null;
		}
	}

	public void SetInitCamera()
	{
		if (!(CameraController == null))
		{
			CameraController.SetInitCamera(initCamera: true);
		}
	}

	public void PlayNodeTree(string nodeTreeName, Action endAction)
	{
		_nodeTreeNameQueue.Enqueue(new Tuple<string, Action>(nodeTreeName, endAction));
		if (!_isLoadingNodeTree)
		{
			_coroutineList.Add(VespaCoroutine.Start(this, CoPlayNodeTree()));
		}
	}

	public IEnumerator CoPlayNodeTree()
	{
		_isLoadingNodeTree = true;
		while (!(_cinema == null))
		{
			while (!_cinema.IsCompleteLoad())
			{
				yield return null;
			}
			Tuple<string, Action> args = _nodeTreeNameQueue.Dequeue();
			_cinema.LoadNodeTree(NodeTreeInfo_.NodeTreePackCode, args.Item1);
			yield return null;
			while (!_cinema.IsCompleteLoad())
			{
				yield return null;
			}
			_cinema.StartNodeTree(args.Item2);
			if (_nodeTreeNameQueue.Count == 0)
			{
				break;
			}
			yield return null;
		}
		_isLoadingNodeTree = false;
	}

	public override void ReOpen()
	{
		base.ReOpen();
		RefreshNewSoulInven();
	}

	public override void OnSystemBackButton()
	{
		if ((_sceneChanger == null || _sceneChanger.IsClosed()) && (!(CameraController != null) || CameraController.ECameraState_ == PetHouseCameraController.ECameraState.Normal))
		{
			if (!_isShowPetHouseUI)
			{
				ShowHideUI(isShow: true);
			}
			else
			{
				base.OnSystemBackButton();
			}
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		VespaCoroutine.Stop(this, _coroutineList);
		ReleaseCinema();
		if (CameraController != null)
		{
			CameraController.Close();
		}
		if (_iPetHouseManager != null)
		{
			_iPetHouseManager.Close();
		}
		if (_iPetHouseObjectManager != null)
		{
			_iPetHouseObjectManager.Close();
		}
		if (_nodeTreeNameQueue != null)
		{
			_nodeTreeNameQueue.Clear();
		}
		_petExploreNoticeUpdater = null;
		_cachedPetExploreNotice = false;
		_isLoadingNodeTree = false;
		_sceneChanger = null;
		SceneAutoBehaviour<PetManagement>.instance.IsOpenPetHouse = false;
		SceneAutoBehaviour<PetManagement>.instance.ReturnPetHouseBGM();
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
	}

	private void ReleaseCinema()
	{
		RenderTargetCinema.ReleaseInstance(_cinema);
		_cinema = null;
	}

	private void Init(IListener iListener, MapComponent mapComponent)
	{
		Init();
		_iListener = iListener;
		_iPetHouseManager = new PetHouseManager(new PetHouseManager.PetHouseManagerInfo
		{
			CameraController = CameraController,
			MapComponent = mapComponent,
			IListener = this,
			IPetHouseProvider = this
		});
		InitPlacePet();
		_iPetHouseObjectManager = new PetHouseObjectManager(new PetHouseObjectManager.PetHouseObjectManagerInfo
		{
			CameraController = CameraController,
			IListener = this,
			IPetHouseObjectProvider = this
		});
		EnableShowUIButton(enable: false);
		InitEggSupplyButton();
		InitJukeboxBgm();
		RefreshIsNewPet();
		RefreshNewSoulInven();
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
		OpenRaiderHouseUI triggerEvent = new OpenRaiderHouseUI();
		if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent, delegate
		{
			UIHelper.EnableGameObject(TutorialTransform, enable: false);
		}))
		{
			UIHelper.EnableGameObject(TutorialTransform, enable: true);
		}
	}

	private void InitJukeboxBgm()
	{
		JukeboxData data = GameRoot.Instance.JukeboxDataContainer.GetData(PetJukebox.PetHouseBGM.Value);
		if (data != null)
		{
			string effectCode = data.EffectCode;
			if (!String_Extension_Method.IsNullOrEmpty(effectCode))
			{
				PlayNodeTree(effectCode, null);
				SceneAutoBehaviour<PetManagement>.instance.PlayPetHouseBGM();
			}
		}
	}

	private void InitPlacePet()
	{
		List<PlacePetInfo> list = new List<PlacePetInfo>();
		list.Clear();
		List<int> placePetIndexList = GlobalAutoBehaviour<AccountManager>.instance.PetManager.PlacePetIndexList;
		int num = 0;
		for (int i = 0; i < PetPlaceCount; i++)
		{
			num = 0;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(placePetIndexList) && placePetIndexList.Count > i)
			{
				num = placePetIndexList[i];
			}
			list.Add(new PlacePetInfo
			{
				PetIndex = num,
				IsPlace = true
			});
		}
		PlacePet(0.5f, list);
	}

	private void InitEggSupplyButton()
	{
		if (PetEggSupplyButton != null)
		{
			PetEggSupplyButton.Open(this);
		}
	}

	private void EnableShowUIButton(bool enable)
	{
		UIHelper.EnableGameObject(ShowUIButtonTransform, enable);
	}

	private void ShowHideUI(bool isShow)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(HideRootTransformList))
		{
			return;
		}
		Transform transform = null;
		for (int i = 0; i < HideRootTransformList.Count; i++)
		{
			transform = HideRootTransformList[i];
			if (!(transform == null))
			{
				UIHelper.EnableGameObject(transform, isShow);
			}
		}
		_isShowPetHouseUI = isShow;
		if (_iPetHouseObjectManager != null)
		{
			_iPetHouseObjectManager.EnableNameComponentList(isShow);
		}
		if (CameraController != null && CameraController.IsFocus)
		{
			isShow = true;
		}
		EnableShowUIButton(!isShow);
	}

	private void ShowHideAllUI(bool isShow)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(HideRootTransformList))
		{
			return;
		}
		Transform transform = null;
		for (int i = 0; i < HideRootTransformList.Count; i++)
		{
			transform = HideRootTransformList[i];
			if (!(transform == null))
			{
				UIHelper.EnableGameObject(transform, isShow);
			}
		}
		EnableShowUIButton(isShow);
	}

	private void ShowUI()
	{
		ShowHideUI(isShow: true);
		if (_iPetHouseObjectManager != null)
		{
			_iPetHouseObjectManager.EnableNameComponentList(isEnable: true);
			_iPetHouseObjectManager.SetStateIdleAllPetHouseObject();
		}
	}

	public bool IsFocusObject()
	{
		return CameraController.ECameraState_ != PetHouseCameraController.ECameraState.Normal;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdatePetManager();
		UpdatePetHouseObjectManager();
		UpdatePetExploreNotice();
	}

	private void UpdatePetManager()
	{
		if (_iPetHouseManager != null && !(CameraController == null))
		{
			_iPetHouseManager.Update(CameraController.FirstTouch);
		}
	}

	private void UpdatePetHouseObjectManager()
	{
		if (_iPetHouseObjectManager != null && !(CameraController == null))
		{
			_iPetHouseObjectManager.AlwaysUpdate();
			if (_isShowPetHouseUI)
			{
				_iPetHouseObjectManager.Update(CameraController.FirstTouch);
			}
		}
	}

	private void UpdatePetExploreNotice()
	{
		if (_petExploreNoticeUpdater == null)
		{
			_cachedPetExploreNotice = PetHelper.AnyExploreEnd();
			UIHelper.EnableGameObject(PetExploreNoticeGo, _cachedPetExploreNotice);
			_petExploreNoticeUpdater = new NGame2.NUI.NWindow.IntervalUpdater(delegate
			{
				bool flag = PetHelper.AnyExploreEnd();
				if (flag != _cachedPetExploreNotice)
				{
					_cachedPetExploreNotice = flag;
					UIHelper.EnableGameObject(PetExploreNoticeGo, _cachedPetExploreNotice);
				}
			});
			_petExploreNoticeUpdater.SetInterval(1f);
		}
		if (_petExploreNoticeUpdater != null)
		{
			_petExploreNoticeUpdater.Update();
		}
	}

	public void NextPetInteraction(int currentPeIndex, int nextIndex)
	{
		if (_iPetHouseManager != null)
		{
			_iPetHouseManager.NextPetInteraction(currentPeIndex, nextIndex);
		}
	}

	public void PlacePet(float sec, List<PlacePetInfo> placePetInfoList)
	{
		if (_iPetHouseManager != null)
		{
			_iPetHouseManager.PlacePet(sec, placePetInfoList);
		}
	}

	private IEnumerator CoFocusPet(PetAvatar petAvatar)
	{
		if (petAvatar != null && _iListener != null)
		{
			ShowHideUI(isShow: false);
			if (_iPetHouseObjectManager != null)
			{
				_iPetHouseObjectManager.EnableNameComponentList(isEnable: false);
			}
			while (!_endFocus)
			{
				yield return null;
			}
			_endFocus = false;
			_iListener.OpenPetInteraction(petAvatar);
		}
	}

	private IEnumerator CoFocusPetHouseObject(IPetHouseObject iPetHouseObject, bool isPossibleFocus, Action onEnd)
	{
		if (iPetHouseObject == null || __Extension_Method_ICollection__.IsNullOrEmpty(PetHouseObjectInfoList) || _iListener == null)
		{
			yield break;
		}
		if (isPossibleFocus)
		{
			ShowHideUI(isShow: false);
			EnableShowUIButton(enable: false);
			if (_iPetHouseManager != null)
			{
				_iPetHouseManager.AllPetEnable(isEnable: false);
			}
		}
		else
		{
			_endFocus = true;
		}
		PetHouseObjectInfo petHouseObjectInfo = PetHouseObjectInfoList.Find((PetHouseObjectInfo info) => info.EPetHouseObjectType == iPetHouseObject.EPetHouseObjectType);
		if (petHouseObjectInfo != null)
		{
			while (petHouseObjectInfo.FocusDelayTime <= 0f && !_endFocus)
			{
				yield return null;
			}
			if (petHouseObjectInfo.FocusDelayTime > 0f)
			{
				yield return new WaitForSeconds(petHouseObjectInfo.FocusDelayTime);
			}
			_endFocus = false;
			_iListener.OpenPetHouseObject(iPetHouseObject.EPetHouseObjectType);
			if (onEnd != null)
			{
				Vespa_System_Action_Extension_Method.fire(onEnd);
			}
		}
	}

	public void SetIncubatorState(int slotIndex, PetIncubatorObjectInfo.IncubatorStateType incubatorStateType, Action onEnd, bool needInfoUpdate = false)
	{
		_iPetHouseObjectManager.SetIncubatorState(slotIndex, incubatorStateType, onEnd, needInfoUpdate);
	}

	public void AddNewIncubatorObject(int slotIndex)
	{
		_iPetHouseObjectManager.AddNewIncubatorObject(slotIndex);
	}

	public void ChangeIncubatorObject(int slotIndex)
	{
		_iPetHouseObjectManager.ChangeIncubatorObject(slotIndex);
	}

	public void FocusPetIncubator()
	{
		if (_iPetHouseObjectManager != null)
		{
			_iPetHouseObjectManager.FocusPetHouseObject(EPetHouseObjectType.IncubatorHouse, null);
		}
	}

	private void OpenCenterMessageNotHavePet()
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("PET_USE_FACILITY_CONDITION_TEXT"));
	}

	public void FocusPetHouseObject(EPetHouseObjectType ePetHouseObjectType, Action onEnd)
	{
		if (IsOpen() && _iPetHouseObjectManager != null)
		{
			_iPetHouseObjectManager.FocusPetHouseObject(ePetHouseObjectType, onEnd);
		}
	}

	private void RefreshIsNewPet()
	{
		bool enable = GlobalAutoBehaviour<AccountManager>.instance.PetManager.GetAddedPetCount() > 0;
		UIHelper.EnableGameObject(PlacePetNewPetRootTransform, enable);
	}

	private void RefreshNewSoulInven()
	{
		bool enable = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.IsExistsNewPetSoul();
		UIHelper.EnableGameObject(SpriteNoticeSoulInven, enable);
	}

	private void PlayObjectTouchSound(EPetHouseObjectType ePetHouseObjectType)
	{
		string text = "UI2.Button10";
		switch (ePetHouseObjectType)
		{
		case EPetHouseObjectType.Management:
			text = "UI2.Button13";
			break;
		case EPetHouseObjectType.Jukebox:
			text = "UI2.Button2";
			break;
		case EPetHouseObjectType.FeedNormal:
		case EPetHouseObjectType.FeedEmpty:
		case EPetHouseObjectType.FeedMax:
			text = "UI2.Button20";
			break;
		default:
			text = "UI2.Button10";
			break;
		}
		UIHelper.PlaySound(text);
	}

	public void OnClickHideUI()
	{
		ShowHideUI(isShow: false);
	}

	public void OnClickShowUI()
	{
		ShowHideUI(isShow: true);
	}

	public void OnClickPlacePet()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.PetManager.NotHavePet)
		{
			OpenCenterMessageNotHavePet();
		}
		else if (!(CameraController == null) && !CameraController.NotTouchCamera && _iListener != null)
		{
			_iListener.OpenSelectPetPlacePopup();
		}
	}

	public void OnClickPetSoulInven()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.PetManager.NotHavePet)
		{
			OpenCenterMessageNotHavePet();
		}
		else if (_iListener != null)
		{
			_iListener.OpenPetSoulInven();
		}
	}

	public void OnClickPetListBook()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.PetManager.NotHavePet)
		{
			OpenCenterMessageNotHavePet();
		}
		else if (_iListener != null)
		{
			_iListener.OpenPetListBookWindow();
		}
	}

	public void OnClickOpenExploreWIndow()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.PetManager.NotHavePet)
		{
			OpenCenterMessageNotHavePet();
		}
		else if (_iListener != null)
		{
			_iListener.OpenPetExploreWindow();
		}
	}

	void PetHouseManager.IListener.TouchedPet(PetAvatar petAvatar)
	{
		_endFocus = false;
		_coroutineList.Add(VespaCoroutine.Start(this, CoFocusPet(petAvatar)));
	}

	void PetHouseObjectManager.IListener.TouchedPetHouseObject(IPetHouseObject iPetHouseObject, bool isPossibleFocus, Action onEnd)
	{
		PlayObjectTouchSound(iPetHouseObject.EPetHouseObjectType);
		_endFocus = false;
		_coroutineList.Add(VespaCoroutine.Start(this, CoFocusPetHouseObject(iPetHouseObject, isPossibleFocus, onEnd)));
	}

	void PetHouseCameraController.IListener.ProceedingInitCamera()
	{
		if (_iPetHouseManager != null)
		{
			_iPetHouseManager.AllPetEnable(isEnable: true);
		}
		RefreshIsNewPet();
		RefreshNewSoulInven();
		ShowUI();
	}

	void PetHouseCameraController.IListener.EndInitCamera()
	{
		if (_iPetHouseManager != null)
		{
			_iPetHouseManager.AllPetEnable(isEnable: true);
		}
		ShowUI();
	}

	void PetHouseCameraController.IListener.EndFocus(PetHouseCameraController.ECameraState eCameraState)
	{
		_endFocus = true;
	}

	void IPetHouseProvider.CurrentPetsState()
	{
		if (_iPetHouseManager != null)
		{
			_iPetHouseManager.AnnouncePetsCurrentState();
		}
	}

	PetHouseObjectInfo IPetHouseObjectProvider.GetPetHouseObjectInfo(EPetHouseObjectType ePetHouseObjectType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(PetHouseObjectInfoList))
		{
			return null;
		}
		return PetHouseObjectInfoList.Find((PetHouseObjectInfo info) => info.EPetHouseObjectType == ePetHouseObjectType);
	}

	void IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>.OnChanged(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo != null && (changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.AddedPet || changedInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.RemoveAddedPet))
		{
			RefreshIsNewPet();
		}
	}

	void PetEggSupplyButton.IListener.RequestEggReward()
	{
		if (_iListener != null)
		{
			_iListener.RequestPetEggGacha();
		}
	}
}
