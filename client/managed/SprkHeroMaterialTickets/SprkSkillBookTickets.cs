using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NShared.UsePackageSelectItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

public static class SprkSkillBookTickets
{
	public struct Route
	{
		public ItemData Ticket;

		public int Count;

		public int Via;
	}

	private sealed class Entry
	{
		public ItemData Ticket;

		public int Via;
	}

	public sealed class Conversion
	{
		public int Ticket;

		public int Via;

		public int Book;

		public int Count;
	}

	public sealed class Pool
	{
		private readonly Dictionary<int, int> left = new Dictionary<int, int>();

		private readonly Dictionary<int, List<Route>> routes = new Dictionary<int, List<Route>>();

		public readonly List<Conversion> Conversions = new List<Conversion>();

		private List<Route> For(int book)
		{
			if (!routes.TryGetValue(book, out var value))
			{
				value = Routes(book);
				routes[book] = value;
				foreach (Route item in value)
				{
					if (!left.ContainsKey(item.Ticket.Index))
					{
						left[item.Ticket.Index] = item.Count;
					}
				}
			}
			return value;
		}

		public int Available(int book)
		{
			long num = 0L;
			foreach (Route item in For(book))
			{
				num += left[item.Ticket.Index];
			}
			return (int)System.Math.Min(2147483647L, num);
		}

		public int Take(int book, int count)
		{
			int num = 0;
			foreach (Route route in For(book))
			{
				if (num >= count)
				{
					break;
				}
				int num2 = System.Math.Min(left[route.Ticket.Index], count - num);
				if (num2 > 0)
				{
					left[route.Ticket.Index] -= num2;
					num += num2;
					Conversion conversion = Conversions.Find((Conversion c) => c.Ticket == route.Ticket.Index && c.Via == route.Via && c.Book == book);
					if (conversion != null)
					{
						conversion.Count += num2;
						continue;
					}
					Conversions.Add(new Conversion
					{
						Ticket = route.Ticket.Index,
						Via = route.Via,
						Book = book,
						Count = num2
					});
				}
			}
			return num;
		}
	}

	private sealed class LastShown
	{
		public int Value;
	}

	public sealed class UseBook : SelectionItemUsePopup.IListener, SprkHeroMaterialTickets.ITicketPrefill
	{
		private readonly HeroSkillDesc.Listener listener;

		private readonly HeroSkillDesc desc;

		private readonly int slot;

		private readonly int extend;

		public readonly int material;

		public readonly int required;

		private int ticket;

		private int via;

		private string session;

		private bool waiting;

		private int attempt;

		public int Ticket => ticket;

		public int Material => material;

		public int Required => required;

		public int Via => via;

		public UseBook(HeroSkillDesc.Listener listener, HeroSkillDesc desc, int slot, int extend, int material, int required)
		{
			this.listener = listener;
			this.desc = desc;
			this.slot = slot;
			this.extend = extend;
			this.material = material;
			this.required = required;
		}

		public void Show(Route route)
		{
			WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
			AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(material);
			ItemInfo itemInfo = instance.ItemManager.Get(route.Ticket.Index);
			if (requester != null && data != null && itemInfo != null && itemInfo.Count > 0)
			{
				ticket = route.Ticket.Index;
				via = route.Via;
				session = requester.SessionKey;
				ItemInfo itemInfo2 = instance.ItemManager.Get(material);
				SelectionItemUsePopup.Open(new SelectionPopupItemInfo(route.Ticket, itemInfo.Count), new List<SelectionPopupItemInfo>
				{
					new SelectionPopupItemInfo(data, itemInfo2?.Count ?? 0)
				}, this);
			}
		}

