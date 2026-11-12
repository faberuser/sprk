namespace NShared.GetEclipseInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "eclipse/get_eclipse_info";
}
