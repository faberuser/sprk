namespace NShared.ReinforceTeamLevelBuff;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public TeamLevelBuff TeamLevelBuffInfo { get; set; }
}
