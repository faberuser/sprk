using NShared.NWorldBoss;

namespace NShared.G2R_GetGlobalRankerInfo;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public WorldBossRankerInfo[] Rankers { get; set; }
}
