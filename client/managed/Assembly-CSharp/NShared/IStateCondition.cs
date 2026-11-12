namespace NShared;

public interface IStateCondition
{
	string StateType { get; }

	StateConditionType ConditionType { get; }

	bool Check(StateActionData data);
}
