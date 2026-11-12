namespace NShared.NOperationCondition;

public class IsCoolTime : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		return (ArgParser.GetValueBool(values, 1) ? firedOperationInfo.FiredCreature : targetCreature)?.SkillController.CooltimeManager.IsInCooltime(valueInt) ?? false;
	}
}
