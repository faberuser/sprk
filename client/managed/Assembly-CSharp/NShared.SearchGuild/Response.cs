namespace NShared.SearchGuild;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildInfo Info { get; set; }
}
