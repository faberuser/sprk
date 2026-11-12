using NShared.NRaid;

namespace NShared.GetChallengeRaidRankerList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ChallengeRaidRankerInfo[] RankerInfos { get; set; }
}
