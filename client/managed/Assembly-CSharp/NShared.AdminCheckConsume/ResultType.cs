namespace NShared.AdminCheckConsume;

public enum ResultType
{
	Fail,
	Success,
	NotSameDeveloperPayload,
	NotSameProductId,
	WrongPurchaseStatus,
	VerificationFailed,
	InvalidConsumptionState,
	ShopItemDataNotFound,
	PurchaseInfoNotFound,
	ItemDataNotFound,
	InternalServerError
}
