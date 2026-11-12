namespace NShared.UnequipRune;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/unequip_rune";

	public int HeroIndex { get; set; }

	public int SlotNum { get; set; }

	public int RunePage { get; set; }

	public bool IsPreserve { get; set; }
}
