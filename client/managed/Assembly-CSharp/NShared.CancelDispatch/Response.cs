namespace NShared.CancelDispatch;

[Response]
public class Response : GenericResponse<ResultType>
{
	public DispatchBattleInfo DispatchBattleInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }

	public StaminaResultInfo EclipseStaminaResult { get; set; }

	public ExpResultInfo TeamExpResult { get; set; }

	public HeroExpResultInfo[] HeroExpResults { get; set; }

	public FlaskEndCampaignResultInfo[] FlaskResults { get; set; }
}
