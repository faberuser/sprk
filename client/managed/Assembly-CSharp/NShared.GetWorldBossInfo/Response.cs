using NShared.NWorldBoss;

namespace NShared.GetWorldBossInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public WorldBossInfo[] WorldBossInfos { get; set; }
}
