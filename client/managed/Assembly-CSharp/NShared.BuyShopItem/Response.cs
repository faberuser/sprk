namespace NShared.BuyShopItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public RoyalPointResultInfo RoyalPointResult { get; set; }

	public ItemResultInfo ItemResult { get; set; }

	public GuildPointResultInfo GuildPointResult { get; set; }

	public FriendshipPointResultInfo FriendshipPointResult { get; set; }

	public EquipItemInfo[] EquipItems { get; set; }

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo { get; set; }

	public HeroInfo heroInfo { get; set; }

	public PlayerFirstPurchaseInfo firstPurchase { get; set; }

	public ShopItemInfo ShopItem { get; set; }
}
