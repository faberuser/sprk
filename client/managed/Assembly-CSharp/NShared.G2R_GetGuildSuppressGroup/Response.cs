namespace NShared.G2R_GetGuildSuppressGroup;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildSuppressInfo GuildSuppressGroupInfo { get; set; }
}
