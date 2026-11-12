using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserRaidManager : NShared.UserRaidManager
{
	private readonly UserObserverManager<RaidInfo> _observerManager = new UserObserverManager<RaidInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void Apply(RaidInfo info)
	{
		if (info != null)
		{
			Add(info);
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void Apply(IList<RaidInfo> info)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info); i++)
		{
			Apply(info[i]);
		}
	}

	public bool AnyRaidLevelCleared(int MinRaidLevel)
	{
		return ToList().Find((RaidInfo raidInfo) => raidInfo.RaidLevel >= MinRaidLevel) != null;
	}
}
