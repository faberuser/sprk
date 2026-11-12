namespace NShared.SellItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/sell_item";

	public int[] ItemIndices { get; set; }

	public int[] ItemCount { get; set; }
}
