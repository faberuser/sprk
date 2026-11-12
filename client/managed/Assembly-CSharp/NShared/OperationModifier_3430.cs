namespace NShared;

public class OperationModifier_3430 : IOperationModifier
{
	void IOperationModifier.Modify(OperationModifierArg arg)
	{
		arg.FixedDelayMs = arg.OperationIndex * 100;
	}
}
