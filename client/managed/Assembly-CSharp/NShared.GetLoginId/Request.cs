namespace NShared.GetLoginId;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/get_login_id";

	public long AccountId { get; set; }
}
