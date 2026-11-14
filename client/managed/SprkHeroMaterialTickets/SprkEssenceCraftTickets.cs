using System;
using System.Collections.Generic;
using System.Globalization;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using NShared;
using NShared.UsePackageSelectItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

public static class SprkEssenceCraftTickets
{
	private sealed class Crafting
	{
		public CraftItemSelectMessagePopup Popup;

		public CraftItemData Recipe;

		public ItemData Essence;

		public ItemData Fragment;

		public int Count;

		public int Required;

		public Action<int, int, int> OnCraft;

		public string Session;

		public object Requester;

		public AccountManager Account;
	}

	private static Crafting pending;

	public static CraftItemData.MaterialItemInfo SingleMaterial(List<CraftItemData.MaterialItemInfo> slots)
	{
		CraftItemData.MaterialItemInfo materialItemInfo = null;
		foreach (CraftItemData.MaterialItemInfo slot in slots)
		{
			if (slot != null && (!string.IsNullOrWhiteSpace(slot.ItemCode) || slot.ItemCount != 0))
			{
				if (string.IsNullOrWhiteSpace(slot.ItemCode) || slot.ItemCount <= 0 || materialItemInfo != null)
				{
					return null;
				}
				materialItemInfo = slot;
			}
		}
		return materialItemInfo;
	}

	private static bool Recipe(CraftItemSelectMessagePopup popup, out ItemData essence, out ItemData fragment, out int price)
	{
		return RecipeData(popup.CraftUi.SelectedData, out essence, out fragment, out price);
	}

	private static bool RecipeData(CraftItemData row, out ItemData essence, out ItemData fragment, out int price)
	{
		essence = null;
		fragment = null;
		price = 0;
		if (row == null || !row.IsOpen || !row.Countable || row.ReqTime != 0 || row.ResultItemCount <= 0)
		{
			return false;
		}
		ItemDataContainer itemDataContainer = GameRoot.Instance.ItemDataContainer;
		essence = itemDataContainer.GetDataByCode(row.ItemCode);
		if (!SprkHeroMaterialTickets.IsMaterial(essence, HeroAwakeType.Transcend))
		{
			return false;
		}
		CraftItemData.MaterialItemInfo materialItemInfo = SingleMaterial(row.GetMaterialItemList());
		if (materialItemInfo == null)
		{
			return false;
		}
		fragment = itemDataContainer.GetDataByCode(materialItemInfo.ItemCode);
		price = materialItemInfo.ItemCount;
		return SprkHeroMaterialTickets.IsMaterial(fragment, HeroAwakeType.Awaken);
	}

	public static int Indicator(string code, long required, int owned)
	{
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
		if (SprkHeroMaterialTickets.IsMaterial(dataByCode, HeroAwakeType.Awaken))
		{
			if (owned >= required || SprkHeroMaterialTickets.Tickets(dataByCode.Index).Count <= 0)
			{
				return 0;
			}
			return 1;
		}
		if (!SprkHeroMaterialTickets.IsMaterial(dataByCode, HeroAwakeType.Transcend))
		{
			return -1;
		}
		if (owned >= required)
		{
			return 0;
		}
		IList<CraftItemData> categoryData = GameRoot.Instance.CraftItemDataContainer.GetCategoryData(code);
		if (categoryData == null)
		{
			return 0;
		}
		NGame2.NAccount.UserItemManager itemManager = GlobalAutoBehaviour<AccountManager>.instance.ItemManager;
		foreach (CraftItemData item in categoryData)
		{
			if (!RecipeData(item, out var _, out var fragment, out var price))
			{
				continue;
			}
			long num = itemManager.GetItemCount(fragment.Code);
			foreach (Tuple<ItemData, int> item2 in SprkHeroMaterialTickets.Tickets(fragment.Index))
			{
				num += item2.Item2;
			}
			if (num >= price)
			{
				return 1;
			}
		}
		return 0;
	}

	public static int Available(NGame2.NAccount.UserItemManager manager, string code, CraftItemSelectMessagePopup popup)
	{
		int itemCount = manager.GetItemCount(code);
		if (!Recipe(popup, out var _, out var fragment, out var _) || fragment.Code != code)
		{
			return itemCount;
		}
		long num = itemCount;
		foreach (Tuple<ItemData, int> item in SprkHeroMaterialTickets.Tickets(fragment.Index))
		{
			num += item.Item2;
		}
		return (int)System.Math.Min(2147483647L, num);
	}

	public static int Quantity(int required, int perCraft)
	{
		if (required <= 0 || perCraft <= 0)
		{
			return 1;
		}
		return (int)System.Math.Min(2147483647L, ((long)required + (long)perCraft - 1) / perCraft);
	}

