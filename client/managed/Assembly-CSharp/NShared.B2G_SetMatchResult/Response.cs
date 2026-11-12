namespace NShared.B2G_SetMatchResult;

[BattlePacket]
public class Response : GenericResponse<ResultType>
{
	public MatchResultInfo MatchResult { get; set; }
}
