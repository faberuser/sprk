namespace NShared.SetPurchaseMarketing;

public enum ResultType
{
	Fail,
	Success,
	[Comment("유효하지 않은 정보입니다.")]
	InvalidPurchaseMarketingRequestInfo,
	PurchaseMarketingDataNotFound,
	PurchaseMarketingNotOpened,
	NotAvailableRepeatCooltime,
	RemainRepeatCooltime
}
