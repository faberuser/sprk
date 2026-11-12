namespace NShared.UpgradeEquipTier;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/upgrade_equip_tier";

	public int EquipItemSlotIndex { get; set; }
}
