namespace NShared.BuyGuildRaidBootyItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_raid/buy_guild_raid_booty_item";

	public int ShopIndex { get; set; }

	public long Id { get; set; }

	public int ItemIndex { get; set; }

	public int Count { get; set; }
}
