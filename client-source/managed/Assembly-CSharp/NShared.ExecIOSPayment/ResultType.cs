namespace NShared.ExecIOSPayment;

public enum ResultType
{
	Fail,
	Success,
	VerifyFailed,
	NotDailyEnded,
	ExceededPurchasableCount,
	ShopItemDataNotFound,
	PurchaseInfoNotFound,
	InternalServerError,
	ServerBusy,
	AlreadyFinished,
	ProductIdNotExist,
	BuyDisabled,
	AppleRetryNeeded,
	AlreadyHasHero
}
