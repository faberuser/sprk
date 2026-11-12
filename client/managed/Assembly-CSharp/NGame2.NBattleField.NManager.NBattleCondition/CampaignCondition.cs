using NShared;
using NVespa;

namespace NGame2.NBattleField.NManager.NBattleCondition;

public class CampaignCondition : BattleFacade.BattleConditionBase
{
	public override bool IsLastKill()
	{
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return false;
		}
		if (SingletonAuto<BattleFacade>.instance.GetAliveCreatureCountOfTeam(TeamType.Left, includeResting: true) == 0)
		{
			return true;
		}
		if (SingletonAuto<BattleFacade>.instance.GetAliveCreatureCountOfTeam(TeamType.Right, includeResting: true) == 0)
		{
			BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
			if (battleCache.WaveIndex == battleCache.TotalWaveIndex)
			{
				return true;
			}
		}
		return false;
	}
}
