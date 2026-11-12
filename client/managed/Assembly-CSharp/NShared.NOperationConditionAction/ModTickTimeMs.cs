namespace NShared.NOperationConditionAction;

public class ModTickTimeMs : IOperationConditionAction
{
	void IOperationConditionAction.Run(int operationNo, FiredOperationInfo firedOperationInfo, string[] conditionActionValues)
	{
		int valueInt = ArgParser.GetValueInt(conditionActionValues, 0);
		firedOperationInfo.ModTickTimeMs(valueInt);
	}
}
