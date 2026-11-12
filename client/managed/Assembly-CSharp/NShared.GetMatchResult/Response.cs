namespace NShared.GetMatchResult;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MatchResultInfo MatchResult { get; set; }

	public PlayerBattleInfo BattleInfo { get; set; }

	public PlayerBattleBanPickInfo BattleBanPickInfo { get; set; }

	public StaminaResultInfo SwordResult { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
