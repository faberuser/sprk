namespace NShared.BuyShopItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/buy_shop_item";

	public int ShopIndex { get; set; }

	public int ListNo { get; set; }

	public int ShopItemIndex { get; set; }

	public int ShopItemPurchaseCount { get; set; }
}