		public void OnClickOk(SelectionItemUsePopup popup, int usingItemIndex, int selecteditemIndex, int itemCount)
		{
			if (waiting || ticket == 0 || usingItemIndex != ticket || selecteditemIndex != material || itemCount <= 0 || itemCount > 1000)
			{
				return;
			}
			WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
			AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
			if (requester == null || requester.SessionKey != session)
			{
				popup.Close();
				return;
			}
			ItemInfo itemInfo = instance.ItemManager.Get(ticket);
			if (itemInfo == null || itemInfo.Count < itemCount)
			{
				popup.Close();
				return;
			}
			waiting = true;
			int current = ++attempt;
			UIHelper.EnableButton(popup.ButtonOk, enable: false);
			Step(popup, requester, ticket, (via != 0) ? via : material, itemCount, current);
		}

		private void Step(SelectionItemUsePopup popup, object owner, int use, int select, int count, int current)
		{
			WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
			if (requester == null || requester != owner || requester.SessionKey != session)
			{
				waiting = false;
				return;
			}
			bool completed = false;
			requester.Request(new NShared.UsePackageSelectItem.Request
			{
				ItemIndex = use,
				SelectItemIndex = select,
				ItemCount = count
			}, delegate(NShared.UsePackageSelectItem.Response response)
			{
				if (!completed && current == attempt)
				{
					completed = true;
					WebServiceRequester requester2 = SingletonUpdater<WebServiceManager>.instance.Requester;
					if (requester2 != owner || requester2.SessionKey != session)
					{
						waiting = false;
					}
					else if (response == null)
					{
						Finish(popup, keepOpen: false);
					}
					else if (response.BaseResult != BaseResultType.Success || response.Result != ResultType.Success)
					{
						Finish(popup, use == ticket);
						MessagePopup.OpenError(response.Result);
					}
					else
					{
						AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
						instance.ItemManager.Apply(response.ItemResult);
						instance.ItemManager.Apply(response.RewardItemResult);
						if (select != material)
						{
							Step(popup, owner, select, material, count, current);
						}
						else
						{
							Finish(popup, keepOpen: false);
							Continue();
						}
					}
				}
			});
		}

		private void Continue()
		{
			if (!(desc == null) && desc.HeroInfo != null)
			{
				desc.Refresh();
				Helper.SkillBookInfo skillBookInfo = Helper.GetSkillBookInfo(desc.HeroInfo, slot);
				if (skillBookInfo.BookItemData != null && skillBookInfo.BookItemData.Index == material && skillBookInfo.CurItemCount >= required)
				{
					listener.ExtendSkill(slot, extend, isEnoughItem: true);
				}
			}
		}

		private void Finish(SelectionItemUsePopup popup, bool keepOpen)
		{
			waiting = false;
			if (!(popup == null))
			{
				UIHelper.EnableButton(popup.ButtonOk, enable: true);
				if (!keepOpen)
				{
					popup.Close();
				}
			}
		}
	}

	private static object indexSource;

	private static Dictionary<int, List<Entry>> index;

	private static readonly ConditionalWeakTable<UpgradeBookItemIconComponent, LastShown> Shown = new ConditionalWeakTable<UpgradeBookItemIconComponent, LastShown>();

	private static bool Selector(ItemData item, out SelectItemData row)
	{
		row = null;
		if (item == null || item.Type != ItemType.PackageSelectItem)
		{
			return false;
		}
		row = GameRoot.Instance.SelectItemDataContainer.GetData(item.Code);
		if (row != null && row.SelectType != SelectItemPopupType.RuneSelectItem)
		{
			return row.ItemIndices != null;
		}
		return false;
	}

