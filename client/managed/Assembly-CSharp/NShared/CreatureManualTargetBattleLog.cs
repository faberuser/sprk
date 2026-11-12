namespace NShared;

[JsonSerializable]
public class CreatureManualTargetBattleLog : IBattleLogRuntime
{
	public int CreatureUid { get; set; }

	public int TargetCreatureUid { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureManualTarget;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
		if (creature != null)
		{
			BattleCreature creature2 = battleInstance.CreatureContainer.GetCreature(TargetCreatureUid);
			if (creature2.TransformController.IsManualMoving())
			{
				creature2.TransformController.StopMove(raiseEvent: true);
			}
			creature.SkillController.SetManualTarget(creature2);
		}
	}
}
