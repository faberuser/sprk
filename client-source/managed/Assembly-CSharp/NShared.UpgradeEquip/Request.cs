namespace NShared.UpgradeEquip;

[Request]
public class Request : NShared.Request, IServiceRequest, IRequest
{
	string IServiceRequest.ServiceName => "equip/upgrade_equip";

	public int EquipItemSlotIndex { get; set; }

	public int[] MaterialSlotIndices { get; set; }

	public int UpgradeLevel { get; set; }
}
