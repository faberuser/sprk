namespace NShared.GetGuildSuppressStatusBoard;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_suppress/get_guild_suppress_status_board";
}
