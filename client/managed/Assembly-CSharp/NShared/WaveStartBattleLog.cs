namespace NShared;

[JsonSerializable]
public class WaveStartBattleLog : IBattleLogRuntime
{
	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.WaveStart;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		TeamType[] array = new TeamType[2]
		{
			TeamType.Left,
			TeamType.Right
		};
		foreach (TeamType teamId in array)
		{
			BattleCreatureList allCreaturesOfTeam = battleInstance.CreatureContainer.GetAllCreaturesOfTeam((int)teamId);
			for (int j = 0; j < allCreaturesOfTeam.Count; j++)
			{
				BattleCreature battleCreature = allCreaturesOfTeam[j];
				if (!battleCreature.StatController.IsAlive() && !battleCreature.CreateInfo.IsPlayer && battleCreature.CreateInfo.IsAi)
				{
					battleInstance.CreatureContainer.Destroy(battleCreature.Uid, raiseEvent: true);
				}
			}
		}
		battleInstance.OnWaveStartBattle();
	}
}
