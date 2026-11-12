namespace NShared.RequestShopList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ShopRestockTimeInfo RestockTimeInfo { get; set; }

	public ShopListItemInfo[] ShopItems { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }
}
