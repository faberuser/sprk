using NShared.NUtil;

namespace NShared.NOperationCondition;

public class CompareDamage : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		if (!(firedOperationInfo.TriggerInfo is ITriggerByDamage triggerByDamage))
		{
			return false;
		}
		int valueInt = ArgParser.GetValueInt(values, 0);
		StatType valueEnum = ArgParser.GetValueEnum(values, 1, StatType.None);
		CheckType valueEnum2 = ArgParser.GetValueEnum(values, 2, CheckType.MoreOrEqual);
		bool valueBool = ArgParser.GetValueBool(values, 3);
		long amount = triggerByDamage.GetAmount();
		BattleCreature battleCreature = (valueBool ? firedOperationInfo.FiredCreature : targetCreature);
		if (battleCreature == null)
		{
			return false;
		}
		long checkValue = battleCreature.StatController.GetFinalStat(valueEnum) * valueInt / 1000;
		return Condition.CheckCompareResult(valueEnum2, checkValue, amount);
	}
}
