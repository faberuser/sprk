namespace NShared.G2R_GetPurchaseInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_purchase_info";
}
