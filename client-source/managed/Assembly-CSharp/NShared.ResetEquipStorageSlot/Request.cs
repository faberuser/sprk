namespace NShared.ResetEquipStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_storage_slot/reset_equip_storage_slot";

	public int EquipStorageSlotIndex { get; set; }
}
