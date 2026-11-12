namespace NShared.G2R_GetGuildSuppressPlayInfo;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildSuppressPlayInfo GuildSuppressPlayInfo { get; set; }
}
