namespace NShared.GetGuildArenaRanker;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/get_guild_arena_ranker";

	public int SeasonIndex { get; set; }

	public int PageNo { get; set; }
}
