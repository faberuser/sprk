namespace NShared.InsertUserData;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/insert_user_data";

	public string Region { get; set; }

	public int AccountId { get; set; }
}
