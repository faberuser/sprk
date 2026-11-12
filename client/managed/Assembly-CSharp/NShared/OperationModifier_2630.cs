namespace NShared;

public class OperationModifier_2630 : IOperationModifier
{
	void IOperationModifier.Modify(OperationModifierArg arg)
	{
		arg.FixedDelayMs = arg.OperationIndex * 100;
	}
}
