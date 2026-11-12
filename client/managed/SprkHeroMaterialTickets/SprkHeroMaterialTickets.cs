using System;
using System.Collections.Generic;
using System.Globalization;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.UsePackageSelectItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

public static class SprkHeroMaterialTickets
{
	public interface ITicketPrefill
	{
		int Ticket { get; }

		int Material { get; }

		int Required { get; }
	}

	private sealed class UseTicket : SelectionItemUsePopup.IListener, ITicketPrefill
	{
		public readonly int ticket;

		public readonly int material;

		public readonly int required;

		private readonly string session;

		private bool waiting;

		public int Ticket => ticket;

		public int Material => material;

		public int Required => required;

		public UseTicket(int ticket, int material, int required, string session)
		{
			this.ticket = ticket;
			this.material = material;
			this.required = required;
			this.session = session;
		}

		public void OnClickOk(SelectionItemUsePopup popup, int usingItemIndex, int selecteditemIndex, int itemCount)
		{
			if (waiting || usingItemIndex != ticket || selecteditemIndex != material || itemCount <= 0 || itemCount > 1000)
			{
				return;
			}
			WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
			AccountManager account = GlobalAutoBehaviour<AccountManager>.instance;
			if (requester == null || requester.SessionKey != session)
			{
				popup.Close();
				return;
			}
			ItemInfo itemInfo = account.ItemManager.Get(ticket);
			if (itemInfo == null || itemInfo.Count < itemCount)
			{
				popup.Close();
				return;
			}
			waiting = true;
			bool completed = false;
			UIHelper.EnableButton(popup.ButtonOk, enable: false);
			requester.Request(new NShared.UsePackageSelectItem.Request
			{
				ItemIndex = ticket,
				SelectItemIndex = material,
				ItemCount = itemCount
			}, delegate(NShared.UsePackageSelectItem.Response response)
			{
				if (!completed)
				{
					completed = true;
					waiting = false;
					if (SingletonUpdater<WebServiceManager>.instance.Requester == requester && !(requester.SessionKey != session))
					{
						if (popup != null)
						{
							UIHelper.EnableButton(popup.ButtonOk, enable: true);
						}
						if (response == null)
						{
							if (popup != null)
							{
								popup.Close();
							}
						}
						else if (response.BaseResult != BaseResultType.Success || response.Result != ResultType.Success)
						{
							MessagePopup.OpenError(response.Result);
						}
						else
						{
							account.ItemManager.Apply(response.ItemResult);
							account.ItemManager.Apply(response.RewardItemResult);
							if (popup != null)
							{
								popup.Close();
							}
							BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
							popupOpenData.SetItemResultInfos(new ItemResultInfo[1] { response.RewardItemResult });
							BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
						}
					}
				}
			});
		}
	}

	public static int InitialCount(int required, int owned, int tickets)
	{
		return (int)System.Math.Min(System.Math.Max(0L, (long)required - (long)owned), System.Math.Min(1000L, System.Math.Max(0, tickets)));
	}

	public static void Prefill(SelectionItemUsePopup popup, SelectionItemUsePopup.IListener listener, List<ItemIconComponent> choices)
	{
		if (listener is ITicketPrefill ticketPrefill && !(popup.InputSelectedCount == null))
		{
			AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
			ItemInfo itemInfo = instance.ItemManager.Get(ticketPrefill.Material);
			ItemInfo itemInfo2 = instance.ItemManager.Get(ticketPrefill.Ticket);
			popup.InputSelectedCount.value = InitialCount(ticketPrefill.Required, itemInfo?.Count ?? 0, itemInfo2?.Count ?? 0).ToString(CultureInfo.InvariantCulture);
			popup.OnChangeInputSelectCount();
			if (choices != null && choices.Count == 1 && choices[0] != null && choices[0].ItemIndex == ticketPrefill.Material)
			{
				((ItemIconComponent.Listener)popup).OnClick(choices[0]);
			}
		}
	}

	public static bool Matches(int target, int[] rewards)
	{
		if (target <= 0 || rewards == null)
		{
			return false;
		}
		return Array.IndexOf(rewards, target) >= 0;
	}

