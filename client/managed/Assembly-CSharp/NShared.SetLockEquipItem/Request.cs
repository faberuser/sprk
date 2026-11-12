namespace NShared.SetLockEquipItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/set_lock_equip_item";

	public int EquipItemSlotIndex { get; set; }

	public byte Locked { get; set; }
}
