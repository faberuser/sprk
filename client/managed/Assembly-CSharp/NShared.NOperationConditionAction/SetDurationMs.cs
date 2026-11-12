namespace NShared.NOperationConditionAction;

public class SetDurationMs : IOperationConditionAction
{
	void IOperationConditionAction.Run(int operationNo, FiredOperationInfo firedOperationInfo, string[] conditionActionValues)
	{
		int valueInt = ArgParser.GetValueInt(conditionActionValues, 0);
		firedOperationInfo.SetDurationMs(valueInt);
	}
}
