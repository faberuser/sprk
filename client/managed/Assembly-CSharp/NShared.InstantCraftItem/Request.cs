namespace NShared.InstantCraftItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/instant_craft_item";

	public int SlotIndex { get; set; }

	public int RemainTime { get; set; }
}
