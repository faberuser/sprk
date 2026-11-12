using NShared.NRaid;

namespace NShared.GetChallengeRaidRankInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ChallengeRaidRankerInfo RankInfo { get; set; }
}
