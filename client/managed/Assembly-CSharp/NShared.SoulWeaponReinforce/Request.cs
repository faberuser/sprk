namespace NShared.SoulWeaponReinforce;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/soul_weapon_reinforce";

	public int EquipItemSlotIndex { get; set; }
}
