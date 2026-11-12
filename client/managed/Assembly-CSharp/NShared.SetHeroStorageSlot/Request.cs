namespace NShared.SetHeroStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero_storage_slot/set_hero_storage_slot";

	public string StorageKey { get; set; }
}
