namespace NShared.CancelFlaskItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/cancel_flask_item";

	public int HeroIndex { get; set; }
}
