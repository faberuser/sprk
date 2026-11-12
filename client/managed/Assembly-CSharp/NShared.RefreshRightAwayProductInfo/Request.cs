namespace NShared.RefreshRightAwayProductInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/refresh_right_away_product_info";
}
