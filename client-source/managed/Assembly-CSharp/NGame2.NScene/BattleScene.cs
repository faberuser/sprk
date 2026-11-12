using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NBattleView.NUIOn3d2;
using NGame2.NCutScene;
using NGame2.NMessage.NFastHandler;
using NGame2.NScene.NPreloader;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NScene;

public class BattleScene : BaseScene, IInspectorGuiDrawer
{
	private static BattleScenePreloader _prevPreloader2;

	private static BattleScenePreloader _preloader2;

	public static LocalStorageBool QuickRestart = new LocalStorageBool(i_default: false, typeof(BattleScene).FullName + "QuickRestart");

	private static Dictionary<string, RootPreloader> CustomPreloader = new Dictionary<string, RootPreloader>();

	private DateTime BattleStartTime;

	private bool _showDebugMenu;

	private static int[] _ratioHPList = new int[6] { 25, 33, 50, 66, 75, 100 };

	private bool _showCampaignCache;

	public static bool UseCacheOnBattleLogic { get; set; }

	public static bool CheckBattleIntegrity { get; private set; }

	public override IPreload PreloadAsset()
	{
		if (_preloader2 != null)
		{
			_preloader2.ReleaseAsyncLoadAll();
			_preloader2 = null;
		}
		_preloader2 = new BattleScenePreloader();
		SingletonAuto<BattleContext>.instance.Preload(_preloader2);
		_preloader2.Begin();
		return _preloader2;
	}

	public override void OnSceneUnloading(LoadingManager.ESceneType nextScene)
	{
		if (_prevPreloader2 != null)
		{
			_prevPreloader2.ReleaseAsyncLoadAll();
			_prevPreloader2 = null;
		}
		if (_preloader2 != null)
		{
			if (nextScene == LoadingManager.ESceneType.Battle2)
			{
				_prevPreloader2 = _preloader2;
			}
			else
			{
				_preloader2.ReleaseAsyncLoadAll();
			}
			_preloader2 = null;
		}
		if (__Extension_Method_ICollection__.COUNT(CustomPreloader) <= 0)
		{
			return;
		}
		foreach (RootPreloader value in CustomPreloader.Values)
		{
			value.ReleaseAsyncLoadAll();
		}
		CustomPreloader.Clear();
	}

	public override void OnSceneLoaded(Action onComplete)
	{
		VespaCoroutine.Start(this, coStart(delegate
		{
			if (_prevPreloader2 != null)
			{
				_prevPreloader2.ReleaseAsyncLoadAll();
				_prevPreloader2 = null;
			}
			base.OnSceneLoaded(onComplete);
			BattleStartTime = DateTime.UtcNow;
		}));
	}

	private void Awake()
	{
		SnapshotWritableBlock.BeginCheckSnapshotWritable();
	}

	private void OnDestroy()
	{
		SnapshotWritableBlock.EndCheckSnapshotWritable();
	}

