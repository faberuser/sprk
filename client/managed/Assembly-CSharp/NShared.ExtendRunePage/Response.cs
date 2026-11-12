namespace NShared.ExtendRunePage;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public HeroInfo HeroInfo { get; set; }

	public HeroRunePageInfo[] HeroRunePageInfos { get; set; }
}
