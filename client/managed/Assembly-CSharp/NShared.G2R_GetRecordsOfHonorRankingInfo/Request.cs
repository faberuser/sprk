namespace NShared.G2R_GetRecordsOfHonorRankingInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_records_of_honor_ranking_info";
}
