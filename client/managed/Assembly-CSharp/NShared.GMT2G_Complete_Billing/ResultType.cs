namespace NShared.GMT2G_Complete_Billing;

public enum ResultType
{
	Fail,
	Success,
	PlayerLogined,
	DBError_GetIAPInfo,
	DBError_InsertTransactionId,
	DBError_GetTransactionId,
	DBError_GetFirstPurchaseInfo,
	DBError_InsertFirstPurchaseInfo,
	DBError_UpdateFirstPurchaseInfo,
	DBError_GetPurchaseTimeDurationInfo,
	DBError_InsertPurchaseTimeDurationInfo,
	DBError_UpdatePurchaseTimeDurationInfo,
	DBError_UpdateIAPInfo,
	IAPInfoNotFound,
	VerifyReceiptError,
	NotConsumedYet,
	WrongReceipt,
	ShopItemNotFound,
	InvalidPackageItem,
	ExceededPurchasableCount,
	NotDailyEnded,
	InsufficientIAPInfo,
	AlreadyCompletedIAPInfo
}
