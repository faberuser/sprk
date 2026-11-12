namespace NShared.CreateGuild;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public long GuildId { get; set; }
}