	private static Dictionary<int, List<Entry>> Index()
	{
		ItemDataContainer itemDataContainer = GameRoot.Instance.ItemDataContainer;
		if (index != null && indexSource == itemDataContainer)
		{
			return index;
		}
		Dictionary<int, List<Entry>> dictionary = new Dictionary<int, List<Entry>>();
		Dictionary<int, List<Entry>> dictionary2 = new Dictionary<int, List<Entry>>();
		List<KeyValuePair<ItemData, SelectItemData>> list = new List<KeyValuePair<ItemData, SelectItemData>>();
		foreach (ItemData allDatum in itemDataContainer.GetAllData())
		{
			if (Selector(allDatum, out var row))
			{
				list.Add(new KeyValuePair<ItemData, SelectItemData>(allDatum, row));
			}
		}
		foreach (KeyValuePair<ItemData, SelectItemData> item in list)
		{
			HashSet<int> hashSet = new HashSet<int>();
			int[] itemIndices = item.Value.ItemIndices;
			foreach (int num in itemIndices)
			{
				if (hashSet.Add(num))
				{
					if (!dictionary.TryGetValue(num, out var value))
					{
						value = (dictionary[num] = new List<Entry>());
					}
					value.Add(new Entry
					{
						Ticket = item.Key
					});
				}
			}
		}
		foreach (KeyValuePair<ItemData, SelectItemData> item2 in list)
		{
			HashSet<int> hashSet2 = new HashSet<int>();
			int[] itemIndices = item2.Value.ItemIndices;
			foreach (int via in itemIndices)
			{
				if (!Selector(itemDataContainer.GetData(via), out var row2))
				{
					continue;
				}
				int[] itemIndices2 = row2.ItemIndices;
				foreach (int num2 in itemIndices2)
				{
					if (!SprkHeroMaterialTickets.Matches(num2, item2.Value.ItemIndices) && hashSet2.Add(num2))
					{
						if (!dictionary2.TryGetValue(num2, out var value2))
						{
							value2 = (dictionary2[num2] = new List<Entry>());
						}
						value2.Add(new Entry
						{
							Ticket = item2.Key,
							Via = via
						});
					}
				}
			}
		}
		foreach (KeyValuePair<int, List<Entry>> item3 in dictionary2)
		{
			if (!dictionary.TryGetValue(item3.Key, out var value3))
			{
				value3 = (dictionary[item3.Key] = new List<Entry>());
			}
			value3.AddRange(item3.Value);
		}
		index = dictionary;
		indexSource = itemDataContainer;
		return index;
	}

	public static List<Route> Routes(int book)
	{
		List<Route> list = new List<Route>();
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (book <= 0 || instance == null || !Index().TryGetValue(book, out var value))
		{
			return list;
		}
		foreach (Entry item in value)
		{
			ItemInfo itemInfo = instance.ItemManager.Get(item.Ticket.Index);
			if (itemInfo == null || itemInfo.Count <= 0 || itemInfo.Locked != 0)
			{
				continue;
			}
			if (item.Via != 0)
			{
				ItemInfo itemInfo2 = instance.ItemManager.Get(item.Via);
				if (itemInfo2 != null && itemInfo2.Locked != 0)
				{
					continue;
				}
			}
			list.Add(new Route
			{
				Ticket = item.Ticket,
				Count = itemInfo.Count,
				Via = item.Via
			});
		}
		return list;
	}

	public static int Usable(Helper.SkillBookInfo book)
	{
		if (book.BookItemData == null)
		{
			return book.CurItemCount;
		}
		return WithTickets(book.BookItemData, book.CurItemCount);
	}

