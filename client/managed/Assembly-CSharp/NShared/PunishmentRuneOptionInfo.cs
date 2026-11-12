namespace NShared;

[JsonSerializable(Javascript = true)]
public class PunishmentRuneOptionInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => EquipItemSlotIndex;

	public int EquipItemSlotIndex { get; set; }

	public int MainOptionIndex { get; set; }

	public int MainOptionValue { get; set; }

	public int ExtraSkillIndex1 { get; set; }

	public int ExtraSkillIndex2 { get; set; }

	public int ExtraSkillIndex3 { get; set; }

	public int ExtraSkillValue1 { get; set; }

	public int ExtraSkillValue2 { get; set; }

	public int ExtraSkillValue3 { get; set; }

	public int GetExtraSkillIndex(int index)
	{
		return index switch
		{
			1 => ExtraSkillIndex1, 
			2 => ExtraSkillIndex2, 
			3 => ExtraSkillIndex3, 
			_ => 0, 
		};
	}

	public int GetExtraSkillValue(int index)
	{
		return index switch
		{
			1 => ExtraSkillValue1, 
			2 => ExtraSkillValue2, 
			3 => ExtraSkillValue3, 
			_ => 0, 
		};
	}
}
