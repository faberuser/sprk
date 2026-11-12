using System;

namespace NShared.NOperationCondition;

public class HeroClass : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		if (values == null || values.Length == 0)
		{
			return false;
		}
		foreach (string value in values)
		{
			CreatureTagType creatureTagType = (CreatureTagType)Enum.Parse(typeof(CreatureTagType), value, ignoreCase: true);
			if (targetCreature != null && targetCreature.CreatureData.TagType == creatureTagType)
			{
				return true;
			}
		}
		return false;
	}
}
