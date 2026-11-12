namespace NShared.GetGuildArenaMemberDeck;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/get_guild_arena_member_deck";

	public long SessionIndex { get; set; }
}
