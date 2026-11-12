using System.Collections.Generic;

namespace NShared;

public class MoreCreatureAlive : IWinDecider
{
	private WinDeciderType _winDeciderType;

	void IWinDecider.Init(WinDeciderType type, string[] values)
	{
		_winDeciderType = type;
	}

	bool IWinDecider.Decide(BattleInstance battleInstance, out TeamType teamType)
	{
		teamType = TeamType.Left;
		int aliveCreatureCountOfTeam = battleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(TeamType.Left);
		int aliveCreatureCountOfTeam2 = battleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(TeamType.Right);
		if (aliveCreatureCountOfTeam == 0 || aliveCreatureCountOfTeam2 == 0)
		{
			teamType = ((aliveCreatureCountOfTeam <= 0) ? TeamType.Right : TeamType.Left);
			return true;
		}
		if (battleInstance.IsBattleTimeEnd())
		{
			if (aliveCreatureCountOfTeam == aliveCreatureCountOfTeam2)
			{
				long num = 0L;
				long num2 = 0L;
				long num3 = 0L;
				long num4 = 0L;
				List<BattleCreature> allCreatures = battleInstance.CreatureContainer.GetAllCreatures();
				for (int i = 0; i < allCreatures.Count; i++)
				{
					BattleCreature battleCreature = allCreatures[i];
					_ = battleCreature.Uid;
					switch (battleCreature.TeamController.TeamType)
					{
					case TeamType.Left:
						num += battleCreature.StatController.Hp;
						num2 += battleCreature.StatController.GetMaxHp();
						break;
					case TeamType.Right:
						num3 += battleCreature.StatController.Hp;
						num4 += battleCreature.StatController.GetMaxHp();
						break;
					}
				}
				decimal num5 = ((num2 > 0) ? ((decimal)num / (decimal)num2) : 0m);
				decimal num6 = ((num4 > 0) ? ((decimal)num3 / (decimal)num4) : 0m);
				teamType = ((!(num5 > num6)) ? TeamType.Right : TeamType.Left);
			}
			else
			{
				teamType = ((aliveCreatureCountOfTeam <= aliveCreatureCountOfTeam2) ? TeamType.Right : TeamType.Left);
			}
			return true;
		}
		return false;
	}

	WinDeciderType IWinDecider.GetWinDeciderType()
	{
		return _winDeciderType;
	}
}
