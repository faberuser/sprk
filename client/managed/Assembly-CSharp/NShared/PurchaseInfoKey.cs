namespace NShared;

public class PurchaseInfoKey : Tuple<int, int>
{
	public PurchaseInfoKey(int ShopIndex, int Index)
		: base(ShopIndex, Index)
	{
	}
}
