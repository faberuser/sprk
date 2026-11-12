namespace NShared.GetSelectShopInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/get_select_shop_info";

	public string CategoryGroup { get; set; }
}
