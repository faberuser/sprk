using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MessagePack;
using NShared.NLocalization;
using NVespa;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable]
public class StatData : LevelStatData, IEnumerable<KeyValuePair<StatType, long>>, IEnumerable
{
	public class MultesStatInfo
	{
		public StatType Type;

		public long Value;
	}

	public static StatData NullStat;

	public static StatType[] StatTypes;

	private static Dictionary<StatType, StatType> _factorStats;

	private static StatType[] _weaponMainStats;

	private static StatType[] _armorMainStats;

	private static StatType[] _secondGearMainStats;

	private static StatType[] _accessoryMainStats;

	private static StatType[] _totalMainStats;

	private static StatType[] _physicalHeroMainStats;

	private static StatType[] _magicalHeroMainStats;

	private static StatType[] _visibleStats;

	[Key(5)]
	public short PhysicalCriticalChance
	{
		get
		{
			return (short)GetStat(StatType.PhysicalCriticalChance);
		}
		set
		{
			SetStat(StatType.PhysicalCriticalChance, value);
		}
	}

	[Key(6)]
	public short PhysicalCriticalPower
	{
		get
		{
			return (short)GetStat(StatType.PhysicalCriticalPower);
		}
		set
		{
			SetStat(StatType.PhysicalCriticalPower, value);
		}
	}

	[Key(7)]
	public short PhysicalDodgeChance
	{
		get
		{
			return (short)GetStat(StatType.PhysicalDodgeChance);
		}
		set
		{
			SetStat(StatType.PhysicalDodgeChance, value);
		}
	}

	[Key(8)]
	public short PhysicalBlockChance
	{
		get
		{
			return (short)GetStat(StatType.PhysicalBlockChance);
		}
		set
		{
			SetStat(StatType.PhysicalBlockChance, value);
		}
	}

	[Key(9)]
	public short PhysicalBlockPower
	{
		get
		{
			return (short)GetStat(StatType.PhysicalBlockPower);
		}
		set
		{
			SetStat(StatType.PhysicalBlockPower, value);
		}
	}

	[Key(10)]
	public short PhysicalHitChance
	{
		get
		{
			return (short)GetStat(StatType.PhysicalHitChance);
		}
		set
		{
			SetStat(StatType.PhysicalHitChance, value);
		}
	}

	[Key(11)]
	public short PhysicalPiercePower
	{
		get
		{
			return (short)GetStat(StatType.PhysicalPiercePower);
		}
		set
		{
			SetStat(StatType.PhysicalPiercePower, value);
		}
	}

	[Key(12)]
	public short PhysicalWeakness
	{
		get
		{
			return (short)GetStat(StatType.PhysicalWeakness);
		}
		set
		{
			SetStat(StatType.PhysicalWeakness, value);
		}
	}

	[Key(13)]
	public short PhysicalToughness
	{
		get
		{
			return (short)GetStat(StatType.PhysicalToughness);
		}
		set
		{
			SetStat(StatType.PhysicalToughness, value);
		}
	}

	[Key(14)]
	public short PhysicalCriticalDodgeChance
	{
		get
		{
			return (short)GetStat(StatType.PhysicalCriticalDodgeChance);
		}
		set
		{
			SetStat(StatType.PhysicalCriticalDodgeChance, value);
		}
	}

	[Key(15)]
	public short PhysicalAttackPower
	{
		get
		{
			return (short)GetStat(StatType.PhysicalAttackPower);
		}
		set
		{
			SetStat(StatType.PhysicalAttackPower, value);
		}
	}

	[Key(16)]
	public short PhysicalDefensePower
	{
		get
		{
			return (short)GetStat(StatType.PhysicalDefensePower);
		}
		set
		{
			SetStat(StatType.PhysicalDefensePower, value);
		}
	}

	[Key(17)]
	public short PhysicalDotPower
	{
		get
		{
			return (short)GetStat(StatType.PhysicalDotPower);
		}
		set
		{
			SetStat(StatType.PhysicalDotPower, value);
		}
	}

	[Key(18)]
	public short PhysicalIgnoreBlockPower
	{
		get
		{
			return (short)GetStat(StatType.PhysicalIgnoreBlockPower);
		}
		set
		{
			SetStat(StatType.PhysicalIgnoreBlockPower, value);
		}
	}

