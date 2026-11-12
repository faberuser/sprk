namespace NShared.GetUserInfoRegion;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/get_user_info_region";

	public LoginMethodType LoginMethod { get; set; }

	public string LoginId { get; set; }
}
