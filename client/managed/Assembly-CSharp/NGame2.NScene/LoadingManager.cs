using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LitJson;
using MiniJSON;
using NDrawCall;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NContents;
using NGame2.NEventTrigger;
using NGame2.NFunctionality;
using NGame2.NMatch;
using NGame2.NNodeTree;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NRender3D.NCinema;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUI.NWindow.NLoading;
using NGame2.NUI.NWindow.NTutorial;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NPatch;
using NGameAsset.NSound;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NInternalUsage;
using NVespa.NPrefab2;
using NVespa.NRenderProxy;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NGame2.NScene;

public class LoadingManager : GlobalAutoBehaviour<LoadingManager>, LoadingDefault.ILoadingStatus
{
	public enum ESceneType
	{
		None,
		Logo,
		Login,
		Lobby,
		Battle2,
		Quit,
		GoogleInAppPurchase,
		AppleInAppPurchase,
		OneStoreInAppPurchase,
		UDPInAppPurchase,
		UnityInAppPurchase,
		SamsungInAppPurchase,
		Empty
	}

	public enum ELoadingType
	{
		None,
		Default,
		Black
	}

	public enum EMoveSceneType
	{
		StartGame,
		StartBattle,
		ReturnToLobby,
		Etc
	}

	public interface ILastSceneData
	{
	}

	public static LocalStorageBool ManualLoading = new LocalStorageBool(i_default: false, typeof(LoadingManager).FullName + "ManualLoading");

	public static int LoadingLock;

	private bool _testBattleRun;

	private string _loadingMsg = string.Empty;

	private float _loadingAmount;

	private List<VespaCoroutine.RuntimeData> _runtimeList = new List<VespaCoroutine.RuntimeData>();

	private ESceneType _prevSceneType;

	public static int LoadSceneCount { get; private set; }

	public static bool ApplicationQuit { get; private set; }

	public ESceneType CurrentSceneType { get; private set; }

	public bool IsSceneLoading { get; private set; }

	public ILastSceneData LastSceneData { get; set; }

	string LoadingDefault.ILoadingStatus.Label => _loadingMsg;

	float LoadingDefault.ILoadingStatus.FillAmount => _loadingAmount;

	protected override void Awake()
	{
		base.Awake();
		SceneManager.sceneLoaded += OnSceneLoaded;
		UnityEngine.Debug.Log("Assign OnSceneLoaded");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SceneManager.sceneLoaded -= OnSceneLoaded;
		UnityEngine.Debug.Log("Delete OnSceneLoaded");
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
	}

	public void Quit()
	{
		SprkTestPrefs.Save();
		MoveScene(ESceneType.Quit, ELoadingType.Black, delegate
		{
			MobileQuit();
		});
	}

	public static void MobileQuit()
	{
		ProcessKill();
	}

