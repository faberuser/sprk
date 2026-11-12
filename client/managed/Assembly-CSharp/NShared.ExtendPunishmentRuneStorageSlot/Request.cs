namespace NShared.ExtendPunishmentRuneStorageSlot;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/extend_punishment_rune_storage_slot";

	public int ReqGem { get; set; }
}
