namespace NShared.CompletePurchase;

public enum ResultType
{
	Fail,
	Success,
	WrongPlatform,
	ProductIdIsNULL,
	ProductIndexIsNULL,
	DeveloperPayloadIsNULL,
	OrderIdIsNULL,
	NotSameDeveloperPayload,
	NotSameProductId,
	WrongPurchaseStatus,
	PurchaseInfoNotFound,
	VerificationFailed,
	InvalidConsumptionState,
	ShopItemDataNotFound,
	ItemDataNotFound,
	InternalServerError,
	PaymentHandlerNotFound,
	DBSelectError,
	DBUpdateError,
	InvalidGiveItemResult,
	AlreadyPassedOrderId,
	AlreadyFinished,
	DuplicatedOrderId,
	GoogleRetryNeeded,
	UDPRetryNeeded,
	RequestDuplicatedOrderId,
	ExceededPurchasableCount
}
