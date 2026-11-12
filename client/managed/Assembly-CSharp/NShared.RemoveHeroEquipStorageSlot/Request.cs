namespace NShared.RemoveHeroEquipStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_storage_slot/remove_hero_equip_storage_slot";

	public int EquipStorageSlotIndex { get; set; }
}
