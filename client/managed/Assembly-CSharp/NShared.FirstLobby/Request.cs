namespace NShared.FirstLobby;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/first_lobby";
}
