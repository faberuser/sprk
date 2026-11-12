using NShared;

namespace NGame2.NAccount;

public class UserEventGrowWorldTreeManager : NShared.UserEventGrowWorldTreeManager
{
	private EventStepInfo _playerEventInfo;

	public EventStepInfo PlayerEventStepInfo
	{
		get
		{
			return _playerEventInfo;
		}
		set
		{
			_playerEventInfo = value;
		}
	}
}
