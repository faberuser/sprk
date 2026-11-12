namespace NShared.GetReplay;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "replay/get_replay";

	public long ReplayUid { get; set; }
}