	public static void ProcessKill()
	{
		SprkTestPrefs.Save();
		bool flag = false;
		try
		{
			ProcessThreadCollection threads = Process.GetCurrentProcess().Threads;
			if (threads != null)
			{
				for (int i = 0; i < threads.Count; i++)
				{
					threads[i].Dispose();
				}
			}
		}
		catch (Exception i_object)
		{
			VespaDebug.LogError("Stop Thread Fail: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			flag = true;
		}
		try
		{
			Process.GetCurrentProcess().Kill();
		}
		catch (Exception i_object2)
		{
			flag = true;
			VespaDebug.LogError("Kill CurrentProcess Fail: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object2));
		}
		if (flag)
		{
			ApplicationQuit = true;
			VespaDebug.LogError("UnityEngine.Application.Quit()");
			Application.Quit();
		}
	}

	public void MoveScene(ESceneType sceneType, Action onLoad = null, ILastSceneData lastSceneData = null)
	{
		MoveScene(sceneType, ELoadingType.Default, resetEnvironment: true, onLoad, lastSceneData);
	}

	public void MoveScene(ESceneType sceneType, ELoadingType loadingType, Action onLoad = null, ILastSceneData lastSceneData = null)
	{
		MoveScene(sceneType, loadingType, resetEnvironment: true, onLoad, lastSceneData);
	}

	public void MoveScene(ESceneType sceneType, ELoadingType loadingType, bool resetEnvironment, Action onLoad = null, ILastSceneData lastSceneData = null)
	{
		StopAllVespaCoroutine();
		ResetLoadingLog();
		BaseScene baseScene = UnityEngine.Object.FindObjectOfType(typeof(BaseScene)) as BaseScene;
		if (baseScene != null)
		{
			baseScene.OnSceneUnloading(sceneType);
			baseScene = null;
		}
		IsSceneLoading = true;
		_prevSceneType = CurrentSceneType;
		CurrentSceneType = sceneType;
		LastSceneData = lastSceneData;
		StartVespaCoroutine(coMoveScene(sceneType, loadingType, resetEnvironment, onLoad));
	}

	private void StopAllVespaCoroutine()
	{
		for (int i = 0; i < _runtimeList.Count; i++)
		{
			VespaCoroutine.RuntimeData runtimeData = _runtimeList[i];
			if (runtimeData != null && !runtimeData.IsEnd)
			{
				VespaCoroutine.Stop(runtimeData);
			}
		}
		_runtimeList.Clear();
		_runtimeList = new List<VespaCoroutine.RuntimeData>();
	}

	private Coroutine StartVespaCoroutine(IEnumerator moveSceneCo)
	{
		VespaCoroutine.RuntimeData runtimeData = VespaCoroutine.StartRuntimeGlobal(moveSceneCo, "(loading)");
		if (runtimeData == null)
		{
			return null;
		}
		_runtimeList.Add(runtimeData);
		return runtimeData.ExecuteCo;
	}

	private IEnumerator coMoveScene(ESceneType sceneType, ELoadingType loadingType, bool resetEnvironment, Action onLoad)
	{
		UnityLogQueue.Enqueue_IAP();
		UnityLogQueue.Enqueue_IAP();
		UnityLogQueue.Enqueue_IAP();
		UnityLogQueue.Enqueue_IAP();
		UnityLogQueue.Enqueue_IAP_Seperator();
		UnityLogQueue.Enqueue_IAP("==  화면 전환 == ", "/Users/masang/build/kr/sandbox_patch/r1/Program/GameClient/unity_project/Assets/[2.Game]-2/Scene/LoadingManager.cs", 211);
		UnityLogQueue.Enqueue_IAP_Seperator();
		_ = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		_ = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		_ = SingletonAuto<CampaignContext>.instance.EnterDungeon.IsScenario;
		SingletonAuto<BgmManager2>.instance.PendingRequest = true;
		GameOptionData.ApplySleepMode();
		DebugLog("[LoadingManager] Loading FadeIn");
		_loadingAmount = 0f;
		yield return null;
		EMoveSceneType MoveSceneType = GetMoveSceneType();
		_ = loadingType;
		switch (loadingType)
		{
		case ELoadingType.Default:
		{
			bool openComplete2 = false;
			LoadingDefault.instance.OpenFadeIn(this, MoveSceneType, delegate
			{
				openComplete2 = true;
			});
			while (!openComplete2)
			{
				yield return null;
			}
			_loadingMsg = "Scene Loading";
			break;
		}
		case ELoadingType.Black:
		{
			bool openComplete = false;
			LoadingBlack.instance.OpenFadeIn(delegate
			{
				openComplete = true;
			});
			while (!openComplete)
			{
				yield return null;
			}
			break;
		}
		default:
			UnityEngine.Debug.LogWarning("Not Implemented Yet: " + Vespa_String_Extension_Method.GetEnumString(loadingType));
			break;
		}
		if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
		{
			DebugLog("[LoadingManager] ClearAllOpenedWindowStack");
			GlobalAutoBehaviour<GlobalManagement>.instance.ClearAllOpenedWindowStack();
			yield return null;
		}
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			DebugLog("[LoadingManager] CreatureManager.instance.ReleaseAllCreatures");
			BattleViewGenericSingleton<CreatureManager>.instance.ReleaseAllCreatures();
		}
		if (SceneAutoBehaviour<RuntimeGlobalManager>.isValidInstance)
		{
			DebugLog("[LoadingManager] NNodeTree.RuntimeGlobalManager.instance.ClearAll");
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.ClearAll();
		}
		yield return null;
		SingletonUpdater<AsyncManager>.instance.ForceSyncLoad = true;
		DateTime startTime = DateTime.UtcNow;
		DebugLog("[LoadingManager] coLoadScene(sceneType, resetEnvironment)");
		yield return StartVespaCoroutine(coLoadScene(sceneType, resetEnvironment));
		int num = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
		DebugLog("[LoadingManager] Scene loaded : " + EnumTools.GetName(sceneType) + " elapsedMs=" + num);
		BaseScene baseScene = UnityEngine.Object.FindObjectOfType(typeof(BaseScene)) as BaseScene;
		if (baseScene != null)
		{
			DebugLog("[LoadingManager] baseScene.PreloadAsset()");
			BaseScene.IPreload preload = baseScene.PreloadAsset();
			if (preload != null)
			{
				_loadingMsg = LocalizationManager.GetString(Loading.LOADING_MSG);
				while (!preload.IsEnd)
				{
					_loadingAmount = Mathf.Max(_loadingAmount, 0.4f + preload.Progress * 0.6f);
					preload.Tick();
					yield return null;
				}
				_loadingAmount = 1f;
				preload.OnEnd();
			}
			DebugLog("[LoadingManager] OnEvent(NConfig.ClientDebugEvent.EEventType.OnSceneLoaded)");
			yield return null;
			bool sceneLoadedComplete = false;
			baseScene.OnSceneLoaded(delegate
			{
				sceneLoadedComplete = true;
			});
			while (!sceneLoadedComplete)
			{
				yield return null;
			}
		}
		bool flag = GlobalAutoBehaviour<PatchDownloader>.isValidInstance && GlobalAutoBehaviour<PatchDownloader>.instance.IsPatchDownloading();
		if (MoveSceneType == EMoveSceneType.StartBattle)
		{
			if (flag)
			{
				GlobalAutoBehaviour<PatchDownloader>.instance.Pause();
			}
		}
		else if (flag)
		{
			GlobalAutoBehaviour<PatchDownloader>.instance.Resume();
		}
		yield return null;
		DebugLog("[LoadingManager] WaitOnCloseLoadingWindow begin");
		while (GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.WaitOnCloseLoadingWindow, value: true))
		{
			yield return null;
		}
		DebugLog("[LoadingManager] WaitOnCloseLoadingWindow end");
		DebugLog("[LoadingManager] Loading FadeOut");
		SingletonUpdater<AsyncManager>.instance.ForceSyncLoad = false;
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		switch (loadingType)
		{
		case ELoadingType.Default:
		{
			bool fadeOutComplete2 = false;
			LoadingDefault.instance.CloseFadeOut(delegate
			{
				fadeOutComplete2 = true;
			}, releaseSelf: true);
			while (!fadeOutComplete2)
			{
				yield return null;
			}
			_loadingMsg = string.Empty;
			_loadingAmount = 0f;
			break;
		}
		case ELoadingType.Black:
		{
			bool fadeOutComplete = false;
			LoadingBlack.instance.CloseFadeOut(delegate
			{
				fadeOutComplete = true;
			}, releaseSelf: true);
			while (!fadeOutComplete)
			{
				yield return null;
			}
			break;
		}
		default:
			UnityEngine.Debug.LogWarning("Not Implemented Yet: " + Vespa_String_Extension_Method.GetEnumString(loadingType));
			break;
		case ELoadingType.None:
			break;
		}
		IsSceneLoading = false;
		DebugLog("[LoadingManager] onLoad()");
		onLoad?.Invoke();
		StopAllVespaCoroutine();
	}

