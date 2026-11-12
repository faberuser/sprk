namespace NShared.SetEquipStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_storage_slot/set_equip_storage_slot";

	public int EquipStorageSlotIndex { get; set; }

	public int HeroIndex { get; set; }
}
