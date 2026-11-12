namespace NShared;

[JsonSerializable(Javascript = true)]
public class ValanceRenewSkillInfo
{
	public int RenewSkillEquipItemSlotIndex { get; set; }

	public int RenewSkillResultIndex { get; set; }

	public int RenewSkillResultStep { get; set; }

	public int RenewSkillSlotIndex { get; set; }
}
