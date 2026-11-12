using System;
using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NVespa;

namespace NGame2.NConfig;

public class ClientDebugBattle : SingletonAuto<ClientDebugBattle>
{
	public enum ELogPacket
	{
		None,
		Unhandled,
		All
	}

	public enum EDamagePipelineVersion
	{
		V2,
		Season1
	}

	public enum EDefenceCalculation
	{
		LevelTarget,
		Season1
	}

	public enum ESoulWeaponStat
	{
		MaxValue,
		PerValue
	}

	private static object Sync = new object();

	private Dictionary<string, LocalEditorBool> _boolDic = new Dictionary<string, LocalEditorBool>();

	private Dictionary<string, LocalEditorFloat> _floatDic = new Dictionary<string, LocalEditorFloat>();

	private Dictionary<string, LocalEditorInt> _intDic = new Dictionary<string, LocalEditorInt>();

	private Dictionary<string, LocalEditorString> _stringDic = new Dictionary<string, LocalEditorString>();

	private IncreaseDic<Type, LocalEditorBool> _logPacketFlagDic;

	public int? SimulatorRandomSeed;

	protected override bool ThreadSafe => true;

	public bool LeftTeamDummy
	{
		get
		{
			return GetBoolean("LeftTeamDummy");
		}
		set
		{
			SetBoolean("LeftTeamDummy", value);
		}
	}

	public float LeftTeamMultiply
	{
		get
		{
			return GetFloat("LeftTeamMultiply2", 1f);
		}
		set
		{
			SetFloat("LeftTeamMultiply2", value);
		}
	}

	public bool RightTeamDummy
	{
		get
		{
			return GetBoolean("RightTeamDummy");
		}
		set
		{
			SetBoolean("RightTeamDummy", value);
		}
	}

	public float RightTeamMultiply
	{
		get
		{
			return GetFloat("RightTeamMultiply2", 1f);
		}
		set
		{
			SetFloat("RightTeamMultiply2", value);
		}
	}

	public bool ForceDamage1_Left
	{
		get
		{
			return GetBoolean("ForceDamage1_Left");
		}
		set
		{
			SetBoolean("ForceDamage1_Left", value);
		}
	}

	public bool ForceDamage1_Right
	{
		get
		{
			return GetBoolean("ForceDamage1_Right");
		}
		set
		{
			SetBoolean("ForceDamage1_Right", value);
		}
	}

	public bool FixedRandomSeed
	{
		get
		{
			return GetBoolean("FixedRandomSeed");
		}
		set
		{
			SetBoolean("FixedRandomSeed", value);
		}
	}

	public int FixedRandomSeedValue
	{
		get
		{
			return GetInt("FixedRandomSeedValue");
		}
		set
		{
			SetInt("FixedRandomSeedValue", value);
		}
	}

	public bool DamageAllCritical
	{
		get
		{
			return GetBoolean("DamageAllCritical");
		}
		set
		{
			SetBoolean("DamageAllCritical", value);
		}
	}

	public bool DamageAllMiss
	{
		get
		{
			return GetBoolean("DamageAllMiss");
		}
		set
		{
			SetBoolean("DamageAllMiss", value);
		}
	}

	public bool DamageAllBlock
	{
		get
		{
			return GetBoolean("DamageAllBlock");
		}
		set
		{
			SetBoolean("DamageAllBlock", value);
		}
	}

	public bool AllImmune
	{
		get
		{
			return GetBoolean("AllImmune");
		}
		set
		{
			SetBoolean("AllImmune", value);
		}
	}

	public bool AllResistCc
	{
		get
		{
			return GetBoolean("AllResistCc");
		}
		set
		{
			SetBoolean("AllResistCc", value);
		}
	}

	public bool AllResistDebuff
	{
		get
		{
			return GetBoolean("AllResistDebuff");
		}
		set
		{
			SetBoolean("AllResistDebuff", value);
		}
	}

	public bool FullMana
	{
		get
		{
			return GetBoolean("FullMana");
		}
		set
		{
			SetBoolean("FullMana", value);
		}
	}

	public bool NoCooltime
	{
		get
		{
			return GetBoolean("NoCooltime");
		}
		set
		{
			SetBoolean("NoCooltime", value);
		}
	}

	public bool ReverseDashOffset
	{
		get
		{
			return GetBoolean("ReverseDashOffset");
		}
		set
		{
			SetBoolean("ReverseDashOffset", value);
		}
	}

	public bool InfiniteBattleTime
	{
		get
		{
			return GetBoolean("InfiniteBattleTime");
		}
		set
		{
			SetBoolean("InfiniteBattleTime", value);
		}
	}

	public bool InfiniteWave
	{
		get
		{
			return GetBoolean("InfiniteWave");
		}
		set
		{
			SetBoolean("InfiniteWave", value);
		}
	}

	public bool InfiniteWave2
	{
		get
		{
			return GetBoolean("InfiniteWave2");
		}
		set
		{
			SetBoolean("InfiniteWave2", value);
		}
	}

