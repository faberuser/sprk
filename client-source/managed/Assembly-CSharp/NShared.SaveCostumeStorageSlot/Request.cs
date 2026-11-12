namespace NShared.SaveCostumeStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/save_costume_storage_slot";

	public int HeroIndex { get; set; }

	public int CostumeStorageSlotIndex { get; set; }
}
