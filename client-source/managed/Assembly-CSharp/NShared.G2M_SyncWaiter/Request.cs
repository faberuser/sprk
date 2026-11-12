namespace NShared.G2M_SyncWaiter;

[Request]
public class Request
{
	public Header __Header__ { get; set; }

	public MatchArenaType ArenaType { get; set; }

	public MatchedAccountInfo[] AccountInfosForRegister { get; set; }

	public long[] AccountIdsForTouch { get; set; }

	public long[] CanceledAccountIds { get; set; }

	public long[] PausedAccountIds { get; set; }
}
