using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NGame2.NAccount;
using NGame2.NHelper;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NJit;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkShopSearch
{
	public const string SearchKey = "SprkSearch";

	public const string BookmarkKey = "SprkBookmark";

	public const string ParentKey = "Package";

	public const string SearchName = "Search";

	public const string BookmarkName = "Bookmarks";

	private const string StorageKey = "SprkShopBookmarks";

	private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly Dictionary<string, string> queries = new Dictionary<string, string>
	{
		{ "SprkSearch", "" },
		{ "SprkBookmark", "" }
	};

	private static readonly Dictionary<int, string[]> textCache = new Dictionary<int, string[]>();

	private static Dictionary<int, int> pendingRank;

	private static NewPayShopGroupData searchGroup;

	private static NewPayShopGroupData bookmarkGroup;

	private static HashSet<int> bookmarks;

	private static List<int> bookmarkOrder;

	private static long bookmarkAccount = long.MinValue;

	private static LocalAccountString storage;

	public static int LastCount = -1;

	public static string LastCategory = "";

	public static int BookmarkCount
	{
		get
		{
			LoadBookmarks();
			return bookmarks.Count;
		}
	}

	public static bool IsVirtual(string category)
	{
		if (!(category == "SprkSearch"))
		{
			return category == "SprkBookmark";
		}
		return true;
	}

	public static string Query(string category)
	{
		if (category == null || !queries.TryGetValue(category, out var value))
		{
			return "";
		}
		return value;
	}

	public static void SetQuery(string category, string value)
	{
		if (IsVirtual(category))
		{
			queries[category] = value ?? "";
		}
	}

	public static string Fold(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return "";
		}
		value = NGUIText.StripSymbols(value).Normalize(NormalizationForm.FormD);
		System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(value.Length);
		string text = value;
		foreach (char c in text)
		{
			UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
			if (unicodeCategory != UnicodeCategory.NonSpacingMark && unicodeCategory != UnicodeCategory.SpacingCombiningMark && unicodeCategory != UnicodeCategory.EnclosingMark && char.IsLetterOrDigit(c))
			{
				stringBuilder.Append(char.ToLowerInvariant(c));
			}
		}
		return stringBuilder.ToString();
	}

	public static string[] Terms(string query)
	{
		if (string.IsNullOrEmpty(query))
		{
			return new string[0];
		}
		return (from t in query.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(Fold)
			where t.Length > 0
			select t).Distinct().ToArray();
	}

	public static int Tolerance(string term)
	{
		if (term.Length >= 4)
		{
			if (term.Length > 5)
			{
				if (term.Length > 8)
				{
					return 3;
				}
				return 2;
			}
			return 1;
		}
		return 0;
	}

	public static int Distance(string term, string text)
	{
		int length = term.Length;
		int length2 = text.Length;
		if (length == 0)
		{
			return 0;
		}
		int[,] array = new int[length + 1, length2 + 1];
		for (int i = 0; i <= length; i++)
		{
			array[i, 0] = i;
		}
		for (int j = 1; j <= length; j++)
		{
			for (int k = 1; k <= length2; k++)
			{
				int num = ((term[j - 1] != text[k - 1]) ? 1 : 0);
				int num2 = System.Math.Min(System.Math.Min(array[j - 1, k] + 1, array[j, k - 1] + 1), array[j - 1, k - 1] + num);
				if (j > 1 && k > 1 && term[j - 1] == text[k - 2] && term[j - 2] == text[k - 1])
				{
					num2 = System.Math.Min(num2, array[j - 2, k - 2] + 1);
				}
				array[j, k] = num2;
			}
		}
		int num3 = length;
		for (int l = 0; l <= length2; l++)
		{
			num3 = System.Math.Min(num3, array[length, l]);
		}
		return num3;
	}

	public static int Rank(string[] terms, string[] tiers)
	{
		if (terms.Length == 0)
		{
			return 0;
		}
		for (int i = 0; i < tiers.Length; i++)
		{
			bool flag = true;
			foreach (string value in terms)
			{
				bool flag2 = false;
				for (int k = 0; k <= i; k++)
				{
					if (flag2)
					{
						break;
					}
					flag2 = tiers[k].IndexOf(value, StringComparison.Ordinal) >= 0;
				}
				if (!flag2)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool Fuzzy(string[] terms, string[] tiers)
	{
		if (terms.Length == 0)
		{
			return true;
		}
		string[] source = tiers.Take(2).SelectMany((string t) => t.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
		foreach (string term in terms)
		{
			if (!source.Any((string p) => p.IndexOf(term, StringComparison.Ordinal) >= 0))
			{
				int k = Tolerance(term);
				if (k == 0 || !source.Any((string p) => Distance(term, p) <= k))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static string Join(IEnumerable<string> values)
	{
		return "|" + string.Join("|", (from v in values.Select(Fold)
			where v.Length > 0
			select v).Distinct().ToArray()) + "|";
	}

	private static string Loc(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return "";
		}
		try
		{
			return LocalizationManager.GetStringDirect(key) ?? key;
		}
		catch
		{
			return key;
		}
	}

	private static void AddItem(ItemData item, List<string> names, List<string> descs, bool expand)
	{
		if (item == null)
		{
			return;
		}
		try
		{
			names.Add(item.Name);
		}
		catch
		{
		}
		try
		{
			names.Add(ItemNameBuilder.GetItemName(item));
		}
		catch
		{
		}
		try
		{
			descs.Add(Loc(item.Description));
		}
		catch
		{
		}
		try
		{
			descs.Add(Loc(item.Description2));
		}
		catch
		{
		}
		if (!expand)
		{
			return;
		}
		try
		{
			if (!PackageItemHelper.IsPackageItem(item))
			{
				return;
			}
			List<RewardElementData> list = new List<RewardElementData>();
			List<RewardElementData> list2 = new List<RewardElementData>();
			PackageItemHelper.GetRewardElementList(item, list, list2);
			foreach (RewardElementData item2 in list.Concat(list2))
			{
				if (item2 != null)
				{
					CurrencyType currencyType = item2.GetCurrencyType();
					if (currencyType != CurrencyType.None)
					{
						names.Add((currencyType == CurrencyType.Gem) ? "Ruby" : currencyType.ToString());
					}
					else
					{
						AddItem(GameRoot.Instance.ItemDataContainer.GetDataByCode(item2.ItemCode), names, descs, expand: false);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[SprkShopSearch] package contents: " + ex.Message);
		}
	}

	public static string[] Text(NewPayShopProductInfo info)
	{
		if (textCache.TryGetValue(info.Index, out var value))
		{
			return value;
		}
		List<string> values = new List<string>
		{
			Loc(info.CardTitle),
			Loc(info.ProductName),
			info.ProductName
		};
		List<string> items = new List<string>();
		List<string> descs = new List<string>
		{
			Loc(info.ProductDesc),
			Loc(info.ProductDetailDesc)
		};
		ItemDataContainer itemDataContainer = GameRoot.Instance.ItemDataContainer;
		HashSet<int> seen = new HashSet<int>();
		if (info.HeroItemIndex > 0)
		{
			Add(itemDataContainer.GetData(info.HeroItemIndex));
		}
		if (info.UseItemCode != null)
		{
			string[] useItemCode = info.UseItemCode;
			foreach (string text in useItemCode)
			{
				if (!string.IsNullOrEmpty(text))
				{
					Add(itemDataContainer.GetDataByCode(text));
				}
			}
		}
		if (info.ItemInfos != null)
		{
			PayShopProductItemInfo[] itemInfos = info.ItemInfos;
			foreach (PayShopProductItemInfo payShopProductItemInfo in itemInfos)
			{
				if (payShopProductItemInfo != null)
				{
					Add(itemDataContainer.GetData(payShopProductItemInfo.ItemIndex));
				}
			}
		}
		if (info.Gem > 0 || info.FirstGem > 0 || info.BonusGem > 0 || info.DailyGem > 0)
		{
			items.Add("Ruby");
		}
		if (info.Gold > 0)
		{
			items.Add("Gold");
		}
		if (info.Mileage > 0)
		{
			items.Add("Mileage");
		}
		if (info.LuaPoint > 0)
		{
			items.Add("Lua");
		}
		value = new string[3]
		{
			Join(values),
			Join(items),
			Join(descs)
		};
		textCache[info.Index] = value;
		return value;
		void Add(ItemData item)
		{
			if (item != null && seen.Add(item.Index))
			{
				AddItem(item, items, descs, expand: true);
			}
		}
	}

	private static void LoadBookmarks()
	{
		long accountId = LocalAccountDefine.AccountId;
		if (bookmarks != null && bookmarkAccount == accountId)
		{
			return;
		}
		bookmarkAccount = accountId;
		bookmarks = new HashSet<int>();
		bookmarkOrder = new List<int>();
		storage = new LocalAccountString("SprkShopBookmarks");
		string text = "";
		try
		{
			text = storage.Value ?? "";
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (int.TryParse(array[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) && result != 0 && bookmarks.Add(result))
			{
				bookmarkOrder.Add(result);
			}
		}
	}

	public static string Serialize(IEnumerable<int> order)
	{
		return string.Join(",", order.Select((int i) => i.ToString(CultureInfo.InvariantCulture)).ToArray());
	}

	public static bool IsBookmarked(int index)
	{
		LoadBookmarks();
		return bookmarks.Contains(index);
	}

	public static bool Toggle(int index)
	{
		LoadBookmarks();
		bool flag = !bookmarks.Contains(index);
		if (flag)
		{
			bookmarks.Add(index);
			bookmarkOrder.Add(index);
		}
		else
		{
			bookmarks.Remove(index);
			bookmarkOrder.Remove(index);
		}
		try
		{
			storage.Value = Serialize(bookmarkOrder);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return flag;
	}

	private static NewPayShopGroupData Group(string key, string name, string parent, int sort)
	{
		return new NewPayShopGroupData
		{
			PayShopCategoryType = key,
			CategoryName = name,
			ParentCategory = parent,
			IsCustomize = false,
			Sort = sort,
			IsEnable = true,
			LinkCurrency = CurrencyType.None,
			EnableCondition = EGroupEnableConditionType.None,
			EPayShopCategoryType = NewPayShopCategoryType.None,
			EParentCategory = ((parent != null) ? NewPayShopGroupDataContainer.StringToCategoryType(parent) : NewPayShopCategoryType.None)
		};
	}

	public static void Inject(NewPayShopGroupDataContainer container)
	{
		IList<NewPayShopGroupData> allData = container.GetAllData();
		if (allData == null)
		{
			return;
		}
		List<NewPayShopGroupData> list = allData.Where((NewPayShopGroupData g) => g != null && !IsVirtual(g.PayShopCategoryType)).ToList();
		string hotKey = NewPayShopCategoryType.HotNew.ToString();
		foreach (NewPayShopGroupData item in list.Where((NewPayShopGroupData g) => g.PayShopCategoryType == hotKey))
		{
			item.IsEnable = false;
		}
		NewPayShopGroupData data = container.GetData(hotKey);
		if (data != null)
		{
			data.IsEnable = false;
		}
		bool num = list.Any((NewPayShopGroupData g) => g.PayShopCategoryType == "Package" && string.IsNullOrEmpty(g.ParentCategory));
		List<NewPayShopGroupData> list2 = (num ? list.Where((NewPayShopGroupData g) => g.ParentCategory == "Package").ToList() : list.Where((NewPayShopGroupData g) => string.IsNullOrEmpty(g.ParentCategory) && g.PayShopCategoryType != hotKey).ToList());
		int num2 = ((list2.Count != 0) ? list2.Min((NewPayShopGroupData g) => g.Sort) : 0);
		string text = (num ? "Package" : null);
		bookmarkGroup = bookmarkGroup ?? Group("SprkBookmark", "Bookmarks", text, num2 - 2);
		searchGroup = searchGroup ?? Group("SprkSearch", "Search", text, num2 - 1);
		bookmarkGroup.ParentCategory = text;
		bookmarkGroup.Sort = num2 - 2;
		bookmarkGroup.IsEnable = true;
		searchGroup.ParentCategory = text;
		searchGroup.Sort = num2 - 1;
		searchGroup.IsEnable = true;
		NewPayShopGroupData[] array = new NewPayShopGroupData[2] { bookmarkGroup, searchGroup };
		for (int num3 = 0; num3 < array.Length; num3++)
		{
			array[num3].EParentCategory = ((text != null) ? NewPayShopGroupDataContainer.StringToCategoryType(text) : NewPayShopCategoryType.None);
		}
		List<NewPayShopGroupData> value = new List<NewPayShopGroupData>(list) { searchGroup, bookmarkGroup };
		((JitRef<List<NewPayShopGroupData>>)typeof(JitDataContainer<NewPayShopGroupData>).GetProperty("BaseList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(container, null)).JsonLoad_Set(value);
		Dictionary<string, NewPayShopGroupData> dictionary = ((JitDictionaryWithKey<string, NewPayShopGroupData>)typeof(JitDataContainerDefault<string, NewPayShopGroupData>).GetProperty("BaseDic", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(container, null)).JsonLoad_Get();
		lock (dictionary)
		{
			dictionary["SprkSearch"] = searchGroup;
			dictionary["SprkBookmark"] = bookmarkGroup;
		}
	}

	public static void BeforeOpen(PayShop shop)
	{
		try
		{
			textCache.Clear();
			Inject(GameRoot.Instance.NewPayShopGroupDataContainer);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void AfterOpen(PayShop shop)
	{
		try
		{
			SprkShopSearchBar sprkShopSearchBar = shop.GetComponent<SprkShopSearchBar>();
			if (sprkShopSearchBar == null)
			{
				sprkShopSearchBar = shop.gameObject.AddComponent<SprkShopSearchBar>();
			}
			sprkShopSearchBar.Bind(shop);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void DecorateCategory(PayShopCategoryComponent component, NewPayShopGroupData data)
	{
		try
		{
			SprkShopIcons.ApplyCategoryIcon(component, data != null && data.PayShopCategoryType == "SprkSearch" && string.IsNullOrEmpty(data.ParentCategory));
			if (data != null && IsVirtual(data.PayShopCategoryType))
			{
				component.SetName((data.PayShopCategoryType == "SprkSearch") ? "Search" : "Bookmarks");
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static List<NewPayShopProductInfo> Products(List<NewPayShopProductInfo> native, PayShopProductList list, string category)
	{
		LastCategory = category ?? "";
		pendingRank = null;
		LastCount = -1;
		if (!IsVirtual(category))
		{
			return native;
		}
		try
		{
			NGame2.NAccount.NewUserPayShopProductManager manager = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager;
			IEnumerable<NewPayShopProductInfo> source;
			if (category == "SprkBookmark")
			{
				LoadBookmarks();
				source = from i in bookmarkOrder
					select manager.Get(i) into p
					where p != null
					select p;
			}
			else
			{
				NewPayShopGroupDataContainer groups = GameRoot.Instance.NewPayShopGroupDataContainer;
				source = from p in manager.GetValues()
					where p != null && Visible(groups, p.Category)
					select p;
			}
			string[] array = Terms(Query(category));
			if (category == "SprkSearch" && array.Length == 0)
			{
				LastCount = 0;
				return new List<NewPayShopProductInfo>();
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			List<NewPayShopProductInfo> list2 = new List<NewPayShopProductInfo>();
			List<NewPayShopProductInfo> list3 = source.ToList();
			foreach (NewPayShopProductInfo item in list3)
			{
				int num = Rank(array, Text(item));
				if (num >= 0)
				{
					dictionary[item.Index] = num;
					list2.Add(item);
				}
			}
			if (list2.Count == 0 && array.Length != 0)
			{
				foreach (NewPayShopProductInfo item2 in list3)
				{
					if (Fuzzy(array, Text(item2)))
					{
						dictionary[item2.Index] = 3;
						list2.Add(item2);
					}
				}
			}
			if (category == "SprkSearch" && array.Length != 0)
			{
				pendingRank = dictionary;
			}
			LastCount = 0;
			return list2;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return native;
		}
	}

	private static bool Visible(NewPayShopGroupDataContainer groups, string category)
	{
		if (string.IsNullOrEmpty(category) || IsVirtual(category))
		{
			return false;
		}
		NewPayShopGroupData data = groups.GetData(category);
		if (data == null)
		{
			return false;
		}
		if (!data.IsEnable && category != NewPayShopCategoryType.HotNew.ToString())
		{
			return false;
		}
		if (!string.IsNullOrEmpty(data.ParentCategory))
		{
			NewPayShopGroupData data2 = groups.GetData(data.ParentCategory);
			if (data2 == null || !data2.IsEnable)
			{
				return false;
			}
		}
		return true;
	}

	public static void AfterSort(PayShopProductList list, List<NewPayShopProductInfo> data)
	{
		Dictionary<int, int> ranks = pendingRank;
		pendingRank = null;
		if (data == null)
		{
			return;
		}
		if (IsVirtual(LastCategory))
		{
			LastCount = data.Count;
		}
		if (ranks != null && !data.Any((NewPayShopProductInfo p) => !ranks.ContainsKey(p.Index)))
		{
			List<NewPayShopProductInfo> collection = data.OrderBy((NewPayShopProductInfo p) => ranks[p.Index]).ToList();
			data.Clear();
			data.AddRange(collection);
		}
	}

	public static void Decorate(PayShopProductComponent card, NewPayShopProductInfo info)
	{
		try
		{
			SprkShopStar sprkShopStar = card.GetComponent<SprkShopStar>();
			if (sprkShopStar == null)
			{
				sprkShopStar = card.gameObject.AddComponent<SprkShopStar>();
			}
			sprkShopStar.Bind(card, card.RootWidget, info, 42, 34f);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void DecoratePopup(PayShopProductInfoPopup popup, NewPayShopProductInfo info)
	{
		try
		{
			SprkShopStar sprkShopStar = popup.GetComponent<SprkShopStar>();
			if (sprkShopStar == null)
			{
				sprkShopStar = popup.gameObject.AddComponent<SprkShopStar>();
			}
			UIWidget uIWidget = ((popup.ProductTexture != null) ? popup.ProductTexture.GetComponent<UIWidget>() : null);
			if (uIWidget == null && popup.ProductTexture != null)
			{
				uIWidget = popup.ProductTexture.GetComponentInChildren<UIWidget>(includeInactive: true);
			}
			sprkShopStar.Bind(popup, uIWidget, info, 56, 30f);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static string DefaultCategory(string native)
	{
		return "SprkBookmark";
	}
}
