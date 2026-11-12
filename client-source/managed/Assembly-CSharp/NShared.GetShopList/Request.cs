namespace NShared.GetShopList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/get_shop_list";

	public int ShopIndex { get; set; }
}
