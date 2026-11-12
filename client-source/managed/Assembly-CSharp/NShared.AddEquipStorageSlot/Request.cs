namespace NShared.AddEquipStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_storage_slot/add_equip_storage_slot";

	public int OldEquipStorageSlotIndex { get; set; }

	public int EquipStorageSlotIndex { get; set; }

	public int HeroIndex { get; set; }

	public string Name { get; set; }

	public int[] HeroPartIndex { get; set; }

	public int[] EquipItemSlotIndex { get; set; }
}
