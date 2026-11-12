namespace NShared.GetEventWorldBossScoreInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "event_world_boss/get_event_world_boss_score_info";

	public int EventWorldBossIndex { get; set; }

	public int Season { get; set; }
}
