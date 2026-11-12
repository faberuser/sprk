namespace NShared.G2R_GetGuildSuppressPlayInfos;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildSuppressPlayInfo[] GuildSuppressPlayInfos { get; set; }
}
