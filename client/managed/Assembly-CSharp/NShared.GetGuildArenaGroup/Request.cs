namespace NShared.GetGuildArenaGroup;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/get_guild_arena_group";

	public long SessionIndex { get; set; }
}
