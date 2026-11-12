namespace NShared.RequestCompleteDispatch;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "dispatch/request_complete_dispatch";

	public int SlotIndex { get; set; }
}
