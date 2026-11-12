namespace NShared.AutoApplyGuildSuppress;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_suppress/auto_apply_guild_suppress";

	public bool IsApply { get; set; }
}
