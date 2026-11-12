namespace NShared;

public class OperationModifier_840 : IOperationModifier
{
	void IOperationModifier.Modify(OperationModifierArg arg)
	{
		arg.FixedDelayMs = arg.OperationIndex * 100;
	}
}
