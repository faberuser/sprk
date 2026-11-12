namespace NShared.LevelUpGuild;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildActivityPointResultInfo GuildActivityPointResult { get; set; }

	public GuildBuildingInfo GuildBuildingInfo { get; set; }
}