	public bool ForceAutoPlayable
	{
		get
		{
			return GetBoolean("ForceAutoPlayable");
		}
		set
		{
			SetBoolean("ForceAutoPlayable", value);
		}
	}

	public bool EnforceSimulatorAi
	{
		get
		{
			return GetBoolean("EnableSimulatorAi");
		}
		set
		{
			SetBoolean("EnableSimulatorAi", value);
		}
	}

	public bool EnforceAiSkillRandom
	{
		get
		{
			return GetBoolean("EnforceAiSkillRandom");
		}
		set
		{
			SetBoolean("EnforceAiSkillRandom", value);
		}
	}

	public bool EnforceScenarioMode
	{
		get
		{
			return GetBoolean("EnforceScenarioMode");
		}
		set
		{
			SetBoolean("EnforceScenarioMode", value);
		}
	}

	public int DamagePipelineVersion
	{
		get
		{
			return GetInt("DamagePipelineVersion");
		}
		set
		{
			SetInt("DamagePipelineVersion", value);
		}
	}

	public float AdjustEquipStatRatio
	{
		get
		{
			return GetFloat("AdjustEquipStat", 1f);
		}
		set
		{
			SetFloat("AdjustEquipStat", value);
		}
	}

	public float AdjustModifySkillRatio
	{
		get
		{
			return GetFloat("AdjustModifierRatio", 1f);
		}
		set
		{
			SetFloat("AdjustModifierRatio", value);
		}
	}

	public float AdjustSkillStatRatio
	{
		get
		{
			return GetFloat("AdjustSkillStatRatio", 1f);
		}
		set
		{
			SetFloat("AdjustSkillStatRatio", value);
		}
	}

	public bool ApplyShieldAfterDefence
	{
		get
		{
			return GetBoolean("ApplyShieldAfterDefence");
		}
		set
		{
			SetBoolean("ApplyShieldAfterDefence", value);
		}
	}

	public float AdjustShieldAmountRatio
	{
		get
		{
			return GetFloat("AdjustShieldAmount", 1f);
		}
		set
		{
			SetFloat("AdjustShieldAmount", value);
		}
	}

	public int DefenceCalculation
	{
		get
		{
			return GetInt("DefenceCalculation");
		}
		set
		{
			SetInt("DefenceCalculation", value);
		}
	}

	public int SoulWeaponStat
	{
		get
		{
			return GetInt("SoulWeaponStat");
		}
		set
		{
			SetInt("SoulWeaponStat", value);
		}
	}

	public string ExcludeLevelStatFactorTypesCsv
	{
		get
		{
			return GetString("ExcludeLevesStatFactorTypesCsv", "");
		}
		set
		{
			SetString("ExcludeLevesStatFactorTypesCsv", value);
		}
	}

	public float AdjustGetManaRatio
	{
		get
		{
			return GetFloat("AdjustGetMansRatio", 1f);
		}
		set
		{
			SetFloat("AdjustGetMansRatio", value);
		}
	}

	public bool DisableMonsterPenalty
	{
		get
		{
			return GetBoolean("EnableMonsterPenalty");
		}
		set
		{
			SetBoolean("EnableMonsterPenalty", value);
		}
	}

	public bool TestSkipOnStartSkills
	{
		get
		{
			return GetBoolean("TestSkipOnStartSkills");
		}
		set
		{
			SetBoolean("TestSkipOnStartSkills", value);
		}
	}

	public int ForceManaPerUnitTime
	{
		get
		{
			return GetInt("ForceManaPerUnitTime");
		}
		set
		{
			SetInt("ForceManaPerUnitTime", value);
		}
	}

	public bool ShowStackCount
	{
		get
		{
			return GetBoolean("ShowStackCount");
		}
		set
		{
			SetBoolean("ShowStackCount", value);
		}
	}

	public bool FullStack
	{
		get
		{
			return GetBoolean("FullStack");
		}
		set
		{
			SetBoolean("FullStack", value);
		}
	}

	public bool InfiniteSoulWeaponSkill
	{
		get
		{
			return GetBoolean("InfiniteSoulWeaponSkill");
		}
		set
		{
			SetBoolean("InfiniteSoulWeaponSkill", value);
		}
	}

	public bool InfiniteDungeonSkill
	{
		get
		{
			return GetBoolean("InfiniteDungeonSkill");
		}
		set
		{
			SetBoolean("InfiniteDungeonSkill", value);
		}
	}

	public bool DungeonSkillBaseStateFull
	{
		get
		{
			return GetBoolean("DungeonSkillBaseStateFull");
		}
		set
		{
			SetBoolean("DungeonSkillBaseStateFull", value);
		}
	}

	public bool DungeonSkillEnhancedStateFull
	{
		get
		{
			return GetBoolean("DungeonSkillEnhancedStateFull");
		}
		set
		{
			SetBoolean("DungeonSkillEnhancedStateFull", value);
		}
	}

	public bool PartyUi_IgnoreMine
	{
		get
		{
			return GetBoolean("PartyUi_IgnoreMine");
		}
		set
		{
			SetBoolean("PartyUi_IgnoreMine", value);
		}
	}

