namespace NShared.NOperationConditionAction;

public class ModValue : IOperationConditionAction
{
	void IOperationConditionAction.Run(int operationNo, FiredOperationInfo firedOperationInfo, string[] conditionActionValues)
	{
		int valueInt = ArgParser.GetValueInt(conditionActionValues, 0);
		string valueString = ArgParser.GetValueString(conditionActionValues, 1);
		firedOperationInfo.ModValue(valueInt, valueString);
	}
}
