namespace NShared.InvestGuildBuilding;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildBuildingInfo GuildBuildingInfo { get; set; }

	public int Wood { get; set; }

	public int Stone { get; set; }

	public int Metal { get; set; }
}
