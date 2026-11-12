using NShared;
using NShared.NWorldMap;
using NShared.NWorldMap.NEventSubData;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class GoblinShopManager : EventIndicatorManager<GoblinShop>
{
	public override void Open(EventManager eventManager)
	{
		base.Open(eventManager);
		_eventType = EventType.GoblinShop;
		_useEatEffect = false;
	}

	public override EIndicatorType GetIndicatorType(WorldMapEventInfo worldMapEventInfo)
	{
		return EIndicatorType.GoblinShop;
	}
}