	public static void Prefill(CraftItemSelectMessagePopup popup, int required)
	{
		if (required > 0 && Recipe(popup, out var _, out var _, out var _))
		{
			popup.MaterialUi.NumberInput.value = Quantity(required, popup.CraftUi.SelectedData.ResultItemCount).ToString(CultureInfo.InvariantCulture);
			popup.OnNumberChanged();
		}
	}

	public static bool TryCraft(CraftItemSelectMessagePopup popup, Action<int, int, int> onCraft)
	{
		if (pending != null)
		{
			return true;
		}
		if (onCraft == null || !Recipe(popup, out var essence, out var fragment, out var price))
		{
			return false;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		int number = popup.GetNumber();
		long num = (long)number * (long)price;
		if (number <= 0 || num > int.MaxValue)
		{
			return true;
		}
		if (instance.ItemManager.GetItemCount(fragment.Code) >= num)
		{
			return false;
		}
		if (Available(instance.ItemManager, fragment.Code, popup) < num)
		{
			MessagePopup.Open(SprkText.Get("Craft Essence"), SprkText.Get("Not enough fragments or fragment tickets for this quantity."));
			return true;
		}
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		if (requester == null)
		{
			return true;
		}
		pending = new Crafting
		{
			Popup = popup,
			Recipe = popup.CraftUi.SelectedData,
			Essence = essence,
			Fragment = fragment,
			Count = number,
			Required = (int)num,
			OnCraft = onCraft,
			Session = requester.SessionKey,
			Requester = requester,
			Account = instance
		};
		UIHelper.EnableButton(popup.MaterialUi.Button_Craft, enable: false);
		Next(pending);
		return true;
	}

	private static void Stop(Crafting work, string message)
	{
		if (pending == work)
		{
			pending = null;
			if (work.Popup != null && work.Popup.IsOpen() && work.Popup.CraftUi.SelectedData == work.Recipe)
			{
				UIHelper.EnableButton(work.Popup.MaterialUi.Button_Craft, enable: true);
				work.Popup.OnNumberChanged();
			}
			if (message != null)
			{
				MessagePopup.Open(SprkText.Get("Craft Essence"), message);
			}
		}
	}

	private static void Next(Crafting work)
	{
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		if (pending != work)
		{
			return;
		}
		if (requester != work.Requester || requester == null || requester.SessionKey != work.Session)
		{
			Stop(work, null);
			return;
		}
		if (work.Popup == null || !work.Popup.IsOpen() || work.Popup.CraftUi.SelectedData != work.Recipe || work.Popup.GetNumber() != work.Count)
		{
			Stop(work, SprkText.Get("Crafting was cancelled or changed. Any fragments already redeemed remain in your inventory."));
			return;
		}
		int num = System.Math.Max(0, work.Required - work.Account.ItemManager.GetItemCount(work.Fragment.Code));
		if (num == 0)
		{
			pending = null;
			work.OnCraft(work.Recipe.CraftIndex, work.Essence.Index, work.Count);
			work.Popup.Reset();
			work.Popup.Close();
			return;
		}
		List<Tuple<ItemData, int>> list = SprkHeroMaterialTickets.Tickets(work.Fragment.Index);
		if (list.Count == 0)
		{
			Stop(work, SprkText.Get("Not enough fragment tickets remain. Any fragments already redeemed remain in your inventory."));
			return;
		}
		int itemCount = System.Math.Min(num, System.Math.Min(1000, list[0].Item2));
		bool completed = false;
		try
		{
			requester.Request(new NShared.UsePackageSelectItem.Request
			{
				ItemIndex = list[0].Item1.Index,
				ItemCount = itemCount,
				SelectItemIndex = work.Fragment.Index
			}, delegate(NShared.UsePackageSelectItem.Response response)
			{
				if (!completed)
				{
					completed = true;
					if (requester != SingletonUpdater<WebServiceManager>.instance.Requester || requester.SessionKey != work.Session)
					{
						Stop(work, null);
					}
					else if (response == null || response.BaseResult != BaseResultType.Success || response.Result != ResultType.Success)
					{
						Stop(work, SprkText.Get("Ticket redemption was not confirmed. Check your inventory before retrying; any fragments already redeemed remain there."));
					}
					else
					{
						work.Account.ItemManager.Apply(response.ItemResult);
						work.Account.ItemManager.Apply(response.RewardItemResult);
						Next(work);
					}
				}
			});
		}
		catch (Exception)
		{
			Stop(work, SprkText.Get("Ticket redemption could not be confirmed. Check your inventory before retrying."));
		}
	}
}
