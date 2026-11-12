namespace NShared;

[JsonSerializable]
public class CreatureFireEventSkillBattleLog : IBattleLogRuntime
{
	public int CreatureUid { get; set; }

	public int SkillIndex { get; set; }

	public int ManualTargetUid { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureFireEventSkill;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		if (!battleInstance.IsBattleEnabled())
		{
			return;
		}
		BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
		if (creature == null || GameRoot.Instance.CreatureDataContainer.GetData(creature.CreatureIndex) == null)
		{
			return;
		}
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(SkillIndex);
		if (data == null)
		{
			return;
		}
		BattleCreature battleCreature = null;
		if (ManualTargetUid > 0)
		{
			battleCreature = battleInstance.CreatureContainer.GetCreature(ManualTargetUid);
			if (battleCreature == null)
			{
				return;
			}
		}
		creature.SkillController.StopUsingDurationalSkill(isCancel: true);
		SkillUseOption option = new SkillUseOption(checkOnly: false, checkLevel: true, checkMp: true, isEventSkill: true, battleCreature);
		creature.SkillController.UseSkill(data, option, null);
	}
}
