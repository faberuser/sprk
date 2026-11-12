namespace NShared.GetRecommendPopupInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/get_recommend_popup_info";

	public string Language { get; set; }
}
