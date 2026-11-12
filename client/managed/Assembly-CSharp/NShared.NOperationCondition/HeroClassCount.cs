using NShared.NUtil;

namespace NShared.NOperationCondition;

public class HeroClassCount : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		CreatureTagType valueEnum = ArgParser.GetValueEnum(values, 0, CreatureTagType.None);
		int valueInt = ArgParser.GetValueInt(values, 1);
		CheckType valueEnum2 = ArgParser.GetValueEnum(values, 2, CheckType.Equal);
		bool valueBool = ArgParser.GetValueBool(values, 3, defaultValue: true);
		int num = 0;
		BattleCreature targetCreature2 = firedOperationInfo.TargetCreature;
		if (targetCreature2 != null)
		{
			BattleCreatureList allCreaturesOfTeam = targetCreature2.BattleInstance.CreatureContainer.GetAllCreaturesOfTeam(targetCreature2.TeamController.TeamId);
			if (allCreaturesOfTeam != null)
			{
				for (int i = 0; i < allCreaturesOfTeam.Count; i++)
				{
					BattleCreature battleCreature = allCreaturesOfTeam[i];
					if (battleCreature.CreatureData.TagType == valueEnum)
					{
						if (valueBool && battleCreature.IsAliveAndPlaying())
						{
							num++;
						}
						else if (!valueBool)
						{
							num++;
						}
					}
				}
			}
		}
		return Condition.CheckCompareResult(valueEnum2, valueInt, num);
	}
}