	[Key(19)]
	public short PhysicalIgnoreDefensePower
	{
		get
		{
			return (short)GetStat(StatType.PhysicalIgnoreDefensePower);
		}
		set
		{
			SetStat(StatType.PhysicalIgnoreDefensePower, value);
		}
	}

	[Key(20)]
	public short MagicalCriticalChance
	{
		get
		{
			return (short)GetStat(StatType.MagicalCriticalChance);
		}
		set
		{
			SetStat(StatType.MagicalCriticalChance, value);
		}
	}

	[Key(21)]
	public short MagicalCriticalPower
	{
		get
		{
			return (short)GetStat(StatType.MagicalCriticalPower);
		}
		set
		{
			SetStat(StatType.MagicalCriticalPower, value);
		}
	}

	[Key(22)]
	public short MagicalDodgeChance
	{
		get
		{
			return (short)GetStat(StatType.MagicalDodgeChance);
		}
		set
		{
			SetStat(StatType.MagicalDodgeChance, value);
		}
	}

	[Key(23)]
	public short MagicalBlockChance
	{
		get
		{
			return (short)GetStat(StatType.MagicalBlockChance);
		}
		set
		{
			SetStat(StatType.MagicalBlockChance, value);
		}
	}

	[Key(24)]
	public short MagicalBlockPower
	{
		get
		{
			return (short)GetStat(StatType.MagicalBlockPower);
		}
		set
		{
			SetStat(StatType.MagicalBlockPower, value);
		}
	}

	[Key(25)]
	public short MagicalHitChance
	{
		get
		{
			return (short)GetStat(StatType.MagicalHitChance);
		}
		set
		{
			SetStat(StatType.MagicalHitChance, value);
		}
	}

	[Key(26)]
	public short MagicalPiercePower
	{
		get
		{
			return (short)GetStat(StatType.MagicalPiercePower);
		}
		set
		{
			SetStat(StatType.MagicalPiercePower, value);
		}
	}

	[Key(27)]
	public short MagicalWeakness
	{
		get
		{
			return (short)GetStat(StatType.MagicalWeakness);
		}
		set
		{
			SetStat(StatType.MagicalWeakness, value);
		}
	}

	[Key(28)]
	public short MagicalToughness
	{
		get
		{
			return (short)GetStat(StatType.MagicalToughness);
		}
		set
		{
			SetStat(StatType.MagicalToughness, value);
		}
	}

	[Key(29)]
	public short MagicalCriticalDodgeChance
	{
		get
		{
			return (short)GetStat(StatType.MagicalCriticalDodgeChance);
		}
		set
		{
			SetStat(StatType.MagicalCriticalDodgeChance, value);
		}
	}

	[Key(30)]
	public short HealPower
	{
		get
		{
			return (short)GetStat(StatType.HealPower);
		}
		set
		{
			SetStat(StatType.HealPower, value);
		}
	}

	[Key(31)]
	public short RecoverPower
	{
		get
		{
			return (short)GetStat(StatType.RecoverPower);
		}
		set
		{
			SetStat(StatType.RecoverPower, value);
		}
	}

	[Key(32)]
	public short MagicalAttackPower
	{
		get
		{
			return (short)GetStat(StatType.MagicalAttackPower);
		}
		set
		{
			SetStat(StatType.MagicalAttackPower, value);
		}
	}

	[Key(33)]
	public short MagicalDefensePower
	{
		get
		{
			return (short)GetStat(StatType.MagicalDefensePower);
		}
		set
		{
			SetStat(StatType.MagicalDefensePower, value);
		}
	}

	[Key(34)]
	public short MagicalDotPower
	{
		get
		{
			return (short)GetStat(StatType.MagicalDotPower);
		}
		set
		{
			SetStat(StatType.MagicalDotPower, value);
		}
	}

	[Key(35)]
	public short MagicalIgnoreBlockPower
	{
		get
		{
			return (short)GetStat(StatType.MagicalIgnoreBlockPower);
		}
		set
		{
			SetStat(StatType.MagicalIgnoreBlockPower, value);
		}
	}

	[Key(36)]
	public short MagicalIgnoreDefensePower
	{
		get
		{
			return (short)GetStat(StatType.MagicalIgnoreDefensePower);
		}
		set
		{
			SetStat(StatType.MagicalIgnoreDefensePower, value);
		}
	}

