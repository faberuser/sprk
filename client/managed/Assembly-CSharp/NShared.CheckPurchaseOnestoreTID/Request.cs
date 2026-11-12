namespace NShared.CheckPurchaseOnestoreTID;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/check_purchase_onestore_tid";

	public string TID { get; set; }
}
