namespace NShared.SoulWeaponTransitionTicket;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/soul_weapon_transition_ticket";

	public int ItemIndex { get; set; }

	public int SourceEquipItemSlotIndex { get; set; }

	public int TargetEquipItemSlotIndex { get; set; }
}
