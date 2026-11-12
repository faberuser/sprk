namespace NShared.G2R_GetRankers;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_rankgers";

	public string Keyword { get; set; }

	public int StartRank { get; set; }

	public int RankerCount { get; set; }

	public string ServerGroup { get; set; }
}