	public static IEnumerator coResetBeforeLoading()
	{
		SingletonAuto<VespaCoroutine>.instance.BeforeSceneChange();
		yield return null;
	}

	public static IEnumerator coResetEnvironment()
	{
		if (Define.PatchDownloaded)
		{
			while (!SingletonAuto<BundleManager>.instance.IsReady)
			{
				DebugLog("[LoadingManager] Wait BundlePack Load");
				yield return null;
			}
		}
		if (SingletonAuto<BattleFacade>.isValidInstance)
		{
			SingletonAuto<BattleFacade>.instance.Reset();
		}
		SingletonUpdater<ThreadManager>.CreateInstance();
		SnapshotObjectPoolContainer.Reset();
		GameRoot.ResetClientEnvironment();
		EnumTools.ResetEnum();
		SingletonAuto<ResolutionInfo>.instance.ResetResolution();
		GlobalTimer.Reset();
		MainTimer.Reset();
		UnityTimeScaler.Reset();
		GlobalAutoBehaviour<GlobalCameraManager>.instance.AdjustCameraRenderAndClearFlags();
		SingletonUpdater<WebServiceRequester>.instance.OnSceneChanged();
		NVespa.NTask.TaskManager.OnSceneChanged();
		SingletonUpdater<AudioManager>.instance.OnSceneChanged();
		SingletonAuto<EventTriggerManager>.instance.OnSceneChanged();
		SingletonAuto<AsyncSceneContainer>.instance.OnSceneChanged();
		SingletonAuto<BgmManager2>.instance.OnSceneChanged();
		CameraStat.ResetCameraStock();
		SingletonUpdater<DuelManager>.instance.OnSceneChanged();
		ComponentFactory.OnSceneChanged();
		SingletonAuto<LayerManager>.instance.OnSceneChanged();
		CinemaOwnerContainer.OnSceneChanged();
		HeroPreview.OnSceneChanged();
		ItemPreview.OnSceneChanged();
		ScreenUIEffect.OnSceneChanged();
		ScreenEffect.OnSceneChanged();
		GlobalMessagePopup.OnSceneChanged();
		MessagePopup3Button.OnSceneChanged();
		GlobalCenterMessage.OnSceneChanged();
		TutorialWindow.OnSceneChanged();
		QuitGamePopup.OnSceneChanged();
		EventWebViewPopup.OnSceneChanged();
		OfficialChannelWebView.OnSceneChanged();
		HeroStatWebViewPopup.OnSceneChanged();
		OqupieWebViewPopup.OnSceneChanged();
		DebugWebViewPopup.OnSceneChanged();
		ChattingPreview.OnSceneChanged();
		NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OnSceneChanged();
		SingletonAuto<PackContainer>.instance.OnSceneChanged();
		ReservePoolUtil.ClearUnusedAll();
		SingletonAuto<VespaCoroutine>.instance.OnSceneChanged();
		SingletonAuto<LitJson.JsonPool>.instance.OnSceneChanged();
		MiniJSON.JsonPool.instance.OnSceneChanged();
		List<WeakRef<IUserObserverManager>> managerList = UserObserverManagerContainer.ManagerList;
		for (int i = 0; i < managerList.Count; i++)
		{
			IUserObserverManager value = managerList[i].Value;
			if (value == null)
			{
				managerList.RemoveAt(i);
				i--;
			}
			else
			{
				value.ClearObserver();
			}
		}
		UICamera.OnSceneChanged();
		UIDrawCall.ReleaseInactive();
		SingletonAuto<MaterialFactory>.instance.OnSceneChanged();
		MeshIndexBuffer.IndexPool.Clear();
		yield return null;
		yield return null;
		if (GlobalUIRoot.isValidInstance)
		{
			GlobalUIRoot.instance.OnSceneChanged();
		}
		if (GlobalAutoBehaviour<GlobalCameraManager>.isValidInstance)
		{
			GlobalAutoBehaviour<GlobalCameraManager>.instance.OnSceneChanged();
		}
		yield return null;
		yield return null;
		yield return null;
		if (SingletonAuto<Container>.isValidInstance)
		{
			SingletonAuto<Container>.instance.DestroyOnSceneChanged();
		}
		if (SingletonAuto<ContainerGo>.isValidInstance)
		{
			SingletonAuto<ContainerGo>.instance.DestroyOnSceneChanged();
		}
		if (SingletonAuto<ContainerRes>.isValidInstance)
		{
			SingletonAuto<ContainerRes>.instance.DestroyOnSceneChanged();
		}
		if (SingletonUpdater<AsyncLoader>.isValidInstance)
		{
			SingletonUpdater<AsyncLoader>.instance.DestroyOnSceneChanged();
		}
		SingletonAuto<PrefabAutoContainer>.instance.OnSceneChanged();
		yield return null;
		yield return null;
		if (SingletonAuto<CreatureContainer>.isValidInstance)
		{
			SingletonAuto<CreatureContainer>.instance.OnSceneLoading();
		}
		if (SingletonUpdater<AssetUpdateManager>.isValidInstance)
		{
			SingletonUpdater<AssetUpdateManager>.instance.DestroyOnSceneChanged();
		}
		if (SingletonAuto<NguiAtlasContainer>.isValidInstance)
		{
			NguiAtlasContainer.DestroyUnusedAll();
		}
		if (SingletonAuto<AudioClipContainer>.isValidInstance)
		{
			SingletonAuto<AudioClipContainer>.instance.DestroyUnusedAll();
		}
		if (SingletonAuto<BuilderContainer>.isValidInstance)
		{
			SingletonAuto<BuilderContainer>.instance.DestroyOnSceneChanged();
		}
		yield return null;
		yield return null;
		SingletonAuto<MaterialPoolingFactory2>.instance.OnSceneChanged();
		SingletonAuto<LinkedTransformContainer>.instance.OnSceneChanged();
		SingletonAuto<SmallPoolContainer>.instance.OnSceneChanged();
		SingletonAuto<ArrayCacheContainer>.instance.OnSceneChanged();
		SingletonUpdater<GCManager>.instance.GlobalClear();
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return SingletonUpdater<GCManager>.instance.Collect(GCManager.ECollectType.Deep);
		yield return null;
		yield return null;
		SingletonAuto<ComponentFactory>.CreateInstance();
		bool resetAll = false;
		if (GlobalAutoBehaviour<LoadingManager>.isValidInstance)
		{
			resetAll = GlobalAutoBehaviour<LoadingManager>.instance.GetMoveSceneType() == EMoveSceneType.StartGame;
		}
		ReservePoolUtil.ReserveAll(resetAll);
		if (SingletonAuto<BuilderContainer>.isValidInstance)
		{
			SingletonAuto<BuilderContainer>.instance.ReserveOnSceneChanged();
		}
		yield return null;
	}

