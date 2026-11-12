namespace NShared.UpgradeHeroSkill;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo ItemResult { get; set; }
}
