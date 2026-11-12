namespace NShared.GetEventWorldBossHp;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "event_world_boss/get_event_world_boss_hp";

	public int EventWorldBossIndex { get; set; }
}
