namespace NShared.ChangeNick;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/change_nick";

	public string Nick { get; set; }

	public string CountryCode { get; set; }

	public string Language { get; set; }
}
