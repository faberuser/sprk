namespace NShared.GetWorldBossHp;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "world_boss/get_world_boss_hp";

	public int WorldBossIndex { get; set; }
}
