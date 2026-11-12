namespace NShared;

public class NewPayShopProductInfo2 : NewPayShopProductInfo, IDataKeySelector<int>
{
	public new int Key => base.Index;
}
