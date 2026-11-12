namespace NShared.SetOfflineMatchResult;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerBattleInfo BattleInfo { get; set; }

	public MatchResultInfo MatchResult { get; set; }
}
