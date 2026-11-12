using NShared;

namespace NGame2.NAccount;

public class UserPayShopDiscountProductManager : NShared.UserPayShopDiscountProductManager
{
	public class ChangedPayShopDiscountProductInfo
	{
		public PayShopDiscountInfo[] changedInfo;
	}

	private readonly UserObserverManager<ChangedPayShopDiscountProductInfo> _observerManager = new UserObserverManager<ChangedPayShopDiscountProductInfo>();

	public IUserObserverManager ObserverManager => _observerManager;

	private void AnnounceToObserver(PayShopDiscountInfo[] info)
	{
		if (_observerManager != null)
		{
			_observerManager.AnnounceToObservers(new ChangedPayShopDiscountProductInfo
			{
				changedInfo = info
			});
		}
	}

	public void Apply(PayShopDiscountInfo[] infos)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(infos))
		{
			Set(infos);
			AnnounceToObserver(infos);
		}
	}
}
