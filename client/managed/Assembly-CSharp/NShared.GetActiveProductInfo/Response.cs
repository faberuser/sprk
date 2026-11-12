namespace NShared.GetActiveProductInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public NewPayShopProductInfo PayShopProductInfo { get; set; }
}
