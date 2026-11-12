using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipUpgradeStatData : IDataKeySelector<EquipUpgradeStatKey>
{
	[Key(0)]
	public int Star { get; set; }

	[Key(1)]
	public int DetailIndex { get; set; }

	[Key(2)]
	public int Level { get; set; }

	[Key(3)]
	public int PhysicalAttack { get; set; }

	[Key(4)]
	public int MagicalAttack { get; set; }

	[Key(5)]
	public int MaxHp { get; set; }

	[Key(6)]
	public int PhysicalDefense { get; set; }

	[Key(7)]
	public int MagicalDefense { get; set; }

	[Key(8)]
	public int AddPhysicalAttackR { get; set; }

	[Key(9)]
	public int AddMagicalAttackR { get; set; }

	[Key(10)]
	public int AddMaxHpR { get; set; }

	[Key(11)]
	public int AddPhysicalDefenseR { get; set; }

	[Key(12)]
	public int AddMagicalDefenseR { get; set; }

	[Key(13)]
	public int AddValuePhysicalAttack { get; set; }

	[Key(14)]
	public int AddValueMagicalAttack { get; set; }

	[Key(15)]
	public int AddValueMaxHp { get; set; }

	[Key(16)]
	public int AddValuePhysicalDefense { get; set; }

	[Key(17)]
	public int AddValueMagicalDefense { get; set; }

	[Key(18)]
	public int AddValueAddPhysicalAttackR { get; set; }

	[Key(19)]
	public int AddValueAddMagicalAttackR { get; set; }

	[Key(20)]
	public int AddValueAddMaxHpR { get; set; }

	[Key(21)]
	public int AddValueAddPhysicalDefenseR { get; set; }

	[Key(22)]
	public int AddValueAddMagicalDefenseR { get; set; }

	[Key(23)]
	public int AddValuePhysicalCriticalChance { get; set; }

	[Key(24)]
	public int AddValueMagicalCriticalChance { get; set; }

	[Key(25)]
	public int AddValuePhysicalToughness { get; set; }

	[Key(26)]
	public int AddValueMagicalToughness { get; set; }

	EquipUpgradeStatKey IDataKeySelector<EquipUpgradeStatKey>.Key => new EquipUpgradeStatKey(Star, DetailIndex, Level);

	public override string ToString()
	{
		return this.GETKEY().ToString();
	}

	public int GetStat(StatType statType)
	{
		return statType switch
		{
			StatType.PhysicalAttack => PhysicalAttack, 
			StatType.MagicalAttack => MagicalAttack, 
			StatType.MaxHp => MaxHp, 
			StatType.PhysicalDefense => PhysicalDefense, 
			StatType.MagicalDefense => MagicalDefense, 
			StatType.AddPhysicalAttackR => AddPhysicalAttackR, 
			StatType.AddMagicalAttackR => AddMagicalAttackR, 
			StatType.AddMaxHpR => AddMaxHpR, 
			StatType.AddPhysicalDefenseR => AddPhysicalDefenseR, 
			StatType.AddMagicalDefenseR => AddMagicalDefenseR, 
			_ => 1000, 
		};
	}

	public int GetAddStat(StatType statType)
	{
		return statType switch
		{
			StatType.PhysicalAttack => AddValuePhysicalAttack, 
			StatType.MagicalAttack => AddValueMagicalAttack, 
			StatType.MaxHp => AddValueMaxHp, 
			StatType.PhysicalDefense => AddValuePhysicalDefense, 
			StatType.MagicalDefense => AddValueMagicalDefense, 
			StatType.AddPhysicalAttackR => AddValueAddPhysicalAttackR, 
			StatType.AddMagicalAttackR => AddValueAddMagicalAttackR, 
			StatType.AddMaxHpR => AddValueAddMaxHpR, 
			StatType.AddPhysicalDefenseR => AddValueAddPhysicalDefenseR, 
			StatType.AddMagicalDefenseR => AddValueAddMagicalDefenseR, 
			StatType.PhysicalCriticalChance => AddValuePhysicalCriticalChance, 
			StatType.MagicalCriticalChance => AddValueMagicalCriticalChance, 
			StatType.PhysicalToughness => AddValuePhysicalToughness, 
			StatType.MagicalToughness => AddValueMagicalToughness, 
			_ => 0, 
		};
	}
}
