using System;

namespace NShared.NOperationCondition;

public class HeroClassMyself : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		if (values == null || values.Length == 0)
		{
			return false;
		}
		BattleCreature firedCreature = firedOperationInfo.FiredCreature;
		foreach (string value in values)
		{
			CreatureTagType creatureTagType = (CreatureTagType)Enum.Parse(typeof(CreatureTagType), value, ignoreCase: true);
			if (firedCreature != null && firedCreature.CreatureData.TagType == creatureTagType)
			{
				return true;
			}
		}
		return false;
	}
}
