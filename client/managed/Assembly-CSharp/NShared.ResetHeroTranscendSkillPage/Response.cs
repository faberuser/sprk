namespace NShared.ResetHeroTranscendSkillPage;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public HeroInfo Hero { get; set; }
}
