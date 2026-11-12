namespace NShared.CancelDispatch;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "dispatch/cancel_dispatch";

	public int SlotIndex { get; set; }
}
