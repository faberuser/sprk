namespace NShared.RequestShopList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/request_shop_list";

	public int ShopIndex { get; set; }
}
