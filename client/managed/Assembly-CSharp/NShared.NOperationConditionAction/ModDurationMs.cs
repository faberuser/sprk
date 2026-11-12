namespace NShared.NOperationConditionAction;

public class ModDurationMs : IOperationConditionAction
{
	void IOperationConditionAction.Run(int operationNo, FiredOperationInfo firedOperationInfo, string[] conditionActionValues)
	{
		int valueInt = ArgParser.GetValueInt(conditionActionValues, 0);
		firedOperationInfo.ModDurationMs(valueInt);
	}
}
