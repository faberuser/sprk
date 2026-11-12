namespace NShared;

[JsonSerializable]
public class WaveEndBattleLog : IBattleLogRuntime
{
	public TeamType winner { get; set; }

	public WinDeciderType _winDecider { get; set; }

	public bool _withdraw { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.WaveEnd;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		bool flag = _winDecider == WinDeciderType.DefeatAllEnemy || _winDecider == WinDeciderType.None;
		if (battleInstance.DungeonData.BattleType == BattleType.Eclipse && flag && !_withdraw)
		{
			BattleCreatureList aliveCreaturesOfOtherTeam = battleInstance.CreatureContainer.GetAliveCreaturesOfOtherTeam(winner);
			for (int i = 0; i < aliveCreaturesOfOtherTeam.Count; i++)
			{
				if (aliveCreaturesOfOtherTeam[i].IsAliveAndPlaying())
				{
					aliveCreaturesOfOtherTeam[i].Die(0, 0L);
				}
			}
		}
		battleInstance.ProcessBattleEnd();
	}
}
