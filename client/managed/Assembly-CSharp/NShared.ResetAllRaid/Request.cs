namespace NShared.ResetAllRaid;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/reset_all_raid";
}
