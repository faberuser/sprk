namespace NShared.RequestLevelUpGuildSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/request_level_up_guild_skill";

	public byte GuildSkillIndex { get; set; }
}
