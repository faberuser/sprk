namespace NShared.BuyHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/buy_hero";

	public int HeroIndex { get; set; }

	public int ItemIndex { get; set; }

	public int BuyGem { get; set; }
}
