using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NCutScene;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWindow.NTutorial;
using NGame2.NUI.NWorldMap;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NSubStory;

public class SubStoryEventManager : SingletonAuto<SubStoryEventManager>, TutorialWindow.Listener
{
	public const int NoTutorialIndex = 0;

	private Coroutine _currentTutorialCoroutine;

	private Coroutine _tutorialRuntimeCoroutine;

	private int _tutorialIndex;

	private Action _onComplete;

	private SceneChanger _sceneChanger;

	public SubStoryEventData CurrentTutorialData { get; private set; }

	public SubStoryEventData LastTutorialData { get; private set; }

	public IEventRuntime CurrentTutorialRuntime { get; private set; }

	public bool IsProgressing { get; private set; }

	public int NextTutorialIndex { get; set; }

	public bool HasNextTurorial => NextTutorialIndex != 0;

	public bool StartByEventTrigger(int eventIndex, Action onComplete = null, Action onCompleteLoad = null)
	{
		if (!CanStartByEventTrigger(eventIndex))
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
		SubStoryEventData data = GameRoot.Instance.SubStoryEventDataContainer.GetData(eventIndex, 0);
		StartEvent(data, onComplete, onCompleteLoad);
		return true;
	}

	public bool CanStartByEventTrigger(int eventIndex)
	{
		if (GameRoot.Instance.SubStoryEventDataContainer == null)
		{
			return false;
		}
		if (GameRoot.Instance.SubStoryEventDataContainer.GetData(eventIndex, 0) == null)
		{
			return false;
		}
		return true;
	}

	private void StartEvent(SubStoryEventData eventData, Action onComplete, Action onCompleteLoad)
	{
		_currentTutorialCoroutine = VespaCoroutine.Start(coProcessTutorial(eventData, onComplete, onCompleteLoad));
	}

	private IEnumerator coProcessTutorial(SubStoryEventData eventData, Action onComplete, Action onCompleteLoad)
	{
		if (eventData == null)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		IsProgressing = true;
		CurrentTutorialData = eventData;
		LastTutorialData = null;
		NextTutorialIndex = 0;
		_tutorialIndex = eventData.Index;
		_onComplete = onComplete;
		TutorialWindow tutorialWindow = TutorialWindow.instance;
		tutorialWindow.Open(this);
		Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
		VespaDebug.LogError("~~ BeginCampaign ~~");
		yield return VespaCoroutine.Start(coProcessTutorialInternal(tutorialWindow));
		yield return VespaCoroutine.Start(coEndTutorial());
		VespaDebug.LogError("~~ EndCampaign ~~");
	}

	private IEnumerator coProcessTutorialInternal(TutorialWindow tutorialWindow)
	{
		while (CurrentTutorialData != null)
		{
			if (SceneAutoBehaviour<SubStoryRuntimeFactory>.instance.Create(CurrentTutorialData.Command) is ITutorialRuntime tutorialRuntime)
			{
				TutorialData tutorialData = new TutorialData();
				tutorialData.Index = CurrentTutorialData.Index;
				tutorialData.Sequence = CurrentTutorialData.Sequence;
				tutorialData.Command = CurrentTutorialData.Command;
				tutorialData.Argument = CurrentTutorialData.Argument;
				CurrentTutorialRuntime = tutorialRuntime;
				_tutorialRuntimeCoroutine = VespaCoroutine.Start(tutorialRuntime.Run(tutorialWindow, tutorialData));
				yield return _tutorialRuntimeCoroutine;
				_tutorialRuntimeCoroutine = null;
				CurrentTutorialRuntime = null;
			}
			if (GameRoot.Instance.SubStoryEventDataContainer == null)
			{
				yield return new WaitUntil(() => GameRoot.Instance.SubStoryEventDataContainer != null);
			}
			int num = CurrentTutorialData.Sequence + 1;
			SubStoryEventData data = GameRoot.Instance.SubStoryEventDataContainer.GetData(CurrentTutorialData.Index, num);
			if (data != null)
			{
				_ = $"Next Tutorial Sequence({data.Index}, {num}): ";
			}
			else
			{
				_ = $"End of Tutorial ({CurrentTutorialData.Index})";
			}
			LastTutorialData = CurrentTutorialData;
			CurrentTutorialData = data;
		}
	}

	private IEnumerator coEndTutorial()
	{
		bool onReceive = true;
		while (!onReceive)
		{
			yield return null;
		}
		TutorialWindow.ReleaseInstance();
		CloseExternal();
		onReceive = true;
		while (!onReceive)
		{
			yield return null;
		}
		if (!HasNextTurorial)
		{
			IsProgressing = false;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		NGame2.NAccount.UserCampaignManager.SetChapterDungeonIndex(chapterIndex, dungeonIndex, isForce: true);
		int waitingEndCount = 0;
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
			IsProgressing = false;
		}
		else
		{
			StartEventDirect(nextTutorialIndex, _onComplete);
		}
	}

	public void StartEventDirect(int eventIndex, Action onComplete = null, Action onCompleteLoad = null)
	{
		SubStoryEventDataContainer subStoryEventDataContainer = GameRoot.Instance.SubStoryEventDataContainer;
		if (subStoryEventDataContainer == null)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else
		{
			SubStoryEventData data = subStoryEventDataContainer.GetData(eventIndex, 0);
			StartEvent(data, onComplete, onCompleteLoad);
		}
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
}
