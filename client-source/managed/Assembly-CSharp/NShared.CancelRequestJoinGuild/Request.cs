namespace NShared.CancelRequestJoinGuild;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/cancel_request_join_guild";

	public long GuildId { get; set; }
}
