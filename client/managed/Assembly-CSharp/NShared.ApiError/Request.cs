namespace NShared.ApiError;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "client_log/api_error";

	public long AccountId { get; set; }

	public string[] LoginIds { get; set; }

	public string[] Versions { get; set; }

	public long[] Seqs { get; set; }

	public string[] PreApis { get; set; }

	public string[] Apis { get; set; }

	public string[] HttpErrors { get; set; }

	public string[] ServerErrors { get; set; }

	public string[] Messages { get; set; }

	public string[] ServerMessages { get; set; }
}
