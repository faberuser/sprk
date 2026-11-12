namespace NShared;

[BattlePacket]
public class BattleEndNot
{
	public enum ReasonType
	{
		Ended,
		NotStarted,
		ServerError,
		AllLogOff,
		StartError,
		MasterLogOff
	}

	public ReasonType Reason { get; set; }
}
