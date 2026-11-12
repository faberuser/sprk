namespace NShared.GetDispatchList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "dispatch/get_dispatch_list";
}
