namespace NShared.QueryMarketUrl;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/query_market_url";

	public MarketType MarketType { get; set; }

	public int AppPlayerBuild { get; set; }

	public string Language { get; set; }
}
