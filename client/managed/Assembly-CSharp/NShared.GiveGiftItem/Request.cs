namespace NShared.GiveGiftItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "npc/give_gift_item";

	public int HeroIndex { get; set; }

	public int[] ItemIndices { get; set; }

	public int[] ItemCount { get; set; }
}
