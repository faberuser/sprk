namespace NShared.G2R_GetRecordsOfHonorTopInfo;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public RecordsOfHonorTopInfo[] TopInfo { get; set; }
}
