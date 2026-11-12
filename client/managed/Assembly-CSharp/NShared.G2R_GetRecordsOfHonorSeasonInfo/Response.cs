namespace NShared.G2R_GetRecordsOfHonorSeasonInfo;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public RecordsOfHonorSeasonInfo[] SeasonInfo { get; set; }
}
