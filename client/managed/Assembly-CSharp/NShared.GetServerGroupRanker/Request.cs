namespace NShared.GetServerGroupRanker;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "global_arena/get_server_group_ranker";
}
