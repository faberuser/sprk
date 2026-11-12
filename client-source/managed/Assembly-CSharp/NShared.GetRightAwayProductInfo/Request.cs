namespace NShared.GetRightAwayProductInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/get_right_away_product_info";

	public int ProductIndex { get; set; }
}
