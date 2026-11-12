using System;

namespace NShared.NOperationCondition;

public class CreaturePositionType : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		if (values == null || values.Length == 0)
		{
			return false;
		}
		foreach (string value in values)
		{
			NShared.CreaturePositionType creaturePositionType = (NShared.CreaturePositionType)Enum.Parse(typeof(NShared.CreaturePositionType), value, ignoreCase: true);
			if (firedOperationInfo.TargetCreature.CreatureData.PositionType == creaturePositionType)
			{
				return true;
			}
		}
		return false;
	}
}
