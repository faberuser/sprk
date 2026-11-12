namespace NShared.AppGuardAuth;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/appguard_auth";

	public int AppGuardAuthResult { get; set; }

	public string AppGuardAuthId { get; set; }

	public string ResultData { get; set; }
}
