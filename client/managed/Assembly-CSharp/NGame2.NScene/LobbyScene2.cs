using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NCutScene;
using NGame2.NEventTrigger;
using NGame2.NLobby;
using NGame2.NLogin;
using NGame2.NMatch;
using NGame2.NRoomMessageLog;
using NGame2.NScene.NLobbyScenePrerequisites;
using NGame2.NScene.NPreloader;
using NGame2.NSocket;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUtil;
using NShared;
using NShared.Login;
using NShared.NEvent;
using NShared.NRoomMessage;
using NShared.NRoomMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NNGUIExtension;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NSocket;
using UnityEngine;

namespace NGame2.NScene;

public class LobbyScene2 : BaseScene, IInspectorGuiDrawer
{
	public enum LobbyStateType
	{
		WorldMap
	}

	[Flags]
	public enum ReturnStateOpenFlag
	{
		None = 0,
		OpenPartySetting = 1,
		OpenHeroDeck = 2,
		OpenNextDungeon = 4,
		OpenInventory = 8,
		OpenCashShop = 0x10,
		OpenNewbieShop = 0x40,
		OpenGameGuide = 0x80,
		OpenNextChapterConquestDungeon = 0x100,
		OpenNextChapterHideOut = 0x200,
		OpenEventShop = 0x400
	}

	public class ReturnStateInfo
	{
		public ReturnStateOpenFlag DisabledFlag;

		public ReturnStateOpenFlag OpenFlag;
	}

	public interface IReturnState
	{
		string BgmCode { get; }

		void Execute(ReturnStateInfo info, Action onEnd);
	}

	public static List<IReturnState> ListReturnState = new List<IReturnState>();

	public static ReturnStateInfo ReturnState = new ReturnStateInfo();

	public static LobbyScene2 instance;

	public static List<int> lobbyNoticeIndexList = new List<int>();

	public bool ShowDebugMenu = true;

	public LocalStorageString CutSceneName = new LocalStorageString(typeof(LobbyScene2).FullName + "CutSceneName");

	public LocalStorageString HttpVideoName = new LocalStorageString(typeof(LobbyScene2).FullName + "HttpVideoName");

	public LocalStorageString LocalVideoName = new LocalStorageString(typeof(LobbyScene2).FullName + "LocalVideoName");

	private LobbyScenePreloader _preload;

	private string _abusedBattleChapter;

	private string _abusedBattleDungeon;

	private string _abusedBattleDifficulty;

	private string _abusedBattleLog;

	private string _abusedAffixIndices;

	private string _couponCode;

	private float idleCounter;

	private void Awake()
	{
		instance = this;
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void Start()
	{
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("LobbyScene", this, DrawGui);
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.UserManager == null)
		{
			GlobalAutoBehaviour<NVespa.NGlobal.GuiManager>.instance.FoldoutGui = false;
		}
	}

	public override IPreload PreloadAsset()
	{
		_preload = new LobbyScenePreloader();
		_preload.Begin();
		bool completePrerequisites = false;
		_preload.Preloader.AddAction(delegate
		{
			LobbyScenePrerequisites.Request(delegate
			{
				completePrerequisites = true;
			});
		}, "coRequestLobbyScenePrerequisites");
		_preload.Preloader.AddCompleteFunc(() => completePrerequisites, "completePrerequisites");
		return _preload;
	}

	public override void OnSceneUnloading(LoadingManager.ESceneType nextScene)
	{
		if (_preload != null)
		{
			_preload.ReleaseAsyncLoadAll();
			_preload = null;
		}
	}

	public override void OnSceneLoaded(Action onComplete)
	{
		VespaCoroutine.Start(this, coOnSceneLoaded(onComplete));
	}

