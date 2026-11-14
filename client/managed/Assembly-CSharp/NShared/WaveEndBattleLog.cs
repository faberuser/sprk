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
		// SPRK: punishment bosses end a phase by script (event trigger + a
		// cancelable self-Die). If the Die is skipped, the phase-1 boss would
		// survive into the next wave, so clear survivors of a won wave as
		// Eclipse already does.
		bool punishmentWaveWon = battleInstance.DungeonData.BattleType == BattleType.PunishmentRaid && winner == TeamType.Left;
		if ((battleInstance.DungeonData.BattleType == BattleType.Eclipse || punishmentWaveWon) && flag && !_withdraw)
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
