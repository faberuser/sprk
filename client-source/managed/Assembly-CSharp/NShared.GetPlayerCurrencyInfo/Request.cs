namespace NShared.GetPlayerCurrencyInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/get_player_currency_info";

	public CurrencyType CurrencyType { get; set; }
}
