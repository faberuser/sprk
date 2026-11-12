namespace NShared.PingGuildRaid;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_raid/ping_guild_raid";
}
