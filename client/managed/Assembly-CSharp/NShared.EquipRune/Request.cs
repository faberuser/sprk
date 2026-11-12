namespace NShared.EquipRune;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/equip_rune";

	public int HeroIndex { get; set; }

	public int ItemIndex { get; set; }

	public int EquipItemSlotIndex { get; set; }

	public int SlotNum { get; set; }

	public int RunePage { get; set; }

	public bool IsPreserve { get; set; }
}
