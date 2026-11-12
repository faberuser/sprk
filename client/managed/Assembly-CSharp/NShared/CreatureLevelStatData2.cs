using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureLevelStatData2 : IDataKeySelector<int>
{
	[Key(0)]
	public int CreatureIndex { get; set; }

	[Key(1)]
	public long PhysicalAttackBase { get; set; }

	[Key(2)]
	public int[] PhysicalAttackFactor { get; set; }

	[Key(3)]
	public long MagicalAttackBase { get; set; }

	[Key(4)]
	public int[] MagicalAttackFactor { get; set; }

	[Key(5)]
	public long MaxHpBase { get; set; }

	[Key(6)]
	public int[] MaxHpFactor { get; set; }

	[Key(7)]
	public long PhysicalDefenseBase { get; set; }

	[Key(8)]
	public int[] PhysicalDefenseFactor { get; set; }

	[Key(9)]
	public long MagicalDefenseBase { get; set; }

	[Key(10)]
	public int[] MagicalDefenseFactor { get; set; }

	int IDataKeySelector<int>.Key => CreatureIndex;

	public long GetStat(StatType statType, int level)
	{
		long num;
		int[] factor;
		switch (statType)
		{
		case StatType.PhysicalAttack:
			num = PhysicalAttackBase;
			factor = PhysicalAttackFactor;
			break;
		case StatType.MagicalAttack:
			num = MagicalAttackBase;
			factor = MagicalAttackFactor;
			break;
		case StatType.MaxHp:
			num = MaxHpBase;
			factor = MaxHpFactor;
			break;
		case StatType.PhysicalDefense:
			num = PhysicalDefenseBase;
			factor = PhysicalDefenseFactor;
			break;
		case StatType.MagicalDefense:
			num = MagicalDefenseBase;
			factor = MagicalDefenseFactor;
			break;
		default:
			return 0L;
		}
		if (!GetLevelFactor(level, factor, out var baseValue, out var levelValue))
		{
			return num;
		}
		return num * levelValue / baseValue;
	}

	private bool GetLevelFactor(int level, int[] factor, out long baseValue, out long levelValue)
	{
		CreatureLevelFactorData data = GameRoot.Instance.CreatureLevelFactorDataContainer.GetData(new CreatureLevelFactorDataKey(factor[0], 1));
		CreatureLevelFactorData data2 = GameRoot.Instance.CreatureLevelFactorDataContainer.GetData(new CreatureLevelFactorDataKey(factor[0], level));
		if (data == null || data2 == null)
		{
			baseValue = 0L;
			levelValue = 0L;
			return false;
		}
		baseValue = data.GetValue(factor[1]);
		levelValue = data2.GetValue(factor[1]);
		if (baseValue != 0L)
		{
			return levelValue != 0;
		}
		return false;
	}

	public override string ToString()
	{
		return "CreatureIndex=" + CreatureIndex;
	}
}
