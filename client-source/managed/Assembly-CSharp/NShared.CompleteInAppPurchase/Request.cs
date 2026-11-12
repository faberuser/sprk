namespace NShared.CompleteInAppPurchase;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/complete_inapp_purchase";
}
