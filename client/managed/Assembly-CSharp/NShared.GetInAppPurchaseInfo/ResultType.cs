namespace NShared.GetInAppPurchaseInfo;

public enum ResultType
{
	Fail,
	Success,
	IAPInfoNotFound,
	DBSelectError,
	WrongPlatform,
	ShopItemDataNotFound
}
