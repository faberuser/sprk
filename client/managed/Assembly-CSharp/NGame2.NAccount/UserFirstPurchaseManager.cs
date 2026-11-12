using NShared;

namespace NGame2.NAccount;

public class UserFirstPurchaseManager : NShared.UserFirstPurchaseManager
{
	private readonly UserObserverManager<PlayerFirstPurchaseInfo> _observerManager = new UserObserverManager<PlayerFirstPurchaseInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	protected override void OnAddData(PurchaseInfoKey key, PlayerFirstPurchaseInfo value)
	{
		base.OnAddData(key, value);
		_observerManager.AnnounceToObservers(value);
	}

	public PlayerFirstPurchaseInfo Get(int ShopIndex, int Index)
	{
		return Get(new PurchaseInfoKey(ShopIndex, Index));
	}
}
