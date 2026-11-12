namespace NShared.EnterLobby;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "lobby/enter_lobby";
}
