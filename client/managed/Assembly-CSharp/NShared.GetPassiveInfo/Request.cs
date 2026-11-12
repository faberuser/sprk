namespace NShared.GetPassiveInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shakmeh_dungeon/get_passive_info";
}