	public bool PartyUi_InclueEnemy
	{
		get
		{
			return GetBoolean("PartyUi_InclueEnemy");
		}
		set
		{
			SetBoolean("PartyUi_InclueEnemy", value);
		}
	}

	public bool AutoCampaign
	{
		get
		{
			return GetBoolean("AutoCampaign");
		}
		set
		{
			SetBoolean("AutoCampaign", value);
		}
	}

	public bool AutoCampaignFast
	{
		get
		{
			return GetBoolean("AutoCampaignFast");
		}
		set
		{
			SetBoolean("AutoCampaignFast", value);
		}
	}

	public bool SkipCampaignBattle
	{
		get
		{
			return GetBoolean("SkipCampaignBattle");
		}
		set
		{
			SetBoolean("SkipCampaignBattle", value);
		}
	}

	public bool ForceTakeSnapshot
	{
		get
		{
			return GetBoolean("ForceTakeSnapshot");
		}
		set
		{
			SetBoolean("ForceTakeSnapshot", value);
		}
	}

	public bool DisableAllEffect
	{
		get
		{
			return GetBoolean("DisableAllEffect");
		}
		set
		{
			SetBoolean("DisableAllEffect", value);
		}
	}

	public bool DisableEventCamera
	{
		get
		{
			return GetBoolean("DisableEventCamera");
		}
		set
		{
			SetBoolean("DisableEventCamera", value);
		}
	}

	public bool DisableTimerEventColor
	{
		get
		{
			return GetBoolean("DisableTimerEventColor");
		}
		set
		{
			SetBoolean("DisableTimerEventColor", value);
		}
	}

	public bool DrawBattleArea
	{
		get
		{
			return GetBoolean("DrawBattleArea");
		}
		set
		{
			SetBoolean("DrawBattleArea", value);
		}
	}

	public bool DrawCameraPlanes
	{
		get
		{
			return GetBoolean("DrawCameraPlanes");
		}
		set
		{
			SetBoolean("DrawCameraPlanes", value);
		}
	}

	public bool SkipAdjustCamera
	{
		get
		{
			return GetBoolean("SkipAdjustCamera");
		}
		set
		{
			SetBoolean("SkipAdjustCamera", value);
		}
	}

	public bool LogPacketToFile
	{
		get
		{
			return GetBoolean("LogPacketToFile");
		}
		set
		{
			SetBoolean("LogPacketToFile", value);
		}
	}

	public bool LogPacket
	{
		get
		{
			return GetBoolean("LogPacket");
		}
		set
		{
			SetBoolean("LogPacket", value);
		}
	}

	public int BattleLogDelayMs
	{
		get
		{
			return GetInt("BattleLogDelayMs");
		}
		set
		{
			SetInt("BattleLogDelayMs", value);
		}
	}

	public bool BattleLog_OpenWindow
	{
		get
		{
			return GetBoolean("BattleLog_OpenWindow");
		}
		set
		{
			SetBoolean("BattleLog_OpenWindow", value);
		}
	}

	public int BattleLogFilterSkill
	{
		get
		{
			return GetInt("BattleLogFilterSkill");
		}
		set
		{
			SetInt("BattleLogFilterSkill", value);
		}
	}

	public bool BattleLog2_OpenWindow
	{
		get
		{
			return GetBoolean("BattleLog2_OpenWindow");
		}
		set
		{
			SetBoolean("BattleLog2_OpenWindow", value);
		}
	}

	public bool EnableResetTimeScale
	{
		get
		{
			return GetBoolean("EnableResetTimeScale");
		}
		set
		{
			SetBoolean("EnableResetTimeScale", value);
		}
	}

	public float ResetTimeScale
	{
		get
		{
			return GetFloat("ResetTimeScale", 1f);
		}
		set
		{
			SetFloat("ResetTimeScale", value);
		}
	}

	private bool _enableValue => false;

	public void OpenErrorPopup(string message)
	{
		VespaDebug.ActionOnUnityMainThread(delegate
		{
			MessagePopup.OpenError(message);
		});
	}

	private bool GetBoolean(string key, bool defaultValue = false)
	{
		lock (Sync)
		{
			return defaultValue;
		}
	}

	private void SetBoolean(string key, bool value)
	{
		lock (Sync)
		{
		}
	}

	private float GetFloat(string key, float defaultValue = 0f)
	{
		lock (Sync)
		{
			return defaultValue;
		}
	}

	private void SetFloat(string key, float value)
	{
		lock (Sync)
		{
		}
	}

	private int GetInt(string key, int defaultValue = 0)
	{
		lock (Sync)
		{
			return defaultValue;
		}
	}

	private void SetInt(string key, int value)
	{
		lock (Sync)
		{
		}
	}

	private string GetString(string key, string defaultValue = null)
	{
		lock (Sync)
		{
			return defaultValue;
		}
	}

	private void SetString(string key, string value)
	{
		lock (Sync)
		{
		}
	}

	public bool LogThisPacket(Type type)
	{
		lock (Sync)
		{
			return false;
		}
	}
}
