using NShared.NWorldBoss;

namespace NShared.GetEventWorldBossInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EventWorldBossInfo[] EventWorldBossInfos { get; set; }
}
