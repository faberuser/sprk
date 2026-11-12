using NShared;

namespace NGame2.NAccount;

public class UserNPCFriendlyManager : NShared.UserNPCFriendlyManager
{
	private readonly UserObserverManager<NPCFriendlyInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserNPCFriendlyManager()
	{
		_observerManager = new UserObserverManager<NPCFriendlyInfo>();
	}

	public void Apply(NPCFriendlyInfo info)
	{
		if (info != null)
		{
			Add(info);
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void Apply(NPCFriendlyInfo[] infos)
	{
		if (infos != null)
		{
			Set(infos);
		}
	}
}
