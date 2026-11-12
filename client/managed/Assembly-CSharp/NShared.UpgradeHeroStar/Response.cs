namespace NShared.UpgradeHeroStar;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public GoldResultInfo GoldResult { get; set; }

	public ItemResultInfo ItemResult { get; set; }

	public int NewHeroStar { get; set; }

	public ExpResultInfo TeamExpResult { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
