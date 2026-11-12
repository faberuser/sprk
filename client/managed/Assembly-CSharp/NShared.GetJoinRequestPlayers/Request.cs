namespace NShared.GetJoinRequestPlayers;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/get_join_request_players";
}
