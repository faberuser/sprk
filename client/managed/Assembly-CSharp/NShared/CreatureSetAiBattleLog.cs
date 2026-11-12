namespace NShared;

[JsonSerializable]
public class CreatureSetAiBattleLog : IBattleLogRuntime
{
	public long PlayerId { get; set; }

	public bool SetAi { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.CreatureSetAiBattleLog;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		if (!battleInstance.IsBattleEnabled())
		{
			return;
		}
		BattleCreatureList creaturesOfPlayer = battleInstance.CreatureContainer.GetCreaturesOfPlayer(PlayerId);
		if (creaturesOfPlayer == null || creaturesOfPlayer.Count == 0)
		{
			return;
		}
		for (int i = 0; i < creaturesOfPlayer.Count; i++)
		{
			BattleCreature battleCreature = creaturesOfPlayer[i];
			if (battleCreature == null)
			{
				break;
			}
			battleCreature.BehaviorController.StartPersistentAi(SetAi);
		}
	}
}
