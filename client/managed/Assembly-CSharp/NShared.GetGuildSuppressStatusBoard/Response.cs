namespace NShared.GetGuildSuppressStatusBoard;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildSuppressStatusBoardInfo[] GuildSuppressStatusBoardInfos { get; set; }

	public GuildSuppressGroupRankerInfo[] GuildSuppressGroupRankerInfos { get; set; }

	public GuildSuppressSessionInfo GuildSuppressSessionInfo { get; set; }
}
