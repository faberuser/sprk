namespace NShared.G2R_GetGlobalServerTotalRankingList;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_global_total_ranking_list";

	public string Keyword { get; set; }
}
