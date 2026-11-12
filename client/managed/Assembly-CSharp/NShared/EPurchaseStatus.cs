namespace NShared;

[JsonSerializable(Javascript = true)]
public enum EPurchaseStatus
{
	None,
	DeveloperPayloadSuccess,
	RequestedPurchase,
	RecvPurchaseSuccess,
	RecvPurchaseFail,
	CheckPurchaseSuccess,
	CheckPurchaseFail,
	RequestedConsume,
	RecvConsumeSuccess,
	RecvConsumeFail,
	CheckConsumeSuccess,
	RequestedForceConsume,
	RecvForceConsumeSuccess,
	RecvForceConsumeFail,
	VerificationFailed,
	CheckConsumeFailed,
	PaymentSuccess,
	PaymentFail,
	VerificationError,
	InternalServerError,
	UserCanceled,
	InitPurchase,
	CompletePurchase
}