	private void Start()
	{
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("BattleScene", this, DrawGui);
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("BattleScenePreloader", this, delegate
		{
			if (_preloader2 != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				_preloader2.GetLoadingTime(stringBuilder);
				float num = GlobalAutoBehaviour<NVespa.NGlobal.GuiManager>.instance.DisplayWidth - 300f;
				float height = GlobalAutoBehaviour<NVespa.NGlobal.GuiManager>.instance.DisplayHeight - 200f;
				using (new BlockManager().Vertical(GUI.skin.box, GUILayout.Width(num), GUILayout.Height(height)).Scroll(this, "OneStoreIAPManager_Output", alwaysShowHorizontal: false, alwaysShowVertical: true))
				{
					GUILayout.Label(stringBuilder.ToString(), VespaGuiStyleBuilder_Extension_Method.Builder(GUI.skin.label).WordWrap().RichText()
						.Style, GUILayout.Width(num - 50f));
					}
				}
			});
			if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType != LoadingManager.ESceneType.Battle2)
			{
				VespaCoroutine.Start(this, coInstantStart());
			}
		}

		private IEnumerator coInstantStart()
		{
			SingletonAuto<TableDataManager>.instance.Load();
			while (!SingletonAuto<TableDataManager>.instance.IsLoaded())
			{
				yield return null;
			}
			GameSetting.Init();
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Battle2);
		}

		private IEnumerator coStart(Action onReadyBattle)
		{
			yield return null;
			SingletonAuto<TableDataManager>.instance.Load();
			while (!SingletonAuto<TableDataManager>.instance.IsLoaded())
			{
				yield return null;
			}
			SingletonUpdater<Executer>.instance.NextRun(InitBattleStep);
			yield return null;
			yield return null;
			while (_preloader2 != null && !Vespa_Common_Class_Extension_Method.Interface((IPreload)_preloader2).IsEnd)
			{
				yield return null;
			}
			SingletonAuto<BattleContext>.instance.StartContextInBattleScene(onReadyBattle);
			SceneAutoBehaviour<NGame2.NMessage.NFastHandler.Manager>.CreateInstance();
		}

		private static void InitBattleStep()
		{
			TypeIterator.InitSnapshotObjectFactory();
			GameSetting.Init();
			NGame2.NBattleView.NUIOn3d2.Manager.instance.ReserveCache();
		}

		public override void Update()
		{
			base.Update();
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				if (SingletonUpdater<TutorialManager>.instance.IsProgressing || (SceneAutoBehaviour<CutSceneManager>.isValidInstance && SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying))
				{
					return;
				}
				using (new SystemBackButtonBlock())
				{
					if (GlobalAutoBehaviour<GlobalManagement>.instance.ExecuteBack())
					{
						return;
					}
					if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
					{
						SceneAutoBehaviour<BattleManagement>.instance.OpenPausedMenu();
					}
				}
			}
			bool flag = false;
			bool flag2 = false;
			if (SceneAutoBehaviour<BattleManagement>.isValidInstance && SceneAutoBehaviour<BattleManagement>.instance.IsOpened<PausedMenu>())
			{
				double totalSeconds = (DateTime.UtcNow - SceneAutoBehaviour<BattleManagement>.instance.PausedTime).TotalSeconds;
				int num = 300;
				if (totalSeconds > (double)num)
				{
					flag = true;
				}
			}
			if (SingletonAuto<BattleFacade>.isValidInstance)
			{
				BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
				if (battleCache != null)
				{
					int num2 = Mathf.Clamp(battleCache.MaxBattleTime * 3, 300, 1200);
					if ((DateTime.UtcNow - BattleStartTime).TotalSeconds > (double)num2)
					{
						flag2 = true;
					}
				}
			}
			if (flag || flag2)
			{
				ServerChanger.CheckForwardedServer();
			}
		}

		public static void AddCustomPreload(string key, RootPreloader preloader)
		{
			if (CustomPreloader.ContainsKey(key))
			{
				RootPreloader rootPreloader = CustomPreloader[key];
				if (rootPreloader != null)
				{
					rootPreloader.ReleaseAsyncLoadAll();
					Resources.UnloadUnusedAssets();
				}
			}
			CustomPreloader[key] = preloader;
		}

		private void DrawGui()
		{
			using (new BlockManager().Vertical())
			{
				using (new BlockManager().Horizontal())
				{
					if (GUILayout.Button("Wave Cancel BattleLog", GUILayout.Width(150f)))
					{
						CampaignContext instance = SingletonAuto<CampaignContext>.instance;
						if (instance != null)
						{
							new BattleCancelNot().Reason = BattleCancelNot.ReasonType.WaveNotStart;
							instance.ForceBattleCancel();
						}
					}
					if (GUILayout.Button("Battle Cancel Packet", GUILayout.Width(150f)))
					{
						CampaignContext instance2 = SingletonAuto<CampaignContext>.instance;
						if (instance2 != null)
						{
							new BattleCancelNot().Reason = BattleCancelNot.ReasonType.UserLogOff;
							instance2.ForceBattleCancel();
						}
					}
				}
				GUILayout.Space(20f);
				using (new BlockManager().Horizontal())
				{
					SkillModifierContainer.UseCacheOnBattleLogic = GUILayout.Toggle(SkillModifierContainer.UseCacheOnBattleLogic, "Use Cache on Battle Logic");
					if (SkillModifierContainer.UseCacheOnBattleLogic)
					{
						SkillModifierContainer.CheckBattleIntegrity = GUILayout.Toggle(SkillModifierContainer.CheckBattleIntegrity, "Check Battle Cache Integrity");
					}
				}
			}
		}
	}
