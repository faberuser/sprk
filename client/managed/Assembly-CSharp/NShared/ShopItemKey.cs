namespace NShared;

public class ShopItemKey : Tuple<int, int>
{
	public ShopItemKey(int ShopIndex, int ListNo)
		: base(ShopIndex, ListNo)
	{
	}
}
