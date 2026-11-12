namespace NShared.SetPurchaseMarketing;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/set_purchase_marketing";

	public int[] PurchaseMarketingIndices { get; set; }

	public bool IsActive { get; set; }

	public bool IsOpenPopup { get; set; }
}
