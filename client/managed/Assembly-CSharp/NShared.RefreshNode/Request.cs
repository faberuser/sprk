namespace NShared.RefreshNode;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "ordeal_arena/refresh_node";

	public int NodeIndex { get; set; }

	public bool UseGem { get; set; }
}