	private IEnumerator coLoadScene(ESceneType sceneType, bool resetEnvironment)
	{
		if (resetEnvironment)
		{
			for (int index = 0; index < 3; index++)
			{
				yield return null;
			}
			DebugLog("[LoadingManager] Start: coResetBeforeLoading");
			yield return StartVespaCoroutine(coResetBeforeLoading());
			yield return null;
			DebugLog("[LoadingManager] End: coResetBeforeLoading");
		}
		DebugLog("[LoadingManager] Start: Load LoadingScene");
		yield return null;
		LoadSceneCount++;
		LoadingLock = 1;
		yield return SceneManager.LoadSceneAsync("Loading");
		LoadingLock = 0;
		DebugLog("[LoadingManager] End: Load LoadingScene");
		_loadingAmount = 0.05f;
		yield return null;
		if (resetEnvironment)
		{
			for (int index = 0; index < 10; index++)
			{
				yield return null;
			}
			DebugLog("[LoadingManager] Start: ResetEnvironment");
			yield return StartVespaCoroutine(coResetEnvironment());
			yield return null;
			DebugLog("[LoadingManager] End: ResetEnvironment");
		}
		_loadingAmount = 0.2f;
		yield return null;
		if (ManualLoading.Value)
		{
			bool loadingNext = false;
			NGame2.NUI.NWindow.NLoading.ManualLoading.instance.Open(delegate
			{
				loadingNext = true;
			});
			while (!loadingNext)
			{
				yield return null;
			}
			NGame2.NUI.NWindow.NLoading.ManualLoading.ReleaseInstance();
		}
		string sceneName = EnumTools.GetName(sceneType);
		_ = DateTime.UtcNow;
		DebugLog("[LoadingManager] Start: Load " + sceneName);
		LoadSceneCount++;
		LoadingLock = 1;
		if (SingletonAuto<ContentsValueManager>.instance.GetContentsBoolValue("LoadSceneAsync", defaultValue: true))
		{
			AsyncOperation loadAsyncOperation = SceneManager.LoadSceneAsync(sceneName);
			while (!loadAsyncOperation.isDone)
			{
				_loadingAmount = 0.2f + loadAsyncOperation.progress * 0.2f;
				yield return null;
			}
		}
		else
		{
			SceneManager.LoadScene(sceneName);
		}
		LoadingLock = 0;
		DebugLog("[LoadingManager] End: Load " + sceneName);
		yield return null;
		_loadingAmount = 0.4f;
	}

