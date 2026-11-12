namespace NShared.G2R_GetRecordsOfHonorTopInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_records_of_honor_top_info";
}