	[Key(37)]
	public short AntiCcChance
	{
		get
		{
			return (short)GetStat(StatType.AntiCcChance);
		}
		set
		{
			SetStat(StatType.AntiCcChance, value);
		}
	}

	[Key(38)]
	public short AntiDebuffChance
	{
		get
		{
			return (short)GetStat(StatType.AntiDebuffChance);
		}
		set
		{
			SetStat(StatType.AntiDebuffChance, value);
		}
	}

	[Key(39)]
	public short HitCcChance
	{
		get
		{
			return (short)GetStat(StatType.HitCcChance);
		}
		set
		{
			SetStat(StatType.HitCcChance, value);
		}
	}

	[Key(40)]
	public short HitDebuffChance
	{
		get
		{
			return (short)GetStat(StatType.HitDebuffChance);
		}
		set
		{
			SetStat(StatType.HitDebuffChance, value);
		}
	}

	[Key(41)]
	public int HpStealPower
	{
		get
		{
			return (int)GetStat(StatType.HpStealPower);
		}
		set
		{
			SetStat(StatType.HpStealPower, value);
		}
	}

	[Key(42)]
	public int HpOnDamage
	{
		get
		{
			return (int)GetStat(StatType.HpOnDamage);
		}
		set
		{
			SetStat(StatType.HpOnDamage, value);
		}
	}

	[Key(43)]
	public int HpOnAttack
	{
		get
		{
			return (int)GetStat(StatType.HpOnAttack);
		}
		set
		{
			SetStat(StatType.HpOnAttack, value);
		}
	}

	[Key(44)]
	public int HpOnKill
	{
		get
		{
			return (int)GetStat(StatType.HpOnKill);
		}
		set
		{
			SetStat(StatType.HpOnKill, value);
		}
	}

	[Key(45)]
	public int HpOnWave
	{
		get
		{
			return (int)GetStat(StatType.HpOnWave);
		}
		set
		{
			SetStat(StatType.HpOnWave, value);
		}
	}

	[Key(46)]
	public int HpOnTime
	{
		get
		{
			return (int)GetStat(StatType.HpOnTime);
		}
		set
		{
			SetStat(StatType.HpOnTime, value);
		}
	}

	[Key(47)]
	public short HpRatioOnDamage
	{
		get
		{
			return (short)GetStat(StatType.HpRatioOnDamage);
		}
		set
		{
			SetStat(StatType.HpRatioOnDamage, value);
		}
	}

	[Key(48)]
	public short HpRatioOnAttack
	{
		get
		{
			return (short)GetStat(StatType.HpRatioOnAttack);
		}
		set
		{
			SetStat(StatType.HpRatioOnAttack, value);
		}
	}

	[Key(49)]
	public short HpRatioOnKill
	{
		get
		{
			return (short)GetStat(StatType.HpRatioOnKill);
		}
		set
		{
			SetStat(StatType.HpRatioOnKill, value);
		}
	}

	[Key(50)]
	public short HpRatioOnWave
	{
		get
		{
			return (short)GetStat(StatType.HpRatioOnWave);
		}
		set
		{
			SetStat(StatType.HpRatioOnWave, value);
		}
	}

	[Key(51)]
	public short HpRatioOnTime
	{
		get
		{
			return (short)GetStat(StatType.HpRatioOnTime);
		}
		set
		{
			SetStat(StatType.HpRatioOnTime, value);
		}
	}

	[Key(52)]
	public int MaxMp
	{
		get
		{
			return (int)GetStat(StatType.MaxMp);
		}
		set
		{
			SetStat(StatType.MaxMp, value);
		}
	}

	[Key(53)]
	public short MpOnDamage
	{
		get
		{
			return (short)GetStat(StatType.MpOnDamage);
		}
		set
		{
			SetStat(StatType.MpOnDamage, value);
		}
	}

	[Key(54)]
	public short MpOnAttack
	{
		get
		{
			return (short)GetStat(StatType.MpOnAttack);
		}
		set
		{
			SetStat(StatType.MpOnAttack, value);
		}
	}

	[Key(55)]
	public short MpOnKill
	{
		get
		{
			return (short)GetStat(StatType.MpOnKill);
		}
		set
		{
			SetStat(StatType.MpOnKill, value);
		}
	}

	[Key(56)]
	public short MpOnWave
	{
		get
		{
			return (short)GetStat(StatType.MpOnWave);
		}
		set
		{
			SetStat(StatType.MpOnWave, value);
		}
	}

