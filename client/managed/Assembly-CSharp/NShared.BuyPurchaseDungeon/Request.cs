namespace NShared.BuyPurchaseDungeon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "shop/buy_purchase_dungeon";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int Price { get; set; }
}
