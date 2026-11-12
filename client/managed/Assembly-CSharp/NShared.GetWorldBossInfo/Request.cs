namespace NShared.GetWorldBossInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "world_boss/get_world_boss_info";
}
