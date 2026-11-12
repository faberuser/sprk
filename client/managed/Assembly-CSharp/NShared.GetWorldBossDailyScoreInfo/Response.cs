namespace NShared.GetWorldBossDailyScoreInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public WorldBossDailyScore WorldBossDailyScore { get; set; }
}
