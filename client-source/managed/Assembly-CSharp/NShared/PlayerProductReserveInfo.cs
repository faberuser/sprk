namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerProductReserveInfo : IAccountAttrKey<PlayerProductReserveKey>
{
	PlayerProductReserveKey IAccountAttrKey<PlayerProductReserveKey>.Key => new PlayerProductReserveKey(Platform, ProductId);

	public string Platform { get; set; }

	public string ProductId { get; set; }

	public int ProductIndex { get; set; }

	public string ReservedTime { get; set; }

	public string ExpireTime { get; set; }

	public string ExtraInfo { get; set; }
}
