using NShared.NWorldMap;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class DummyNodeIndicatorData : ITimerEvent
{
	bool ITimerEvent.IsExpired()
	{
		return false;
	}
}
