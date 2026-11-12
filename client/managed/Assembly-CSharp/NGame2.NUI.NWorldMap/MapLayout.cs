using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NodeFunctionType;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGame2.NUI.NWorldMap.NMapLayout.NExtraAction;
using NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;
using NGameAsset;
using NGameAsset.NPatch;
using NGameAsset.NWorldMap;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWorldMap;

public class MapLayout : BaseWindow, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver<ChapterDungeonInfo>, IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>, IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>, ChapterFlowManager.IListener, IObserver
{
	public interface Listener
	{
	}

	public interface IProvider
	{
		void ChangePlayerAvatarRenderFacadeOnly(NGame2.NUI.NWorldMap.NMapLayout.Avatar avatar);
	}

	[Serializable]
	public class AvatarOptionInfo
	{
		public float RearrangeRadius = 50f;

		public float RearrangeLerpFactor = 5f;

		public float RearrangeRotate = MathF.PI * 2f;

		public float RearrangeXyAspect = 1f;

		public float BasicRotate = MathF.PI;
	}

	public const string PrefabPath = "Prefab2/WorldMap/MapLayout";

	public Transform BuildingRoot;

	public Transform NodeRoot;

	public Transform EventRoot;

	public Transform AvatarRoot;

	public Transform IndicatorRoot;

	public Transform PrefabPropRoot;

	public UITexture[] MapTexture;

	public MapViewer MapViewer;

	public float Render3dAngle = -45f;

	public float CrowdCollisionRadius = 100f;

	public AvatarOptionInfo AvatarOption;

	private Listener _listener;

	private Mediator _mediator = new Mediator();

	private NodeGraph _nodeGraph = new NodeGraph();

	private MapImageLoader _imageLoader = new MapImageLoader();

	private EventManager _eventManager = new EventManager();

	private CrowdManager _crowdManager = new CrowdManager();

	private PropManager _propManager = new PropManager();

	private NodeBackground _nodeBackground = new NodeBackground();

	private AvatarRearranger _avatarRearranger = new AvatarRearranger();

	private BuildingManager _buildingManager = new BuildingManager();

	private PcAvatarManager _pcAvatarManager = new PcAvatarManager();

	private ChapterFlowManager _chapterFlowManager;

	private string _eventBgmCode = string.Empty;

	private bool _show = true;

	private int _avatarIndex;

	public EventManager EventManager => _eventManager;

	public CrowdManager CrowdManager => _crowdManager;

	public NodeBackground NodeBackground => _nodeBackground;

	public Mediator Mediator => _mediator;

	public BuildingManager BuildingManager => _buildingManager;

	public PropManager PropManager => _propManager;

	public PcAvatarManager PcAvatarManger => _pcAvatarManager;

	public bool IsEntryMenuVisible => _nodeBackground.IsShow;

	public bool IsShowMap => _show;

	public bool IsAvatarRootTouched { get; set; }

