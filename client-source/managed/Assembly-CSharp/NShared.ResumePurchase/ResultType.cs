namespace NShared.ResumePurchase;

public enum ResultType
{
	Fail,
	Success,
	DeveloperPayloadIsNULL,
	WrongPlatform,
	ProductIdIsNULL,
	OrderIdIsNULL,
	NotSameDeveloperPayload,
	ShopItemDataNotFound,
	IAPInfoNotFound,
	DBSelectError,
	WrongPurchaseStatus,
	AlreadyFinished,
	SignedOutUserPurchased
}
