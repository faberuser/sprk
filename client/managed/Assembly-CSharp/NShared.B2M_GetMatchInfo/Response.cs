namespace NShared.B2M_GetMatchInfo;

[BattlePacket]
public class Response : GenericResponse<ResultType>
{
	public MatchInfo MatchInfo { get; set; }
}
