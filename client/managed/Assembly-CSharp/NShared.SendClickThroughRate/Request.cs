namespace NShared.SendClickThroughRate;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "ctr/send_click_through_rate";

	public string AdId { get; set; }

	public int ClickType { get; set; }

	public string MetaInfo { get; set; }
}
