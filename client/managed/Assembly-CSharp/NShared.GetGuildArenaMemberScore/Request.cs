namespace NShared.GetGuildArenaMemberScore;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/get_guild_arena_member_score";

	public int SessionIndex { get; set; }

	public int PageNo { get; set; }
}
