namespace NShared.SetReviewPopup;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/set_review_popup";

	public AppReviewPopupType ReviewType { get; set; }
}
