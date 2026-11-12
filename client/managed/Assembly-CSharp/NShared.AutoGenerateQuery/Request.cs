namespace NShared.AutoGenerateQuery;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "admin/auto_generate_query";

	public int AccountId { get; set; }
}
