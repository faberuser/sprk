namespace NShared.CheckPayShopProduct;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/check_payshop_product";

	public EPurchasePlatform Platform { get; set; }

	public string ProductID { get; set; }
}
