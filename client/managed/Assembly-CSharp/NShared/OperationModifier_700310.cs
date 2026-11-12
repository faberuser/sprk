namespace NShared;

public class OperationModifier_700310 : IOperationModifier
{
	void IOperationModifier.Modify(OperationModifierArg arg)
	{
		arg.FixedDelayMs = arg.OperationIndex * 100;
	}
}
