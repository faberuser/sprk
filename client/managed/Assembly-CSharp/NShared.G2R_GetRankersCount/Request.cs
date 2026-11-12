namespace NShared.G2R_GetRankersCount;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_rankgers_count";

	public string Keyword { get; set; }
}
