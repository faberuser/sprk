namespace NShared.InitGuildSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/init_guild_skill";
}
