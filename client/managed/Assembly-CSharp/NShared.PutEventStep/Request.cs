namespace NShared.PutEventStep;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "event_step/put_event_step";
}
