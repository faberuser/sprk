namespace NShared.ChangeEquipStorageSlotName;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_storage_slot/change_equip_storage_slot_name";

	public int EquipStorageSlotIndex { get; set; }

	public string Name { get; set; }
}
