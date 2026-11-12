namespace NShared.GetChallengeRaidRankerList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/get_challenge_raid_ranker_list";

	public int RaidIndex { get; set; }

	public int PageNo { get; set; }
}
