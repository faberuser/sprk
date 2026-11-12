using NShared;

namespace NGame2.NUI.NWindow.NodeFunctionType.Condition;

public interface ICondition
{
	NShared.NodeFunctionType GetFunctionType(string[] values, NShared.NodeFunctionType originType);

	bool EnterConditionCheck(string[] values);

	bool EnterConditionCheck(string[] values, out int chpaterIndex, out int dunjeonIndex);
}
