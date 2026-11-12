namespace NShared.UseGuildNameChangeItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_guild_name_change_item";

	public int ItemIndex { get; set; }

	public string Name { get; set; }
}
