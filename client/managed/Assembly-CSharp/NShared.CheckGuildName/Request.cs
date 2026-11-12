namespace NShared.CheckGuildName;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/check_guild_name";

	public string name { get; set; }
}
