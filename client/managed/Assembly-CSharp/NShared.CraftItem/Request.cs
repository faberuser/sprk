namespace NShared.CraftItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/craft_item";

	public int SlotIndex { get; set; }

	public int CraftIndex { get; set; }

	public int ItemIndex { get; set; }

	public int ItemCount { get; set; }
}
