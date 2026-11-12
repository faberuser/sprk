namespace NShared.RefreshPayshopDiscountInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/refresh_payshop_discount_info";
}
