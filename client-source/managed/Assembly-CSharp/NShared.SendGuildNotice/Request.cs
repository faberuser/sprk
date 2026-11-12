namespace NShared.SendGuildNotice;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/send_guild_notice";

	public string Notice { get; set; }
}
