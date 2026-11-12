namespace NShared.NOperationCondition;

public class PrevCritical : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		bool valueBool = ArgParser.GetValueBool(values, 0);
		bool flag = CheckPrevDamageWasCritical(firedOperationInfo, targetCreature);
		return valueBool == flag;
	}

	private bool CheckPrevDamageWasCritical(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature)
	{
		if (targetCreature == null)
		{
			return false;
		}
		PrevDamageInfo prevDamageInfo = targetCreature.StatController.PrevDamageInfo;
		if (prevDamageInfo == null)
		{
			return false;
		}
		return (prevDamageInfo.DamageFlag & DamageFlag.Critical) != 0;
	}
}