	private IEnumerator coOnSceneLoaded(Action onComplete)
	{
		SceneAutoBehaviour<WorldMapManagement>.instance.ShowChapterIntroWindow();
		EnterLobby enterLobbyEvent = new EnterLobby();
		if (MainStoryManagement.IsPlayMainStoryMode)
		{
			enterLobbyEvent = null;
		}
		if (!SingletonAuto<EventTriggerManager>.instance.CanRaiseEvent(enterLobbyEvent))
		{
			if (ListReturnState.Count > 0)
			{
				IReturnState[] returnStates = ListReturnState.ToArray();
				ListReturnState.Clear();
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(returnStates); i++)
				{
					IReturnState returnState = returnStates[i];
					bool processed = false;
					float startWaitTime = Time.time;
					returnState.Execute(ReturnState, delegate
					{
						processed = true;
					});
					while (!processed)
					{
						yield return null;
						if (Time.time - startWaitTime > 5f)
						{
							VespaDebug.LogError("Too Long Wait return state end. Type=" + returnState.GetType().Name);
							break;
						}
					}
				}
			}
		}
		else
		{
			GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.ReleaseFocus();
		}
		ListReturnState.Clear();
		ReturnState = new ReturnStateInfo();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(enterLobbyEvent, delegate
		{
			SceneAutoBehaviour<LobbyPopupQueue>.instance.Open();
			SceneAutoBehaviour<AchievementClearedPopupQueue>.instance.Open();
		});
		base.OnSceneLoaded(onComplete);
	}

	private void DrawGui()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		using (new BlockManager().Vertical())
		{
			using (new BlockManager().Horizontal())
			{
				UIScrollGrid.ForceEnableOutside.Value = GUILayout.Toggle(UIScrollGrid.ForceEnableOutside.Value, "UIScrollGrid: Force Enable Outside");
			}
			GUILayout.TextField($"ServerName : {GlobalAutoBehaviour<AccountManager>.instance.CountryServerName}");
			if (GUILayout.Button(ShowDebugMenu ? "Hide" : "Show"))
			{
				ShowDebugMenu = !ShowDebugMenu;
			}
			if (!ShowDebugMenu)
			{
				return;
			}
			if (GUILayout.Button("Login : " + (GlobalAutoBehaviour<AccountManager>.instance.UserManager?.Id ?? 0)))
			{
				SingletonUpdater<LoginManager>.instance.Relogin(delegate(ResultType result)
				{
					if (result == ResultType.Success)
					{
						GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
					}
				});
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label($"Current LoginID : {GlobalAutoBehaviour<AccountManager>.instance.LoginId}");
			}
			GUILayout.Button("TEST");
			if (GUILayout.Button("ArenaDebug=" + MatchManager.ShowDebugLabel))
			{
				MatchManager.ShowDebugLabel = !MatchManager.ShowDebugLabel;
			}
			using (new BlockManager().Horizontal())
			{
				if (GUILayout.Button("RoomServer:" + GlobalAutoBehaviour<SocketServiceManager>.instance.RoomConnector.State))
				{
					if (GlobalAutoBehaviour<SocketServiceManager>.instance.RoomConnector.State == ConnectState.Connected)
					{
						GlobalAutoBehaviour<SocketServiceManager>.instance.RoomConnector.LeaveRoom();
					}
					else if (GlobalAutoBehaviour<SocketServiceManager>.instance.RoomConnector.State == ConnectState.Disconnected)
					{
						string hostAddress = SingletonUpdater<WebServiceManager>.instance.Requester.GetHostAddress();
						int port = 3003;
						int num = -1;
						RoomSocketConnector roomConnector = GlobalAutoBehaviour<SocketServiceManager>.instance.RoomConnector;
						roomConnector.EnterRoom(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId, hostAddress, port, num);
						roomConnector.OnRoomServerConnected = delegate
						{
						};
						roomConnector.OnRoomServerDisconnected = delegate
						{
						};
						roomConnector.SocketHandler.Register<RoomMessageNot>(SingletonAuto<RoomMessageLogHandler>.instance.Handle);
					}
				}
				if (GUILayout.Button("Chat") && GlobalAutoBehaviour<SocketServiceManager>.instance.RoomConnector.State == ConnectState.Connected)
				{
					GlobalAutoBehaviour<SocketServiceManager>.instance.RoomConnector.SendRoomMessageNot(new RoomChat
					{
						SenderName = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick,
						SenderAvatarIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex,
						SenderTeamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.TeamLevel,
						SendTime = Vespa_DateTime_Extension_Method.ToNetworkString2(DateTime.UtcNow),
						Chat = "TESTCHAT"
					});
				}
			}
			using (new BlockManager().Horizontal())
			{
				_abusedBattleChapter = GUILayout.TextField(string.IsNullOrEmpty(_abusedBattleChapter) ? "" : _abusedBattleChapter, GUI.skin.box, GUILayout.Width(50f));
				_abusedBattleDungeon = GUILayout.TextField(string.IsNullOrEmpty(_abusedBattleDungeon) ? "" : _abusedBattleDungeon, GUI.skin.box, GUILayout.Width(50f));
				_abusedBattleDifficulty = GUILayout.TextField(string.IsNullOrEmpty(_abusedBattleDifficulty) ? "" : _abusedBattleDifficulty, GUI.skin.box, GUILayout.Width(50f));
				_abusedAffixIndices = GUILayout.TextField(string.IsNullOrEmpty(_abusedAffixIndices) ? "" : _abusedAffixIndices, GUI.skin.box, GUILayout.Width(50f));
				_abusedBattleLog = GUILayout.TextField(string.IsNullOrEmpty(_abusedBattleLog) ? "" : _abusedBattleLog, GUI.skin.box, GUILayout.Width(200f));
				if (GUILayout.Button("ReplayAbuse"))
				{
					int chapterIndex = Vespa_String_Extension_Method.TOINT(_abusedBattleChapter, 1);
					int dungeonIndex = Vespa_String_Extension_Method.TOINT(_abusedBattleDungeon, 1);
					DungeonDifficultyType difficulty = (DungeonDifficultyType)Vespa_String_Extension_Method.TOINT(_abusedBattleDifficulty, 1);
					List<BaseBattleLog> list = StringToBattleLogList(_abusedBattleLog);
					SingletonAuto<BattleContext>.instance.SetCampaignReplay(chapterIndex, dungeonIndex, difficulty, list.ToArray());
					SingletonAuto<BattleContext>.instance.MoveToBattleScene();
				}
				if (GUILayout.Button("IntegrityCheck"))
				{
					BattleInstanceBuilder obj = BattleInstanceIntegrityChecker.CreateBattleInstanceBuilderForCheck(Vespa_String_Extension_Method.TOINT(_abusedBattleChapter, 1), Vespa_String_Extension_Method.TOINT(_abusedBattleDungeon, 1), extraData: new BattleInstanceExtraData
					{
						AffixIndices = StringToIntArray(_abusedAffixIndices)
					}, difficulty: Vespa_String_Extension_Method.TOINT(_abusedBattleDifficulty, 1));
					if (obj == null)
					{
						VespaDebug.LogError("BattleInstanceBuilderError");
					}
					List<BaseBattleLog> battleLogList = StringToBattleLogList(_abusedBattleLog);
					float realtimeSinceStartup = Time.realtimeSinceStartup;
					BattleInstance battleInstance = null;
					bool timeout = false;
					IEnumerator enumerator = BattleInstanceIntegrityChecker.coRun(obj, battleLogList, int.MaxValue, delegate(BattleInstance b, bool t)
					{
						battleInstance = b;
						timeout = t;
					}, 150000);
					while (enumerator.MoveNext())
					{
					}
					VespaDebug.LogError("elapsed time : " + (Time.realtimeSinceStartup - realtimeSinceStartup));
					if (battleInstance == null)
					{
						VespaDebug.LogError("NullBattleInstance");
					}
				}
			}
			if (GUILayout.Button("Load EmptyScene"))
			{
				GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Empty);
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.TextField("CutScene");
				CutSceneName.Value = GUILayout.TextField(CutSceneName.Value, GUI.skin.box, GUILayout.Width(200f));
			}
			using (new BlockManager().Horizontal())
			{
				if (GUILayout.Button("Start"))
				{
					SceneAutoBehaviour<CutSceneManager>.instance.Open(String_Extension_Method.CombinePath("CutScene", CutSceneName.Value), canSkip: true, null);
				}
				if (GUILayout.Button("Stop"))
				{
					SceneAutoBehaviour<CutSceneManager>.instance.Close(stopImmediate: false);
				}
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label($"Current Video Spec : {SystemOptionData.VideoSpec.ToString()}");
			}
			using (new BlockManager().Horizontal())
			{
				HttpVideoName.Value = GUILayout.TextField(HttpVideoName.Value, GUI.skin.box, GUILayout.Width(200f));
				if (GUILayout.Button("Play Streaming Video"))
				{
					SceneAutoBehaviour<LobbyManagement>.instance.OpenURLVideo(HttpVideoName.Value);
				}
			}
			using (new BlockManager().Horizontal())
			{
				LocalVideoName.Value = GUILayout.TextField(LocalVideoName.Value, GUI.skin.box, GUILayout.Width(200f));
				if (GUILayout.Button("Play Local Video (Video/FileName)"))
				{
					SceneAutoBehaviour<LobbyManagement>.instance.OpenVideo(LocalVideoName.Value);
				}
			}
			if (GUILayout.Button("Stop Video"))
			{
				SceneAutoBehaviour<LobbyManagement>.instance.StopVideo();
			}
			if (GUILayout.Button("CutScene Log"))
			{
				if (SceneAutoBehaviour<CutSceneManager>.isValidInstance)
				{
					BackgroundController backgroundControl = SceneAutoBehaviour<CutSceneManager>.instance.BackgroundControl;
					VespaDebug.LogError(string.Format("{0}", (backgroundControl.Window == null) ? "null" : backgroundControl.Window.GetDebug()));
				}
				else
				{
					VespaDebug.LogError("CutScene Unvalid.");
				}
			}
		}
	}

	private List<BaseBattleLog> StringToBattleLogList(string battleLogStr)
	{
		List<BaseBattleLog> list = ReplayUtil.ParseBattleLogString(battleLogStr, JsonMarshaler.Parse<BaseBattleLog>, decompress: true);
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			list = ReplayUtil.ParseBattleLogString(WWW.UnEscapeURL(battleLogStr), JsonMarshaler.Parse<BaseBattleLog>, decompress: true);
		}
		return list;
	}

	private int[] StringToIntArray(string intArrayStr)
	{
		string[] array = intArrayStr.Split(',');
		if (__Extension_Method_ICollection__.IsNullOrEmpty(array))
		{
			return null;
		}
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			int result = 0;
			if (int.TryParse(array[i], out result))
			{
				array2[i] = result;
			}
		}
		return array2;
	}

	public override void Update()
	{
		base.Update();
		if (Input.GetMouseButtonDown(0))
		{
			idleCounter = 0f;
		}
		else
		{
			idleCounter += Time.deltaTime;
			if (idleCounter > GlobalAutoBehaviour<ProfileManager>.instance.SetUserIdleAfterSeconds)
			{
				idleCounter = 0f;
				if (GlobalAutoBehaviour<ProfileManager>.instance.HasLogItem())
				{
					GlobalAutoBehaviour<ProfileManager>.instance.SendLogToServer();
				}
			}
		}
		ChattingWindow chattingWindow = SceneAutoBehaviour<ChattingManagement>.instance.GetInstance<ChattingWindow>(openedOnly: true);
		if (chattingWindow != null && chattingWindow.IsOpen())
		{
			return;
		}
		if (RestoredPortal.MenuShortcutKeyUp(KeyCode.B))
		{
			SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
		}
		if (RestoredPortal.MenuShortcutKeyUp(KeyCode.H))
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
		}
		if (RestoredPortal.MenuShortcutKeyUp(KeyCode.P))
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop();
		}
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			using (new SystemBackButtonBlock())
			{
				ActivityManager.RequestStop();
				if (GlobalAutoBehaviour<GlobalManagement>.instance.ExecuteBack() || (PvpLoading.isValidInstance && PvpLoading.instance.IsOpen()))
				{
					return;
				}
				ShowQuitPopup();
			}
		}
		ServerChanger.CheckForwardedServer();
	}
}
