namespace NShared.GetRecommendPopupInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ShopRecommendPopupInfo RecommendPopupInfo { get; set; }
}
