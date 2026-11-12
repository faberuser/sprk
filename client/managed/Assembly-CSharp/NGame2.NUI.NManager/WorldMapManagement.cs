using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NTutorial;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWorldMap;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGame2.NUI.NWorldMap.NMapLayout.NEventManager;
using NGame2.NUtil;
using NShader;
using NShared;
using NShared.NEvent;
using NShared.ReceiveCompletedReward;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class WorldMapManagement : BaseManagement<WorldMapManagement>, MapLayout.Listener, IUserObserver<ItemInfo>, IUserObserver, ITouchDisabler
{
	[Flags]
	public enum AvatarHideFlag
	{
		None = 0,
		HidePlayer = 1,
		HideNpc = 2,
		HideAll = 3
	}

	public interface IAvatarHider
	{
		bool IsOpen();

		AvatarHideFlag GetAvatarHideFlag();
	}

	public static List<int> DisplayedChapterIndices = new List<int>();

	public static List<Tuple<string, float>> LoadingLogList = new List<Tuple<string, float>>();

	private List<WeakRef<IAvatarHider>> _hideAvatarWindowList = new List<WeakRef<IAvatarHider>>();

	private AvatarHideFlag _currentAvatarHideFlag;

	public bool IsMapLoading { get; private set; }

	public bool IsHidden { get; private set; }

	public bool IsPossibleMove { get; set; }

	bool ITouchDisabler.IsValid => true;

	protected override void Awake()
	{
		base.Awake();
		_showLobbyMenu = true;
		IsPossibleMove = false;
		AddInstance<FirstEnterChapter>("Resources/Prefab2/Lobby/FirstEnterChapter");
		AddInstance<MapLayout>("Prefab2/WorldMap/MapLayout", GameUIRoot.ELayerType.BottomUI, isPassiveWindow: true);
		AddInstance<ExcavationRewardPopup>("Resources/Prefab2/WorldMap/ExcavationRewardPopup");
		AddInstance<FilteredWorldMapBackground>("Resources/Prefab2/WorldMap/FilteredWorldMapBackground");
	}

	public void Open(Action onEnd)
	{
		VespaCoroutine.Start(this, coOpen(onEnd));
	}

	private IEnumerator coOpen(Action onEnd)
	{
		IsMapLoading = true;
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.ObserverManager.AddObserver(this);
		GetInstance<FilteredWorldMapBackground>().Open();
		MapLayout mapLayout = GetInstance<MapLayout>();
		mapLayout.Open();
		bool isComplete = false;
		mapLayout.InitAsync(this, delegate
		{
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
		SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyChapterReward();
		SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyShakmehBossGauge();
		IsMapLoading = false;
		IsHidden = false;
		GlobalAutoBehaviour<GlobalManagement>.instance.AddDepthReorderedCallback(OnWindowDepthReordered);
		Vespa_System_Action_Extension_Method.fire(onEnd);
		MapViewer mapViewer = mapLayout.MapViewer;
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("WorldMap", this, delegate
		{
			using (new BlockManager().Vertical(GUI.skin.box, GUILayout.Width(500f), GUILayout.Height(300f)).Scroll(this, "MessageLog", alwaysShowHorizontal: false, alwaysShowVertical: true))
			{
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("ZoomSpeed");
					mapViewer.ZoomSpeed = GUILayout.HorizontalSlider(mapViewer.ZoomSpeed, 1f, 5f);
				}
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(LoadingLogList); i++)
				{
					Tuple<string, float> tuple = LoadingLogList[i];
					float item = tuple.Item2;
					if (i < __Extension_Method_ICollection__.COUNT(LoadingLogList) - 1)
					{
						item = LoadingLogList[i + 1].Item2;
					}
					GUILayout.Label($"{tuple.Item1} {item - tuple.Item2}s");
				}
			}
		});
	}

	public override void CloseAllActiveWindow(bool immediately, IEnumerable<IWindow> exceptions)
	{
		CloseBut<MapLayout, FilteredWorldMapBackground>();
	}

	public override bool IsPersistent()
	{
		return true;
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		if (AccountManager.IsLogined())
		{
			UpdateWorldMapHiddenState();
			UpdateShowLobbyMenu();
			UpdateAvatarHider();
		}
	}

	private void UpdateShowLobbyMenu()
	{
		if (GetInstance<MapLayout>().IsEntryMenuVisible)
		{
			_showLobbyMenu = false;
		}
		else
		{
			_showLobbyMenu = true;
		}
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
			{
				GlobalAutoBehaviour<GlobalManagement>.instance.RemoveDepthReorderedCallback(OnWindowDepthReordered);
			}
			_hideAvatarWindowList.Clear();
		}
	}

	public void AddAvatarHider(IAvatarHider hiderWindow)
	{
		WeakRef<IAvatarHider> weakRef = new WeakRef<IAvatarHider>();
		weakRef.Value = hiderWindow;
		_hideAvatarWindowList.Add(weakRef);
	}

	private void UpdateAvatarHider()
	{
		AvatarHideFlag avatarHideFlag = AvatarHideFlag.None;
		for (int num = __Extension_Method_ICollection__.COUNT(_hideAvatarWindowList) - 1; num >= 0; num--)
		{
			IAvatarHider value = _hideAvatarWindowList[num].Value;
			if (value == null || !value.IsOpen())
			{
				_hideAvatarWindowList.RemoveAt(num);
			}
			else
			{
				avatarHideFlag |= value.GetAvatarHideFlag();
			}
		}
		MapLayout mapLayout = GetInstance<MapLayout>();
		if (avatarHideFlag != _currentAvatarHideFlag || (mapLayout != null && mapLayout.IsAvatarRootTouched))
		{
			_currentAvatarHideFlag = avatarHideFlag;
			mapLayout.IsAvatarRootTouched = false;
			UIHelper.EnableGameObject(mapLayout.AvatarRoot, (avatarHideFlag & AvatarHideFlag.HidePlayer) == 0);
			UIHelper.EnableGameObject(mapLayout.NodeBackground.NpcRenderFacade, (avatarHideFlag & AvatarHideFlag.HideNpc) == 0);
			UIHelper.EnableGameObject(mapLayout.NodeBackground.NpcRenderFacade2, (avatarHideFlag & AvatarHideFlag.HideNpc) == 0);
		}
	}

	public void ShowChapterIntroWindow()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		if (DisplayedChapterIndices.Contains(chapterIndex))
		{
			return;
		}
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null)
		{
			DisplayedChapterIndices.Add(chapterIndex);
			if (!SingletonUpdater<TutorialManager>.isValidInstance || (!SingletonUpdater<TutorialManager>.instance.IsProgressing && !SingletonUpdater<TutorialManager>.instance.HasNextTurorial))
			{
				string stringDirect = LocalizationManager.GetStringDirect(data.Name);
				GetInstance<FirstEnterChapter>().Open(stringDirect);
			}
		}
	}

	private void OnWindowDepthReordered()
	{
	}

	public void ReloadDungeonOfReqTutorial(int tutorialIndex)
	{
		MapLayout mapLayout = GetInstance<MapLayout>();
		IList<CampaignNodeData> allChapterData = GameRoot.Instance.CampaignNodeDataContainer.GetAllChapterData(NGame2.NAccount.UserCampaignManager.ChapterIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allChapterData); i++)
		{
			CampaignNodeData campaignNodeData = allChapterData[i];
			if (SingletonAuto<ConditionChecker>.instance.IsNodeAffectedByTutorial(campaignNodeData, tutorialIndex))
			{
				mapLayout.RecreateNode(campaignNodeData.DungeonIndex);
			}
		}
	}

	public void UpdateWorldMapHiddenState()
	{
		FilteredWorldMapBackground filteredWorldMapBackground = GetInstance<FilteredWorldMapBackground>();
		if (filteredWorldMapBackground == null || !filteredWorldMapBackground.IsOpen())
		{
			return;
		}
		bool flag = GlobalAutoBehaviour<GlobalManagement>.instance.HasHideWorldMapWindow();
		if (flag == IsHidden)
		{
			return;
		}
		IsHidden = flag;
		if (flag)
		{
			GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.ReleaseFocus();
			GetInstance<FilteredWorldMapBackground>().OpenFilter(new BlurFilter(), skipAnim: false, delegate
			{
				GetInstance<MapLayout>().Show(show: false);
			});
		}
		else
		{
			GetInstance<FilteredWorldMapBackground>().Close(isHide: true, skipAnimation: false, null);
			GetInstance<MapLayout>().Show(show: true);
		}
	}

	public void MoveMap(int chapterIndex, int dungeonIndex, Action onEndMapMove = null, Action onEndSceneChanger = null)
	{
		Vespa_Task_ExtensionMethod.ADDTASK(Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask(), coMoveMap(chapterIndex, dungeonIndex, onEndMapMove, onEndSceneChanger)), delegate
		{
			ShowChapterIntroWindow();
		}).Commit();
	}

	private IEnumerator coMoveMap(int chapterIndex, int dungeonIndex, Action onEndMapMove = null, Action onEndSceneChanger = null)
	{
		LoadingLogList.Clear();
		IsMapLoading = true;
		LoadingLogList.Add(Tuple.Create("SceneChange Create", Time.realtimeSinceStartup));
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(30f, GlobalTopmostWindow.ESceneChangeType.Cloud);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		LoadingLogList.Add(Tuple.Create("Unload", Time.realtimeSinceStartup));
		GetInstance<MapLayout>().MapViewer.ChasePlayer(smoothChase: false);
		GetInstance<MapLayout>().UnloadMap();
		yield return null;
		yield return null;
		LoadingLogList.Add(Tuple.Create("GC.Collect", Time.realtimeSinceStartup));
		GCManager.ECollectType collectType = ((UnityCache.MemorySpec != UnityCache.EMemorySpec.Low) ? GCManager.ECollectType.Lite : GCManager.ECollectType.Normal);
		bool completeGcCollect = false;
		SingletonUpdater<GCManager>.instance.Collect(collectType, delegate
		{
			completeGcCollect = true;
		});
		while (!completeGcCollect)
		{
			yield return null;
		}
		yield return null;
		yield return null;
		LoadingLogList.Add(Tuple.Create("LoadMap", Time.realtimeSinceStartup));
		MapLayout mapLayout = GetInstance<MapLayout>();
		bool completeExtraAction = false;
		mapLayout.ExtraActionBeforeChangeMap(chapterIndex, delegate
		{
			completeExtraAction = true;
		});
		while (!completeExtraAction)
		{
			yield return null;
		}
		bool isComplete = false;
		mapLayout.LoadMap(chapterIndex, dungeonIndex, delegate
		{
			isComplete = true;
		}, delegate(string msg)
		{
			LoadingLogList.Add(Tuple.Create(msg, Time.realtimeSinceStartup));
		});
		while (!isComplete)
		{
			yield return null;
		}
		LoadingLogList.Add(Tuple.Create("RaiseEvent", Time.realtimeSinceStartup));
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new ChapterMove
		{
			ChapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex
		});
		Vespa_System_Action_Extension_Method.fire(onEndMapMove);
		LoadingLogList.Add(Tuple.Create("SceneChange Close", Time.realtimeSinceStartup));
		sceneChanger.Close();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.StartEffect();
		while (GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.IsPlaying)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onEndSceneChanger);
		IsMapLoading = false;
		LoadingLogList.Add(Tuple.Create("end of Load", Time.realtimeSinceStartup));
	}

	public void MovePlayerToDungeon(int chapterIndex, int dungeonIndex, bool DisableTouch = true, bool warp = false, Action onMoveBegin = null)
	{
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapterIndex, dungeonIndex))
		{
			Vespa_System_Action_Extension_Method.fire(onMoveBegin);
			return;
		}
		bool MoveChapter = false;
		Action action = delegate
		{
			if (DisableTouch)
			{
				GlobalTopmostWindow.instance.AddTouchDisabler(this);
			}
			GetInstance<MapLayout>().MovePlayerToDungeon(chapterIndex, dungeonIndex, warp);
			if (MoveChapter)
			{
				if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
				{
					SceneAutoBehaviour<LobbyManagement>.instance.OnEndMoveMap();
				}
				ShowChapterIntroWindow();
			}
			Vespa_System_Action_Extension_Method.fire(onMoveBegin);
		};
		if (NGame2.NAccount.UserCampaignManager.ChapterIndex == chapterIndex)
		{
			Vespa_System_Action_Extension_Method.fire(action);
			return;
		}
		MoveChapter = true;
		VespaCoroutine.StartScene(coMoveMap(chapterIndex, dungeonIndex, action));
	}

	public void RemoveTouchDisable()
	{
		GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
	}

	public CampaignChapterData GetCampaignChapterData()
	{
		if (GameRoot.isValidInstance)
		{
			return GameRoot.Instance.CampaignChapterDataContainer.GetData(NGame2.NAccount.UserCampaignManager.ChapterIndex);
		}
		return null;
	}

	public void ProcessChapterFlow(int chapterIndex, int dungeonIndex, ChapterFlowManager.ConditionKeyType type)
	{
		MapLayout mapLayout = GetInstance<MapLayout>();
		if (!(mapLayout == null))
		{
			mapLayout.ProcessChapterFlow(chapterIndex, dungeonIndex, type);
		}
	}

	public void UpdateGuildAttendanceNewIndicator()
	{
		MapLayout mapLayout = GetInstance<MapLayout>();
		if (!(mapLayout == null) && mapLayout.PropManager != null)
		{
			mapLayout.PropManager.SetGuildAttendanceNewIndicator();
		}
	}

	public void UpdateEventWorldTreeIndicator()
	{
		MapLayout mapLayout = GetInstance<MapLayout>();
		if (!(mapLayout == null) && mapLayout.PropManager != null)
		{
			mapLayout.PropManager.SetEventWorldTreeIndicator();
		}
	}

	public void StartExcavation()
	{
		AddTask(base.TaskFactory.Run(delegate(MapLayout layout)
		{
			layout.EventManager.GetComponent<ExcavationManager>().StartExcavation(ShowExcavationRewardPopup);
		}));
	}

	private void ShowExcavationRewardPopup(NShared.ReceiveCompletedReward.Response response)
	{
		if (response != null)
		{
			AddTask(base.TaskFactory.Branch(base.TaskFactory.Run(() => response != null), base.TaskFactory.Run(delegate(ExcavationRewardPopup window)
			{
				BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
				popupOpenData.SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemInfos).SetCurrencyResultInfos3(response.CurrencyResults);
				window.OpenWithData(popupOpenData, isGlobal: false);
			})));
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
	}
}
