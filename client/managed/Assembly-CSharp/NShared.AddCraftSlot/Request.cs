namespace NShared.AddCraftSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/add_craft_slot";

	public int SlotIndex { get; set; }
}
