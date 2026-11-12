namespace NShared.RestockSelectShopInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/restock_select_shop_info";

	public string CategoryGroup { get; set; }
}