	[Key(57)]
	public short MpOnTime
	{
		get
		{
			return (short)GetStat(StatType.MpOnTime);
		}
		set
		{
			SetStat(StatType.MpOnTime, value);
		}
	}

	[Key(58)]
	public short MpRatioOnDamage
	{
		get
		{
			return (short)GetStat(StatType.MpRatioOnDamage);
		}
		set
		{
			SetStat(StatType.MpRatioOnDamage, value);
		}
	}

	[Key(59)]
	public short MpRatioOnAttack
	{
		get
		{
			return (short)GetStat(StatType.MpRatioOnAttack);
		}
		set
		{
			SetStat(StatType.MpRatioOnAttack, value);
		}
	}

	[Key(60)]
	public short MpRatioOnTime
	{
		get
		{
			return (short)GetStat(StatType.MpRatioOnTime);
		}
		set
		{
			SetStat(StatType.MpRatioOnTime, value);
		}
	}

	[Key(61)]
	public short MpRegenRatioM
	{
		get
		{
			return (short)GetStat(StatType.MpRegenRatioM);
		}
		set
		{
			SetStat(StatType.MpRegenRatioM, value);
		}
	}

	[Key(62)]
	public int AttackSpeed
	{
		get
		{
			return (int)GetStat(StatType.AttackSpeed);
		}
		set
		{
			SetStat(StatType.AttackSpeed, value);
		}
	}

	[Key(63)]
	public int MoveSpeedMms
	{
		get
		{
			return (int)GetStat(StatType.MoveSpeedMms);
		}
		set
		{
			SetStat(StatType.MoveSpeedMms, value);
		}
	}

	[Key(64)]
	public int ShieldPowerR
	{
		get
		{
			return (int)GetStat(StatType.ShieldPowerR);
		}
		set
		{
			SetStat(StatType.ShieldPowerR, value);
		}
	}

	[Key(65)]
	public int AddMaxHpR
	{
		get
		{
			return (int)GetStat(StatType.AddMaxHpR);
		}
		set
		{
			SetStat(StatType.AddMaxHpR, value);
		}
	}

	[Key(66)]
	public int AddPhysicalAttackR
	{
		get
		{
			return (int)GetStat(StatType.AddPhysicalAttackR);
		}
		set
		{
			SetStat(StatType.AddPhysicalAttackR, value);
		}
	}

	[Key(67)]
	public int AddPhysicalDefenseR
	{
		get
		{
			return (int)GetStat(StatType.AddPhysicalDefenseR);
		}
		set
		{
			SetStat(StatType.AddPhysicalDefenseR, value);
		}
	}

	[Key(68)]
	public int AddMagicalAttackR
	{
		get
		{
			return (int)GetStat(StatType.AddMagicalAttackR);
		}
		set
		{
			SetStat(StatType.AddMagicalAttackR, value);
		}
	}

	[Key(69)]
	public int AddMagicalDefenseR
	{
		get
		{
			return (int)GetStat(StatType.AddMagicalDefenseR);
		}
		set
		{
			SetStat(StatType.AddMagicalDefenseR, value);
		}
	}

	[Key(70)]
	public int AddMpOnDamageR
	{
		get
		{
			return (int)GetStat(StatType.AddMpOnDamageR);
		}
		set
		{
			SetStat(StatType.AddMpOnDamageR, value);
		}
	}

	[Key(71)]
	public int AddMpOnAttackR
	{
		get
		{
			return (int)GetStat(StatType.AddMpOnAttackR);
		}
		set
		{
			SetStat(StatType.AddMpOnAttackR, value);
		}
	}

	[Key(72)]
	public int AddMpOnTimeR
	{
		get
		{
			return (int)GetStat(StatType.AddMpOnTimeR);
		}
		set
		{
			SetStat(StatType.AddMpOnTimeR, value);
		}
	}

	[Key(73)]
	public int SkillLevelAllSlot
	{
		get
		{
			return (int)GetStat(StatType.SkillLevelAllSlot);
		}
		set
		{
			SetStat(StatType.SkillLevelAllSlot, value);
		}
	}

	[Key(74)]
	public int SkillLevelSlot1
	{
		get
		{
			return (int)GetStat(StatType.SkillLevelSlot1);
		}
		set
		{
			SetStat(StatType.SkillLevelSlot1, value);
		}
	}

