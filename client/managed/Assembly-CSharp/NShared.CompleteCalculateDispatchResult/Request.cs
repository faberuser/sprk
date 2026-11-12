namespace NShared.CompleteCalculateDispatchResult;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "dispatch/complete_calculate_dispatch_result";
}
