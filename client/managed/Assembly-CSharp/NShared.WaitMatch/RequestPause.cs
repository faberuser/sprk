namespace NShared.WaitMatch;

[Request]
public class RequestPause : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "match/pause_wait_match";
}
