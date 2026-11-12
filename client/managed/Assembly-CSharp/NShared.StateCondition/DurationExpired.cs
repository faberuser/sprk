namespace NShared.StateCondition;

public class DurationExpired : IStateCondition
{
	private string _stateType;

	StateConditionType IStateCondition.ConditionType => StateConditionType.DurationExpired;

	string IStateCondition.StateType => _stateType;

	public DurationExpired(string stateType)
	{
		_stateType = stateType;
	}

	bool IStateCondition.Check(StateActionData data)
	{
		return true;
	}
}
