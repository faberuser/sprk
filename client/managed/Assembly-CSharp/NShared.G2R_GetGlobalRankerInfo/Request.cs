namespace NShared.G2R_GetGlobalRankerInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_global_rankger_info";
}
