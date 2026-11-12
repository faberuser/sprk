namespace NShared.G2R_ChangeCountryCodeRanker;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_change_country_code_rankger";

	public string Keyword { get; set; }

	public string ServerName { get; set; }

	public long AccountId { get; set; }
}
