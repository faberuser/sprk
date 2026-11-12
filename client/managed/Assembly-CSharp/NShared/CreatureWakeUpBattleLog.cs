namespace NShared;

[JsonSerializable]
public class CreatureWakeUpBattleLog : IBattleLogRuntime
{
	public int CreatureUid { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureWakeUp;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
		if (creature != null && creature.IsResting)
		{
			creature.BeginPlaying();
		}
	}
}