	private EMoveSceneType GetMoveSceneType()
	{
		if ((_prevSceneType == ESceneType.Login || _prevSceneType == ESceneType.None) && CurrentSceneType == ESceneType.Lobby)
		{
			return EMoveSceneType.StartGame;
		}
		if (_prevSceneType == ESceneType.Lobby && CurrentSceneType == ESceneType.Battle2)
		{
			return EMoveSceneType.StartBattle;
		}
		if (_prevSceneType == ESceneType.Battle2 && CurrentSceneType == ESceneType.Lobby)
		{
			return EMoveSceneType.ReturnToLobby;
		}
		if (_prevSceneType == ESceneType.Battle2 && CurrentSceneType == ESceneType.Battle2)
		{
			return EMoveSceneType.StartBattle;
		}
		return EMoveSceneType.Etc;
	}

	public void LoadingTestSceneOn()
	{
		if (CurrentSceneType == ESceneType.Lobby)
		{
			StopAllVespaCoroutine();
			ESceneType eSceneType = ESceneType.Lobby;
			BaseScene baseScene = UnityEngine.Object.FindObjectOfType(typeof(BaseScene)) as BaseScene;
			if (baseScene != null)
			{
				baseScene.OnSceneUnloading(eSceneType);
				baseScene = null;
			}
			IsSceneLoading = true;
			_prevSceneType = CurrentSceneType;
			CurrentSceneType = eSceneType;
			LoadingData firstData = GameRoot.Instance.LoadingDataContainer.GetFirstData();
			if (firstData != null)
			{
				StartVespaCoroutine(coLoadingTestSceneOn(firstData.Index));
			}
		}
	}

