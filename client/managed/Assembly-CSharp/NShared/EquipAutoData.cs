using System;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipAutoData : IDataKeySelector<EquipAutoKey>
{
	[Key(0)]
	public CreatureTagType CreatureTagType { get; set; }

	[Key(1)]
	public int EquipSlotIndex { get; set; }

	[Key(2)]
	public StatType[] MainStatType1 { get; set; }

	[Key(3)]
	public int MainStatFactor1 { get; set; }

	[Key(4)]
	public StatType[] MainStatType2 { get; set; }

	[Key(5)]
	public int MainStatFactor2 { get; set; }

	[Key(6)]
	public StatType[] MainStatType3 { get; set; }

	[Key(7)]
	public int MainStatFactor3 { get; set; }

	[Key(8)]
	public StatType[] ExtraStatType { get; set; }

	[Key(9)]
	public string[] ExtraStatApplyCondition { get; set; }

	EquipAutoKey IDataKeySelector<EquipAutoKey>.Key => new EquipAutoKey(CreatureTagType, EquipSlotIndex);

	private long GetMaxStat(StatData statData, StatType[] typeArray)
	{
		if (typeArray == null)
		{
			return 0L;
		}
		long num = 0L;
		for (int i = 0; i < typeArray.Length; i++)
		{
			long stat = statData.GetStat(typeArray[i]);
			num = Math.Max(num, stat);
		}
		return num;
	}

	public long GetMaxStat1(StatData statData)
	{
		return GetMaxStat(statData, MainStatType1);
	}

	public long GetMaxStat2(StatData statData)
	{
		return GetMaxStat(statData, MainStatType2);
	}

	public long GetMaxStat3(StatData statData)
	{
		return GetMaxStat(statData, MainStatType3);
	}
}
