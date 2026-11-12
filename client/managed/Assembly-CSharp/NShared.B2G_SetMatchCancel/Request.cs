namespace NShared.B2G_SetMatchCancel;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_set_match_cancel";

	public int SeasonIndex { get; set; }

	public long MatchUid { get; set; }

	public long AccountId { get; set; }

	public MatchArenaType ArenaType { get; set; }
}
