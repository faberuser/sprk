using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public static class HelperItem
{
	public static ItemInfo GetFreeGachaItem(int gachaIndex)
	{
		if (!AccountManager.IsLogined())
		{
			return null;
		}
		int gachaTicketIndex = GameRoot.Instance.GachaSelectItemDataContainer.GetGachaTicketIndex(gachaIndex);
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(gachaTicketIndex);
	}

	public static int GetFreeGachaItemCount(int gachaIndex)
	{
		return GetFreeGachaItem(gachaIndex)?.Count ?? 0;
	}
}
