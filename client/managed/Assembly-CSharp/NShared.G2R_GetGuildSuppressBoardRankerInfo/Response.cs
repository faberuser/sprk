namespace NShared.G2R_GetGuildSuppressBoardRankerInfo;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildSuppressGroupRankerInfo[] RankerInfo { get; set; }
}
