using NGame2.NBattleField;
using NShared;
using NVespa;

namespace NGame2.NUtil.NConditionChecker;

public class PlayWave : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance || !SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return false;
		}
		BattleFacade.CampaignCacheData campaignCache = SingletonAuto<BattleFacade>.instance.GetCampaignCache();
		if (campaignCache == null)
		{
			return false;
		}
		if (ArgParser.GetValueInt(values, 1) != campaignCache.ChapterIndex)
		{
			return false;
		}
		if (ArgParser.GetValueInt(values, 2) != campaignCache.DungeonIndex)
		{
			return false;
		}
		if (ArgParser.GetValueInt(values, 3) != campaignCache.WaveIndex)
		{
			return false;
		}
		if (ArgParser.HasValue(values, 4) && ArgParser.GetValueBool(values, 4) != campaignCache.Scenario)
		{
			return false;
		}
		return true;
	}
}
