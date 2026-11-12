namespace NShared.G2R_GetGlobalWorldBossRankers;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_global_world_boss_rankgers";
}
