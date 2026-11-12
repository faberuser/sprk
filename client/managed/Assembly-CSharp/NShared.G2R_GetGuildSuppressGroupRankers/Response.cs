namespace NShared.G2R_GetGuildSuppressGroupRankers;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildSuppressGroupRankerInfo[] Rankers { get; set; }
}
