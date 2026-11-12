namespace NShared.GetGuildList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildInfo[] guildInfos { get; set; }
}
