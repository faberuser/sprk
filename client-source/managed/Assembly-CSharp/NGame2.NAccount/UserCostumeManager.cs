using NShared;

namespace NGame2.NAccount;

public class UserCostumeManager : NShared.UserCostumeManager
{
	private readonly UserObserverManager<CostumeInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserCostumeManager()
	{
		_observerManager = new UserObserverManager<CostumeInfo>();
	}

	public void Apply(CostumeInfo info)
	{
		Add(info);
		_observerManager.AnnounceToObservers(info);
	}

	public void Apply(CostumeInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
