namespace NShared.G2R_InitRedisServerGroupRankKey;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_init_global_server_total_rank_key";
}
