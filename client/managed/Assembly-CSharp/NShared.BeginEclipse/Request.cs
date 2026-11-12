namespace NShared.BeginEclipse;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "eclipse/begin_eclipse";

	public int MatchIndex { get; set; }
}
