using NVespa;

namespace NShared;

public class BattleConstant
{
	public static int PhysicalCriticalDamageFactor;

	public static int MagicalCriticalDamageFactor;

	public static int MinAttackDamageFactor;

	public static int ShortKnockbackDamageMinRate;

	public static int ShortKnockbackDurationMs;

	public static int PhysicalDefaultHitChance;

	public static int MagicalDefaultHitChance;

	public static int DefaultBlockPower;

	public static int LevelChanceAttenuationFactor;

	public static int LevelDamageAttenuationFactor;

	public static int LevelDamageAttenuationMaxLevel;

	public static int WeakAttackAttrDamageFactor;

	public static int DefenceCalcMinFraction;

	public static int DefenceCalcChangeFactor;

	public static int DefenceCalcLevelBaseFactor;

	public static int MoveTargetOffsetMm;

	public static int BattleRangeBonusMm;

	public static int AggroRate;

	public static int PositionSeperationWeight;

	public static int RageModeIntervalTimeMs;

	public static int MinAttackSpeed;

	public static int MaxToughness;

	public static int MaxBlockPower;

	public static int MultedStatAttenuationMinX;

	public static int[] MultedStatAttenuationValue;

	public static int EnvironmentCreatureInex;

	public static bool LegacySoloTime;

	public static bool LegacySameSkillCancel;

	public static int ManaAddictLimit1;

	public static int ManaAddictLimit2;

	public static int ManaAddictLimit3;

	public static int ManaMeterUnitTime;

	public static int ThreatMeterUnitTime;

	public static int ThreatMeterCompressRatio;

	public static bool StopAllOperationInSoloTime
	{
		get
		{
			return !LegacySoloTime;
		}
		set
		{
			LegacySoloTime = !value;
		}
	}

	public static bool EnableSameSkillCancel
	{
		get
		{
			return !LegacySameSkillCancel;
		}
		set
		{
			LegacySameSkillCancel = !value;
		}
	}

	static BattleConstant()
	{
		StaticInitialize();
	}

	public static void StaticInitialize()
	{
		if (GameRoot.Instance == null)
		{
			VespaDebug.LogError("GameRoot not initialized");
			return;
		}
		if (GameRoot.Instance.ConstantDataContainer == null)
		{
			VespaDebug.LogError("ConstantDataContainer not initialized");
			return;
		}
		PhysicalCriticalDamageFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("PhysicalCriticalDamageFactor");
		MagicalCriticalDamageFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("MagicalCriticalDamageFactor");
		MinAttackDamageFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("MinAttackDamageFactor");
		ShortKnockbackDamageMinRate = GameRoot.Instance.ConstantDataContainer.GetValueInt("ShortKnockbackDamageMinRate");
		ShortKnockbackDurationMs = GameRoot.Instance.ConstantDataContainer.GetValueInt("ShortKnockbackDurationMs");
		PhysicalDefaultHitChance = GameRoot.Instance.ConstantDataContainer.GetValueInt("PhysicalDefaultHitChance");
		MagicalDefaultHitChance = GameRoot.Instance.ConstantDataContainer.GetValueInt("MagicalDefaultHitChance");
		DefaultBlockPower = GameRoot.Instance.ConstantDataContainer.GetValueInt("DefaultBlockPower");
		LevelChanceAttenuationFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("LevelChanceAttenuationFactor");
		LevelDamageAttenuationFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("LevelDamageAttenuationFactor");
		LevelDamageAttenuationMaxLevel = GameRoot.Instance.ConstantDataContainer.GetValueInt("LevelDamageAttenuationMaxLevel", 80);
		WeakAttackAttrDamageFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("WeakAttackAttrDamageFactor");
		DefenceCalcMinFraction = GameRoot.Instance.ConstantDataContainer.GetValueInt("DefenceCalcMinFraction");
		DefenceCalcChangeFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("DefenceCalcChangeFactor");
		DefenceCalcLevelBaseFactor = GameRoot.Instance.ConstantDataContainer.GetValueInt("DefenceCalcLevelBaseFactor");
		MoveTargetOffsetMm = GameRoot.Instance.ConstantDataContainer.GetValueInt("MoveTargetOffsetMm");
		BattleRangeBonusMm = GameRoot.Instance.ConstantDataContainer.GetValueInt("BattleRangeBonusMm");
		AggroRate = GameRoot.Instance.ConstantDataContainer.GetValueInt("AggroRate");
		PositionSeperationWeight = GameRoot.Instance.ConstantDataContainer.GetValueInt("PositionSeperationWeight");
		RageModeIntervalTimeMs = GameRoot.Instance.ConstantDataContainer.GetValueInt("RageModeIntervalTimeMs");
		MinAttackSpeed = GameRoot.Instance.ConstantDataContainer.GetValueInt("MinAttackSpeed", 300);
		MaxToughness = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxToughness", 1000);
		MaxBlockPower = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxBlockPower", 1000);
		MultedStatAttenuationMinX = GameRoot.Instance.ConstantDataContainer.GetValueInt("MultedStatAttenuationMinX");
		MultedStatAttenuationValue = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("MultedStatAttenuationValue");
		EnvironmentCreatureInex = GameRoot.Instance.ConstantDataContainer.GetValueInt("EnvironmentCreatureInex");
		ManaAddictLimit1 = GameRoot.Instance.ConstantDataContainer.GetValueInt("ManaAddictLimit1", 500);
		ManaAddictLimit2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("ManaAddictLimit2", 1000);
		ManaAddictLimit3 = GameRoot.Instance.ConstantDataContainer.GetValueInt("ManaAddictLimit3", 1400);
		ManaMeterUnitTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("ManaMeterUnitTime", 10);
		ThreatMeterUnitTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("ThreatMeterUnitTime", 10);
		ThreatMeterCompressRatio = GameRoot.Instance.ConstantDataContainer.GetValueInt("ThreatMeterCompressRatio", 1);
	}
}
