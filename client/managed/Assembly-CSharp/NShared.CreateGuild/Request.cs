namespace NShared.CreateGuild;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/create_guild";

	public byte logo { get; set; }

	public byte logoBackground { get; set; }

	public byte joinWay { get; set; }

	public string introduce { get; set; }

	public string name { get; set; }

	public int reqTeamLevel { get; set; }

	public string countryCode { get; set; }
}
