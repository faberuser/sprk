namespace NShared.ChangeCountryCode;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/change_country_code";

	public string CountryCode { get; set; }
}
