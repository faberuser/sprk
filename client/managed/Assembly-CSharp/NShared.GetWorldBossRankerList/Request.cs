using NShared.NWorldBoss;

namespace NShared.GetWorldBossRankerList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "world_boss/get_world_boss_ranker_list";

	public int WorldBossIndex { get; set; }

	public int Season { get; set; }

	public int PageNo { get; set; }

	public WorldBossRankingType RankingType { get; set; }
}
