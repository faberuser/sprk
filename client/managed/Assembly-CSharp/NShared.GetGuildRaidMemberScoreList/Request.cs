namespace NShared.GetGuildRaidMemberScoreList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_raid/get_guild_raid_member_score_list";
}
