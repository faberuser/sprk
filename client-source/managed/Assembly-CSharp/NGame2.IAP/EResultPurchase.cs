namespace NGame2.IAP;

public enum EResultPurchase
{
	Success,
	InvalidStatus,
	NotExistProduct,
	QueryInventoryFailed,
	InprocessingPurchaseExist,
	GetDeveloperPayloadFailed,
	CheckIosProductBuyableFailed,
	GooglePurchaseFailed,
	ApplePurchaseFailed,
	OneStorePurchaseFailed,
	UDPPurchaseFailed,
	UDPConsumeFailed,
	UserCanceled,
	AlreadyOwned,
	BeginPurchaseFailed,
	CompletePurchaseFailed
}
