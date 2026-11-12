using NShared;

namespace NGame2.NAccount;

public class UserHairCostumeManager : NShared.UserHairCostumeManager
{
	private readonly UserObserverManager<HairCostumeInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserHairCostumeManager()
	{
		_observerManager = new UserObserverManager<HairCostumeInfo>();
	}

	public void Apply(HairCostumeInfo info)
	{
		Add(info);
		_observerManager.AnnounceToObservers(info);
	}

	public void Apply(HairCostumeInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
