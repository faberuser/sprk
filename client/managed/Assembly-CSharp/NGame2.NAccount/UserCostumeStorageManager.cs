using NShared;

namespace NGame2.NAccount;

public class UserCostumeStorageManager : NShared.UserCostumeStorageManager
{
	private readonly UserObserverManager<CostumeStorageSlotInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserCostumeStorageManager()
	{
		_observerManager = new UserObserverManager<CostumeStorageSlotInfo>();
	}

	public void Apply(CostumeStorageSlotInfo info)
	{
		Add(info);
		_observerManager.AnnounceToObservers(info);
	}

	public void Apply(CostumeStorageSlotInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
