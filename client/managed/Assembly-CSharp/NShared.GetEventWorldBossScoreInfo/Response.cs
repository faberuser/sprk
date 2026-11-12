using NShared.NWorldBoss;

namespace NShared.GetEventWorldBossScoreInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EventWorldBossScoreInfo ScoreInfo { get; set; }
}
