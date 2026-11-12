namespace NShared.B2M_GetMatchInfo;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2m_get_match_info";

	public long MatchUid { get; set; }
}
