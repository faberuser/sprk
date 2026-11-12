namespace NShared.GetGuildSuppressGroupRankerList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_suppress/get_guild_suppress_group_ranker_list";

	public int SessionIndex { get; set; }

	public int GroupIndex { get; set; }
}
