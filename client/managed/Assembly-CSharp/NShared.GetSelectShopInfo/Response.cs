namespace NShared.GetSelectShopInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public SelectShopInfo SelectShopInfo { get; set; }
}
