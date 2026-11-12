namespace NShared;

public class OperationModifierArg
{
	public int TickIndex;

	public int OperationIndex;

	public FiredOperationInfo Info;

	public int FixedDelayMs;

	public int DistanceDelayMs;

	public OperationData Data => Info.OperationData;
}
