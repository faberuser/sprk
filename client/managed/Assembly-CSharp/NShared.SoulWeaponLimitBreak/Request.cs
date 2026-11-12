using NShared.UpgradeEquip;

namespace NShared.SoulWeaponLimitBreak;

[Request]
public class Request : NShared.Request, IServiceRequest, IRequest
{
	string IServiceRequest.ServiceName => "equip/soul_weapon_limit_break";

	public int EquipItemSlotIndex { get; set; }

	public int[] MaterialSlotIndices { get; set; }

	public int[] MaterialItemInfors { get; set; }

	public string[] Admin { get; set; }
}
