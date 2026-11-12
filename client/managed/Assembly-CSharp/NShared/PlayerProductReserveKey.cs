namespace NShared;

public class PlayerProductReserveKey : Tuple<string, string>
{
	public PlayerProductReserveKey(string platform, string productId)
		: base(platform, productId)
	{
	}
}
