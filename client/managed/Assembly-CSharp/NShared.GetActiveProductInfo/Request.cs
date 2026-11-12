namespace NShared.GetActiveProductInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/get_active_product_info";

	public int ProductIndex { get; set; }
}
