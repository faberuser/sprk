namespace NShared.ApplyGuildArena;

[Response]
public class Response : GenericResponse<ResultType>
{
	public bool GuildArenaApplied { get; set; }
}
