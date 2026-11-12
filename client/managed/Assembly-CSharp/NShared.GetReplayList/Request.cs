namespace NShared.GetReplayList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "replay/get_replay_list";

	public ReplayType Type { get; set; }

	public int Count { get; set; }
}