	public void LoadingTest(int loadingIndex)
	{
		LoadingDefault.instance.Close();
		bool openComplete = false;
		LoadingDefault.instance.OpenQA(loadingIndex, this, delegate
		{
			openComplete = true;
		});
	}

	public void LoadingTestSceneOff()
	{
		StartVespaCoroutine(coLoadingTestSceneOff());
	}

	private IEnumerator coLoadingTestSceneOn(int loadingIndex)
	{
		SingletonAuto<BgmManager2>.instance.PendingRequest = true;
		GameOptionData.ApplySleepMode();
		_loadingAmount = 0f;
		yield return null;
		bool openComplete = false;
		LoadingDefault.instance.OpenQA(loadingIndex, this, delegate
		{
			openComplete = true;
		});
		while (!openComplete)
		{
			yield return null;
		}
		_loadingMsg = "@QA 로딩 이미지 테스트용";
		if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
		{
			UnityEngine.Debug.Log("[LoadingManager] ClearAllOpenedWindowStack");
			GlobalAutoBehaviour<GlobalManagement>.instance.ClearAllOpenedWindowStack();
			yield return null;
		}
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			UnityEngine.Debug.Log("[LoadingManager] CreatureManager.instance.ReleaseAllCreatures");
			BattleViewGenericSingleton<CreatureManager>.instance.ReleaseAllCreatures();
		}
		if (SceneAutoBehaviour<RuntimeGlobalManager>.isValidInstance)
		{
			UnityEngine.Debug.Log("[LoadingManager] NNodeTree.RuntimeGlobalManager.instance.ClearAll");
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.ClearAll();
		}
	}

	private IEnumerator coLoadingTestSceneOff()
	{
		ESceneType sceneType = ESceneType.Lobby;
		bool resetEnvironment = true;
		SingletonUpdater<AsyncManager>.instance.ForceSyncLoad = true;
		DateTime startTime = DateTime.UtcNow;
		UnityEngine.Debug.Log("[LoadingManager] coLoadScene(sceneType, resetEnvironment)");
		yield return StartVespaCoroutine(coLoadScene(sceneType, resetEnvironment));
		int num = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
		UnityEngine.Debug.Log("[LoadingManager] Scene loaded : " + EnumTools.GetName(sceneType) + " elapsedMs=" + num);
		BaseScene baseScene = UnityEngine.Object.FindObjectOfType(typeof(BaseScene)) as BaseScene;
		if (baseScene != null)
		{
			UnityEngine.Debug.Log("[LoadingManager] baseScene.PreloadAsset()");
			BaseScene.IPreload preload = baseScene.PreloadAsset();
			if (preload != null)
			{
				_loadingMsg = LocalizationManager.GetString(Loading.LOADING_MSG);
				while (!preload.IsEnd)
				{
					_loadingAmount = Mathf.Max(_loadingAmount, 0.4f + preload.Progress * 0.6f);
					preload.Tick();
					yield return null;
				}
				_loadingAmount = 1f;
				preload.OnEnd();
			}
			UnityEngine.Debug.Log("[LoadingManager] OnEvent(NConfig.ClientDebugEvent.EEventType.OnSceneLoaded)");
			yield return null;
			bool sceneLoadedComplete = false;
			baseScene.OnSceneLoaded(delegate
			{
				sceneLoadedComplete = true;
			});
			while (!sceneLoadedComplete)
			{
				yield return null;
			}
		}
		yield return null;
		yield return null;
		UnityEngine.Debug.Log("[LoadingManager] WaitOnCloseLoadingWindow begin");
		while (GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.WaitOnCloseLoadingWindow, value: true))
		{
			yield return null;
		}
		UnityEngine.Debug.Log("[LoadingManager] WaitOnCloseLoadingWindow end");
		UnityEngine.Debug.Log("[LoadingManager] Loading FadeOut");
		SingletonUpdater<AsyncManager>.instance.ForceSyncLoad = false;
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		bool fadeOutComplete = false;
		LoadingDefault.instance.CloseFadeOut(delegate
		{
			fadeOutComplete = true;
		}, releaseSelf: true);
		while (!fadeOutComplete)
		{
			yield return null;
		}
		_loadingMsg = string.Empty;
		_loadingAmount = 0f;
		IsSceneLoading = false;
		StopAllVespaCoroutine();
	}

	private static void DebugLog(string log)
	{
		UnityEngine.Debug.Log(log);
	}

	private static void ResetLoadingLog()
	{
	}

	public static bool IsPreloadFlag(string key)
	{
		return true;
	}
}
