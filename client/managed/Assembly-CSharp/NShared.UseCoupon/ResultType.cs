namespace NShared.UseCoupon;

public enum ResultType
{
	Fail,
	Success,
	MissingCouponCode,
	MissingCountryCode,
	Progressing,
	CouponNotFound,
	AlreadyReceived,
	CountryCodeMismatch,
	CouponExpired,
	PromotionNotFound,
	AlreadyUsedPromotion
}
