namespace NShared;

public interface IOperationConditionAction
{
	void Run(int operationNo, FiredOperationInfo firedOperationInfo, string[] conditionActionValues);
}
