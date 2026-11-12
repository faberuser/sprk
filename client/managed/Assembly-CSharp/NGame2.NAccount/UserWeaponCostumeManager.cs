using NShared;

namespace NGame2.NAccount;

public class UserWeaponCostumeManager : NShared.UserWeaponCostumeManager
{
	private readonly UserObserverManager<WeaponCostumeInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserWeaponCostumeManager()
	{
		_observerManager = new UserObserverManager<WeaponCostumeInfo>();
	}

	public void Apply(WeaponCostumeInfo info)
	{
		Add(info);
		_observerManager.AnnounceToObservers(info);
	}

	public void Apply(WeaponCostumeInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
