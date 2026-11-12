namespace NShared.CheckPurchaseOneStore;

public enum ResultType
{
	Fail,
	Success,
	NotSameDeveloperPayload,
	NotSameProductId,
	WrongPurchaseStatus,
	PurchaseInfoNotFound,
	VerificationFailed,
	InvalidConsumptionState,
	ShopItemDataNotFound,
	ItemDataNotFound,
	InternalServerError,
	DBSelectError,
	DBUpdateError,
	InvalidGiveItemResult,
	AlreadyPassedOrderId,
	AlreadyFinished,
	OneStoreRetryNeeded
}
