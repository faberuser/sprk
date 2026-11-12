using NShared.NRaid;

namespace NShared.GetMultPlayiChallengeRaidRankerList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MultiPlayChallengeRaidRankerInfo[] RankerInfos { get; set; }
}
