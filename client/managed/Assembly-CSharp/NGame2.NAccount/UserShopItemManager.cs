using NShared;

namespace NGame2.NAccount;

public class UserShopItemManager : NShared.UserShopItemManager
{
	public bool ShopItemExists(int ShopIndex)
	{
		if (Get(ShopIndex, 1) == null)
		{
			return false;
		}
		return true;
	}
}
