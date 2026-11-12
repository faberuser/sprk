namespace NShared.GetMultPlayiChallengeRaidRankerList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/get_multi_play_challenge_raid_ranker_list";

	public int RaidIndex { get; set; }
}
