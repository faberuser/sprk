namespace NShared.AppGuardViolation;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/appguard_violation";

	public bool IsEmulateDetected { get; set; }

	public string ResultData { get; set; }
}
