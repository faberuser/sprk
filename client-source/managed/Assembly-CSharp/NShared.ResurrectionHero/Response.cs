namespace NShared.ResurrectionHero;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public int[] DeadHeroIndices { get; set; }
}
