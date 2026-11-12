namespace NShared.ApplyGuildArena;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/apply_guild_arena";

	public int SessionIndex { get; set; }
}
