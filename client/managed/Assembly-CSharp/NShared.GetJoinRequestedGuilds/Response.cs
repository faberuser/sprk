namespace NShared.GetJoinRequestedGuilds;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildInfo[] guildInfos { get; set; }
}
