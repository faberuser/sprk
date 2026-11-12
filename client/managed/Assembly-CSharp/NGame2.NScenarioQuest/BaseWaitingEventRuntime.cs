using NGame2.NEventTrigger;
using NShared.NEvent;
using NVespa;

namespace NGame2.NScenarioQuest;

public abstract class BaseWaitingEventRuntime<T> : IScenarioQuestRuntime, IEventListener where T : IEvent
{
	private bool _ended;

	public string[] StateValue { get; set; }

	void IScenarioQuestRuntime.Run(string[] stateValue)
	{
		StateValue = stateValue;
		SingletonAuto<EventTriggerManager>.instance.AddEventListener(this);
	}

	bool IScenarioQuestRuntime.IsEnd()
	{
		return _ended;
	}

	void IEventListener.OnEvent(IEvent triggerEvent)
	{
		if (triggerEvent is T && OnEvent((T)triggerEvent))
		{
			SingletonAuto<EventTriggerManager>.instance.RemoveEventListener(this);
			_ended = true;
		}
	}

	public abstract bool OnEvent(T triggerEvent);
}
