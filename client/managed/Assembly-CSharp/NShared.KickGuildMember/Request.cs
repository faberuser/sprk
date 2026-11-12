namespace NShared.KickGuildMember;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/kick_guildmember";

	public long AccountId { get; set; }
}
