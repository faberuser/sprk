using System.Collections.Generic;

namespace NShared;

public class Survival : IWinDecider
{
	private int _survivalTimeMs;

	private TeamType _survivalTeam;

	private WinDeciderType _winDeciderType;

	void IWinDecider.Init(WinDeciderType type, string[] values)
	{
		_survivalTimeMs = ArgParser.GetValueInt(values, 0, 60000);
		_survivalTeam = ArgParser.GetValueEnum<string, TeamType>((IList<string>)values, 1, TeamType.Left);
		_winDeciderType = type;
	}

	bool IWinDecider.Decide(BattleInstance battleInstance, out TeamType teamType)
	{
		teamType = TeamType.Left;
		if (battleInstance.GetElapsedBattleTimeMs() >= _survivalTimeMs)
		{
			teamType = _survivalTeam;
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
