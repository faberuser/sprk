namespace NShared.G2R_GetGuildSuppressBoardAllRankers;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildSuppressGroupRankerInfo[] RankerInfos { get; set; }
}
