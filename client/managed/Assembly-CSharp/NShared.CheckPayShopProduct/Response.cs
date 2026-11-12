namespace NShared.CheckPayShopProduct;

[Response]
public class Response : GenericResponse<ResultType>
{
	public NewPayShopProductInfo PayShopProductInfo { get; set; }
}
