namespace NShared.RenewEquipSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/renew_equip_skill";

	public int EquipItemSlotIndex { get; set; }

	public int SkillSlotIndex { get; set; }
}
