namespace NShared.G2R_CheatDeleteGuildSuppressGroup;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_cheat_delete_guild_suppress_group";
}
