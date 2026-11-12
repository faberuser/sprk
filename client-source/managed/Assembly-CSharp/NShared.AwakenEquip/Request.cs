using NShared.UpgradeEquip;

namespace NShared.AwakenEquip;

[Request]
public class Request : NShared.Request, IServiceRequest, IRequest
{
	string IServiceRequest.ServiceName => "equip/awaken_equip";

	public int EquipItemSlotIndex { get; set; }

	public int[] MaterialSlotIndices { get; set; }

	public int[] MaterialItemInfors { get; set; }
}
