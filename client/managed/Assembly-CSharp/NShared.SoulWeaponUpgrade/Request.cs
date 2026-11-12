namespace NShared.SoulWeaponUpgrade;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/soul_weapon_upgrade";

	public int EquipItemSlotIndex { get; set; }
}
