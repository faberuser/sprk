namespace NShared.GetUnderPrisonInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "under_prison/get_under_prison_info";
}
