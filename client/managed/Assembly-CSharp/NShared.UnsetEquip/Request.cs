namespace NShared.UnsetEquip;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/unset_equip";

	public int[] EquipItemSlotIndex { get; set; }
}
