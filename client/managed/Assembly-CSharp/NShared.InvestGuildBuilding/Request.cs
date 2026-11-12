namespace NShared.InvestGuildBuilding;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/invest_guild_building";

	public int BuildingIndex { get; set; }

	public int Wood { get; set; }

	public int Stone { get; set; }

	public int Metal { get; set; }
}
