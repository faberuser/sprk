using NShared.NWorldBoss;

namespace NShared.GetWorldBossRankInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public WorldBossRankerInfo RankInfo { get; set; }

	public int TotalRankerCount { get; set; }
}