	[Key(75)]
	public int SkillLevelSlot2
	{
		get
		{
			return (int)GetStat(StatType.SkillLevelSlot2);
		}
		set
		{
			SetStat(StatType.SkillLevelSlot2, value);
		}
	}

	[Key(76)]
	public int SkillLevelSlot3
	{
		get
		{
			return (int)GetStat(StatType.SkillLevelSlot3);
		}
		set
		{
			SetStat(StatType.SkillLevelSlot3, value);
		}
	}

	[Key(77)]
	public int SkillLevelSlot4
	{
		get
		{
			return (int)GetStat(StatType.SkillLevelSlot4);
		}
		set
		{
			SetStat(StatType.SkillLevelSlot4, value);
		}
	}

	[Key(78)]
	public int SoulWeaponMaxUseCount
	{
		get
		{
			return (int)GetStat(StatType.SoulWeaponMaxUseCount);
		}
		set
		{
			SetStat(StatType.SoulWeaponMaxUseCount, value);
		}
	}

	[Key(201)]
	public int ThreatGain
	{
		get
		{
			return (int)GetStat(StatType.ThreatGain);
		}
		set
		{
			SetStat(StatType.ThreatGain, value);
		}
	}

	[Key(202)]
	public int ThreatGainR
	{
		get
		{
			return (int)GetStat(StatType.ThreatGainR);
		}
		set
		{
			SetStat(StatType.ThreatGainR, value);
		}
	}

	static StatData()
	{
		NullStat = new StatData();
		_factorStats = new Dictionary<StatType, StatType>(StatTypeComparer.Default);
		_weaponMainStats = new StatType[2]
		{
			StatType.PhysicalAttack,
			StatType.MagicalAttack
		};
		_armorMainStats = new StatType[1] { StatType.PhysicalDefense };
		_secondGearMainStats = new StatType[1] { StatType.MagicalDefense };
		_accessoryMainStats = new StatType[14]
		{
			StatType.PhysicalAttack,
			StatType.MagicalAttack,
			StatType.PhysicalDefense,
			StatType.MagicalDefense,
			StatType.MaxHp,
			StatType.AddPhysicalAttackR,
			StatType.AddMagicalAttackR,
			StatType.AddMaxHpR,
			StatType.AddPhysicalDefenseR,
			StatType.AddMagicalDefenseR,
			StatType.PhysicalCriticalChance,
			StatType.MagicalCriticalChance,
			StatType.PhysicalToughness,
			StatType.MagicalToughness
		};
		_totalMainStats = null;
		_physicalHeroMainStats = new StatType[4]
		{
			StatType.MaxHp,
			StatType.PhysicalAttack,
			StatType.PhysicalDefense,
			StatType.MagicalDefense
		};
		_magicalHeroMainStats = new StatType[4]
		{
			StatType.MaxHp,
			StatType.MagicalAttack,
			StatType.PhysicalDefense,
			StatType.MagicalDefense
		};
		_visibleStats = null;
		StatTypes = Enum.GetValues(typeof(StatType)).Cast<StatType>().ToArray();
		_factorStats[StatType.AddMaxHpR] = StatType.MaxHp;
		_factorStats[StatType.AddPhysicalAttackR] = StatType.PhysicalAttack;
		_factorStats[StatType.AddPhysicalDefenseR] = StatType.PhysicalDefense;
		_factorStats[StatType.AddMagicalAttackR] = StatType.MagicalAttack;
		_factorStats[StatType.AddMagicalDefenseR] = StatType.MagicalDefense;
		_factorStats[StatType.AddMpOnAttackR] = StatType.MpOnAttack;
		_factorStats[StatType.AddMpOnDamageR] = StatType.MpOnDamage;
		_factorStats[StatType.AddMpOnTimeR] = StatType.MpOnTime;
	}

	public void AddStat(StatData rhs, StatType[] filteredTypes = null)
	{
		if (rhs != null)
		{
			StatType[] array = ((filteredTypes != null) ? filteredTypes : StatTypes);
			foreach (StatType statType in array)
			{
				long stat = rhs.GetStat(statType);
				AddStat(statType, stat);
			}
		}
	}

