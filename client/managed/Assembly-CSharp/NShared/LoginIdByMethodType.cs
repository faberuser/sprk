namespace NShared;

[JsonSerializable(Javascript = true)]
public class LoginIdByMethodType : IAccountAttrKey<LoginMethodType>
{
	LoginMethodType IAccountAttrKey<LoginMethodType>.Key => LoginMethod;

	public LoginMethodType LoginMethod { get; set; }

	public string LoginId { get; set; }
}
