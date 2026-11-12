namespace NShared.UseFlaskItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_flask_item";

	public int ItemIndex { get; set; }

	public int HeroIndex { get; set; }
}
