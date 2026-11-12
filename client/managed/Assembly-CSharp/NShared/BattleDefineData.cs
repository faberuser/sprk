using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable]
public class BattleDefineData : IDataKeySelector<string>
{
	private Dictionary<StatType, int> _cache_for_ModStatRatioM;

	string IDataKeySelector<string>.Key => Code;

	[Key(0)]
	public HashString Code { get; set; }

	[Key(1)]
	public int MaxTime { get; set; }

	[Key(2)]
	public int ExtraTime { get; set; }

	[Key(3)]
	public string LabelPrefix { get; set; }

	[Key(4)]
	public int StartMpRatioM { get; set; }

	[Key(5)]
	public int AutoPlayStarCount { get; set; }

	[Key(6)]
	public int FastSpeedStarCount { get; set; }

	[Key(7)]
	public int AutoRepeatStarCount { get; set; }

	[Key(8)]
	public bool ApplyLevelPenalty { get; set; }

	[Key(9)]
	public bool FastSkillTooltip { get; set; }

	[Key(10)]
	public bool DenyRaidInvite { get; set; }

	[Key(11)]
	public bool DenyDuel { get; set; }

	[Key(12)]
	public bool HPPercent { get; set; }

	[Key(13)]
	public DamageMeterFlag DamageMeterFlag { get; set; }

	[Key(14)]
	public bool DisableBossHUD { get; set; }

	[Key(15)]
	public bool HideAutoStartButton { get; set; }

	[Key(16)]
	public BattleRetryConditionType[] RetryCondition { get; set; }

	[Key(17)]
	public string BattleCameraName { get; set; }

	[Key(18)]
	public bool IgnoreDropItem { get; set; }

	[Key(19)]
	public StatType ModStatType1 { get; set; }

	[Key(20)]
	public int ModStatRatioM1 { get; set; }

	[Key(21)]
	public StatType ModStatType2 { get; set; }

	[Key(22)]
	public int ModStatRatioM2 { get; set; }

	[Key(23)]
	public StatType ModStatType3 { get; set; }

	[Key(24)]
	public int ModStatRatioM3 { get; set; }

	[Key(25)]
	public StatType ModStatType4 { get; set; }

	[Key(26)]
	public int ModStatRatioM4 { get; set; }

	[Key(27)]
	public StatType ModStatType5 { get; set; }

	[Key(28)]
	public int ModStatRatioM5 { get; set; }

	[Key(29)]
	public EBattleMenu BattleMenu { get; set; }

	[Key(30)]
	public bool SkillCameraOff { get; set; }

	[Key(31)]
	public bool DisableInventoryInResult { get; set; }

	[Key(32)]
	public bool DisablePartySettingInResult { get; set; }

	[Key(33)]
	public bool DisableHeroListInResult { get; set; }

	[Key(34)]
	public ExInfoType ExInfoTypeOnPartySetting { get; set; }

	[Key(35)]
	public bool DisableLastKill { get; set; }

	[Key(36)]
	public bool ShowBossManaGauge { get; set; }

	[Key(37)]
	public int CameraFarClipPlane { get; set; }

	[Key(38)]
	public bool EnhanceTimeSyncMode { get; set; }

	[Key(39)]
	public bool BetterBossHpUI { get; set; }

	public override string ToString()
	{
		return "BattleDefine=" + Code;
	}

	public int GetModStatRatioM(StatType statType)
	{
		lock (this)
		{
			if (_cache_for_ModStatRatioM == null)
			{
				_cache_for_ModStatRatioM = new Dictionary<StatType, int>(StatTypeComparer.Default);
				if (ModStatType1 != StatType.None)
				{
					_cache_for_ModStatRatioM[ModStatType1] = ModStatRatioM1;
				}
				if (ModStatType2 != StatType.None)
				{
					_cache_for_ModStatRatioM[ModStatType2] = ModStatRatioM2;
				}
				if (ModStatType3 != StatType.None)
				{
					_cache_for_ModStatRatioM[ModStatType3] = ModStatRatioM3;
				}
				if (ModStatType4 != StatType.None)
				{
					_cache_for_ModStatRatioM[ModStatType4] = ModStatRatioM4;
				}
				if (ModStatType5 != StatType.None)
				{
					_cache_for_ModStatRatioM[ModStatType5] = ModStatRatioM5;
				}
			}
			if (_cache_for_ModStatRatioM.TryGetValue(statType, out var value))
			{
				return value;
			}
			return 1000;
		}
	}
}
