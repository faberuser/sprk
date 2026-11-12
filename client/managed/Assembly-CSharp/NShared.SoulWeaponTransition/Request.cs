namespace NShared.SoulWeaponTransition;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/soul_weapon_transition";

	public int EquipItemSlotIndex { get; set; }

	public int ReceiveEquipItemSlotIndex { get; set; }
}
