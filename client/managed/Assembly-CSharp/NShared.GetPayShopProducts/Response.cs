namespace NShared.GetPayShopProducts;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PayShopProductInfo[] PayShopProductInfos { get; set; }

	public PlayerProductPurchaseInfo[] PlayerProductPurchaseInfos { get; set; }
}
