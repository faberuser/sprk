namespace NShared.SetGuildAdminRank;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/set_guild_admin_rank";

	public long AccountId { get; set; }

	public int GuildRank { get; set; }
}
