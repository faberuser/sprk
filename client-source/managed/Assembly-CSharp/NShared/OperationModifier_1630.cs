namespace NShared;

public class OperationModifier_1630 : IOperationModifier
{
	void IOperationModifier.Modify(OperationModifierArg arg)
	{
		arg.FixedDelayMs = arg.OperationIndex * 250;
	}
}
