using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NCutScene;
using NGame2.NLobby;
using NGame2.NParty;
using NGame2.NSubStory;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUtil;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NEventTrigger;

public class EventTriggerManager : SingletonAuto<EventTriggerManager>
{
	public static Dictionary<string, BaseNodeEvent> NodeEventTemplates;

	private readonly List<WeakRef<IEventListener>> _eventListenerList = new List<WeakRef<IEventListener>>();

	private Dictionary<string, int> _sceneWindowTriggerSequance = new Dictionary<string, int>();

	public EventTriggerData CurrentEventTriggerData { get; private set; }

	public EventTriggerData CurrentMainScenarioData { get; private set; }

	public void OnSceneChanged()
	{
		_sceneWindowTriggerSequance.Clear();
	}

	public int GetEnterMenuSequance(string windowName)
	{
		if (!_sceneWindowTriggerSequance.ContainsKey(windowName))
		{
			_sceneWindowTriggerSequance[windowName] = 0;
		}
		_sceneWindowTriggerSequance[windowName] += 1;
		return _sceneWindowTriggerSequance[windowName];
	}

	public EventTriggerManager()
	{
		if (NodeEventTemplates == null)
		{
			NodeEventTemplates = new Dictionary<string, BaseNodeEvent>();
			RegisterEventTemplate<EnterLobby>();
			RegisterEventTemplate<PlayerMoveEnd>();
			RegisterEventTemplate<PlayerVisitNode>();
			RegisterEventTemplate<BattleBegin>();
			RegisterEventTemplate<BattleEnd>();
			RegisterEventTemplate<NShared.NEvent.WaveReady>();
			RegisterEventTemplate<WaveStart>();
			RegisterEventTemplate<WaveEnd>();
			RegisterEventTemplate<EnterBattle>();
			RegisterEventTemplate<NodeFunctionMenu>();
			RegisterEventTemplate<OnClickStartBattle>();
			RegisterEventTemplate<OnClickMoveChapter>();
			RegisterEventTemplate<EnterScenarioBattle>();
			RegisterEventTemplate<ScenarioBattleBegin>();
			RegisterEventTemplate<ScenarioBattleEnd>();
			RegisterEventTemplate<ScenarioWaveReady>();
			RegisterEventTemplate<ScenarioWaveStart>();
			RegisterEventTemplate<ScenarioWaveEnd>();
			RegisterEventTemplate<EnterChapter>();
		}
	}

	private void RegisterEventTemplate<TEvent>() where TEvent : BaseNodeEvent, new()
	{
		NodeEventTemplates.Add(typeof(TEvent).Name, new TEvent());
	}

	public void AddEventListener(IEventListener listener)
	{
		if (!_eventListenerList.Any((WeakRef<IEventListener> e) => e.Value == listener))
		{
			WeakRef<IEventListener> weakRef = new WeakRef<IEventListener>();
			weakRef.Value = listener;
			_eventListenerList.Add(weakRef);
		}
	}

	public void RemoveEventListener(IEventListener listener)
	{
		_eventListenerList.RemoveAll((WeakRef<IEventListener> e) => e.Value == listener);
	}

