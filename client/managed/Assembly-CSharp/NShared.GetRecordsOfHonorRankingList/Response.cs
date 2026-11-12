namespace NShared.GetRecordsOfHonorRankingList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RecordsOfHonorSeasonInfo[] SeasonInfo { get; set; }

	public RecordsOfHonorRankingInfo[] ServerInfo { get; set; }

	public RecordsOfHonorRankingInfo[] WorldInfo { get; set; }

	public RecordsOfHonorMyInfo[] MyInfo { get; set; }
}
