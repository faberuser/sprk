using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;

// Technomagic Enhancement: when the player is short on enhancement stones but owns selection tickets
// that offer them, keep Enhance usable and open the ticket popup prefilled with the shortfall.
// After the tickets are redeemed the enhancement continues as if the stones had been there.
public static class SprkTechnomagicStoneTickets
{
	private static ItemData Stone(ValanceAwakenData data)
	{
		if (data == null || data.ItemCount1 <= 0 || string.IsNullOrEmpty(data.ItemCode1))
		{
			return null;
		}
		return GameRoot.Instance.ItemDataContainer.GetDataByCode(data.ItemCode1);
	}

	private static int Owned(ValanceAwakenData data)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data.ItemCode1);
	}

	// True when owned stones plus redeemable tickets cover the selected enhancement.
	public static bool Coverable(ValanceAwakenData data)
	{
		ItemData itemData = Stone(data);
		if (itemData == null)
		{
			return false;
		}
		long num = Owned(data);
		if (num >= data.ItemCount1)
		{
			return true;
		}
		return num + new SprkSkillBookTickets.Pool().Available(itemData.Index) >= data.ItemCount1;
	}

	// Returns true when it took over the click (stones are short and a ticket popup was opened).
	public static bool Offer(ValanceAwakenWindow window)
	{
		if (window == null)
		{
			return false;
		}
		ValanceAwakenData data = window.ValanceAwakenData;
		EquipItemInfo equipItemInfo = window.EquipItemInfo;
		ItemData itemData = Stone(data);
		if (equipItemInfo == null || itemData == null || Owned(data) >= data.ItemCount1)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold < data.ReqGold)
		{
			return false;
		}
		int slot = equipItemInfo.SlotIndex;
		int star = window.AfterStar;
		return SprkSkillBookTickets.Offer(itemData, data.ItemCount1, SprkText.Get("Material Tickets"), delegate
		{
			// Continue only if the window still shows the same gear, target star and cost.
			if (!(window == null) && window.IsOpen() && window.EquipItemInfo != null && window.EquipItemInfo.SlotIndex == slot && window.ValanceAwakenData == data && window.AfterStar == star && Owned(data) >= data.ItemCount1 && GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold >= data.ReqGold)
			{
				window.OnClickAwaken();
			}
		});
	}
}
