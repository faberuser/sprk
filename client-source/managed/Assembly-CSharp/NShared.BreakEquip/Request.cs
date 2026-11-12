namespace NShared.BreakEquip;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/break_equip";

	public int[] EquipItemSlotIndices { get; set; }
}
