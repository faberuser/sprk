namespace NShared;

[JsonSerializable]
public class CreatureSwitchBattleLog : IBattleLogRuntime
{
	public int OutCreatureUid { get; set; }

	public int InCreatureUid { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureSwitch;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		if (!battleInstance.IsBattleEnabled())
		{
			return;
		}
		if (OutCreatureUid > 0)
		{
			BattleCreature creature = battleInstance.CreatureContainer.GetCreature(OutCreatureUid);
			if (creature != null && !creature.IsResting)
			{
				creature.BeginResting(0, 0, wakeUpFomAlly: true, WakeUpPivotType.Center, 0);
			}
		}
		if (InCreatureUid > 0)
		{
			BattleCreature creature2 = battleInstance.CreatureContainer.GetCreature(InCreatureUid);
			if (creature2 != null && creature2.IsResting)
			{
				creature2.BeginPlaying();
			}
		}
	}
}
