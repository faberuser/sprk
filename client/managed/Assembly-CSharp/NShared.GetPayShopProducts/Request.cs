namespace NShared.GetPayShopProducts;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/get_payshop_products";

	public PayShopCategory CategoryType { get; set; }
}