	private static int Owned(int item)
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		return ((instance == null) ? null : instance.ItemManager.Get(item))?.Count ?? 0;
	}

	private static int WithTickets(ItemData book, int count)
	{
		if (book == null || count < 0)
		{
			return count;
		}
		return (int)System.Math.Min(2147483647L, (long)count + (long)new Pool().Available(book.Index));
	}

	public static int Display(UpgradeBookItemIconComponent counter, int count)
	{
		if (counter == null)
		{
			return count;
		}
		int num = WithTickets(counter.ItemData, count);
		Shown.GetOrCreateValue(counter).Value = num;
		return num;
	}

	public static void RefreshCounters(IEnumerable<UpgradeBookItemIconComponent> counters)
	{
		if (counters == null)
		{
			return;
		}
		foreach (UpgradeBookItemIconComponent counter in counters)
		{
			if (!(counter == null) && counter.ItemData != null)
			{
				int num = Owned(counter.ItemData.Index);
				if (!Shown.TryGetValue(counter, out var value) || value.Value != WithTickets(counter.ItemData, num))
				{
					counter.SetItemCount(num);
				}
			}
		}
	}

	public static void Redeem(List<Conversion> plan, Action<bool> done)
	{
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		if (requester == null)
		{
			done(obj: false);
			return;
		}
		List<int[]> list = new List<int[]>();
		foreach (Conversion item in plan)
		{
			for (int num = item.Count; num > 0; num -= 1000)
			{
				int num2 = System.Math.Min(1000, num);
				if (item.Via != 0)
				{
					list.Add(new int[3] { item.Ticket, item.Via, num2 });
					list.Add(new int[3] { item.Via, item.Book, num2 });
				}
				else
				{
					list.Add(new int[3] { item.Ticket, item.Book, num2 });
				}
			}
		}
		RedeemStep(list, 0, requester, requester.SessionKey, done);
	}

	private static void RedeemStep(List<int[]> steps, int at, object owner, string session, Action<bool> done)
	{
		if (at >= steps.Count)
		{
			done(obj: true);
			return;
		}
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		if (requester == null || requester != owner || requester.SessionKey != session)
		{
			done(obj: false);
			return;
		}
		int[] array = steps[at];
		if (Owned(array[0]) < array[2])
		{
			done(obj: false);
			return;
		}
		bool completed = false;
		requester.Request(new NShared.UsePackageSelectItem.Request
		{
			ItemIndex = array[0],
			SelectItemIndex = array[1],
			ItemCount = array[2]
		}, delegate(NShared.UsePackageSelectItem.Response response)
		{
			if (!completed)
			{
				completed = true;
				WebServiceRequester requester2 = SingletonUpdater<WebServiceManager>.instance.Requester;
				if (requester2 != owner || requester2.SessionKey != session || response == null)
				{
					done(obj: false);
				}
				else if (response.BaseResult != BaseResultType.Success || response.Result != ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
					done(obj: false);
				}
				else
				{
					AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
					instance.ItemManager.Apply(response.ItemResult);
					instance.ItemManager.Apply(response.RewardItemResult);
					RedeemStep(steps, at + 1, owner, session, done);
				}
			}
		});
	}

	public static void Extend(HeroSkillDesc.Listener listener, int slot, int extend, bool enough, HeroSkillDesc desc)
	{
		if (listener == null)
		{
			return;
		}
		if (desc == null || desc.HeroInfo == null || desc.ExtendSkills == null || extend < 1 || extend > desc.ExtendSkills.Length)
		{
			listener.ExtendSkill(slot, extend, enough);
			return;
		}
		Helper.SkillBookInfo skillBookInfo = Helper.GetSkillBookInfo(desc.HeroInfo, slot);
		int extendItemCount = desc.ExtendSkills[extend - 1].ExtendInfo.ExtendItemCount;
		if (skillBookInfo.BookItemData == null || extendItemCount <= 0)
		{
			listener.ExtendSkill(slot, extend, isEnoughItem: false);
			return;
		}
		if (skillBookInfo.CurItemCount >= extendItemCount)
		{
			listener.ExtendSkill(slot, extend, isEnoughItem: true);
			return;
		}
		List<Route> list = Routes(skillBookInfo.BookItemData.Index);
		if (list.Count == 0)
		{
			listener.ExtendSkill(slot, extend, isEnoughItem: false);
			return;
		}
		UseBook use = new UseBook(listener, desc, slot, extend, skillBookInfo.BookItemData.Index, extendItemCount);
		if (list.Count == 1)
		{
			use.Show(list[0]);
			return;
		}
		List<Tuple<ItemData, int>> list2 = new List<Tuple<ItemData, int>>();
		foreach (Route item in list)
		{
			list2.Add(new Tuple<ItemData, int>(item.Ticket, item.Count));
		}
		SelectItemMessagePopup.Open2(SprkText.Get("Skill Book Tickets"), SprkText.Format("Choose a ticket for {0}.", skillBookInfo.BookItemData.Name), list2, delegate(SelectItemMessagePopup popup, int index, int count)
		{
			foreach (Route item2 in Routes(use.material))
			{
				if (item2.Ticket.Index == index)
				{
					use.Show(item2);
					break;
				}
			}
		});
	}
}
