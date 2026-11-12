namespace NShared.SoulWeaponAbilityTicket;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/soul_weapon_ability_ticket";

	public int ItemIndex { get; set; }

	public int TargetEquipItemSlotIndex { get; set; }
}
