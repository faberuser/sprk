using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class LevelStatData
{
	[IgnoreMember]
	public StatContainer StatContainer = new StatContainer();

	[Key(0)]
	public int PhysicalAttack
	{
		get
		{
			return (int)GetStat(StatType.PhysicalAttack);
		}
		set
		{
			SetStat(StatType.PhysicalAttack, value);
		}
	}

	[Key(1)]
	public int MagicalAttack
	{
		get
		{
			return (int)GetStat(StatType.MagicalAttack);
		}
		set
		{
			SetStat(StatType.MagicalAttack, value);
		}
	}

	[Key(2)]
	public long MaxHp
	{
		get
		{
			return GetStat(StatType.MaxHp);
		}
		set
		{
			SetStat(StatType.MaxHp, value);
		}
	}

	[Key(3)]
	public int PhysicalDefense
	{
		get
		{
			return (int)GetStat(StatType.PhysicalDefense);
		}
		set
		{
			SetStat(StatType.PhysicalDefense, value);
		}
	}

	[Key(4)]
	public int MagicalDefense
	{
		get
		{
			return (int)GetStat(StatType.MagicalDefense);
		}
		set
		{
			SetStat(StatType.MagicalDefense, value);
		}
	}

	public long GetStat(StatType statType)
	{
		return StatContainer.GetStat(statType);
	}

	public void SetStat(StatType statType, long value)
	{
		StatContainer.SetStat(statType, value);
	}

	public void SetStat(StatType statType, int value)
	{
		StatContainer.SetStat(statType, value);
	}

	public void SetStat(StatType statType, short value)
	{
		StatContainer.SetStat(statType, value);
	}

	public void AddStat(StatType statType, long value)
	{
		StatContainer.AddStat(statType, value);
	}

	public void AddStat(StatType statType, int value)
	{
		StatContainer.AddStat(statType, value);
	}

	public void AddStat(StatType statType, short value)
	{
		StatContainer.AddStat(statType, value);
	}

	public void MultStat(StatType statType, int value)
	{
		StatContainer.MultStat(statType, value);
	}
}
