namespace NShared.DestroyGuild;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int GuildWithdrawCount { get; set; }

	public string GuildWithdrawTime { get; set; }
}
