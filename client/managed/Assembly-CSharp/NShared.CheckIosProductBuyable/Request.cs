namespace NShared.CheckIosProductBuyable;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/check_ios_product_buyable";

	public string ProductID { get; set; }

	public int ProductIndex { get; set; }
}
