namespace NShared.RequestJoinGuild;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int JoinWay { get; set; }

	public MyGuildInfo guildInfo { get; set; }
}
