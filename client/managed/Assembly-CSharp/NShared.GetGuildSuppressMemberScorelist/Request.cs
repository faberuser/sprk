namespace NShared.GetGuildSuppressMemberScorelist;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_suppress/get_guild_suppress_member_score_list";

	public int SessionIndex { get; set; }
}