	private bool IsMultiAvatar
	{
		get
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			return GameRoot.Instance.CampaignMultiAvatarDataContainer.IsChapter(chapterIndex);
		}
	}

	public override bool IsOpen()
	{
		return true;
	}

	public void InitAsync(Listener listener, Action onComplete)
	{
		if (MapViewer == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else
		{
			VespaCoroutine.Start(coInitAsync(listener, onComplete));
		}
	}

	private IEnumerator coInitAsync(Listener listener, Action onComplete)
	{
		Show(show: true);
		MapComponent mapComponent = new MapComponent();
		mapComponent.Layout = this;
		mapComponent.Viewer = MapViewer;
		mapComponent.NodeGraph = _nodeGraph;
		CreatePlayerAvatar(mapComponent);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.AddObserver(this);
		InitManager();
		InitAvatarRearranger();
		MapViewer.AddObserver(this);
		_listener = listener;
		base.gameObject.SetActive(value: true);
		bool completeLoadMap = false;
		CheckCanEnterChapter(out var chapterIndex, out var dungeonIndex);
		bool completeExtraAction = false;
		ExtraActionBeforeChangeMap(chapterIndex, delegate
		{
			completeExtraAction = true;
		});
		while (!completeExtraAction)
		{
			yield return null;
		}
		ChangeMap(chapterIndex, dungeonIndex, delegate
		{
			completeLoadMap = true;
		});
		while (!completeLoadMap)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private void InitManager()
	{
		_imageLoader.Open(MapTexture);
		MapViewer.Open(_mediator);
		_nodeGraph.Open(_mediator, NodeRoot);
		_eventManager.Open(_mediator);
		_crowdManager.Open(_mediator, CrowdCollisionRadius);
		_propManager.Open(_mediator);
		_nodeBackground.Open(_mediator);
		_buildingManager.Open(_mediator, BuildingRoot);
		_pcAvatarManager.Open(_mediator);
	}

	private void InitAvatarRearranger()
	{
		_avatarRearranger = new AvatarRearranger();
		_avatarRearranger.Open(AvatarOption);
		_avatarRearranger.AddAvatarContainer(_mediator);
		_avatarRearranger.AddAvatarContainer(_crowdManager);
		_avatarRearranger.AddAvatarContainer(_eventManager.NpcAvatarManager);
		_avatarRearranger.AddAvatarContainer(_pcAvatarManager);
	}

	private void InitChapterFlowManager(int chapterIndex)
	{
		if (!CheckPossibleFlowChapter(chapterIndex))
		{
			CloseChapterFlowManager();
			return;
		}
		_chapterFlowManager.Init(new ChapterFlowManager.ChapterFlowInfo
		{
			ChapterIndex = chapterIndex,
			MyAvatarMediator = _mediator,
			CrowdManager = _crowdManager
		});
	}

	private bool CheckPossibleFlowChapter(int chapterIndex)
	{
		if (_chapterFlowManager == null)
		{
			_chapterFlowManager = new ChapterFlowManager(this, chapterIndex);
		}
		if (!IsMultiAvatar)
		{
			SetUserAvatar();
			return false;
		}
		if (_chapterFlowManager.CheckEndAllLine(chapterIndex))
		{
			ChangePlayerAvatar(_mediator.Avatar, GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex, UserManager.UserStatType.AvatarHeroIndex);
			return false;
		}
		return true;
	}

	public void ProcessChapterFlow(int chapterIndex, int dungeonIndex, ChapterFlowManager.ConditionKeyType type)
	{
		if (_chapterFlowManager != null)
		{
			_chapterFlowManager.ProcessChapterFlow(chapterIndex, dungeonIndex, type);
		}
	}

	private void CreatePlayerAvatar(MapComponent mapComponent)
	{
		SprkLobbyAvatar.Bind(this);
		_mediator.MapComponent = mapComponent;
		_mediator.Avatar = new NGame2.NUI.NWorldMap.NMapLayout.Avatar();
		_mediator.MapWidth = 1;
		_mediator.MapHeight = 1;
		_mediator.AvatarController = new AvatarController(_mediator);
		if (_mediator.Avatar != null)
		{
			RenderFacadeBuilder renderFacadeBuilder = null;
			_avatarIndex = GetChapterAvatarIndex(out var isForcedAvatar);
			renderFacadeBuilder = ((!isForcedAvatar) ? CreateAvatarHeroRenderFacadeBuilder(_avatarIndex) : UIAvatarHelper.CreateRenderFacadeBuilder(_avatarIndex));
			_mediator.Avatar.Open(new NGame2.NUI.NWorldMap.NMapLayout.Avatar.AvatarInfo
			{
				AvatarController = _mediator.AvatarController,
				MapComponent = mapComponent,
				RenderFacadeBuilder = renderFacadeBuilder,
				IAiAvatarListener = _crowdManager
			});
			_mediator.Avatar.CreateDailyGiftBox();
			SprkLobbyAvatar.Track(this, IsForcedHeroAvatar());
		}
	}

	private void ChangePlayerAvatarRenderFacadeOnly(NGame2.NUI.NWorldMap.NMapLayout.Avatar avatar, int avatarIndex)
	{
		if (avatar != null)
		{
			RenderFacadeBuilder renderFacadeBuilder = CreateAvatarHeroRenderFacadeBuilder(avatarIndex);
			avatar.CreateRenderFacade(renderFacadeBuilder);
			SprkLobbyAvatar.Track(this, IsForcedHeroAvatar());
		}
	}

	private void ChangePlayerAvatar(NGame2.NUI.NWorldMap.NMapLayout.Avatar avatar, int avatarIndex, UserManager.UserStatType changeType)
	{
		if (avatar != null)
		{
			_avatarIndex = avatarIndex;
			RenderFacadeBuilder renderFacadeBuilder = null;
			renderFacadeBuilder = ((changeType != UserManager.UserStatType.SubStoryAvatarHeroIndex) ? CreateAvatarHeroRenderFacadeBuilder(avatarIndex) : UIAvatarHelper.CreateRenderFacadeBuilder(avatarIndex));
			avatar.CreateRenderFacade(renderFacadeBuilder);
			SprkLobbyAvatar.Track(this, IsForcedHeroAvatar());
		}
	}

	private RenderFacadeBuilder CreateAvatarHeroRenderFacadeBuilder(int avatarHeroIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.PlayerAvatarHeroInfo == null)
		{
			return UIAvatarHelper.CreateRenderFacadeBuilder(avatarHeroIndex);
		}
		UIAvatarHelper.AvatarInfo avatarInfo = new UIAvatarHelper.AvatarInfo(avatarHeroIndex);
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(avatarInfo.HeroIndex);
		if (cachedHeroInfo == null)
		{
			cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.First();
		}
		cachedHeroInfo = SprkLobbyAvatar.Create(cachedHeroInfo, GlobalAutoBehaviour<AccountManager>.instance.UserManager.PlayerAvatarHeroInfo);
		return SprkLobbyAvatar.Build(null, TeamType.Left, cachedHeroInfo);
	}

	private int GetChapterAvatarIndex(out bool isForcedAvatar)
	{
		int avatarHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex;
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		isForcedAvatar = IsForcedHeroAvatar();
		if (!isForcedAvatar)
		{
			return avatarHeroIndex;
		}
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data == null)
		{
			return avatarHeroIndex;
		}
		return data.ForcedAvatarIndex + 10000;
	}

	private bool IsForcedHeroAvatar()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		if (IsMultiAvatar)
		{
			if (_chapterFlowManager == null)
			{
				return false;
			}
			return !_chapterFlowManager.CheckEndAllLine(chapterIndex);
		}
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null)
		{
			return data.ForcedAvatarIndex > 0;
		}
		return false;
	}

	private bool CheckCanEnterChapter(out int chapterIndex, out int dungeonIndex)
	{
		chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data == null)
		{
			return true;
		}
		if (string.IsNullOrEmpty(data.EnterCheckerName))
		{
			return true;
		}
		return SingletonAuto<SwitchingContainer>.instance.GetCondition(data.EnterCheckerName)?.EnterConditionCheck(data.Values, out chapterIndex, out dungeonIndex) ?? true;
	}

	public void ExtraActionBeforeChangeMap(int chapterIndex, Action action)
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data == null || string.IsNullOrEmpty(data.BeforeEnterMap))
		{
			Vespa_System_Action_Extension_Method.fire(action);
			return;
		}
		NGame2.NUI.NWorldMap.NMapLayout.NExtraAction.IAction extraAction = SingletonAuto<ExtraActionContainer>.instance.GetExtraAction(data.BeforeEnterMap);
		if (extraAction == null)
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
		else
		{
			extraAction.Execute(action);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.PlayerAvatarHeroInfo || changedInfo.UserStatType == UserManager.UserStatType.AvatarHeroIndex || changedInfo.UserStatType == UserManager.UserStatType.SubStoryAvatarHeroIndex)
		{
			if (IsForcedHeroAvatar())
			{
				CenterMessage.Open(LocalizationManager.GetString(SelectAvatar.CHANGE_COMPLETED_BUT_AVATAR_NOT_CHANGE));
			}
			else if (changedInfo.UserStatType == UserManager.UserStatType.PlayerAvatarHeroInfo)
			{
				ChangePlayerAvatarRenderFacadeOnly(_mediator.Avatar, _avatarIndex);
			}
			else if (changedInfo.UserStatType == UserManager.UserStatType.AvatarHeroIndex || changedInfo.UserStatType == UserManager.UserStatType.SubStoryAvatarHeroIndex)
			{
				long num = changedInfo.AfterValue;
				NGame2.NUI.NWorldMap.NMapLayout.Avatar avatar = _mediator.Avatar;
				UserManager.UserStatType userStatType = changedInfo.UserStatType;
				long num2 = num;
				ChangePlayerAvatar(avatar, checked((int)num2), userStatType);
			}
		}
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedHeroInfo)
	{
		SprkLobbyAvatar.Notify(this, changedHeroInfo, IsForcedHeroAvatar());
	}

	void IUserObserver<ChapterDungeonInfo>.OnChanged(ChapterDungeonInfo changedInfo)
	{
		if (changedInfo != null)
		{
			RecreateNode(changedInfo.DungeonIndex);
		}
	}

	private bool CheckDropBonusEvent(DropBonusEventManager.DropBonusEventChangeInfo changedInfo, out int chapterIndex, out int dungeonIndex)
	{
		chapterIndex = 0;
		dungeonIndex = 0;
		if (changedInfo == null)
		{
			return false;
		}
		if (changedInfo.bonusEvent.BonusType != EventBonusType.EventNode)
		{
			return false;
		}
		string condition = changedInfo.bonusEvent.Condition;
		if (String_Extension_Method.IsNullOrEmpty(condition))
		{
			return false;
		}
		string[] array = condition.Split(new char[1] { ',' });
		if (__Extension_Method_ICollection__.COUNT(array) < 2)
		{
			return false;
		}
		string s = array[0].Trim();
		if (!int.TryParse(array[1].Trim(), out dungeonIndex) || !int.TryParse(s, out chapterIndex))
		{
			return false;
		}
		if (chapterIndex != NGame2.NAccount.UserCampaignManager.ChapterIndex)
		{
			return false;
		}
		return true;
	}

	void IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>.OnChanged(DropBonusEventManager.DropBonusEventChangeInfo changedInfo)
	{
		int chapterIndex = 0;
		int dungeonIndex = 0;
		if (!CheckDropBonusEvent(changedInfo, out chapterIndex, out dungeonIndex))
		{
			return;
		}
		bool flag = changedInfo.changeType == DropBonusEventManager.DropBonusEventChangeType.EventAdded;
		if (!flag && dungeonIndex == NGame2.NAccount.UserCampaignManager.DungeonIndex)
		{
			CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
			if (data != null)
			{
				MovePlayerToDungeon(chapterIndex, data.StartNodeIndex, warp: true);
			}
		}
		if (flag)
		{
			if (_nodeGraph != null)
			{
				_nodeGraph.OnChangeDropBonusEvent(chapterIndex, dungeonIndex, flag);
			}
			if (EventManager != null)
			{
				EventManager.OnChangeDropBonusEvent(chapterIndex, dungeonIndex, flag);
			}
		}
		else
		{
			if (EventManager != null)
			{
				EventManager.OnChangeDropBonusEvent(chapterIndex, dungeonIndex, flag);
			}
			if (_nodeGraph != null)
			{
				_nodeGraph.OnChangeDropBonusEvent(chapterIndex, dungeonIndex, flag);
			}
		}
	}

	public override void Close()
	{
		base.Close();
		ReleaseGameAsset();
	}

	public void RecreateNode(int dungeonIndex)
	{
		if (_nodeGraph != null)
		{
			_nodeGraph.RecreateNode(dungeonIndex);
		}
		if (EventManager != null)
		{
			EventManager.OnNodeChanged(dungeonIndex);
		}
	}

	public bool IsPlayerMoving()
	{
		if (_mediator == null || _mediator.AvatarController == null)
		{
			return false;
		}
		return _mediator.AvatarController.IsAvatarMoving();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!_show)
		{
			if (MapViewer != null)
			{
				MapViewer.ChainUpdate();
			}
			return;
		}
		if (_mediator.AvatarController != null)
		{
			_mediator.AvatarController.ChainUpdate();
		}
		if (MapViewer != null)
		{
			MapViewer.ChainUpdate();
		}
		_eventManager.ChainUpdate();
		_crowdManager.ChainUpdate();
		_propManager.ChainUpdate();
		_nodeBackground.ChainUpdate();
		_avatarRearranger.ChainUpdate();
		_buildingManager.ChainUpdate();
		_pcAvatarManager.ChainUpdate();
	}

	private void LateUpdate()
	{
		if (_show && _mediator.Avatar != null)
		{
			_mediator.Avatar.ChainUpdate();
		}
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			ReleaseGameAsset();
		}
	}

	private void ReleaseGameAsset()
	{
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
		_imageLoader.ReleaseGameAsset();
		_eventManager.ReleaseGameAsset();
		_pcAvatarManager.UnLoadMap();
		_crowdManager.Close();
		_propManager.Close();
		_buildingManager.Close();
		_nodeBackground.Close();
		_mediator.Close();
		CloseChapterFlowManager();
	}

	private void CloseChapterFlowManager()
	{
		if (_chapterFlowManager != null)
		{
			_chapterFlowManager.Close();
			_chapterFlowManager = null;
		}
	}

	public ITask OpenNodeTreeEntryMenu(EntryMenuSettingData menuAppearance, bool skipAnim)
	{
		return Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask(), delegate
		{
			if (!String_Extension_Method.IsNullOrEmpty(menuAppearance.EventBgmCode))
			{
				_eventBgmCode = menuAppearance.EventBgmCode;
				SingletonAuto<BgmManager2>.instance.Request(_eventBgmCode, isEvent: true);
			}
			if (!String_Extension_Method.IsNullOrEmpty(menuAppearance.NpcCode2))
			{
				_mediator.Avatar.Hide();
			}
		}).ADDTASK(new ParallelTask().ADDTASK(BuildingManager.BuildingHideTask(menuAppearance)).ADDTASK(PcAvatarManger.PcAvatarHideTask(menuAppearance)).ADDTASK(MapViewer.CreateEnterNodeMenuAsync(menuAppearance, skipAnim))
			.ADDTASK(PropManager.PropHideTask(menuAppearance))).WrapperTouchControl()
			.DisableTouch()
			.WrapperStopIfNull(this);
	}

	public ITask OpenEntryMenuAsync(EntryMenuSettingData menuAppearance, bool skipAnim)
	{
		return Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask(), delegate
		{
			if (!String_Extension_Method.IsNullOrEmpty(menuAppearance.EventBgmCode))
			{
				_eventBgmCode = menuAppearance.EventBgmCode;
				SingletonAuto<BgmManager2>.instance.Request(_eventBgmCode, isEvent: true);
			}
			if (!String_Extension_Method.IsNullOrEmpty(menuAppearance.NpcCode2))
			{
				_mediator.Avatar.Hide();
			}
			_nodeBackground.Load(menuAppearance);
		}).ADDTASK(new WaitTask(() => !_nodeBackground.IsAssetLoadDone)).ADDTASK(new ParallelTask().ADDTASK(BuildingManager.BuildingHideTask(menuAppearance)).ADDTASK(PcAvatarManger.PcAvatarHideTask(menuAppearance)).ADDTASK(MapViewer.CreateEnterNodeMenuAsync(menuAppearance, skipAnim))
			.ADDTASK(_nodeBackground.CreateShowTask(skipAnim))
			.ADDTASK(PropManager.PropHideTask(menuAppearance)))
			.WrapperTouchControl()
			.DisableTouch()
			.WrapperStopIfNull(this);
	}

	public ITask CloseEntryMenuAsync()
	{
		return Vespa_Task_ExtensionMethod.NAME(Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask(), delegate
		{
			if (!String_Extension_Method.IsNullOrEmpty(_eventBgmCode))
			{
				SingletonAuto<BgmManager2>.instance.StopEvent(_eventBgmCode);
				_eventBgmCode = string.Empty;
			}
			if (!_mediator.Avatar.IsShow)
			{
				_mediator.Avatar.Show();
			}
		}).ADDTASK(new ParallelTask().ADDTASK(MapViewer.CreateLeaveNodeMenuAsync()).ADDTASK(_nodeBackground.CreateHideTask()).ADDTASK(BuildingManager.BuildingShowTask())
			.ADDTASK(PcAvatarManger.AvatarShowTask())
			.ADDTASK(PropManager.PropShowTask())).WrapperTouchControl()
			.DisableTouch()
			.WrapperStopIfNull(this), "CloseEntryMenuAsync");
	}

	public void ChangeMap(int chapterIndex, int dungeonIndex, Action onComplete)
	{
		UnloadMap();
		LoadMap(chapterIndex, dungeonIndex, onComplete);
	}

	public void UnloadMap()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager != null)
		{
			_mediator.OnUnloadMap();
			_eventManager.NpcAvatarManager.Close();
			_nodeGraph.ClearNodeGraph();
			_eventManager.OnUnloadMap();
			_crowdManager.OnUnloadMap();
			_propManager.OnUnloadMap();
			_buildingManager.OnUnLoadMap();
			_pcAvatarManager.UnLoadMap();
			_mediator.AvatarController.OnUnloadMap();
			_imageLoader.Unload();
			CloseChapterFlowManager();
		}
	}

	public void LoadMap(int chapterIndex, int dungeonIndex, Action onComplete, Action<string> onMessage = null)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		CampaignChapterData campaignChapterData = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (campaignChapterData == null)
		{
			campaignChapterData = GameRoot.Instance.CampaignChapterDataContainer.GetFirstData();
			if (campaignChapterData == null)
			{
				VespaDebug.LogError("[LoadMap] Chapter data is null. chapterIndex=" + chapterIndex);
				Vespa_System_Action_Extension_Method.fire(onComplete);
				return;
			}
			if (chapterIndex != 0)
			{
				VespaDebug.LogError("[LoadMap] Reset chapterindex from " + chapterIndex + " to " + campaignChapterData.Index);
			}
			chapterIndex = campaignChapterData.Index;
		}
		if (dungeonIndex == 0)
		{
			dungeonIndex = ((campaignChapterData.StartNodeIndex == 0) ? 1 : campaignChapterData.StartNodeIndex);
		}
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			data = GameRoot.Instance.CampaignNodeDataContainer.GetFirstData();
			if (data == null)
			{
				VespaDebug.LogError("[LoadMap] Dungeon data is null. chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
				Vespa_System_Action_Extension_Method.fire(onComplete);
				return;
			}
			if (dungeonIndex != 0)
			{
				VespaDebug.LogError("[LoadMap] Reset dungeonindex from " + dungeonIndex + " to " + data.DungeonIndex);
			}
			dungeonIndex = data.DungeonIndex;
		}
		Vespa_System_Action_Extension_Method.fire(onMessage, "OnLoadMap");
		NGame2.NAccount.UserCampaignManager.ChapterIndex = chapterIndex;
		NGame2.NAccount.UserCampaignManager.DungeonIndex = dungeonIndex;
		_mediator.OnLoadMap();
		EnterChapter triggerEvent = new EnterChapter
		{
			ChapterIndex = chapterIndex,
			IsEnabledAutoNext = AutoNextBattle.IsEnabled()
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		WorldMapData worldMapData = SingletonAuto<Container>.instance.Get(campaignChapterData.Map);
		if (worldMapData != null)
		{
			_mediator.MapComponent.ScaleData = worldMapData.ScaleData;
		}
		_mediator.MapWidth = campaignChapterData.MapSizeX;
		_mediator.MapHeight = campaignChapterData.MapSizeY;
		_mediator.Viewer.SetSize(campaignChapterData.MapSizeX, campaignChapterData.MapSizeY);
		SingletonAuto<BgmManager2>.instance.Request(campaignChapterData.BgmCode, isEvent: false);
		Vespa_System_Action_Extension_Method.fire(onMessage, "_nodeGraph.Build");
		_nodeGraph.Build(campaignChapterData, onMessage);
		Vespa_System_Action_Extension_Method.fire(onMessage, "Managers OnLoadMap");
		_eventManager.OnLoadMap();
		_crowdManager.OnLoadMap();
		_propManager.OnLoadMap();
		_buildingManager.OnLoadMap();
		_pcAvatarManager.OnLoadMap();
		Vespa_System_Action_Extension_Method.fire(onMessage, "Avatar OnLoadMap");
		SetUserAvatar();
		InitChapterFlowManager(chapterIndex);
		_mediator.Avatar.OnLoadMap(NGame2.NAccount.UserCampaignManager.ChapterIndex, NGame2.NAccount.UserCampaignManager.DungeonIndex);
		_mediator.AvatarController.OnLoadMap();
		CreateMainPetAvatar();
		Vespa_System_Action_Extension_Method.fire(onMessage, "_imageLoader.Load");
		_imageLoader.Load(campaignChapterData, onComplete);
		CheckRequireResource(chapterIndex);
	}

	private void SetUserAvatar()
	{
		bool isForcedAvatar;
		int chapterAvatarIndex = GetChapterAvatarIndex(out isForcedAvatar);
		ChangePlayerAvatar(_mediator.Avatar, chapterAvatarIndex, isForcedAvatar ? UserManager.UserStatType.SubStoryAvatarHeroIndex : UserManager.UserStatType.AvatarHeroIndex);
	}

	private void CreateMainPetAvatar()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.PetManager != null)
		{
			int mainPetAvatarIndex = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MainPetAvatarIndex;
			_crowdManager.CreateMainPetAvatar(GetMainPetAvatarInfo(mainPetAvatarIndex));
		}
	}

	private void ChangeMainPetAvatar()
	{
		if (_crowdManager != null)
		{
			int mainPetAvatarIndex = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MainPetAvatarIndex;
			if (mainPetAvatarIndex == 0)
			{
				_crowdManager.RemoveAvatarPet();
			}
			else
			{
				_crowdManager.ChangeMainPetAvatar(GetMainPetAvatarInfo(mainPetAvatarIndex));
			}
		}
	}

	private CrowdManager.AvatarInfo GetMainPetAvatarInfo(int petAvatarIndex)
	{
		return new CrowdManager.AvatarInfo
		{
			CreatureIndex = petAvatarIndex,
			ChapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex,
			DungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex
		};
	}

	public void MovePlayerToDungeon(int chapterIndex, int dungeonIndex, bool warp)
	{
		if (_chapterFlowManager != null && _chapterFlowManager.CheckMoveStartNode(chapterIndex, dungeonIndex))
		{
			WorldMapManagement instance = SceneAutoBehaviour<WorldMapManagement>.instance;
			if (instance == null || !instance.IsPossibleMove)
			{
				return;
			}
			if (instance.IsPossibleMove)
			{
				instance.IsPossibleMove = false;
			}
		}
		MapNode nodeByDungeonIndex = _nodeGraph.GetNodeByDungeonIndex(dungeonIndex);
		if (!(nodeByDungeonIndex == null) && nodeByDungeonIndex.CanVisit)
		{
			if (warp)
			{
				_mediator.AvatarController.WarpTo(dungeonIndex);
			}
			else
			{
				_mediator.AvatarController.OnTouchNode(nodeByDungeonIndex, userInput: false);
			}
		}
	}

	public void Show(bool show)
	{
		_show = show;
		IsAvatarRootTouched = true;
		UIHelper.EnableGameObject(NodeRoot, show);
		UIHelper.EnableGameObject(EventRoot, show);
		UIHelper.EnableGameObject(AvatarRoot, show);
		UIHelper.EnableGameObject(IndicatorRoot, show);
		UIHelper.EnableGameObject(BuildingRoot, show);
	}

	public void RefreshMonthlyHeroRewardableNew()
	{
		if (_propManager != null)
		{
			_propManager.SetMonthlyHeroRewardableNew();
		}
	}

	public void RefreshEventDungeonCurrencyCheck()
	{
		if (_propManager != null)
		{
			_propManager.SetEventDungeonCurrencyChecknew();
		}
	}

	public void ShopRestockEffectPlay(Dictionary<int, bool> playList)
	{
		if (_propManager != null)
		{
			_propManager.ShopRestockEffectPlay(playList);
		}
	}

	private void CheckRequireResource(int chapterIndex)
	{
		if (PatchDownloader.CurrentVersionPatchIsAllDownloaded || !GameRoot.isValidInstance)
		{
			return;
		}
		EDownloadCategory[] downloadCategories = GameRoot.Instance.DownloadControlDataContainer.GetDownloadCategories(chapterIndex);
		if (downloadCategories == null || downloadCategories.Length == 0)
		{
			return;
		}
		bool flag = false;
		EDownloadCategory[] array = downloadCategories;
		foreach (EDownloadCategory item in array)
		{
			if (PatchDownloader.DownloadedCategory.Add(item))
			{
				flag = true;
			}
		}
		if (flag && !PatchDownloader.IsBackgorundDownloading() && GlobalAutoBehaviour<PatchDownloader>.isValidInstance)
		{
			PatchDownloader instance = GlobalAutoBehaviour<PatchDownloader>.instance;
			if (instance != null)
			{
				instance.StartBackgroundDownload(downloadCategories);
			}
		}
	}

	void ChapterFlowManager.IListener.ChangeStatus(ChapterFlowManager.StatusType type)
	{
		switch (type)
		{
		case ChapterFlowManager.StatusType.ChangeToMyAvatar:
			SetUserAvatar();
			break;
		case ChapterFlowManager.StatusType.EndAllLine:
			CloseChapterFlowManager();
			break;
		}
	}

	void IObserver.OnEvent(FocusEvent changedInfo)
	{
	}

	void IObserver.OnTouch(bool isPress)
	{
		if (_crowdManager != null)
		{
			_crowdManager.Touch(isPress);
		}
	}

	void IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>.OnChanged(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo != null)
		{
			switch (changedInfo.EChangedPetInfoType)
			{
			case NGame2.NAccount.UserPetManager.EChangedPetInfoType.CreateMainAvatarPet:
				CreateMainPetAvatar();
				break;
			case NGame2.NAccount.UserPetManager.EChangedPetInfoType.ChangeMainAvatarPet:
				ChangeMainPetAvatar();
				break;
			}
		}
	}
}
