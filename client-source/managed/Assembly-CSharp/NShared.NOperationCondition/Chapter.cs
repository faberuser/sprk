namespace NShared.NOperationCondition;

public class Chapter : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		CampaignDungeonData dungeonData = firedOperationInfo.FiredCreature.BattleInstance.DungeonData;
		if (dungeonData == null)
		{
			return false;
		}
		bool result = false;
		int num = 0;
		while (true)
		{
			int valueInt = ArgParser.GetValueInt(values, num);
			if (valueInt == 0)
			{
				break;
			}
			if (dungeonData.ChapterIndex == valueInt)
			{
				result = true;
				break;
			}
			num++;
		}
		return result;
	}
}
