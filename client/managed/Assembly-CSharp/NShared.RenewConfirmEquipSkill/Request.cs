namespace NShared.RenewConfirmEquipSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/renew_confirm_equip_skill";

	public int EquipItemSlotIndex { get; set; }

	public int ConfirmSkillIndex { get; set; }

	public int SkillSlotIndex { get; set; }

	public bool IsNew { get; set; }
}
