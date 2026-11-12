namespace NShared.ChangeHeroStorageSlotName;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero_storage_slot/change_hero_storage_slot_name";

	public string StorageKey { get; set; }

	public string Name { get; set; }
}
