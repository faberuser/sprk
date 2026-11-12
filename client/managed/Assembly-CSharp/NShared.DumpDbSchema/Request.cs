namespace NShared.DumpDbSchema;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "admin/dump_db_schema";

	public int AccountId { get; set; }
}
