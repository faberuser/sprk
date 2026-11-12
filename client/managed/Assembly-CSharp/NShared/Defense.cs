namespace NShared;

public class Defense : IWinDecider
{
	private WinDeciderType _winDeciderType;

	void IWinDecider.Init(WinDeciderType type, string[] values)
	{
		_winDeciderType = type;
	}

	bool IWinDecider.Decide(BattleInstance battleInstance, out TeamType teamType)
	{
		teamType = TeamType.Left;
		if (battleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(TeamType.Left) == 0)
		{
			return true;
		}
		if (battleInstance.IsBattleTimeEnd())
		{
			return true;
		}
		return false;
	}

	WinDeciderType IWinDecider.GetWinDeciderType()
	{
		return _winDeciderType;
	}
}
