namespace NShared.SetCostumeStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/set_costume_storage_slot";

	public int HeroIndex { get; set; }

	public int CostumeStorageSlotIndex { get; set; }
}
