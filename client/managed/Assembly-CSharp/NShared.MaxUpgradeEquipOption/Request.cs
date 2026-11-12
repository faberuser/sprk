namespace NShared.MaxUpgradeEquipOption;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/max_upgrade_equip_option";

	public int EquipItemSlotIndex { get; set; }

	public int MaterialItemIndex { get; set; }

	public long MaterialItemCount { get; set; }
}
