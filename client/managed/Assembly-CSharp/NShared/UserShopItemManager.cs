namespace NShared;

public class UserShopItemManager : AccountAttrManager<ShopItemKey, ShopListItemInfo>
{
	public ShopListItemInfo Get(int ShopIndex, int ListNo)
	{
		return Get(new ShopItemKey(ShopIndex, ListNo));
	}
}
