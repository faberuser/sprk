using NShared;

namespace NGame2.NAccount;

public class UserAccessoryCostumeManager : NShared.UserAccessoryCostumeManager
{
	private readonly UserObserverManager<PlayerAccessoryCostumeInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserAccessoryCostumeManager()
	{
		_observerManager = new UserObserverManager<PlayerAccessoryCostumeInfo>();
	}

	public void Apply(PlayerAccessoryCostumeInfo info)
	{
		Add(info);
		_observerManager.AnnounceToObservers(info);
	}

	public void Apply(PlayerAccessoryCostumeInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
