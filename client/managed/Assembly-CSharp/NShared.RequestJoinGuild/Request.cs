namespace NShared.RequestJoinGuild;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/request_join_guild";

	public long GuildId { get; set; }
}