	private void ForEachListener(Action<IEventListener> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_eventListenerList); i++)
		{
			WeakRef<IEventListener> weakRef = _eventListenerList[i];
			if (weakRef.Value == null)
			{
				_eventListenerList.RemoveAt(i);
				i--;
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(action, weakRef.Value);
			}
		}
	}

	private void RemoveInvalidListener()
	{
		_eventListenerList.RemoveAll((WeakRef<IEventListener> weakRef) => weakRef.Value == null);
	}

	public bool ParseEventPosition(EventTriggerData data, out int chapterIndex, out int dungeonIndex)
	{
		chapterIndex = 0;
		dungeonIndex = 0;
		if (data == null)
		{
			return false;
		}
		BaseNodeEvent vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(NodeEventTemplates, data.EventType, null);
		if (vALUE == null)
		{
			return false;
		}
		vALUE.ParseNodePosition(data.EventCondition, out chapterIndex, out dungeonIndex);
		return true;
	}

	public bool CanRaiseEvent(IEvent triggerEvent, bool ignoreCurrent = false)
	{
		if (triggerEvent == null)
		{
			return false;
		}
		EventTriggerDataContainer eventTriggerDataContainer = GameRoot.Instance.EventTriggerDataContainer;
		if (eventTriggerDataContainer == null)
		{
			return false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		if (!ignoreCurrent && SingletonUpdater<TutorialManager>.instance.CurrentTutorialRuntime != null)
		{
			return false;
		}
		string name = triggerEvent.GetType().Name;
		IList<EventTriggerData> dataByEventType = eventTriggerDataContainer.GetDataByEventType(name);
		if (dataByEventType == null || dataByEventType.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(dataByEventType); i++)
		{
			EventTriggerData eventTriggerData = dataByEventType[i];
			if (eventTriggerData != null)
			{
				int tableChapterIndex = 0;
				int tableDungeonIndex = 0;
				if (triggerEvent.Check(eventTriggerData.EventCondition, out tableChapterIndex, out tableDungeonIndex) && CanRaiseEvent(eventTriggerData))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanRaiseEvent(EventTriggerData data)
	{
		if (!AccountManager.IsLogined())
		{
			return false;
		}
		if (!CheckCurrentCondition(data))
		{
			return false;
		}
		return CanStartEventTrigger(data);
	}

	private bool CanStartEventTrigger(EventTriggerData data)
	{
		if (data.Command == EventTriggerCommand.StartTutorial)
		{
			return CanStartTutorial(data);
		}
		if (data.Command == EventTriggerCommand.StartCutScene)
		{
			return CanStartCutScene(data);
		}
		if (data.Command == EventTriggerCommand.StartDungeon)
		{
			return CanStartDungeon(data);
		}
		if (data.Command == EventTriggerCommand.StartSubStory)
		{
			return CanStartSubStory(data);
		}
		return false;
	}

	private bool CanStartTutorial(EventTriggerData eventTriggerData)
	{
		if (!SingletonUpdater<TutorialManager>.instance.IsTutorialEnabled)
		{
			return RestoredPortal.OrvelTutorialAvailable(available: false, eventTriggerData);
		}
		if (__Extension_Method_ICollection__.COUNT(eventTriggerData.Argument) < 1)
		{
			return RestoredPortal.OrvelTutorialAvailable(available: false, eventTriggerData);
		}
		if (!String_Extension_Method.TryToInt(eventTriggerData.Argument[0], out var output))
		{
			return RestoredPortal.OrvelTutorialAvailable(available: false, eventTriggerData);
		}
		return RestoredPortal.OrvelTutorialAvailable(SingletonUpdater<TutorialManager>.instance.CanStartByEventTrigger(output), eventTriggerData);
	}

	private bool CanStartSubStory(EventTriggerData eventTriggerData)
	{
		if (__Extension_Method_ICollection__.COUNT(eventTriggerData.Argument) < 1)
		{
			return false;
		}
		if (!String_Extension_Method.TryToInt(eventTriggerData.Argument[0], out var output))
		{
			return false;
		}
		return SingletonAuto<SubStoryEventManager>.instance.CanStartByEventTrigger(output);
	}

	private bool CanStartCutScene(EventTriggerData eventTriggerData)
	{
		if (__Extension_Method_ICollection__.COUNT(eventTriggerData.Argument) < 1)
		{
			return false;
		}
		return true;
	}

	private bool CanStartDungeon(EventTriggerData eventTriggerData)
	{
		if (__Extension_Method_ICollection__.COUNT(eventTriggerData.Argument) < 2)
		{
			return false;
		}
		return true;
	}

	public bool RaiseEventDirect(int eventIndex, Action onComplete = null, Action onCompleteLoad = null)
	{
		EventTriggerDataContainer eventTriggerDataContainer = GameRoot.Instance.EventTriggerDataContainer;
		if (eventTriggerDataContainer == null)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (!AccountManager.IsLogined())
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		EventTriggerData data = eventTriggerDataContainer.GetData(eventIndex);
		if (data == null)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (!CheckCurrentCondition(data))
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (!StartEventTrigger(data, onComplete, onCompleteLoad))
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		return true;
	}

	public bool RaiseEvent(IEvent triggerEvent, Action onComplete = null, Action onCompleteLoad = null)
	{
		EventTriggerData triggeredData;
		return RaiseEventInternal(triggerEvent, null, out triggeredData, onComplete, onCompleteLoad);
	}

	public bool RaiseEvent(IEvent triggerEvent, HashSet<int> exceptions, out EventTriggerData triggeredData, Action onComplete = null, Action onCompleteLoad = null)
	{
		return RaiseEventInternal(triggerEvent, exceptions, out triggeredData, onComplete, onCompleteLoad);
	}

	private bool RaiseEventInternal(IEvent triggerEvent, HashSet<int> exceptions, out EventTriggerData triggeredData, Action onComplete = null, Action onCompleteLoad = null)
	{
		triggeredData = null;
		if (triggerEvent == null)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		EventTriggerDataContainer eventTriggerDataContainer = GameRoot.Instance.EventTriggerDataContainer;
		if (eventTriggerDataContainer == null)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (!AccountManager.IsLogined())
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (SingletonUpdater<TutorialManager>.instance.CurrentTutorialRuntime != null && SingletonUpdater<TutorialManager>.instance.CurrentTutorialRuntime.OnEvent(triggerEvent))
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return true;
		}
		ForEachListener(delegate(IEventListener listener)
		{
			listener.OnEvent(triggerEvent);
		});
		string name = triggerEvent.GetType().Name;
		IList<EventTriggerData> dataByEventType = eventTriggerDataContainer.GetDataByEventType(name);
		if (dataByEventType == null || dataByEventType.Count == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		foreach (EventTriggerData item in dataByEventType)
		{
			if (item != null && (exceptions == null || !exceptions.Contains(item.Index)) && CheckCurrentCondition(item))
			{
				int tableChapterIndex = 0;
				int tableDungeonIndex = 0;
				if (triggerEvent.Check(item.EventCondition, out tableChapterIndex, out tableDungeonIndex) && StartEventTrigger(item, onComplete, onCompleteLoad))
				{
					triggeredData = item;
					ActivityManager.RequestStop();
					return true;
				}
			}
		}
		Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
		Vespa_System_Action_Extension_Method.fire(onComplete);
		return false;
	}

	private bool CheckCurrentCondition(EventTriggerData data)
	{
		if (data == null)
		{
			return false;
		}
		return SingletonAuto<ConditionChecker>.instance.Check(data.CurrentCondition);
	}

	private bool StartEventTrigger(EventTriggerData eventTriggerData, Action onComplete, Action onCompleteLoad)
	{
		if (eventTriggerData.ForceLeaveParty && GlobalAutoBehaviour<PartyManager>.isValidInstance)
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
		}
		if (eventTriggerData.Command == EventTriggerCommand.StartTutorial)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			return StartTutorial(eventTriggerData, onComplete, onCompleteLoad);
		}
		if (eventTriggerData.Command == EventTriggerCommand.StartCutScene)
		{
			return StartCutScene(eventTriggerData, onComplete, onCompleteLoad);
		}
		if (eventTriggerData.Command == EventTriggerCommand.StartDungeon)
		{
			return StartDungeon(eventTriggerData, onComplete, onCompleteLoad);
		}
		if (eventTriggerData.Command == EventTriggerCommand.StartSubStory)
		{
			return StartSubStory(eventTriggerData, onComplete, onCompleteLoad);
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
		return false;
	}

	private bool StartTutorial(EventTriggerData eventTriggerData, Action onComplete, Action onCompleteLoad)
	{
		if (__Extension_Method_ICollection__.COUNT(eventTriggerData.Argument) < 1)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (!String_Extension_Method.TryToInt(eventTriggerData.Argument[0], out var output))
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		return SingletonUpdater<TutorialManager>.instance.StartByEventTrigger(output, onComplete, onCompleteLoad);
	}

	private bool StartSubStory(EventTriggerData eventTriggerData, Action onComplete, Action onCompleteLoad)
	{
		if (__Extension_Method_ICollection__.COUNT(eventTriggerData.Argument) < 1)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (!String_Extension_Method.TryToInt(eventTriggerData.Argument[0], out var output))
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		return SingletonAuto<SubStoryEventManager>.instance.StartByEventTrigger(output, onComplete, onCompleteLoad);
	}

	private bool StartCutScene(EventTriggerData eventTriggerData, Action onComplete, Action onCompleteLoad)
	{
		if (__Extension_Method_ICollection__.COUNT(eventTriggerData.Argument) < 1)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		if (eventTriggerData.ImmediatelyLoadComplate)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			onCompleteLoad = null;
		}
		string scriptPath = eventTriggerData.Argument[0];
		SceneAutoBehaviour<CutSceneManager>.instance.Open(scriptPath, canSkip: true, onComplete, onCompleteLoad);
		return true;
	}

	private bool StartDungeon(EventTriggerData eventTriggerData, Action onComplete, Action onCompleteLoad)
	{
		if (__Extension_Method_ICollection__.COUNT(eventTriggerData.Argument) < 2)
		{
			Vespa_System_Action_Extension_Method.fire(onCompleteLoad);
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return false;
		}
		int chapterIndex = Vespa_String_Extension_Method.TOINT(eventTriggerData.Argument[0], 0);
		int dungeonIndex = Vespa_String_Extension_Method.TOINT(eventTriggerData.Argument[1], 0);
		SceneAutoBehaviour<MainStoryManagement>.instance.OpenPartySetting(chapterIndex, dungeonIndex);
		return true;
	}
}
