namespace NShared.LevelUpGuild;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/level_up_guild";

	public byte Level { get; set; }
}
