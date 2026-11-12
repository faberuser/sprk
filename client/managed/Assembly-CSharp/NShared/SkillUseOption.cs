namespace NShared;

public class SkillUseOption
{
	public static SkillUseOption NullValue = new SkillUseOption(checkOnly: false, checkLevel: false, checkMp: false, isEventSkill: false, null, checkBattleEnabled: false, checkReqHpRatio: false, isIgnoreCoolTime: false, isIgnoreSoloTime: false, isCanBeCancel: false, checkStateCount: false);

	private static SkillUseOption _soulWeaponSkillUseOption = new SkillUseOption(checkOnly: false, checkLevel: false, checkMp: false, isEventSkill: false, null, checkBattleEnabled: true, checkReqHpRatio: true, isIgnoreCoolTime: false, isIgnoreSoloTime: false, isCanBeCancel: false, checkStateCount: false);

	public bool CheckOnly { get; set; }

	public bool CheckLevel { get; set; }

	public bool CheckMp { get; set; }

	public bool IsIgnoreCoolTime { get; set; }

	public bool IsIgnoreSoloTime { get; set; }

	public bool IsCanBeCancel { get; set; }

	public bool IsEventSkill { get; set; }

	public BattleCreature ManualTarget { get; set; }

	public BattleCreature[] PrevUsedTargets { get; set; }

	public BattleCreature[] PrevFiredTargets { get; set; }

	public bool CheckBattleEnabled { get; set; }

	public bool CheckReqHpRatio { get; set; }

	public bool CheckStateCount { get; set; }

	public static SkillUseOption GetSoulWeaponSkillUseOption(BattleCreature manualTarget = null)
	{
		_soulWeaponSkillUseOption.ManualTarget = manualTarget;
		return _soulWeaponSkillUseOption;
	}

	public SkillUseOption(bool checkOnly = false, bool checkLevel = true, bool checkMp = true, bool isEventSkill = false, BattleCreature manualTarget = null, bool checkBattleEnabled = true, bool checkReqHpRatio = true, bool isIgnoreCoolTime = false, bool isIgnoreSoloTime = false, bool isCanBeCancel = false, bool checkStateCount = true)
	{
		CheckOnly = checkOnly;
		CheckLevel = checkLevel;
		CheckMp = checkMp;
		IsEventSkill = isEventSkill;
		ManualTarget = manualTarget;
		CheckBattleEnabled = checkBattleEnabled;
		CheckReqHpRatio = checkReqHpRatio;
		IsIgnoreCoolTime = isIgnoreCoolTime;
		IsIgnoreSoloTime = isIgnoreSoloTime;
		IsCanBeCancel = isCanBeCancel;
		CheckStateCount = checkStateCount;
	}
}
