namespace NShared.GetDeveloperPayload;

public enum ResultType
{
	Fail,
	Success,
	NoShopData,
	ShopItemDataNotFound,
	NotDailyEnded,
	ExceededPurchasableCount,
	BuyDisabled,
	NeedCompleteInAppPurchase,
	OtherDailyEffective,
	DBSelectError,
	DBInsertError,
	AlreadyHasHero,
	[Comment("다른 스토어에서 결제가 진행 중인 상품입니다")]
	InProgressOtherStoreInAppPurchase,
	PtsUnSupported
}
