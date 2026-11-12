namespace NShared.G2R_SetPurchaseInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_set_purchase_info";
}
