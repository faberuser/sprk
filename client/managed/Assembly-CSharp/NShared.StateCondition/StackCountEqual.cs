namespace NShared.StateCondition;

public class StackCountEqual : IStateCondition
{
	private string _stateType;

	private int _stackCount;

	StateConditionType IStateCondition.ConditionType => StateConditionType.StackCountEqual;

	string IStateCondition.StateType => _stateType;

	public StackCountEqual(string stateType, int stackCount)
	{
		_stateType = stateType;
		_stackCount = stackCount;
	}

	bool IStateCondition.Check(StateActionData data)
	{
		return data.CheckConditionValue(_stackCount);
	}
}
