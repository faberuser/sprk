namespace NShared.G2R_GetGuildSuppressCurrentSessionInfo;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildSuppressSessionInfo GuildSuppressSessionInfo { get; set; }
}
