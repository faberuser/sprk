namespace NShared.LevelUpGuildBuilding;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/level_up_guild_building";

	public int BuildingIndex { get; set; }

	public byte BuildingLevel { get; set; }
}
