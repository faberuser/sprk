namespace NShared.GetAllShopItemPurchaseCount;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ShopItemInfo[] ShopItems { get; set; }
}
