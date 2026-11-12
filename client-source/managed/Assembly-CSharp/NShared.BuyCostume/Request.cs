namespace NShared.BuyCostume;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/buy_costume";

	public int HeroIndex { get; set; }

	public int CostumeIndex { get; set; }

	public int BuyGem { get; set; }

	public int BuyGold { get; set; }

	public int BuyMileage { get; set; }
}
