namespace NShared.GetChallengeRaidRankInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/get_challenge_raid_rank_info";

	public int RaidIndex { get; set; }
}
