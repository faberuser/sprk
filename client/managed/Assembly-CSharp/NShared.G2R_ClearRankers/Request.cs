namespace NShared.G2R_ClearRankers;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_clear_rankgers";

	public string Keyword { get; set; }
}
