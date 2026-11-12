namespace NShared.GetOnestoreIncompleteAppPurchaseInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/get_onestore_incomplete_app_purchase_info";
}
