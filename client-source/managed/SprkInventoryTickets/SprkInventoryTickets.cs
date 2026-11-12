using System;
using System.Collections.Generic;
using System.Globalization;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.UsePotionItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

public static class SprkInventoryTickets
{
	private sealed class Quote
	{
		public int Old;

		public int Added;

		public int Unused;

		public int Gems;

		public int SlotSize;

		public string Session;

		public string Token;

		public NShared.UsePotionItem.Request Request;
	}

	private static Quote pending;

	private static bool waiting;

	private static IconConfirmPopup prompt;

	public static int Compensation(int remaining, int ticket, int price)
	{
		if (remaining < 0 || ticket <= 0 || price <= 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		return checked(System.Math.Max(0, ticket - remaining) * price);
	}

	public static string ConfirmationText(int addedSlots, int unusedSlots, int gems)
	{
		return ((addedSlots > 0) ? ("This ticket will expand your inventory by " + addedSlots.ToString("N0", CultureInfo.InvariantCulture) + " slots. ") : "Your inventory is fully expanded. ") + unusedSlots.ToString("N0", CultureInfo.InvariantCulture) + " unused ticket slots will be converted into " + gems.ToString("N0", CultureInfo.InvariantCulture) + " Rubies.\nUse 1 Large Inventory Expansion Ticket?";
	}

	public static bool TryUse(ItemData<PotionItemData> item)
	{
		if (item == null || item.Index != 2401)
		{
			return false;
		}
		if (waiting || (prompt != null && prompt.gameObject.activeInHierarchy))
		{
			return true;
		}
		prompt = null;
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		if (instance == null || requester == null || string.IsNullOrEmpty(requester.SessionKey))
		{
			return true;
		}
		int inventoryExtend = instance.UserManager.MiscInfo.InventoryExtend;
		int extendMaxCount = GameRoot.Instance.InventoryExtendDataContainer.GetExtendMaxCount();
		InventoryExtendData nextDataByExtendCount = GameRoot.Instance.InventoryExtendDataContainer.GetNextDataByExtendCount(extendMaxCount - 1);
		int actionValue = item.SubData.ActionValue;
		if (nextDataByExtendCount == null || inventoryExtend < 0 || inventoryExtend > extendMaxCount || actionValue <= 0 || nextDataByExtendCount.EquipItemExtendSize <= 0 || nextDataByExtendCount.EquipItemExtendPrice <= 0)
		{
			return false;
		}
		int num = extendMaxCount - inventoryExtend;
		int num2 = Compensation(num, actionValue, nextDataByExtendCount.EquipItemExtendPrice);
		if (num2 == 0)
		{
			return false;
		}
		Quote quote = new Quote
		{
			Old = inventoryExtend,
			Added = System.Math.Min(actionValue, num),
			Unused = System.Math.Max(0, actionValue - num),
			Gems = num2,
			SlotSize = nextDataByExtendCount.EquipItemExtendSize,
			Session = requester.SessionKey,
			Token = Guid.NewGuid().ToString("N")
		};
		prompt = IconConfirmPopup.Open(ConfirmationText(quote.Added * quote.SlotSize, quote.Unused * quote.SlotSize, quote.Gems), "Use", "Cancel", delegate
		{
			prompt = null;
			Use(quote);
		}, delegate
		{
			prompt = null;
		});
		if (prompt != null)
		{
			prompt.AddItem(item.Index, 1);
		}
		return true;
	}

	public static Dictionary<string, object> AddParameters(Dictionary<string, object> fields, NShared.UsePotionItem.Request request)
	{
		Quote quote = pending;
		if (quote == null || request != quote.Request)
		{
			return fields;
		}
		fields["InventoryTicketToken"] = quote.Token;
		fields["ExpectedInventoryExtend"] = quote.Old.ToString(CultureInfo.InvariantCulture);
		fields["ExpectedCompensationGem"] = quote.Gems.ToString(CultureInfo.InvariantCulture);
		return fields;
	}

	private static void Use(Quote quote)
	{
		if (waiting)
		{
			return;
		}
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		AccountManager account = GlobalAutoBehaviour<AccountManager>.instance;
		if (requester == null || requester.SessionKey != quote.Session || account.UserManager.MiscInfo.InventoryExtend != quote.Old)
		{
			MessagePopup.Open("Inventory Expansion", "Your inventory changed. Reopen the ticket and confirm the updated amounts.");
			return;
		}
		waiting = true;
		bool completed = false;
		quote.Request = new NShared.UsePotionItem.Request
		{
			ItemIndex = 2401,
			ItemCount = 1
		};
		pending = quote;
		try
		{
			requester.Request(quote.Request, delegate(NShared.UsePotionItem.Response response)
			{
				if (!completed)
				{
					completed = true;
					waiting = false;
					pending = null;
					if (SingletonUpdater<WebServiceManager>.instance.Requester == requester && !(requester.SessionKey != quote.Session))
					{
						if (response == null || response.BaseResult != BaseResultType.Success || response.Result != ResultType.Success)
						{
							MessagePopup.Open("Inventory Expansion", "The conversion was not confirmed. Reopen your inventory to check the ticket and Ruby balances before trying again.");
						}
						else
						{
							account.ItemManager.Apply(response.ItemResult);
							account.UserManager.SetInventoryExtend(response.NewInventoryExtend);
							account.UserManager.Apply(response.CurrencyResult);
							account.PlayerCurrencyManager.Apply(response.CurrencyResult);
							MessagePopup.Open("Inventory Expansion", string.IsNullOrEmpty(response.InternalErrorMessage) ? "Inventory expanded and unused ticket capacity converted to Rubies." : response.InternalErrorMessage);
						}
					}
				}
			});
		}
		catch (Exception)
		{
			waiting = false;
			pending = null;
			MessagePopup.Open("Inventory Expansion", "Unable to confirm the conversion. Reopen your inventory before retrying.");
		}
	}
}
