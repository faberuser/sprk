namespace NShared.WaitMatch;

[Request]
public class RequestResume : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "match/resume_wait_match";
}
