using NShared;
using NVespa;

namespace NGame2.NBattleField.NManager.NBattleCondition;

public class ArenaCondition : BattleFacade.BattleConditionBase
{
	public override bool IsLastKill()
	{
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return false;
		}
		if (SingletonAuto<BattleFacade>.instance.GetAliveCreatureCountOfTeam(TeamType.Left) == 0)
		{
			return true;
		}
		if (SingletonAuto<BattleFacade>.instance.GetAliveCreatureCountOfTeam(TeamType.Right) == 0)
		{
			return true;
		}
		return false;
	}
}
