namespace NShared.RefreshRightAwayProductInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RightAwayProductInfo[] RightAwayProductInfos { get; set; }

	public SelectShopNoticeInfo[] SelectShopNoticeInfos { get; set; }
}
