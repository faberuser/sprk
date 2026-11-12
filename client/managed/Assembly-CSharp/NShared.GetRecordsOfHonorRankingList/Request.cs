namespace NShared.GetRecordsOfHonorRankingList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "records_of_honor/get_records_of_honor_ranking_list";

	public RecordsOfHonorContentType ContentType { get; set; }

	public int Season { get; set; }
}
