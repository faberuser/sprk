namespace NShared;

[JsonSerializable]
public class CreatureFireNextSkillBattleLog : IBattleLogRuntime
{
	public int CreatureUid { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureFireNextSkill;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		if (battleInstance.IsBattleEnabled())
		{
			BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
			if (creature != null && GameRoot.Instance.CreatureDataContainer.GetData(creature.CreatureIndex) != null)
			{
				creature.SkillController.FireNextSkill();
			}
		}
	}
}
