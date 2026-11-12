namespace NShared;

[JsonSerializable(Javascript = true)]
public class PayShopItemEventInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => Id;

	public long Id { get; set; }

	public PayShopCategory PayShopCategory { get; set; }

	public int Index { get; set; }

	public string Group { get; set; }

	public int DiscountPercentage { get; set; }

	public int Bonus { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }
}
