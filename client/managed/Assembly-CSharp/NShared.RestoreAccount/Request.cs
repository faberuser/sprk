namespace NShared.RestoreAccount;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/restore_account";

	public LoginMethodType LoginMethod { get; set; }

	public string LoginId { get; set; }

	public string LegacyLoginId { get; set; }

	public long EncryptSalt { get; set; }
}
