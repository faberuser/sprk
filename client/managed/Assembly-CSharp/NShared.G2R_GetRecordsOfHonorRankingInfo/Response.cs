namespace NShared.G2R_GetRecordsOfHonorRankingInfo;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public RecordsOfHonorRankingInfo[] RankingInfo { get; set; }
}
