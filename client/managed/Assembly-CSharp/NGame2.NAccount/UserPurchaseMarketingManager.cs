using NShared;

namespace NGame2.NAccount;

public class UserPurchaseMarketingManager : NShared.UserPurchaseMarketingManager
{
	private readonly UserObserverManager<PurchaseMarketingInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserPurchaseMarketingManager()
	{
		_observerManager = new UserObserverManager<PurchaseMarketingInfo>();
	}

	public static PurchaseMarketingInfo Convert(PurchaseMarketingResultInfo purchaseMarketingResultInfo)
	{
		return new PurchaseMarketingInfo
		{
			PurchaseIndex = purchaseMarketingResultInfo.PurchaseIndex,
			BeginTime = purchaseMarketingResultInfo.BeginTime,
			EndTime = purchaseMarketingResultInfo.EndTime,
			PurchasedTime = purchaseMarketingResultInfo.PurchasedTime,
			Count = purchaseMarketingResultInfo.Count,
			Active = purchaseMarketingResultInfo.Active,
			IsOpenPopup = purchaseMarketingResultInfo.IsOpenPopup
		};
	}

	public void Apply(PurchaseMarketingResultInfo purchaseMarketingResultInfo)
	{
		if (purchaseMarketingResultInfo != null)
		{
			PurchaseMarketingInfo purchaseMarketingInfo = Get(purchaseMarketingResultInfo.PurchaseIndex);
			if (purchaseMarketingInfo == null)
			{
				purchaseMarketingInfo = Convert(purchaseMarketingResultInfo);
			}
			else
			{
				purchaseMarketingInfo.BeginTime = purchaseMarketingResultInfo.BeginTime;
				purchaseMarketingInfo.EndTime = purchaseMarketingResultInfo.EndTime;
				purchaseMarketingInfo.PurchasedTime = purchaseMarketingResultInfo.PurchasedTime;
				purchaseMarketingInfo.Count = purchaseMarketingResultInfo.Count;
				purchaseMarketingInfo.Active = purchaseMarketingResultInfo.Active;
				purchaseMarketingInfo.IsOpenPopup = purchaseMarketingResultInfo.IsOpenPopup;
			}
			Apply(purchaseMarketingInfo);
		}
	}

	public void Apply(PurchaseMarketingResultInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public void Apply(PurchaseMarketingInfo info)
	{
		Add(info);
		_observerManager.AnnounceToObservers(info);
	}

	public void Apply(PurchaseMarketingInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public PurchaseMarketingInfo GetPurchaseMarketingInfo(int productIndex)
	{
		PurchaseMarketingData purchaseMarketingData = GameRoot.Instance.PurchaseMarketingDataContainer.GetPurchaseMarketingData(productIndex);
		if (purchaseMarketingData == null)
		{
			return null;
		}
		return Get(purchaseMarketingData.Index);
	}
}
