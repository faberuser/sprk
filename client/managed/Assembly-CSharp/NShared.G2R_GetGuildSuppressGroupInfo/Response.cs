namespace NShared.G2R_GetGuildSuppressGroupInfo;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildSuppressInfo GuildSuppressGroupInfo { get; set; }

	public int GuildSuppressGroupRank { get; set; }
}
