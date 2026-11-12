namespace NShared.ApplyGuildSuppress;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_suppress/apply_guild_suppress";

	public int SessionIndex { get; set; }
}
