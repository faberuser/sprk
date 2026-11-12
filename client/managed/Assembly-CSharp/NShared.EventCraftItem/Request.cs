namespace NShared.EventCraftItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/event_craft_item";

	public int CraftIndex { get; set; }

	public int CraftCount { get; set; }
}
