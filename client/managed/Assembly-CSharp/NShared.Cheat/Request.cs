namespace NShared.Cheat;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "admin/cheat";

	public string Command { get; set; }
}
