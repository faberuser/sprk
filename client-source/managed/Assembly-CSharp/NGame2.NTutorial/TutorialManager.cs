using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NConfig;
using NGame2.NCutScene;
using NGame2.NEventTrigger;
using NGame2.NLogin;
using NGame2.NScene;
using NGame2.NTutorial.NRuntime;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWindow.NTutorial;
using NGame2.NUI.NWorldMap;
using NGame2.NUtil;
using NGame2.NUtil.NModifyChecker;
using NShared;
using NShared.BeginTutorial;
using NShared.CompleteTutorial;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NTutorial;

public class TutorialManager : SingletonUpdater<TutorialManager>, TutorialWindow.Listener, IServiceDisabler
{
	public LocalStorageString TestTutorialIndex = new LocalStorageString(string.Empty, typeof(TutorialManager).FullName + "TestTutorialIndex");

	public LocalStorageBool TestTutorialLoop = new LocalStorageBool(i_default: false, typeof(TutorialManager).FullName + "TestTutorialLoop");

	public LocalStorageBool ShowCompleteTutorial = new LocalStorageBool(i_default: false, typeof(TutorialManager).FullName + "ShowCompleteTutorial");

	public const int NoTutorialIndex = 0;

	private Coroutine _currentTutorialCoroutine;

	private Coroutine _tutorialRuntimeCoroutine;

	private int _tutorialIndex;

	private Action _onComplete;

	private readonly TutorialRunningState _runningState = new TutorialRunningState();

	private SceneChanger _sceneChanger;

	private List<WeakRef<ITutorialListener>> _listenerList = new List<WeakRef<ITutorialListener>>();

	private TutorialData _reservedNextTutorialData;

	private string[] disableServiceName = new string[2] { "user/get_certificate", "user/login" };

	public TutorialData CurrentTutorialData { get; private set; }

	public TutorialData LastTutorialData { get; private set; }

	public ITutorialRuntime CurrentTutorialRuntime { get; private set; }

	public bool IsProgressing { get; private set; }

	public bool IsTutorialEnabled
	{
		get
		{
			if (SingletonAuto<ClientDebugTutorial>.instance.TutorialEnabled)
			{
				return RestoredPortal.RequiredBattleStoryEnabled(!GlobalAutoBehaviour<AccountManager>.instance.IsTutorialSkip);
			}
			return RestoredPortal.RequiredBattleStoryEnabled(enabled: false);
		}
	}

	public int NextTutorialIndex { get; set; }

	public bool HasNextTurorial => NextTutorialIndex != 0;

	public int SystemBackButtonSkip { get; set; }

	public TutorialManager()
	{
		NextTutorialIndex = 0;
		GlobalAutoBehaviour<GuiManager>.instance.Add("Tutorial", this, delegate
		{
			using (new BlockManager().Horizontal().Enable(!IsProgressing))
			{
				GUILayout.Label("Test Index");
				TestTutorialIndex.Value = GUILayout.TextField(TestTutorialIndex.Value);
				if (Vespa_String_Extension_Method.TOINT(TestTutorialIndex.Value, 0) > 0 && GUILayout.Button("Start"))
				{
					VespaCoroutine.Start(coLoopTestTutorial());
				}
				TestTutorialLoop.Value = GUILayout.Toggle(TestTutorialLoop.Value, "Loop");
			}
			using (new BlockManager().Enable(IsProgressing))
			{
				if (GUILayout.Button("Skip Tutorial", GUILayout.Height(30f)))
				{
					SkipTutorial();
				}
			}
			if (GUILayout.Button("Auto = " + _runningState.AutoType, GUILayout.Height(30f)))
			{
				Array values = Enum.GetValues(typeof(TutorialRunningState.TutorialAutoType));
				for (int i = 0; i < values.Length; i++)
				{
					if ((TutorialRunningState.TutorialAutoType)values.GetValue(i) == _runningState.AutoType)
					{
						_runningState.AutoType = (TutorialRunningState.TutorialAutoType)values.GetValue((i + 1) % values.Length);
						break;
					}
				}
			}
			UnityTimeScaler.EBaseTimeScale value = UnityTimeScaler.BaseTimeScale.Value;
			UnityTimeScaler.BaseTimeScale.Value = EnumTools.IngamePopup("Base TimeScale", UnityTimeScaler.BaseTimeScale.Value);
			if (value != UnityTimeScaler.BaseTimeScale.Value)
			{
				UnityTimeScaler.UpdateUnityScale();
			}
			GUILayout.Label("------------------");
			ShowCompleteTutorial.Value = GUILayout.Toggle(ShowCompleteTutorial.Value, "Show Complete");
			if (ShowCompleteTutorial.Value)
			{
				foreach (TutorialInfo item in GlobalAutoBehaviour<AccountManager>.instance.TutorialManager)
				{
					GUILayout.Label($"{item.TutorialIndex} Complete at {item.CompletedTime}");
				}
			}
			GUILayout.Label("------------------");
			if (GUILayout.Button("Open Ratingbar", GUILayout.Height(30f)))
			{
				AppReviewPopup.ForceOpen();
			}
			if (AppReviewPopup.ErrorReviewPopup != null)
			{
				GUILayout.Label($"Last Open Ratingbar Error: {AppReviewPopup.ErrorReviewPopup.Value}");
			}
		});
	}