	public void AddEquipItemStat(StatData rhs, int star, int detailIndex, int level, StatType[] filteredTypes = null)
	{
		if (rhs == null)
		{
			return;
		}
		StatType[] obj = ((filteredTypes != null) ? filteredTypes : StatTypes);
		EquipItemStarData data = GameRoot.Instance.EquipItemStarDataContainer.GetData(star);
		EquipUpgradeStatData data2 = GameRoot.Instance.EquipUpgradeStatDataContainer.GetData(star, detailIndex, level);
		StatType[] array = obj;
		foreach (StatType statType in array)
		{
			long num = rhs.GetStat(statType);
			if (data2 != null && data != null)
			{
				num = StatCalculator.GetEquipFinalStat(statType, num, data, data2);
			}
			else
			{
				VespaDebug.LogError("StatData is null. star=" + star + " detailIndex=" + detailIndex + " level=" + level);
			}
			AddStat(statType, num);
		}
	}

	public void ModifyStat(StatType statType, Func<long, long> modifier)
	{
		if (modifier != null)
		{
			long stat = GetStat(statType);
			long num = modifier(stat);
			if (num != stat)
			{
				SetStat(statType, num);
			}
		}
	}

	public long GetMaxStat(params StatType[] statTypes)
	{
		long num = 0L;
		int num2 = 0;
		while (statTypes != null && num2 < statTypes.Length)
		{
			num = System.Math.Max(num, GetStat(statTypes[num2]));
			num2++;
		}
		return num;
	}

