namespace NShared.GetShopList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ShopRestockTimeInfo RestockTimeInfo { get; set; }

	public ShopListItemInfo[] ShopItems { get; set; }
}
