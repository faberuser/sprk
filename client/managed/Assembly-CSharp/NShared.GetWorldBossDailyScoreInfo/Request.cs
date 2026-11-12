namespace NShared.GetWorldBossDailyScoreInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "world_boss/get_world_boss_daily_score_info";

	public int WorldBossIndex { get; set; }

	public int Season { get; set; }
}
