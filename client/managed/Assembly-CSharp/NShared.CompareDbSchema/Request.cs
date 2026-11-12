namespace NShared.CompareDbSchema;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "admin/compare_db_schema";

	public int AccountId { get; set; }
}
