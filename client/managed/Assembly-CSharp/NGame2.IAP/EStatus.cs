namespace NGame2.IAP;

public enum EStatus
{
	None,
	InitBilling,
	Ready,
	QueryInventory,
	Purchase,
	ErrorFromGameServer,
	BillingNotSupported,
	DetailProcessScene
}
