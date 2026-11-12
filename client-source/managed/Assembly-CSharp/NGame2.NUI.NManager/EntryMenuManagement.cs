using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NRender3D.NCinema;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NEntryMenu;
using NGame2.NUI.NWorldMap;
using NGameAsset;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class EntryMenuManagement : BaseManagement<EntryMenuManagement>, NGame2.NUI.NWindow.EntryMenu.Listener, SubEntryMenu.Listener, IInspectorGuiDrawer
{
	public enum OpenType
	{
		None,
		Normal,
		UnNodeData
	}

	private CampaignNodeData _nodeData;

	private ISound _npcSound;

	public bool IsLoadingEnterEntryMenu;

	private bool _entryMenuInit;

	private OpenType _openType;

	private readonly UserObserverManager<OpenType> _observerManager = new UserObserverManager<OpenType>();

	private CinemaOwner _cinemaOwner;

	public int? ForceShopGroupIndex;

	public string EntryNPCCode { get; private set; }

	public CampaignNodeData PrevNodeData { get; private set; }

	public bool IsEntryMenuInit => _entryMenuInit;

	public OpenType EntryMenuOpenType => _openType;

	public IUserObserverManager ObserverManager => _observerManager;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<NodeTreeEntry>("Resources/Prefab2/Window/EntryMenu/NodeTreeMenu");
		AddInstance<NGame2.NUI.NWindow.EntryMenu>("Resources/Prefab2/Window/EntryMenu/EntryMenu");
		AddInstance<MenuTitle>("Resources/Prefab2/Window/EntryMenu/MenuTitle", GameUIRoot.ELayerType.BottomFixedUI);
		AddInstance<SubEntryMenu>("Resources/Prefab2/Window/EntryMenu/SubEntryMenu");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		StopNPCSound();
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		if (_npcSound != null)
		{
			_npcSound.Update();
		}
		if (_cinemaOwner != null)
		{
			_cinemaOwner.ChainUpdate();
		}
	}

	private void EnterOpenMenu(Action onComplete = null)
	{
		_entryMenuInit = false;
		IsLoadingEnterEntryMenu = true;
		onComplete = (Action)Delegate.Combine(onComplete, (Action)delegate
		{
			IsLoadingEnterEntryMenu = false;
		});
	}

	public void OpenEntryMenu(int ChapterIndex, int DungeonIndex, bool skipAni, bool playNPCSound, bool isIgnoreTrigger, Action onComplete)
	{
		EnterOpenMenu(onComplete);
		_openType = OpenType.Normal;
		EnterEntryMenu enterEntryMenu = new EnterEntryMenu();
		enterEntryMenu.ChapterIndex = ChapterIndex;
		enterEntryMenu.DungeonIndex = DungeonIndex;
		enterEntryMenu.SkipTrigger = AutoNextBattle.IsEnabled() || AutoRepeatBattle.IsEnabled();
		if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(enterEntryMenu))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else
		{
			OpenEntryMenuReal(ChapterIndex, DungeonIndex, skipAni, playNPCSound, isIgnoreTrigger, onComplete);
		}
	}

	public void OpenEntryMenuUnNode(string EntryNPCCode, bool skipAni, bool playNPCSound, bool isIgnoreTrigger, Action onComplete)
	{
		SetPrevNodeData();
		EnterOpenMenu(onComplete);
		_openType = OpenType.UnNodeData;
		_nodeData = null;
		this.EntryNPCCode = EntryNPCCode;
		Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask().ADDTASK(OpenEntryMenuAsync(skipAni, playNPCSound, isIgnoreTrigger)), onComplete).WrapperTouchControl().DisableTouch()
			.Commit();
	}

	public void OpenEntryMenuReal(int ChapterIndex, int DungeonIndex, bool skipAni, bool playNPCSound, bool isIgnoreTrigger, Action onComplete)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data != null)
		{
			_nodeData = data;
			EntryNPCCode = _nodeData.EntryNPCCode;
			Vespa_Task_ExtensionMethod.ADDTASK(Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask().ADDTASK(OpenEntryMenuAsync(skipAni, playNPCSound, isIgnoreTrigger)), onComplete), delegate
			{
				EntryMenuLoadCompleted triggerEvent = new EntryMenuLoadCompleted
				{
					ChapterIndex = ChapterIndex,
					DungeonIndex = DungeonIndex
				};
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
			}).WrapperTouchControl().DisableTouch()
				.Commit();
		}
	}

	public void OpenEntryMenuReal_Task(int ChapterIndex, int DungeonIndex, bool skipAni, bool playNPCSound, bool isIgnoreTrigger, ITask onCompleteTask)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data != null)
		{
			_nodeData = data;
			EntryNPCCode = _nodeData.EntryNPCCode;
			Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask().ADDTASK(OpenEntryMenuAsync(skipAni, playNPCSound, isIgnoreTrigger)).ADDTASK(onCompleteTask), delegate
			{
				EntryMenuLoadCompleted triggerEvent = new EntryMenuLoadCompleted
				{
					ChapterIndex = ChapterIndex,
					DungeonIndex = DungeonIndex
				};
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
			}).WrapperTouchControl().DisableTouch()
				.Commit();
		}
	}

	public override void CloseAllActiveWindow(bool immediately, IEnumerable<IWindow> exceptions = null)
	{
		base.CloseAllActiveWindow(immediately, exceptions);
		Vespa_Task_ExtensionMethod.NAME(Vespa_Task_ExtensionMethod.ADDTASK(new ParallelTask().ADDTASK(SceneAutoBehaviour<WorldMapManagement>.instance.TaskFactory.Run((MapLayout window) => window.CloseEntryMenuAsync())), delegate
		{
			StopNPCSound();
		}).WrapperTouchControl().DisableTouch(), "EntryMenuManagement.CloseAll").Commit();
	}

	private void TriggerEntryMenuEnterEvent(int chapterIndex, int dungeonIndex)
	{
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new EnterEntryMenu
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex
		});
	}

	public void ExecuteEntryMenuButtonAction(Type actionType, bool ExecuteButtonAction = true)
	{
		if (!(actionType == null) && IsOpened<NGame2.NUI.NWindow.EntryMenu>())
		{
			if (ExecuteButtonAction)
			{
				GetInstance<NGame2.NUI.NWindow.EntryMenu>().ExecButtonAction(actionType);
			}
			else
			{
				SingletonAuto<ActionContainer>.instance.GetAction(actionType)?.Run(null);
			}
		}
	}

	private ITask OpenEntryMenuAsync(bool skipAni, bool playNPCSound, bool isIgnoreTrigger)
	{
		return Vespa_Task_ExtensionMethod.NAME(new DelegateTask(delegate
		{
			if (String_Extension_Method.IsNullOrEmpty(EntryNPCCode))
			{
				return OpenEntryMenuWindowAsync(playNPCSound, isIgnoreTrigger);
			}
			EntryMenuSettingData appearance = GameRoot.Instance.EntryMenuSettingDataContainer.GetDataByMenuType(EntryNPCCode);
			if (appearance == null)
			{
				return OpenEntryMenuWindowAsync(playNPCSound, isIgnoreTrigger);
			}
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (mapLayout == null)
			{
				return OpenEntryMenuWindowAsync(playNPCSound, isIgnoreTrigger);
			}
			if (appearance.UseNodeTree)
			{
				EntryDataProvider provider = new EntryDataProvider(appearance, _nodeData);
				_cinemaOwner = SingletonAuto<CinemaOwnerContainer>.instance.GetCinemaOwner(ECinemaOwnerType.NodeTreeEntry);
				_cinemaOwner.Open(provider);
				_cinemaOwner.SetAction(null, delegate
				{
					SingletonAuto<ActionContainer>.instance.GetAction(appearance.LobbyWindowName)?.Run(_nodeData, null);
				});
				return new SequenceTask().ADDTASK(new WaitTask(() => !_cinemaOwner.IsCompleted)).ADDTASK(OpenNodeTreeEntry(mapLayout.OpenNodeTreeEntryMenu(appearance, skipAni), base.TaskFactory.Open(delegate(NodeTreeEntry window)
				{
					window.Init(_cinemaOwner, provider);
				})));
			}
			return new SequenceTask().ADDTASK(mapLayout.OpenEntryMenuAsync(appearance, skipAni)).ADDTASK(OpenEntryMenuWindowAsync(playNPCSound, isIgnoreTrigger));
		}), "OpenEntryMenuAsync");
	}

	private ITask OpenNodeTreeEntry(ITask mapLayoutTask, ITask nodeTreeEntryOpenTask)
	{
		return new ParallelTask().ADDTASK(mapLayoutTask).ADDTASK(nodeTreeEntryOpenTask);
	}

	private ITask OpenEntryMenuWindowAsync(bool playNPCSound, bool isIgnoreTrigger)
	{
		if (_nodeData != null)
		{
			return new ParallelTask().ADDTASK(base.TaskFactory.Open(delegate(MenuTitle window)
			{
				window.Init(EntryNPCCode);
			})).ADDTASK(base.TaskFactory.Open(delegate(NGame2.NUI.NWindow.EntryMenu window)
			{
				window.Init(this, _nodeData, playNPCSound, isIgnoreTrigger, delegate
				{
					_entryMenuInit = true;
				});
			}));
		}
		return new ParallelTask().ADDTASK(base.TaskFactory.Open(delegate(MenuTitle window)
		{
			window.Init(EntryNPCCode);
		})).ADDTASK(base.TaskFactory.Open(delegate(NGame2.NUI.NWindow.EntryMenu window)
		{
			window.Init(this, EntryNPCCode, playNPCSound, isIgnoreTrigger, delegate
			{
				_entryMenuInit = true;
			});
		}));
	}

	public void SetNPCAction(string[] ActionValues, bool playsound = true)
	{
		GetActionValues(ActionValues, out var speech, out var sound, out var motion);
		SetNPCAction(speech, playsound ? sound : null, motion);
	}

	public void SetNPCAction(string speech, string sound = null, string motion = null)
	{
		if (String_Extension_Method.IsNullOrEmpty(speech))
		{
			return;
		}
		if (IsOpened<NGame2.NUI.NWindow.EntryMenu>())
		{
			GetInstance<NGame2.NUI.NWindow.EntryMenu>().SetSpeech(speech);
		}
		if (!String_Extension_Method.IsNullOrEmpty(sound))
		{
			StopNPCSound();
			SoundStock stock = new SoundStock
			{
				Code = sound,
				LocalTimer = GlobalTimer.DefaultLocal
			};
			_npcSound = SingletonAuto<SoundContainer>.instance.CreateSound(stock);
		}
		if (!String_Extension_Method.IsNullOrEmpty(motion))
		{
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (mapLayout != null && mapLayout.NodeBackground != null)
			{
				mapLayout.NodeBackground.MotionNpc(motion);
			}
		}
	}

	public void SetSpeech(string speech)
	{
		if (IsOpened<NGame2.NUI.NWindow.EntryMenu>())
		{
			GetInstance<NGame2.NUI.NWindow.EntryMenu>().SetSpeech(speech);
		}
	}

	public void CloseNodeTreeEntryMenu(Action onComplete)
	{
		_observerManager.AnnounceToObservers(_openType);
		if (GetInstance<NodeTreeEntry>().IsOpen())
		{
			Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask().ADDTASK(Vespa_Task_ExtensionMethod.ADDTASK(new ParallelTask().ADDTASK(base.TaskFactory.Close<NodeTreeEntry>()).ADDTASK(SceneAutoBehaviour<WorldMapManagement>.instance.TaskFactory.Run((MapLayout window) => window.CloseEntryMenuAsync())), delegate
			{
				StopNPCSound();
			})), delegate
			{
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}).WrapperTouchControl().DisableTouch()
				.Commit();
		}
		else
		{
			Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask().ADDTASK(Vespa_Task_ExtensionMethod.ADDTASK(new ParallelTask().ADDTASK(SceneAutoBehaviour<WorldMapManagement>.instance.TaskFactory.Run((MapLayout window) => window.CloseEntryMenuAsync())), delegate
			{
				StopNPCSound();
			})), delegate
			{
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}).WrapperTouchControl().DisableTouch()
				.Commit();
		}
		SceneAutoBehaviour<LobbyManagement>.instance.EnableLobbyChapterReward();
		SceneAutoBehaviour<LobbyManagement>.instance.EnableLobbyShakmehBossGauge();
	}

	public void CloseEntryMenu(Action onComplete)
	{
		_observerManager.AnnounceToObservers(_openType);
		Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask().ADDTASK(Vespa_Task_ExtensionMethod.ADDTASK(new ParallelTask().ADDTASK(base.TaskFactory.Close<MenuTitle>()).ADDTASK(base.TaskFactory.Close<NGame2.NUI.NWindow.EntryMenu>()).ADDTASK(SceneAutoBehaviour<WorldMapManagement>.instance.TaskFactory.Run((MapLayout window) => window.CloseEntryMenuAsync())), delegate
		{
			StopNPCSound();
		})), delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}).WrapperTouchControl().DisableTouch()
			.Commit();
		SceneAutoBehaviour<LobbyManagement>.instance.EnableLobbyChapterReward();
		SceneAutoBehaviour<LobbyManagement>.instance.EnableLobbyShakmehBossGauge();
	}

	bool NGame2.NUI.NWindow.EntryMenu.Listener.OnCloseRequest()
	{
		CloseEntryMenu(null);
		return true;
	}

	void NGame2.NUI.NWindow.EntryMenu.Listener.SetHideEntryMenuTitle(bool Hide)
	{
		MenuTitle menuTitle = GetInstance<MenuTitle>();
		if (!(menuTitle == null))
		{
			if (Hide)
			{
				menuTitle.Close();
			}
			else
			{
				OpenEntryMenuWindowAsync(playNPCSound: false, isIgnoreTrigger: false).Commit();
			}
		}
	}

	void NGame2.NUI.NWindow.EntryMenu.Listener.ExecNPCAction(string[] actionValues, bool playSound)
	{
		SetNPCAction(actionValues, playSound);
		SceneAutoBehaviour<ShopManagement>.instance.OnNPCSpeak();
	}

	void SubEntryMenu.Listener.OnCloseSubMenu()
	{
	}

	public static void GetActionValues(string[] actionValues, out string speech, out string sound, out string motion)
	{
		speech = string.Empty;
		sound = string.Empty;
		motion = string.Empty;
		if (actionValues != null && actionValues.Length >= 3 && actionValues.Length % 3 == 0)
		{
			int maxExclusive = actionValues.Length / 3;
			int num = UnityEngine.Random.Range(0, maxExclusive);
			speech = LocalizationManager.GetStringDirect(actionValues[num * 3]);
			sound = actionValues[num * 3 + 1];
			motion = actionValues[num * 3 + 2];
		}
	}

	private void StopNPCSound()
	{
		if (_npcSound != null)
		{
			SoundContainer.ReleaseSound(_npcSound);
			_npcSound = null;
		}
	}

	public void OpenSubEntryMenu(NShared.EntryMenu menu)
	{
		string textKey = ArgParser.GetValueString(menu.EntryMenuButton, 0);
		new SequenceTask().ADDTASK(base.TaskFactory.Run(delegate(SubEntryMenu window)
		{
			window.title.Key = textKey;
		})).ADDTASK(base.TaskFactory.Open(delegate(SubEntryMenu window)
		{
			window.Init(this, _nodeData, menu.Index);
		})).WrapperTouchControl()
			.DisableTouch()
			.Commit();
	}

	public void SetPrevNodeData()
	{
		if (_nodeData != null)
		{
			PrevNodeData = _nodeData;
		}
	}

	public void ClearPrevNodeData()
	{
		PrevNodeData = null;
		_openType = OpenType.None;
	}
}
