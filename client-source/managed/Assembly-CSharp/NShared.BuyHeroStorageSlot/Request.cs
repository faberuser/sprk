namespace NShared.BuyHeroStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero_storage_slot/buy_hero_storage_slot";

	public string StorageKey { get; set; }

	public string StorageName { get; set; }
}
