using NShared.NWorldBoss;

namespace NShared.GetWorldBossRankInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "world_boss/get_world_boss_rank_info";

	public int WorldBossIndex { get; set; }

	public int Season { get; set; }

	public WorldBossRankingType RankingType { get; set; }
}