	public static bool IsEqual(StatData lhs, StatData rhs)
	{
		if (lhs == null || rhs == null)
		{
			return false;
		}
		StatType[] statTypes = StatTypes;
		foreach (StatType statType in statTypes)
		{
			long stat = lhs.GetStat(statType);
			long stat2 = rhs.GetStat(statType);
			if (stat != stat2)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsEqual(StatData lhs, StatData rhs, out StatType differentStatType)
	{
		differentStatType = StatType.None;
		if (lhs == null || rhs == null)
		{
			return false;
		}
		StatType[] statTypes = StatTypes;
		foreach (StatType statType in statTypes)
		{
			long stat = lhs.GetStat(statType);
			long stat2 = rhs.GetStat(statType);
			if (stat != stat2)
			{
				differentStatType = statType;
				return false;
			}
		}
		return true;
	}

	public void GetAllMultedExtraStat(out List<MultesStatInfo> multedStatList)
	{
		multedStatList = null;
		foreach (KeyValuePair<StatType, StatType> factorStat in _factorStats)
		{
			StatType key = factorStat.Key;
			StatType value = factorStat.Value;
			long stat = GetStat(key);
			if (stat != 0L)
			{
				multedStatList = multedStatList ?? new List<MultesStatInfo>();
				multedStatList.Add(new MultesStatInfo
				{
					Type = value,
					Value = stat
				});
			}
		}
	}

	public static StatType GetMultedStatTypeOfFactorStat(StatType factorStat)
	{
		StatType value = StatType.None;
		_factorStats.TryGetValue(factorStat, out value);
		return value;
	}

	public static StatType GetFactorStatTypeByMultedStat(StatType multedStat)
	{
		StatType result = StatType.None;
		foreach (KeyValuePair<StatType, StatType> factorStat in _factorStats)
		{
			if (factorStat.Value == multedStat)
			{
				result = factorStat.Key;
				break;
			}
		}
		return result;
	}

	public IEnumerator<KeyValuePair<StatType, long>> GetEnumerator()
	{
		StatType[] statTypes = StatTypes;
		foreach (StatType statType in statTypes)
		{
			long stat = GetStat(statType);
			yield return new KeyValuePair<StatType, long>(statType, stat);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static StatType[] GetVisibleStats()
	{
		if (_visibleStats == null)
		{
			List<StatType> list = new List<StatType>();
			foreach (StatType value in Enum.GetValues(typeof(StatType)))
			{
				if (GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetData(value) != null)
				{
					list.Add(value);
				}
			}
			list.Sort(delegate(StatType l, StatType r)
			{
				ClientStatDisplayScoreData data = GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetData(l);
				ClientStatDisplayScoreData data2 = GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetData(r);
				return (data != null && data2 != null) ? data.DisplayScore.CompareTo(data2.DisplayScore) : 0;
			});
			_visibleStats = list.ToArray();
		}
		return _visibleStats;
	}

	public Dictionary<StatType, long> GetVisibleStats(SkillAttrType attrType, bool all)
	{
		Dictionary<StatType, long> dictionary = new Dictionary<StatType, long>(StatTypeComparer.Default);
		StatType[] visibleStats = GetVisibleStats();
		foreach (StatType statType in visibleStats)
		{
			long stat = GetStat(statType);
			if ((all || stat > 0) && (attrType != SkillAttrType.Physical || statType != StatType.MagicalAttack) && (attrType != SkillAttrType.Magical || statType != StatType.PhysicalAttack))
			{
				dictionary.Add(statType, stat);
			}
		}
		return dictionary;
	}

	public static StatType[] GetMainStatType(EquipPartType partType)
	{
		StatType[] array = null;
		switch (partType)
		{
		case EquipPartType.Weapon:
			return _weaponMainStats;
		case EquipPartType.Armor:
			return _armorMainStats;
		case EquipPartType.Accessory:
			return _accessoryMainStats;
		case EquipPartType.SecondGear:
			return _secondGearMainStats;
		default:
			if (_totalMainStats == null)
			{
				HashSet<StatType> hashSet = new HashSet<StatType>();
				StatType[] weaponMainStats = _weaponMainStats;
				foreach (StatType item in weaponMainStats)
				{
					hashSet.Add(item);
				}
				weaponMainStats = _armorMainStats;
				foreach (StatType item2 in weaponMainStats)
				{
					hashSet.Add(item2);
				}
				weaponMainStats = _accessoryMainStats;
				foreach (StatType item3 in weaponMainStats)
				{
					hashSet.Add(item3);
				}
				weaponMainStats = _secondGearMainStats;
				foreach (StatType item4 in weaponMainStats)
				{
					hashSet.Add(item4);
				}
				_totalMainStats = hashSet.ToArray();
			}
			return _totalMainStats;
		}
	}

	public bool GetMainStat(EquipPartType partType, out StatType statType, out long statValue)
	{
		statType = StatType.None;
		statValue = 0L;
		StatType[] mainStatType = GetMainStatType(partType);
		for (int i = 0; i < mainStatType.Length; i++)
		{
			statType = mainStatType[i];
			statValue = GetStat(statType);
			if (statValue > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPercentStat(StatType statType)
	{
		return StatTypeAttribute.IsPercentStat(statType);
	}

	public static bool IsPercentStat(OperationType statType)
	{
		return OperationTypeAttribute.IsPercentOperation(statType);
	}

	public static bool IsPercentStat(Enum statType)
	{
		if (statType.GetType() == typeof(StatType))
		{
			return IsPercentStat((StatType)(object)statType);
		}
		if (statType.GetType() == typeof(OperationType))
		{
			return IsPercentStat((OperationType)(object)statType);
		}
		return false;
	}

	public static string GetStatDisplayValue(Enum statType, long optionValue, bool showSign = true, bool isCommaString = false)
	{
		return GetStatDisplayValue(statType, optionValue, optionValue, string.Empty, showSign, isCommaString);
	}

	public static string GetStatDisplayValue(bool isPercentStat, long optionValue, bool showSign = true, bool isCommaString = false)
	{
		return GetStatDisplayValue(isPercentStat, optionValue, optionValue, string.Empty, showSign, isCommaString);
	}

	public static string GetStatDisplayValue(Enum statType, long optionValue, long optionValueMax, string optionValueSplitter = "~", bool showSign = true, bool isCommaString = false)
	{
		return GetStatDisplayValue(IsPercentStat(statType), optionValue, optionValueMax, optionValueSplitter, showSign, isCommaString);
	}

	public static string GetStatDisplayValue(bool isPercentStat, long optionValue, long optionValueMax, string optionValueSplitter = "~", bool showSign = true, bool isCommaString = false)
	{
		bool num = optionValue != optionValueMax;
		double num2 = optionValue;
		if (isPercentStat)
		{
			num2 /= 10.0;
		}
		CultureInfo cultureInfo = GameRoot.Instance.LocalizationValueDataContainer.GetCultureInfo();
		string text = (isCommaString ? num2.ToString("#,##0.#", cultureInfo) : num2.ToString("0.#", cultureInfo));
		if (showSign && num2 > 0.0)
		{
			text = "+" + text;
		}
		if (num)
		{
			string text2 = optionValueSplitter;
			double num3 = optionValueMax;
			if (isPercentStat)
			{
				num3 /= 10.0;
			}
			text2 = ((!showSign || !(num3 > 0.0)) ? (text2 + num3.ToString("0.#", cultureInfo)) : (text2 + "+" + num3.ToString("0.#", cultureInfo)));
			text += text2;
		}
		if (isPercentStat)
		{
			text += "%";
		}
		return text;
	}

	public static StatType[] GetHeroMainStats(SkillAttrType attrType)
	{
		if (attrType != SkillAttrType.Physical)
		{
			return _magicalHeroMainStats;
		}
		return _physicalHeroMainStats;
	}

	private static string GetOptionDisplayInternal(OperationType optionType, int optionValue, int optionValueMax, string optionValueSplitter, bool opened = true, bool appendColor = true, string prefix = null)
	{
		string empty = string.Empty;
		if (optionType == OperationType.UseSkill)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(optionValue);
			empty = ((data == null) ? GameRoot.LocalizeProxy.GetEnumString(optionType, optionValue) : data.Description);
		}
		else
		{
			string enumString = GameRoot.LocalizeProxy.GetEnumString(optionType);
			string statDisplayValue = GetStatDisplayValue(optionType, optionValue, optionValueMax, optionValueSplitter);
			empty = enumString + " " + statDisplayValue;
		}
		if (appendColor)
		{
			string value = GameRoot.Instance.ConstantDataContainer.GetValue("EquipItemOptionOpenedColor", "E28C53");
			string value2 = GameRoot.Instance.ConstantDataContainer.GetValue("EquipItemOptionClosedColor", "808080");
			string text = "[" + (opened ? value : value2) + "]";
			string text2 = ((prefix != null) ? (prefix + " ") : "");
			return text + text2 + empty + "[-]";
		}
		return ((prefix != null) ? (prefix + " ") : "") + empty;
	}

	public static string GetOptionDisplay(OperationType optionType, int optionValue, bool opened = true, int openLevel = 0, bool appendColor = true, bool isFixedOption = true)
	{
		return GetOptionDisplay(optionType, optionValue, optionValue, string.Empty, opened, openLevel, appendColor, isFixedOption);
	}

	public static string GetExtraOptionDisplay(OperationType optionType, int optionValue, string prefix = null, bool appendColor = true)
	{
		return GetOptionDisplayInternal(optionType, optionValue, optionValue, string.Empty, opened: true, appendColor, prefix);
	}

	public static string GetOptionDisplay(OperationType optionType, int optionValue, int optionValueMax, string optionValueSplitter, bool opened = true, int openLevel = 0, bool appendColor = true, bool isFixedOption = true)
	{
		string prefix = null;
		if (!isFixedOption)
		{
			prefix = GameRoot.LocalizeProxy.GetEnumString(Equip.EQUIP_OPEN_OPTION_ALL);
		}
		return GetOptionDisplayInternal(optionType, optionValue, optionValueMax, optionValueSplitter, opened, appendColor, prefix);
	}

	public static string GetSetOptionDisplay(OperationType optionType, int optionValue, bool opened = true, int setCount = 0, bool appendColor = true)
	{
		return GetSetOptionDisplay(optionType, optionValue, optionValue, string.Empty, opened, setCount, appendColor);
	}

	public static string GetSetOptionDisplay(OperationType optionType, int optionValue, int optionValueMax, string optionValueSplitter, bool opened = true, int setCount = 0, bool appendColor = true)
	{
		string enumString = GameRoot.LocalizeProxy.GetEnumString(Equip.EQUIP_SET_OPTION_NUMBER, setCount);
		return GetOptionDisplayInternal(optionType, optionValue, optionValueMax, optionValueSplitter, opened, appendColor, enumString);
	}

	public static string GetEnchantOptionDisplay(OperationType optionType, int optionValue, int optionValueMax, string optionValueSplitter, bool appendColor = true)
	{
		string prefix = GameRoot.LocalizeProxy.GetEnumString(EquipEnchant.ENCHANT_OPTION) + ":";
		return GetOptionDisplayInternal(optionType, optionValue, optionValueMax, optionValueSplitter, opened: true, appendColor, prefix);
	}

	public static string GetUnIdentifiedOptionDisplay(string prefix = "", string unidentified = "")
	{
		string value = GameRoot.Instance.ConstantDataContainer.GetValue("EquipItemOptionOpenedColor", "E28C53");
		string arg = "[" + value + "]";
		if (prefix != null)
		{
			_ = prefix + " ";
		}
		return $"{arg}{prefix}{unidentified}[-]";
	}
}
