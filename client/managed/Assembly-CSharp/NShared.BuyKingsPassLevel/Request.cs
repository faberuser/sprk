namespace NShared.BuyKingsPassLevel;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "kings_pass/buy_kings_pass_level";

	public int SeasonIndex { get; set; }

	public int ReqLevel { get; set; }
}
