namespace NShared.ResetHeroTranscendSkill;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }
}
