using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PunishmentRuneData : IDataKeySelector<int>
{
	[Key(0)]
	public int PunishmentRuneIndex { get; set; }

	[Key(1)]
	public RuneSlotType[] SlotTypes { get; set; }

	[Key(2)]
	public RuneColorType ColorType { get; set; }

	[Key(3)]
	public int VoidRuneIndex { get; set; }

	[Key(4)]
	public int LegendRuneIndex { get; set; }

	[Key(5)]
	public int ApostleBloodIndex { get; set; }

	[Key(6)]
	public string[] RuneTags { get; set; }

	[Key(7)]
	public string[] GaugePerRune { get; set; }

	[Key(8)]
	public int MaxGauge { get; set; }

	[Key(9)]
	public int MainOptionIndex { get; set; }

	[Key(10)]
	public int ExtraSkillIndex1 { get; set; }

	[Key(11)]
	public int ExtraSkillValue1Min { get; set; }

	[Key(12)]
	public int ExtraSkillValue1Max { get; set; }

	[Key(13)]
	public int ExtraSkill1Step { get; set; }

	[Key(14)]
	public int ExtraSkillIndex2 { get; set; }

	[Key(15)]
	public int ExtraSkillValue2Min { get; set; }

	[Key(16)]
	public int ExtraSkillValue2Max { get; set; }

	[Key(17)]
	public int ExtraSkill2Step { get; set; }

	[Key(18)]
	public int ExtraSkillIndex3 { get; set; }

	[Key(19)]
	public int ExtraSkillValue3Min { get; set; }

	[Key(20)]
	public int ExtraSkillValue3Max { get; set; }

	[Key(21)]
	public int ExtraSkill3Step { get; set; }

	[Key(22)]
	public int ExtraSkillFraction { get; set; }

	[Key(23)]
	public string[] GuideSkillCode { get; set; }

	int IDataKeySelector<int>.Key => PunishmentRuneIndex;

	public static int GetOptionIndex(CreatureTagType tagType)
	{
		switch (tagType)
		{
		case CreatureTagType.Warrior:
		case CreatureTagType.Knight:
			return 1;
		case CreatureTagType.Archer:
		case CreatureTagType.Assassin:
		case CreatureTagType.Mechanic:
			return 2;
		case CreatureTagType.Wizard:
		case CreatureTagType.Priest:
			return 3;
		default:
			return 0;
		}
	}

	public int GetSkillOptionIndex(int index)
	{
		return index switch
		{
			1 => ExtraSkillIndex1, 
			2 => ExtraSkillIndex2, 
			3 => ExtraSkillIndex3, 
			_ => 0, 
		};
	}

	public int GetSkillOptionValueMin(int index)
	{
		return index switch
		{
			1 => ExtraSkillValue1Min, 
			2 => ExtraSkillValue2Min, 
			3 => ExtraSkillValue3Min, 
			_ => 0, 
		};
	}

	public int GetSkillOptionValueMax(int index)
	{
		return index switch
		{
			1 => ExtraSkillValue1Max, 
			2 => ExtraSkillValue2Max, 
			3 => ExtraSkillValue3Max, 
			_ => 0, 
		};
	}

	public int GetSkillOptionStep(int index)
	{
		return index switch
		{
			1 => ExtraSkill1Step, 
			2 => ExtraSkill2Step, 
			3 => ExtraSkill3Step, 
			_ => 0, 
		};
	}
}
