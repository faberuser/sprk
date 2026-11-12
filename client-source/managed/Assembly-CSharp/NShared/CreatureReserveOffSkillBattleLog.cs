namespace NShared;

[JsonSerializable]
public class CreatureReserveOffSkillBattleLog : IBattleLogRuntime
{
	public int CreatureUid { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureReserveOffSkill;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
		if (creature != null && GameRoot.Instance.CreatureDataContainer.GetData(creature.CreatureIndex) != null)
		{
			creature.SkillController.CancelReservedSkill();
		}
	}
}
