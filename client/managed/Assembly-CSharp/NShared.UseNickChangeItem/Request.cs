namespace NShared.UseNickChangeItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_nick_change_item";

	public int ItemIndex { get; set; }

	public string Nick { get; set; }
}
