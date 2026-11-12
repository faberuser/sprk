namespace NShared.GetAllGuildMemberInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/get_all_guild_member_info";

	public long GuildId { get; set; }
}
