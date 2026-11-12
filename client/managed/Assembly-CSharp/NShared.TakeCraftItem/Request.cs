namespace NShared.TakeCraftItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/take_craft_item";

	public int SlotIndex { get; set; }
}