	private IEnumerator coLoopTestTutorial()
	{
		while (!IsProgressing)
		{
			int num = Vespa_String_Extension_Method.TOINT(TestTutorialIndex.Value, 0);
			if (num > 0)
			{
				bool isComplete = false;
				StartTutorial(num, delegate
				{
					isComplete = true;
				});
				while (!isComplete)
				{
					yield return null;
				}
				if (TestTutorialLoop.Value)
				{
					yield return new WaitForSeconds(1f);
					continue;
				}
				break;
			}
			break;
		}
	}

	public override void ChainUpdate()
	{
		UpdateRunAutomatically();
	}

	private void CloseExternal()
	{
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance)
		{
			SceneAutoBehaviour<CutSceneManager>.instance.Close(stopImmediate: true);
		}
		if (SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>().MapViewer.ReleaseFocusNode(null);
		}
	}

	public SceneChanger PlaySceneChangeEffect(GlobalTopmostWindow.ESceneChangeType changeType, float timeOut)
	{
		if (_sceneChanger != null)
		{
			_sceneChanger.Close();
			_sceneChanger = null;
		}
		_sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(timeOut, changeType);
		return _sceneChanger;
	}

	public void StopSceneChangeEffect()
	{
		if (_sceneChanger != null)
		{
			_sceneChanger.Close();
			_sceneChanger = null;
		}
	}

	public void AddListener(ITutorialListener listener)
	{
		if (!_listenerList.Any((WeakRef<ITutorialListener> e) => e.Value == listener))
		{
			WeakRef<ITutorialListener> weakRef = new WeakRef<ITutorialListener>();
			weakRef.Value = listener;
			_listenerList.Add(weakRef);
		}
	}

	public void RemoveListener(ITutorialListener listener)
	{
		_listenerList.RemoveAll((WeakRef<ITutorialListener> e) => e.Value == listener);
	}

	public bool CanStartByEventTrigger(int tutorialIndex)
	{
		TutorialDataContainer tutorialDataContainer = GameRoot.Instance.TutorialDataContainer;
		if (tutorialDataContainer == null)
		{
			return false;
		}
		NGame2.NAccount.UserTutorialManager tutorialManager = GlobalAutoBehaviour<AccountManager>.instance.TutorialManager;
		if (tutorialManager == null)
		{
			return false;
		}
		TutorialData data = tutorialDataContainer.GetData(tutorialIndex, 0);
		if (data == null)
		{
			return false;
		}
		if (!data.Repeatable && tutorialManager.IsComplete(tutorialIndex))
		{
			return false;
		}
		int reqIndex = data.ReqIndex;
		if (reqIndex != 0 && !tutorialManager.IsComplete(reqIndex))
		{
			return false;
		}
		return true;
	}

	public bool StartByEventTrigger(int tutorialIndex, Action onComplete = null, Action onCompleteLoad = null)
	{
		if (RestoredPortal.TrySkippedOrvelTransition(this, tutorialIndex, onComplete, onCompleteLoad))
		{
			return true;
		}
		if (!CanStartByEventTrigger(tutorialIndex))
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (CurrentTutorialRuntime != null)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (!IsTutorialEnabled)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		TutorialData data = GameRoot.Instance.TutorialDataContainer.GetData(tutorialIndex, 0);
		StartTutorial(data, onComplete, onCompleteLoad);
		return true;
	}

	public void StopTutorial()
	{
		if (_currentTutorialCoroutine != null)
		{
			VespaCoroutine.Stop(_currentTutorialCoroutine);
			_currentTutorialCoroutine = null;
		}
		CloseExternal();
		TutorialWindow.ReleaseInstance();
		CurrentTutorialData = null;
		LastTutorialData = null;
		CurrentTutorialRuntime = null;
		IsProgressing = false;
	}

	public void SkipTutorial(bool endTutorial = true)
	{
		if (_currentTutorialCoroutine != null)
		{
			CurrentTutorialRuntime = null;
			VespaCoroutine.Stop(_tutorialRuntimeCoroutine);
			_tutorialRuntimeCoroutine = null;
			VespaCoroutine.Stop(_currentTutorialCoroutine);
			_currentTutorialCoroutine = null;
			if (endTutorial)
			{
				_currentTutorialCoroutine = VespaCoroutine.Start(coEndTutorial());
			}
		}
	}

	public void StartTutorial(int tutorialIndex, Action onComplete = null, Action onCompleteLoad = null)
	{
		TutorialDataContainer tutorialDataContainer = GameRoot.Instance.TutorialDataContainer;
		if (tutorialDataContainer == null)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else
		{
			TutorialData data = tutorialDataContainer.GetData(tutorialIndex, 0);
			StartTutorial(data, onComplete, onCompleteLoad);
		}
	}

	public void StartTutorial(TutorialData tutorialData, Action onComplete, Action onCompleteLoad)
	{
		_currentTutorialCoroutine = VespaCoroutine.Start(coProcessTutorial(tutorialData, onComplete, onCompleteLoad));
	}

	private IEnumerator coProcessTutorial(TutorialData tutorialData, Action onComplete, Action onCompleteLoad)
	{
		if (tutorialData == null)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		IsProgressing = true;
		SystemBackButtonSkip = 3;
		CurrentTutorialData = tutorialData;
		LastTutorialData = null;
		NextTutorialIndex = 0;
		_tutorialIndex = tutorialData.Index;
		_onComplete = onComplete;
		TutorialWindow tutorialWindow = TutorialWindow.instance;
		tutorialWindow.Open(this);
		Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
		yield return VespaCoroutine.Start(coProcessTutorialInternal(tutorialWindow));
		yield return VespaCoroutine.Start(coEndTutorial());
	}

	private IEnumerator coProcessTutorialInternal(TutorialWindow tutorialWindow)
	{
		_listenerList.ForEach(delegate(WeakRef<ITutorialListener> e)
		{
			if (e.Value != null)
			{
				e.Value.OnTutorialBegin(CurrentTutorialData);
			}
		});
		bool executeBegin = false;
		NShared.BeginTutorial.Response response = null;
		RequestBeginTutorial(delegate(NShared.BeginTutorial.Response iResponse)
		{
			response = iResponse;
			executeBegin = true;
		});
		while (!executeBegin)
		{
			yield return null;
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.AddServiceDisabler(this);
		while (true)
		{
			if (CurrentTutorialData == null)
			{
				yield break;
			}
			ITutorialRuntime tutorialRuntime = SceneAutoBehaviour<TutorialRuntimeFactory>.instance.Create(CurrentTutorialData.Command);
			if (tutorialRuntime != null)
			{
				CurrentTutorialRuntime = tutorialRuntime;
				yield return VespaCoroutine.Start(coRunTutorial(tutorialWindow, CurrentTutorialRuntime, CurrentTutorialData));
				CurrentTutorialRuntime = null;
			}
			if (CurrentTutorialData == null)
			{
				break;
			}
			TutorialData tutorialData;
			if (_reservedNextTutorialData != null)
			{
				tutorialData = _reservedNextTutorialData;
				_reservedNextTutorialData = null;
			}
			else
			{
				int sequence = CurrentTutorialData.Sequence + 1;
				tutorialData = GameRoot.Instance.TutorialDataContainer.GetData(CurrentTutorialData.Index, sequence);
			}
			if (tutorialData != null)
			{
				_ = $"Next Tutorial Sequence({tutorialData.Index}, {tutorialData.Sequence}): ";
			}
			else
			{
				_ = $"End of Tutorial ({CurrentTutorialData.Index})";
			}
			LastTutorialData = CurrentTutorialData;
			CurrentTutorialData = tutorialData;
		}
		VespaDebug.LogError(Define.LogPrefix + " CurrentTutorialData is null from external.");
	}

	private IEnumerator coRunTutorial(TutorialWindow tutorialWindow, ITutorialRuntime runtime, TutorialData tutorialData)
	{
		if (tutorialWindow == null || runtime == null || tutorialData == null)
		{
			yield break;
		}
		_ = tutorialData;
		if (tutorialWindow.IsSkipping())
		{
			if (runtime is ICustomTutorialSkippable customTutorialSkippable)
			{
				yield return VespaCoroutine.Start(customTutorialSkippable.Skip(tutorialWindow, tutorialData));
			}
			else if (runtime is ITutorialRuntimeImmediate tutorialRuntimeImmediate)
			{
				tutorialRuntimeImmediate.RunImmediate(tutorialWindow, tutorialData);
			}
			yield break;
		}
		if (runtime is ITutorialRuntimeImmediate tutorialRuntimeImmediate2)
		{
			tutorialRuntimeImmediate2.RunImmediate(tutorialWindow, tutorialData);
			yield break;
		}
		bool runtimeCompleted = false;
		_tutorialRuntimeCoroutine = VespaCoroutine.StartGlobal(runtime.Run(tutorialWindow, tutorialData), delegate
		{
			runtimeCompleted = true;
		}, "(tutorial)");
		while (!runtimeCompleted && !tutorialWindow.IsSkipping())
		{
			yield return null;
		}
		_tutorialRuntimeCoroutine = null;
	}

	private IEnumerator coEndTutorial()
	{
		SingletonUpdater<WebServiceManager>.instance.Requester.RemoveServiceDisabler(this);
		NShared.CompleteTutorial.Response response = null;
		yield return SingletonUpdater<TutorialManager>.instance.coRequestCompleteTutorial(delegate(NShared.CompleteTutorial.Response iResponse)
		{
			response = iResponse;
		});
		TutorialWindow.ReleaseInstance();
		CloseExternal();
		if (!GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(_tutorialIndex) && (response == null || response.BaseResult != BaseResultType.Success || response.Result != NShared.CompleteTutorial.ResultType.Success))
		{
			GlobalMessagePopup.OpenGlobalError(NShared.NLocalization.System.HTTP_INTERNAL_SERVER_ERROR, delegate
			{
				Vespa_System_Action_Extension_Method.fire(_onComplete);
				_onComplete = null;
				IsProgressing = false;
			});
			yield break;
		}
		yield return coResponseCompleteTutorial(response, null);
		if (!HasNextTurorial)
		{
			IsProgressing = false;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		NGame2.NAccount.UserCampaignManager.SetChapterDungeonIndex(chapterIndex, dungeonIndex, isForce: true);
		int waitingEndCount = 0;
		_listenerList.ForEach(delegate(WeakRef<ITutorialListener> e)
		{
			if (e.Value != null)
			{
				waitingEndCount++;
				e.Value.OnTutorialEnd(_tutorialIndex, delegate
				{
					waitingEndCount--;
				});
			}
		});
		while (waitingEndCount > 0)
		{
			yield return null;
		}
		_currentTutorialCoroutine = null;
		int nextTutorialIndex = NextTutorialIndex;
		NextTutorialIndex = 0;
		if (GameRoot.Instance.TutorialDataContainer.GetData(nextTutorialIndex, 0) == null)
		{
			Vespa_System_Action_Extension_Method.fire(_onComplete);
			_onComplete = null;
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new NShared.NEvent.CompleteTutorial
			{
				TutorialIndex = _tutorialIndex
			});
			IsProgressing = false;
		}
		else
		{
			StartTutorial(nextTutorialIndex, _onComplete);
		}
	}

	public void RequestBeginTutorial(Action<NShared.BeginTutorial.Response> onComplete)
	{
		VespaCoroutine.Start(coRequestBeginTutorial(onComplete));
	}

	private IEnumerator coRequestBeginTutorial(Action<NShared.BeginTutorial.Response> onComplete)
	{
		if (!IsTutorialEnabled)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, null);
			yield break;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(_tutorialIndex))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, null);
			yield break;
		}
		bool executeBegin = false;
		NShared.BeginTutorial.Request request = new NShared.BeginTutorial.Request();
		NShared.BeginTutorial.Response response = null;
		request.TutorialIndex = _tutorialIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BeginTutorial.Response iResponse)
		{
			response = iResponse;
			executeBegin = true;
		});
		while (!executeBegin)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete, response);
	}

	public IEnumerator coRequestCompleteTutorial(Action<NShared.CompleteTutorial.Response> onComplete)
	{
		if (!IsTutorialEnabled)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, null);
			yield break;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(_tutorialIndex))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, null);
			yield break;
		}
		bool executeEnd = false;
		NShared.CompleteTutorial.Request request = new NShared.CompleteTutorial.Request();
		NShared.CompleteTutorial.Response response = null;
		request.TutorialIndex = _tutorialIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CompleteTutorial.Response iResponse)
		{
			response = iResponse;
			executeEnd = true;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete, response);
	}

	public IEnumerator coResponseCompleteTutorial(NShared.CompleteTutorial.Response response, Action onComplete)
	{
		SingletonAuto<ModifyChecker>.instance.CheckStart<ClearTutorial>(new string[1] { _tutorialIndex.ToString() });
		if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.CompleteTutorial.ResultType.Success)
		{
			TutorialInfo info = response.Info;
			if (info != null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.Add(info);
				_ = (object)null;
				(new object[1])[0] = info.TutorialIndex;
			}
			if (response.DungeonInfos != null)
			{
				ChapterDungeonInfo[] dungeonInfos = response.DungeonInfos;
				foreach (ChapterDungeonInfo chapterDungeonInfo in dungeonInfos)
				{
					GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.CheckNewClearNode(chapterDungeonInfo.ChapterIndex, chapterDungeonInfo.DungeonIndex);
					_ = (object)null;
					_ = new object[2] { chapterDungeonInfo.ChapterIndex, chapterDungeonInfo.DungeonIndex };
				}
			}
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
			GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Apply(response.DungeonInfos);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
			GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHeroForTutorial(response.HeroInfos, response.TeamExpResultInfos);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResultInfos);
			GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Set(response.FreeEquipGachaInfo);
			if (__Extension_Method_ICollection__.COUNT(response.DungeonInfos) > 0)
			{
				ChapterDungeonInfo chapterDungeonInfo2 = response.DungeonInfos[__Extension_Method_ICollection__.COUNT(response.DungeonInfos) - 1];
				if (chapterDungeonInfo2 != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.SaveLatelyBattleNodePos(chapterDungeonInfo2.ChapterIndex, chapterDungeonInfo2.DungeonIndex);
				}
			}
			if (response != null && response.OpendMissionCategories != null)
			{
				LoginData.MissionCategoryInfo = response.OpendMissionCategories;
			}
			if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
			{
				GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.StartEffect();
			}
		}
		if (SingletonAuto<ModifyChecker>.instance.CheckResult<ClearTutorial>(new string[1] { _tutorialIndex.ToString() }))
		{
			string[] modifyCondition = SingletonAuto<ModifyChecker>.instance.GetModifyCondition<ClearTutorial>(new string[1] { _tutorialIndex.ToString() });
			if (GameRoot.Instance.AppReviewConditionDataContainer.IsCheck(modifyCondition))
			{
				AppReviewPopup.Open();
			}
		}
		if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.CompleteTutorial.ResultType.Success && LastTutorialData != null && !LastTutorialData.HideRewardPopup)
		{
			BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Tutorial.TUTORIAL_REWARD_TITLE)).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemResults)
				.SetCurrencyResultInfos3(response.CurrencyResults);
			BaseRewardPopup popup = BaseRewardPopup.OpenPopup(popupOpen, isGlobal: true);
			float waitStartTime = Time.time;
			while (!(popup == null) && popup.gameObject.activeInHierarchy && popup.IsOpen())
			{
				if (_runningState.AutoType == TutorialRunningState.TutorialAutoType.All && Time.time - waitStartTime > 1f)
				{
					popup.Close();
					break;
				}
				yield return null;
			}
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public void JumpToSkipEnd()
	{
		TutorialData currentTutorialData = CurrentTutorialData;
		if (currentTutorialData == null)
		{
			return;
		}
		int index = currentTutorialData.Index;
		int num = currentTutorialData.Sequence;
		TutorialData tutorialData = null;
		while (true)
		{
			int sequence = ++num;
			TutorialData data = GameRoot.Instance.TutorialDataContainer.GetData(index, sequence);
			if (data == null)
			{
				break;
			}
			if (data.Command == typeof(SkipEnd).Name)
			{
				tutorialData = data;
				break;
			}
		}
		if (tutorialData != null)
		{
			_reservedNextTutorialData = tutorialData;
		}
	}

	bool IServiceDisabler.IsDisabled(string host, string serviceName)
	{
		for (int i = 0; i < disableServiceName.Count(); i++)
		{
			if (string.Equals(disableServiceName[i], serviceName, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
		}
		return true;
	}

	public TutorialRunningState GetRunningState()
	{
		return _runningState;
	}

	private void RunStepAutomatically()
	{
		if (!TutorialWindow.isValidInstance)
		{
			return;
		}
		TutorialWindow tutorialWindow = TutorialWindow.instance;
		if (!(tutorialWindow == null))
		{
			TutorialWindow.ActionInfo firstTouchActionInfo = tutorialWindow.GetFirstTouchActionInfo();
			if (firstTouchActionInfo != null)
			{
				tutorialWindow.ProcessTouchAction(firstTouchActionInfo);
			}
		}
	}

	private void UpdateRunAutomatically()
	{
		if (_runningState.AutoType != TutorialRunningState.TutorialAutoType.None)
		{
			RunStepAutomatically();
		}
	}
}
