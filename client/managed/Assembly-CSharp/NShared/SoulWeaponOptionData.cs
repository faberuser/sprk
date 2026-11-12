using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponOptionData : IDataKeySelector<CreatureTagType>
{
	[Key(0)]
	public CreatureTagType TagType { get; set; }

	[Key(1)]
	public int OptionIndex1 { get; set; }

	[Key(2)]
	public int OptionIndex2 { get; set; }

	[Key(3)]
	public StatType[] OptionStatType1 { get; set; }

	[Key(4)]
	public StatType[] OptionStatType2 { get; set; }

	[Key(5)]
	public int OptionRatioPerValue1 { get; set; }

	[Key(6)]
	public int OptionRatioPerValue2 { get; set; }

	CreatureTagType IDataKeySelector<CreatureTagType>.Key => TagType;

	public StatType[] GetOptionStatTypes(int index)
	{
		return index switch
		{
			0 => OptionStatType1, 
			1 => OptionStatType2, 
			_ => null, 
		};
	}

	public int GetOptionRatioPerValue(ESoulWeaponStatType statType)
	{
		return statType switch
		{
			ESoulWeaponStatType.Attack => OptionRatioPerValue1, 
			ESoulWeaponStatType.Hp => OptionRatioPerValue2, 
			_ => -1, 
		};
	}
}
