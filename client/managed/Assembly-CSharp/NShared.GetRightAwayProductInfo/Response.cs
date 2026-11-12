namespace NShared.GetRightAwayProductInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public NewPayShopProductInfo PayShopProductInfo { get; set; }
}
