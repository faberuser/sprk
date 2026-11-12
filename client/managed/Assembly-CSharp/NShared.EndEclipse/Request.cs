namespace NShared.EndEclipse;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "eclipse/end_eclipse";

	public int MatchIndex { get; set; }
}
