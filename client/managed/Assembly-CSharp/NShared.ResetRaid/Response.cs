namespace NShared.ResetRaid;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public RaidInfo RaidResult { get; set; }
}
