namespace NShared.BuyHero;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public HeroAddResultInfo HeroResult { get; set; }
}
