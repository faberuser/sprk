namespace NShared.G2R_GetGuildSuppressBoardRankerInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_guild_suppress_board_ranker_info";
}
