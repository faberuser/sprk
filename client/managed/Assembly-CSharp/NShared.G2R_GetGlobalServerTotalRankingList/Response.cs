using NShared.NWorldBoss;

namespace NShared.G2R_GetGlobalServerTotalRankingList;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public WorldBossServerTotalRankingInfo[] Rankers { get; set; }
}
