namespace NShared;

public class DefeatAllEnemy : IWinDecider
{
	private WinDeciderType _winDeciderType;

	void IWinDecider.Init(WinDeciderType type, string[] values)
	{
		_winDeciderType = type;
	}

	bool IWinDecider.Decide(BattleInstance battleInstance, out TeamType teamType)
	{
		teamType = TeamType.Left;
		if (battleInstance.IsBattleTimeEnd())
		{
			teamType = TeamType.Right;
			return true;
		}
		int aliveCreatureCountOfTeam = battleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(TeamType.Left);
		int aliveCreatureCountOfTeam2 = battleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(TeamType.Right);
		if (aliveCreatureCountOfTeam > 0 && aliveCreatureCountOfTeam2 > 0)
		{
			return false;
		}
		teamType = ((aliveCreatureCountOfTeam <= 0) ? TeamType.Right : TeamType.Left);
		return true;
	}

	WinDeciderType IWinDecider.GetWinDeciderType()
	{
		return _winDeciderType;
	}
}
