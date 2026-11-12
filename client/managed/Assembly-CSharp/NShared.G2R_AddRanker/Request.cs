namespace NShared.G2R_AddRanker;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_add_rankger";

	public string Keyword { get; set; }

	public long Score { get; set; }

	public string ServerName { get; set; }

	public long AccountId { get; set; }
}
