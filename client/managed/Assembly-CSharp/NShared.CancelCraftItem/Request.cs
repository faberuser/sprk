namespace NShared.CancelCraftItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/cancel_craft_item";

	public int SlotIndex { get; set; }
}