	public static bool IsMaterial(ItemData item, HeroAwakeType kind)
	{
		if (item == null || (kind != HeroAwakeType.Awaken && kind != HeroAwakeType.Transcend))
		{
			return false;
		}
		foreach (CreatureAwakeChallengeItemData allDatum in GameRoot.Instance.CreatureAwakeChallengeItemDataContainer.GetAllData())
		{
			for (int i = 1; i <= 3; i++)
			{
				if (item.Code == ((kind == HeroAwakeType.Awaken) ? allDatum.GetItemCode(i) : allDatum.GetTranscendItemCode(i)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static List<Tuple<ItemData, int>> Tickets(int target)
	{
		List<Tuple<ItemData, int>> list = new List<Tuple<ItemData, int>>();
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (instance == null)
		{
			return list;
		}
		foreach (ItemData allDatum in GameRoot.Instance.ItemDataContainer.GetAllData())
		{
			if (allDatum == null || allDatum.Type != ItemType.PackageSelectItem)
			{
				continue;
			}
			ItemInfo itemInfo = instance.ItemManager.Get(allDatum.Index);
			if (itemInfo != null && itemInfo.Count > 0 && itemInfo.Locked == 0)
			{
				SelectItemData data = GameRoot.Instance.SelectItemDataContainer.GetData(allDatum.Code);
				if (data != null && data.SelectType != SelectItemPopupType.RuneSelectItem && Matches(target, data.ItemIndices))
				{
					list.Add(new Tuple<ItemData, int>(allDatum, itemInfo.Count));
				}
			}
		}
		return list;
	}

	public static void Open(HeroAwakeItemComponent component, ItemData material, int required, int owned, bool canStart, HeroAwakeType kind, Action<ItemIconComponent> fallback)
	{
		component.Open(material, required, owned, canStart, kind, fallback);
		if (IsMaterial(material, kind))
		{
			component.ShowCountLabel(canStart);
			int num = SprkEssenceCraftTickets.Indicator(material.Code, required, owned);
			if (num >= 0)
			{
				UIHelper.EnableGameObject(component.Sprite_New, num == 1);
			}
			component.SetListener(TicketClick(material, required, fallback));
		}
	}

	public static void OpenHall(AwakeChallengeItemComponent component, ItemData material, long required, int owned, Action<ItemIconComponent> fallback)
	{
		component.Open(material, required, owned, fallback);
		if (IsMaterial(material, HeroAwakeType.Awaken) || IsMaterial(material, HeroAwakeType.Transcend))
		{
			component.SetListener(TicketClick(material, (int)System.Math.Min(2147483647L, System.Math.Max(0L, required)), fallback));
		}
	}

	private static Action<ItemIconComponent> TicketClick(ItemData material, int required, Action<ItemIconComponent> fallback)
	{
		return delegate(ItemIconComponent icon)
		{
			List<Tuple<ItemData, int>> list = Tickets(material.Index);
			if (list.Count == 0)
			{
				if (IsMaterial(material, HeroAwakeType.Awaken))
				{
					MessagePopup.Open("Fragment Tickets", "You have no matching fragment tickets. Fragments cannot be crafted.");
				}
				else if (fallback != null)
				{
					fallback(icon);
				}
			}
			else if (list.Count == 1)
			{
				Show(list[0].Item1, material, required);
			}
			else
			{
				SelectItemMessagePopup.Open2("Material Tickets", "Choose a ticket for " + material.Name + ".", list, delegate(SelectItemMessagePopup popup, int index, int count)
				{
					ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
					if (data != null)
					{
						Show(data, material, required);
					}
				});
			}
		};
	}

	private static void Show(ItemData ticket, ItemData material, int required)
	{
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		ItemInfo itemInfo = instance.ItemManager.Get(ticket.Index);
		SelectItemData data = GameRoot.Instance.SelectItemDataContainer.GetData(ticket.Code);
		if (requester != null && itemInfo != null && itemInfo.Count > 0 && data != null && Matches(material.Index, data.ItemIndices))
		{
			ItemInfo itemInfo2 = instance.ItemManager.Get(material.Index);
			SelectionItemUsePopup.Open(new SelectionPopupItemInfo(ticket, itemInfo.Count), new List<SelectionPopupItemInfo>
			{
				new SelectionPopupItemInfo(material, itemInfo2?.Count ?? 0)
			}, new UseTicket(ticket.Index, material.Index, required, requester.SessionKey));
		}
	}
}
