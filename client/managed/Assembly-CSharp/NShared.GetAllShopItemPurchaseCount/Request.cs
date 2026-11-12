namespace NShared.GetAllShopItemPurchaseCount;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/get_all_shop_item_purchase_count";

	public int ShopIndex { get; set; }
}
