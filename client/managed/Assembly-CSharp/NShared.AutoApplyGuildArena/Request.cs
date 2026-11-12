namespace NShared.AutoApplyGuildArena;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_arena/auto_apply_guild_arena";

	public bool IsApply { get; set; }
}
