namespace NShared.OpenGuildBuilding;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string OpenGuildTime { get; set; }

	public string GuildMemberRewardedTime { get; set; }

	public GuildBuildingInfo[] GuildBuildingInfos { get; set; }
}
