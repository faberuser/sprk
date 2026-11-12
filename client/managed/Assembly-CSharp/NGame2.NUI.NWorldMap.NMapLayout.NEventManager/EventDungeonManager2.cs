using NShared;
using NShared.NWorldMap;
using NShared.NWorldMap.NEventSubData;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class EventDungeonManager2 : EventIndicatorManager<EventDungeon>
{
	public override void Open(EventManager eventManager)
	{
		base.Open(eventManager);
		_eventType = EventType.EventDungeon;
		_useEatEffect = false;
	}

	public override EIndicatorType GetIndicatorType(WorldMapEventInfo worldMapEventInfo)
	{
		return EIndicatorType.Question;
	}
}
