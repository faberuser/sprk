namespace NShared.AddHeroStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero_storage_slot/add_hero_storage_slot";

	public string StorageKey { get; set; }

	public string StorageName { get; set; }
}
