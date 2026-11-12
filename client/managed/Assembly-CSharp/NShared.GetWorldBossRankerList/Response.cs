using NShared.NWorldBoss;

namespace NShared.GetWorldBossRankerList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public WorldBossRankerInfo[] RankerInfos { get; set; }
}
