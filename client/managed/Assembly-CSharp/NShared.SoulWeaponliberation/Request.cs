namespace NShared.SoulWeaponliberation;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/soul_weapon_liberation";

	public int EquipItemSlotIndex { get; set; }
}
