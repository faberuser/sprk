namespace NShared;

[BattlePacket]
public class BattleCancelNot
{
	public enum ReasonType
	{
		None,
		WaveNotStart,
		UserLogOff
	}

	public ReasonType Reason { get; set; }
}
