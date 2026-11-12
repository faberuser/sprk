namespace NShared.CheckPurchaseOnestoreTID;

public enum ResultType
{
	Fail,
	Success,
	IAPInfoNotFound,
	DBSelectError,
	WrongPurchaseStatus,
	VerificationFailed,
	ShopItemDataNotFound,
	ItemDataNotFound,
	InternalServerError,
	DBUpdateError,
	InvalidGiveItemResult,
	AlreadyFinished,
	OneStoreRetryNeeded
}
