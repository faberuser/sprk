using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NGame2;
using NGame2.NUI;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using UnityEngine;

public static class SprkTicketSearch
{
	public struct Geometry
	{
		public Vector3 position;

		public int width;

		public Vector3 collider;

		public Dictionary<Transform, Vector3> childPositions;

		public Dictionary<UIWidget, int> childWidths;
	}

	private enum Kind
	{
		None,
		Equipment,
		SoulWeapon,
		Hero,
		Costume
	}

	private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const string BoxSprite = "Common_Box_2";

	private const string ClearSprite = "Common_Close";

	private const int RowHeight = 62;

	private static readonly Color TextColor = new Color(1f, 1f, 1f, 1f);

	private static readonly Color PlaceholderColor = new Color(0.953f, 0.925f, 0.855f, 0.55f);

	private static readonly Dictionary<string, string> equipText = new Dictionary<string, string>(StringComparer.Ordinal);

	private static readonly Dictionary<int, string> heroText = new Dictionary<int, string>();

	private static readonly Dictionary<int, string> itemText = new Dictionary<int, string>();

	private static readonly Dictionary<int, string> costumeText = new Dictionary<int, string>();

	private const float HeroRowScale = 0.82f;

	private const float HeroRowY = 397f;

	private const float HeroClassBack = 340f;

	private const float HeroClassX = -170f;

	private const float HeroSearchWidth = 340f;

	private const float HeroSearchX = 170f;

	public const string HeroRowName = "SprkHeroFilterRow";

	private static T Field<T>(object owner, string name) where T : class
	{
		Type type = owner.GetType();
		while (type != null)
		{
			FieldInfo field = type.GetField(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return field.GetValue(owner) as T;
			}
			type = type.BaseType;
		}
		return null;
	}

	private static object Call(object owner, string name, params object[] args)
	{
		Type type = owner.GetType();
		while (type != null)
		{
			MethodInfo methodInfo = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((MethodInfo m) => m.Name == name && m.GetParameters().Length == args.Length);
			if (methodInfo != null)
			{
				return methodInfo.Invoke(owner, args);
			}
			type = type.BaseType;
		}
		throw new MissingMethodException(owner.GetType().Name, name);
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

	private static string Join(IEnumerable<string> names)
	{
		List<string> source = names.Where((string n) => !string.IsNullOrEmpty(n)).ToList();
		string[] value = (from n in source.Select(Fold)
			where n.Length > 0
			select n).Distinct().ToArray();
		string[] value2 = (from n in source.Select(Initials)
			where n.Length >= 2
			select n).Distinct().ToArray();
		return "|" + string.Join("|", value) + "|#" + string.Join("#", value2) + "#";
	}

	private static string Initials(string name)
	{
		System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
		string[] array = NGUIText.StripSymbols(name).Split(new char[8] { ' ', '-', ',', '.', '\'', '’', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string text = Fold(array[i]);
			if (text.Length > 0)
			{
				stringBuilder.Append(text[0]);
			}
		}
		return stringBuilder.ToString();
	}

	private static string NamesPart(string haystack)
	{
		int num = haystack.IndexOf('#');
		if (num >= 0)
		{
			return haystack.Substring(0, num);
		}
		return haystack;
	}

	public static bool Matches(string[] terms, string haystack)
	{
		if (terms == null || terms.Length == 0)
		{
			return true;
		}
		if (string.IsNullOrEmpty(haystack))
		{
			return false;
		}
		string text = NamesPart(haystack);
		foreach (string value in terms)
		{
			if (text.IndexOf(value, StringComparison.Ordinal) < 0)
			{
				return false;
			}
		}
		return true;
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

	public static bool FuzzyMatches(string[] terms, string haystack)
	{
		if (terms == null || terms.Length == 0)
		{
			return true;
		}
		if (string.IsNullOrEmpty(haystack))
		{
			return false;
		}
		string text = NamesPart(haystack);
		string[] source = text.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
		string[] source2 = haystack.Substring(text.Length).Split(new char[1] { '#' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string term in terms)
		{
			if (text.IndexOf(term, StringComparison.Ordinal) < 0 && (term.Length < 2 || !source2.Any((string text2) => text2.StartsWith(term, StringComparison.Ordinal))))
			{
				int k = Tolerance(term);
				if (k == 0 || !source.Any((string part) => Distance(term, part, k) <= k))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static int Distance(string term, string text, int limit = int.MaxValue)
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
		for (int j = 0; j <= length2; j++)
		{
			array[0, j] = 0;
		}
		for (int k = 1; k <= length; k++)
		{
			for (int l = 1; l <= length2; l++)
			{
				int num = ((term[k - 1] != text[l - 1]) ? 1 : 0);
				int num2 = Math.Min(Math.Min(array[k - 1, l] + 1, array[k, l - 1] + 1), array[k - 1, l - 1] + num);
				if (k > 1 && l > 1 && term[k - 1] == text[l - 2] && term[k - 2] == text[l - 1])
				{
					num2 = Math.Min(num2, array[k - 2, l - 2] + 1);
				}
				array[k, l] = num2;
			}
		}
		int num3 = length;
		for (int m = 0; m <= length2; m++)
		{
			num3 = Math.Min(num3, array[length, m]);
		}
		return num3;
	}

	private static IEnumerable<string> HeroNames(int index)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(index);
		if (data != null)
		{
			string text = null;
			string title = null;
			string code = null;
			try
			{
				text = data.Name;
			}
			catch
			{
			}
			try
			{
				title = data.Title;
			}
			catch
			{
			}
			try
			{
				code = data.CodeName;
			}
			catch
			{
			}
			if (!string.IsNullOrEmpty(text))
			{
				yield return text;
			}
			if (!string.IsNullOrEmpty(title))
			{
				yield return title;
			}
			if (!string.IsNullOrEmpty(code))
			{
				yield return code;
			}
		}
	}

	public static string HeroText(int index)
	{
		if (!heroText.TryGetValue(index, out var value))
		{
			value = (heroText[index] = Join(HeroNames(index)));
		}
		return value;
	}

	private static IEnumerable<string> ItemNames(ItemData item)
	{
		if (item != null)
		{
			string text = null;
			string built = null;
			try
			{
				text = item.Name;
			}
			catch
			{
			}
			try
			{
				built = ItemNameBuilder.GetItemName(item);
			}
			catch
			{
			}
			if (!string.IsNullOrEmpty(text))
			{
				yield return text;
			}
			if (!string.IsNullOrEmpty(built))
			{
				yield return built;
			}
		}
	}

	public static string EquipText(EquipItemData equip)
	{
		if (equip == null)
		{
			return "";
		}
		string text = equip.EquipCode ?? ((HashString)"");
		if (equipText.TryGetValue(text, out var value))
		{
			return value;
		}
		List<string> list = new List<string>(ItemNames(GameRoot.Instance.ItemDataContainer.GetDataByCode(text)));
		if (equip.CreatureIndex != null)
		{
			int[] creatureIndex = equip.CreatureIndex;
			foreach (int index in creatureIndex)
			{
				list.AddRange(HeroNames(index));
			}
		}
		return equipText[text] = Join(list);
	}

	public static string SoulStoneText(ItemData item)
	{
		if (item == null)
		{
			return "";
		}
		if (itemText.TryGetValue(item.Index, out var value))
		{
			return value;
		}
		List<string> list = new List<string>(ItemNames(item));
		int num = 0;
		try
		{
			num = item.GetValueInt(0);
		}
		catch
		{
		}
		if (num > 0)
		{
			list.AddRange(HeroNames(num));
		}
		return itemText[item.Index] = Join(list);
	}

	public static string CostumeText(int index)
	{
		if (costumeText.TryGetValue(index, out var value))
		{
			return value;
		}
		List<string> list = new List<string>();
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(index);
		if (data != null)
		{
			try
			{
				if (!string.IsNullOrEmpty(data.CostumeName))
				{
					list.Add(data.CostumeName);
				}
			}
			catch
			{
			}
			list.AddRange(HeroNames(data.HeroIndex));
		}
		return costumeText[index] = Join(list);
	}

	private static void ClearCaches()
	{
		equipText.Clear();
		heroText.Clear();
		itemText.Clear();
		costumeText.Clear();
	}

	private static SprkTicketSearchBox Box(Component window)
	{
		if (!(window == null))
		{
			return window.GetComponent<SprkTicketSearchBox>();
		}
		return null;
	}

	public static bool MatchEquip(WeaponUniqueSelect window, EquipItemData equip)
	{
		SprkTicketSearchBox sprkTicketSearchBox = Box(window);
		if (!(sprkTicketSearchBox == null) && sprkTicketSearchBox.Active)
		{
			return sprkTicketSearchBox.Accepts(EquipText(equip));
		}
		return true;
	}

	public static bool MatchSoulStone(SoulStoneSelect window, ItemData item)
	{
		SprkTicketSearchBox sprkTicketSearchBox = Box(window);
		if (!(sprkTicketSearchBox == null) && sprkTicketSearchBox.Active)
		{
			return sprkTicketSearchBox.Accepts(SoulStoneText(item));
		}
		return true;
	}

	public static bool MatchHero(Component window, int heroIndex)
	{
		SprkTicketSearchBox sprkTicketSearchBox = Box(window);
		if (!(sprkTicketSearchBox == null) && sprkTicketSearchBox.Active)
		{
			return sprkTicketSearchBox.Accepts(HeroText(heroIndex));
		}
		return true;
	}

	public static bool MatchHeroInfo(InvenUseAttrTypeHeroSelect window, EmptyHeroPortraitSmallComponent.HeroInfo info)
	{
		if (info != null)
		{
			return MatchHero(window, info.HeroIndex);
		}
		return true;
	}

	public static bool MatchCostume(InvenUseHeroCostumeSelect window, int costumeIndex)
	{
		SprkTicketSearchBox sprkTicketSearchBox = Box(window);
		if (!(sprkTicketSearchBox == null) && sprkTicketSearchBox.Active)
		{
			return sprkTicketSearchBox.Accepts(CostumeText(costumeIndex));
		}
		return true;
	}

	private static Kind KindOf(SelectFilter filter, out BaseWindow window)
	{
		Transform transform = ((filter == null) ? null : filter.transform.parent);
		window = ((transform == null) ? null : transform.GetComponentInParent<BaseWindow>(includeInactive: true));
		if (window == null)
		{
			return Kind.None;
		}
		Type type = window.GetType();
		if (type == typeof(WeaponUniqueSelect))
		{
			return Kind.Equipment;
		}
		if (type == typeof(SoulStoneSelect))
		{
			return Kind.SoulWeapon;
		}
		if (type == typeof(InvenUseHeroSelect))
		{
			return Kind.Hero;
		}
		if (type == typeof(InvenUseHeroCostumeSelect))
		{
			return Kind.Costume;
		}
		return Kind.None;
	}

	public static void BeforeFilter(SelectFilter filter)
	{
		if (KindOf(filter, out var window) != Kind.None)
		{
			ClearCaches();
			SprkTicketSearchBox sprkTicketSearchBox = Box(window);
			if (sprkTicketSearchBox != null)
			{
				sprkTicketSearchBox.ResetSilently();
			}
		}
	}

	public static void AfterFilter(SelectFilter filter)
	{
		BaseWindow window;
		Kind kind = KindOf(filter, out window);
		if (kind == Kind.None)
		{
			return;
		}
		try
		{
			SprkTicketSearchBox sprkTicketSearchBox = Ensure(window, filter.transform, Placeholder(kind, filter));
			sprkTicketSearchBox.rebuild = Rebuilder(kind, window, filter);
			sprkTicketSearchBox.visible = Visibility(kind, window);
			sprkTicketSearchBox.pool = Pool(kind, window);
			LayoutFilterRow(sprkTicketSearchBox, filter);
			sprkTicketSearchBox.ResetSilently();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private static string Placeholder(Kind kind, SelectFilter filter)
	{
		switch (kind)
		{
		case Kind.Costume:
			return SprkText.Get("Search hero");
		case Kind.SoulWeapon:
		case Kind.Hero:
			return SprkText.Get("Search hero");
		default:
		{
			object obj = null;
			try
			{
				obj = typeof(SelectFilter).GetField("_selectType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(filter);
			}
			catch
			{
			}
			if (obj != null && (SelectFilter.SelectType)obj == SelectFilter.SelectType.SELECT_EQUIP_ARTIFACT)
			{
				return SprkText.Get("Search artifact");
			}
			return SprkText.Get("Search hero or item");
		}
		}
	}

	private static Func<IEnumerable<string>> Pool(Kind kind, BaseWindow window)
	{
		switch (kind)
		{
		case Kind.Equipment:
		case Kind.SoulWeapon:
			return delegate
			{
				ItemData itemData = Field<ItemData>(window, "_useItemData");
				WeaponUniqueSelectItemData weaponUniqueSelectItemData = ((itemData == null) ? null : (Call(window, "GetSelectItemData", itemData) as WeaponUniqueSelectItemData));
				return (weaponUniqueSelectItemData == null || weaponUniqueSelectItemData.ItemIndices == null) ? ((IEnumerable<string>)new string[0]) : ((IEnumerable<string>)(from i in weaponUniqueSelectItemData.ItemIndices
					select GameRoot.Instance.ItemDataContainer.GetData(i) into i
					where i != null
					select (kind != Kind.SoulWeapon) ? EquipText(GameRoot.Instance.EquipItemDataContainer.GetData(i.Code)) : SoulStoneText(i)).ToList());
			};
		case Kind.Hero:
			return () => (Call(window, "get_HeroSelectItemData") is HeroSelectItemData { HeroIndices: not null } heroSelectItemData) ? ((IEnumerable<string>)heroSelectItemData.HeroIndices.Select(HeroText).ToList()) : ((IEnumerable<string>)new string[0]);
		case Kind.Costume:
			return delegate
			{
				CostumeSelectItemData costumeSelectItemData = ((!(Call(window, "get_ItemData") is ItemData<CostumeSelectItemData> itemData)) ? null : itemData.SubData);
				List<int> list = new List<int>();
				if (costumeSelectItemData != null && costumeSelectItemData.CostumeIndices != null)
				{
					list.AddRange(costumeSelectItemData.CostumeIndices);
				}
				if (costumeSelectItemData != null && costumeSelectItemData.CostumeCategories != null)
				{
					string[] costumeCategories = costumeSelectItemData.CostumeCategories;
					foreach (string text in costumeCategories)
					{
						foreach (CostumeData item in GameRoot.Instance.CostumeDataContainer.GetDataByGroup(text) ?? new List<CostumeData>())
						{
							list.Add(item.CostumeIndex);
						}
					}
				}
				return list.Distinct().Select(CostumeText).ToList();
			};
		default:
			return null;
		}
	}

	private static Func<bool> Visibility(Kind kind, BaseWindow window)
	{
		switch (kind)
		{
		case Kind.Hero:
		{
			UIScrollGrid grid = Field<UIScrollGrid>(window, "HeroListGrid");
			return () => grid == null || grid.gameObject.activeSelf;
		}
		case Kind.Costume:
		{
			UIScrollGrid grid2 = Field<UIScrollGrid>(window, "HeroListGrid");
			return () => grid2 == null || grid2.gameObject.activeSelf;
		}
		default:
			return () => true;
		}
	}

	private static void ResetScroll(UIScrollGrid grid)
	{
		if (!(grid == null) && !(grid.ScrollView == null))
		{
			UIScrollView scrollView = grid.ScrollView;
			scrollView.DisableSpring();
			scrollView.currentMomentum = Vector3.zero;
			scrollView.ResetPosition();
		}
	}

	private static Action Rebuilder(Kind kind, BaseWindow window, SelectFilter filter)
	{
		switch (kind)
		{
		case Kind.Equipment:
		case Kind.SoulWeapon:
			return delegate
			{
				Field<SelectFilter.Callback>(filter, "_callback")?.Invoke();
				WeaponUniqueSelect obj = (WeaponUniqueSelect)window;
				DropHiddenSelection(obj);
				ResetScroll(obj.ItemGrid);
			};
		case Kind.Hero:
			return delegate
			{
				CreatureData creatureData = SelectedHero(window);
				Call(window, "CreateHeroPortraitList", new object[1]);
				if (creatureData != null)
				{
					Call(window, "SelectHero", creatureData, false);
				}
				ResetScroll(Field<UIScrollGrid>(window, "HeroListGrid"));
			};
		case Kind.Costume:
			return delegate
			{
				Field<SelectFilter.Callback>(filter, "_callback")?.Invoke();
				ResetScroll(Field<UIScrollGrid>(window, "HeroListGrid"));
			};
		default:
			return null;
		}
	}

	private static CreatureData SelectedHero(Component window)
	{
		HeroPortraitComponent heroPortraitComponent = Field<HeroPortraitComponent>(window, "_selectedHeroPortrait");
		if (heroPortraitComponent == null || !heroPortraitComponent.gameObject.activeInHierarchy)
		{
			return null;
		}
		try
		{
			return heroPortraitComponent.CreatureData;
		}
		catch
		{
			return null;
		}
	}

	private static void DropHiddenSelection(WeaponUniqueSelect window)
	{
		ItemData itemData = Field<ItemData>(window, "_buyItemData");
		if (itemData == null)
		{
			return;
		}
		IList list = Field<IList>(window, "_shopItemComponent");
		if (list != null)
		{
			foreach (object item in list)
			{
				ItemData itemData2 = ((item == null) ? null : Field<ItemData>(item, "_itemData"));
				if (itemData2 != null && itemData2.Index == itemData.Index)
				{
					return;
				}
			}
		}
		typeof(WeaponUniqueSelect).GetField("_buyItemData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(window, null);
		Call(window, "SetItemDescription", new object[1]);
		UIHelper.EnableButton(window.Button_Buy, enable: false, grayscaleIfDisable: true);
	}

	public static void BeforeAttr(InvenUseAttrTypeHeroSelect window)
	{
		ClearCaches();
		SprkTicketSearchBox sprkTicketSearchBox = Box(window);
		if (sprkTicketSearchBox != null)
		{
			sprkTicketSearchBox.ResetSilently();
		}
	}

	public static void AfterAttr(InvenUseAttrTypeHeroSelect window)
	{
		try
		{
			Transform listRoot = Field<Transform>(window, "HeroListRootTranform");
			UIScrollGrid physical = Field<UIScrollGrid>(window, "PhysicalHeroScrollGrid");
			UIScrollGrid magical = Field<UIScrollGrid>(window, "MagicalHeroScollGrid");
			if (physical == null || magical == null)
			{
				return;
			}
			Transform parent = physical.transform.parent;
			SprkTicketSearchBox sprkTicketSearchBox = Ensure(window, parent, SprkText.Get("Search hero"));
			sprkTicketSearchBox.visible = () => listRoot == null || listRoot.gameObject.activeInHierarchy;
			sprkTicketSearchBox.pool = () => (Call(window, "get_HeroIndexList") is List<int> source) ? ((IEnumerable<string>)source.Select(HeroText).ToList()) : ((IEnumerable<string>)new string[0]);
			sprkTicketSearchBox.rebuild = delegate
			{
				CreatureData creatureData = SelectedHero(window);
				Call(window, "SetHeroList", new object[1]);
				if (creatureData != null)
				{
					Call(window, "SelectHero", creatureData, false);
				}
				ResetScroll(physical);
				ResetScroll(magical);
			};
			LayoutAttrRow(sprkTicketSearchBox, window, parent, physical, magical);
			sprkTicketSearchBox.ResetSilently();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private static UIAtlas FindAtlas(Component window, string sprite)
	{
		UISprite[] componentsInChildren = window.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite uISprite in componentsInChildren)
		{
			if (uISprite.atlas != null && uISprite.atlas.GetSprite(sprite) != null)
			{
				return uISprite.atlas;
			}
		}
		return null;
	}

	private static UILabel FontSource(Component window)
	{
		UILabel uILabel = null;
		UILabel[] componentsInChildren = window.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel uILabel2 in componentsInChildren)
		{
			if (!(uILabel2.bitmapFont == null) || !(uILabel2.trueTypeFont == null))
			{
				if (uILabel2.GetComponentInParent<UIComboBox>(includeInactive: true) != null)
				{
					return uILabel2;
				}
				if (uILabel == null)
				{
					uILabel = uILabel2;
				}
			}
		}
		return uILabel;
	}

	private static SprkTicketSearchBox Ensure(BaseWindow window, Transform parent, string placeholder)
	{
		SprkTicketSearchBox sprkTicketSearchBox = window.GetComponent<SprkTicketSearchBox>();
		if (sprkTicketSearchBox == null)
		{
			sprkTicketSearchBox = window.gameObject.AddComponent<SprkTicketSearchBox>();
		}
		if (sprkTicketSearchBox.content == null)
		{
			GameObject gameObject = new GameObject("SprkTicketSearch");
			gameObject.layer = parent.gameObject.layer;
			gameObject.transform.SetParent(parent, worldPositionStays: false);
			sprkTicketSearchBox.content = gameObject;
			BuildField(sprkTicketSearchBox, window, gameObject.transform);
		}
		if (sprkTicketSearchBox.input != null)
		{
			sprkTicketSearchBox.input.defaultText = placeholder;
		}
		return sprkTicketSearchBox;
	}

	private static void BuildField(SprkTicketSearchBox box, BaseWindow window, Transform root)
	{
		UIAtlas uIAtlas = FindAtlas(window, "Common_Box_2");
		UILabel uILabel = FontSource(window);
		int val = 30;
		UIWidget[] componentsInChildren = window.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			if (uIWidget.panel == null || uIWidget.panel == root.GetComponentInParent<UIPanel>())
			{
				val = Math.Max(val, uIWidget.depth + 1);
			}
		}
		val = Math.Min(val, 60);
		GameObject gameObject = new GameObject("Input");
		gameObject.layer = root.gameObject.layer;
		gameObject.transform.SetParent(root, worldPositionStays: false);
		UISprite uISprite = gameObject.AddComponent<UISprite>();
		uISprite.atlas = uIAtlas;
		uISprite.spriteName = "Common_Box_2";
		uISprite.type = UIBasicSprite.Type.Sliced;
		uISprite.width = 360;
		uISprite.height = 62;
		uISprite.depth = val;
		gameObject.AddComponent<BoxCollider>();
		GameObject gameObject2 = new GameObject("Label_Search");
		gameObject2.layer = root.gameObject.layer;
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		UILabel uILabel2 = gameObject2.AddComponent<UILabel>();
		if (uILabel != null)
		{
			uILabel2.bitmapFont = uILabel.bitmapFont;
			if (uILabel.bitmapFont == null)
			{
				uILabel2.trueTypeFont = uILabel.trueTypeFont;
			}
		}
		uILabel2.fontSize = 30;
		uILabel2.pivot = UIWidget.Pivot.Left;
		uILabel2.alignment = NGUIText.Alignment.Left;
		uILabel2.overflowMethod = UILabel.Overflow.ClampContent;
		uILabel2.supportEncoding = false;
		uILabel2.effectStyle = UILabel.Effect.None;
		uILabel2.color = PlaceholderColor;
		uILabel2.depth = val + 1;
		uILabel2.height = 46;
		UIInput uIInput = gameObject.AddComponent<UIInput>();
		uIInput.label = uILabel2;
		uIInput.characterLimit = 24;
		uIInput.inputType = UIInput.InputType.Standard;
		uIInput.validation = UIInput.Validation.None;
		uIInput.onReturnKey = UIInput.OnReturnKey.Submit;
		uIInput.selectAllTextOnFocus = false;
		uIInput.activeTextColor = TextColor;
		uIInput.caretColor = new Color(0.953f, 0.925f, 0.855f, 1f);
		uIInput.selectionColor = new Color(0.85f, 0.68f, 0.36f, 0.45f);
		uIInput.value = "";
		EventDelegate.Add(uIInput.onChange, box.OnChanged);
		EventDelegate.Add(uIInput.onSubmit, box.OnSubmitted);
		GameObject gameObject3 = new GameObject("Button_Clear");
		gameObject3.layer = root.gameObject.layer;
		gameObject3.transform.SetParent(gameObject.transform, worldPositionStays: false);
		UISprite uISprite2 = gameObject3.AddComponent<UISprite>();
		uISprite2.atlas = FindAtlas(window, "Common_Close") ?? uIAtlas;
		uISprite2.spriteName = "Common_Close";
		uISprite2.type = UIBasicSprite.Type.Simple;
		uISprite2.width = 34;
		uISprite2.height = 30;
		uISprite2.depth = val + 2;
		gameObject3.AddComponent<BoxCollider>().size = new Vector3(58f, 58f, 1f);
		UIButton uIButton = gameObject3.AddComponent<UIButton>();
		uIButton.tweenTarget = gameObject3;
		uIButton.hover = new Color(1f, 0.92f, 0.75f, 1f);
		uIButton.pressed = new Color(0.75f, 0.68f, 0.55f, 1f);
		EventDelegate.Add(uIButton.onClick, box.Clear);
		gameObject3.SetActive(value: false);
		box.input = uIInput;
		box.label = uILabel2;
		box.clear = gameObject3;
	}

	private static void SizeField(SprkTicketSearchBox box, Vector3 center, int width)
	{
		UISprite component = box.input.GetComponent<UISprite>();
		box.input.transform.localPosition = Vector3.zero;
		box.content.transform.localPosition = center;
		component.width = width;
		component.height = 62;
		BoxCollider component2 = box.input.GetComponent<BoxCollider>();
		component2.center = Vector3.zero;
		component2.size = new Vector3(width, 62f, 1f);
		box.label.transform.localPosition = new Vector3((float)(-width) * 0.5f + 46f, 0f, 0f);
		box.label.width = Math.Max(60, width - 46 - 98);
		box.clear.transform.localPosition = new Vector3((float)width * 0.5f - 70f, 0f, 0f);
	}

	private static Geometry Capture(Transform combo)
	{
		Geometry result = new Geometry
		{
			position = combo.localPosition,
			childPositions = new Dictionary<Transform, Vector3>(),
			childWidths = new Dictionary<UIWidget, int>()
		};
		UIWidget component = combo.GetComponent<UIWidget>();
		result.width = ((component != null) ? component.width : 0);
		BoxCollider component2 = combo.GetComponent<BoxCollider>();
		result.collider = ((component2 != null) ? component2.size : Vector3.zero);
		foreach (Transform item in combo)
		{
			if (!(item.GetComponent<UIPopupRoot>() != null))
			{
				result.childPositions[item] = item.localPosition;
				UIWidget component3 = item.GetComponent<UIWidget>();
				if (component3 != null && (component3 is UISprite || component3 is UILabel) && component3.width > 60)
				{
					result.childWidths[component3] = component3.width;
				}
			}
		}
		return result;
	}

	private static void LayoutFilterRow(SprkTicketSearchBox box, SelectFilter filter)
	{
		List<Transform> list = new List<Transform>();
		string[] array = new string[3] { "ComboBoxMain", "ComboBoxSecondaryMain", "ComboBoxSub" };
		foreach (string name in array)
		{
			Component component = Field<Component>(filter, name);
			if (component != null && component.transform.parent == filter.transform && !list.Contains(component.transform))
			{
				list.Add(component.transform);
			}
		}
		foreach (Transform item in list)
		{
			if (!box.original.ContainsKey(item))
			{
				box.original[item] = Capture(item);
			}
		}
		List<List<Transform>> list2 = (from c in list
			where box.original[c].width > 0
			group c by Mathf.RoundToInt(box.original[c].position.x) into source
			where source.Any((Transform c) => c.gameObject.activeSelf)
			orderby source.Key
			select source.ToList()).ToList();
		if (list2.Count == 0)
		{
			list2 = (from c in list
				where box.original[c].width > 0
				group c by Mathf.RoundToInt(box.original[c].position.x) into source
				orderby source.Key
				select source.ToList()).ToList();
		}
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = 0f;
		foreach (Transform item2 in list)
		{
			Geometry geometry = box.original[item2];
			if (geometry.width > 0)
			{
				num = Math.Min(num, geometry.position.x - (float)geometry.width * 0.5f);
				num2 = Math.Max(num2, geometry.position.x + (float)geometry.width * 0.5f);
				num3 = geometry.position.y;
			}
		}
		foreach (Transform item3 in list)
		{
			Apply(item3, box.original[item3], box.original[item3].position.x, box.original[item3].width);
		}
		int num4 = list2.Count + 1;
		float num5 = (num2 - num) / (float)num4;
		for (int num6 = 0; num6 < list2.Count; num6++)
		{
			foreach (Transform item4 in list2[num6])
			{
				Geometry g = box.original[item4];
				Apply(item4, g, num + num5 * ((float)num6 + 0.5f), (int)Math.Min(g.width, num5 - 16f));
			}
		}
		List<Transform> list3 = list2.Select((List<Transform> list4) => list4.FirstOrDefault((Transform c) => c.gameObject.activeSelf) ?? list4[0]).ToList();
		float num7 = 0f;
		box.styleSource = null;
		if (list3.Count > 0)
		{
			Geometry geometry2 = box.original[list3[0]];
			UISprite styleSource = null;
			int num8 = 0;
			foreach (KeyValuePair<UIWidget, int> childWidth in geometry2.childWidths)
			{
				UISprite uISprite = childWidth.Key as UISprite;
				if (uISprite != null && childWidth.Value > num8)
				{
					styleSource = uISprite;
					num8 = childWidth.Value;
				}
			}
			box.styleSource = styleSource;
		}
		if (list3.Count > 0)
		{
			UILabel uILabel = list3[0].GetComponentsInChildren<UILabel>(includeInactive: true).FirstOrDefault((UILabel l) => l.GetComponentInParent<UIPopupRoot>() == null);
			if (uILabel != null)
			{
				num7 = uILabel.transform.localPosition.y;
			}
		}
		SizeField(box, new Vector3(num + num5 * ((float)list3.Count + 0.5f) - 14f, num3 + num7, 0f), (int)Math.Min(500f, num5 - 34f));
	}

	private static void Apply(Transform combo, Geometry g, float x, int width)
	{
		combo.localPosition = new Vector3(x, g.position.y, g.position.z);
		float num = ((g.width > 0) ? ((float)width / (float)g.width) : 1f);
		UIWidget component = combo.GetComponent<UIWidget>();
		if (component != null)
		{
			component.width = width;
		}
		BoxCollider component2 = combo.GetComponent<BoxCollider>();
		if (component2 != null)
		{
			component2.size = new Vector3(g.collider.x * num, g.collider.y, g.collider.z);
		}
		UIWidget uIWidget = null;
		int num2 = 0;
		foreach (KeyValuePair<UIWidget, int> childWidth in g.childWidths)
		{
			if (childWidth.Key is UISprite && childWidth.Value > num2)
			{
				uIWidget = childWidth.Key;
				num2 = childWidth.Value;
			}
		}
		HashSet<Transform> hashSet = new HashSet<Transform>(from w in g.childWidths.Keys
			where w != null
			select w.transform);
		float num3 = ((uIWidget != null && g.childPositions.ContainsKey(uIWidget.transform)) ? g.childPositions[uIWidget.transform].x : 0f);
		float num4 = Math.Max(40, (int)Math.Round((float)num2 * num));
		foreach (KeyValuePair<Transform, Vector3> childPosition in g.childPositions)
		{
			if (!(childPosition.Key == null))
			{
				float x2 = childPosition.Value.x * num;
				if (uIWidget != null && !hashSet.Contains(childPosition.Key))
				{
					float num5 = num3 - (float)num2 * 0.5f;
					float num6 = num3 + (float)num2 * 0.5f;
					float num7 = num3 * num;
					x2 = ((childPosition.Value.x <= num3) ? (num7 - num4 * 0.5f + (childPosition.Value.x - num5)) : (num7 + num4 * 0.5f - (num6 - childPosition.Value.x)));
				}
				childPosition.Key.localPosition = new Vector3(x2, childPosition.Value.y, childPosition.Value.z);
			}
		}
		foreach (KeyValuePair<UIWidget, int> childWidth2 in g.childWidths)
		{
			if (!(childWidth2.Key == null))
			{
				childWidth2.Key.width = Math.Max(40, (int)Math.Round((float)childWidth2.Value * num));
				UILabel uILabel = childWidth2.Key as UILabel;
				if (uILabel != null && uILabel.overflowMethod == UILabel.Overflow.ResizeFreely)
				{
					uILabel.overflowMethod = UILabel.Overflow.ShrinkContent;
				}
			}
		}
	}

	private static void LayoutAttrRow(SprkTicketSearchBox box, InvenUseAttrTypeHeroSelect window, Transform frame, UIScrollGrid physical, UIScrollGrid magical)
	{
		Transform transform = frame.Find("SprkTicketSearchLayout");
		UIWidget component = frame.GetComponent<UIWidget>();
		float num = ((component != null) ? ((float)component.height * 0.5f) : 353f);
		float num2 = ((component != null) ? ((float)component.width) : 1060f);
		if (transform == null)
		{
			new GameObject("SprkTicketSearchLayout").transform.SetParent(frame, worldPositionStays: false);
			Transform transform2 = (from c in frame.GetComponentsInChildren<UIComboBox>(includeInactive: true)
				select c.transform).FirstOrDefault((Transform t) => t.parent == frame);
			if (transform2 != null)
			{
				foreach (Transform item in transform2)
				{
					if (item.GetComponent<UILabel>() != null)
					{
						item.localPosition += new Vector3(0f, -58f, 0f);
					}
				}
				BoxCollider component2 = transform2.GetComponent<BoxCollider>();
				if (component2 != null)
				{
					component2.center += new Vector3(0f, -58f, 0f);
					component2.size = new Vector3(component2.size.x, Math.Min(component2.size.y, 80f), component2.size.z);
				}
			}
			UIScrollGrid[] array = new UIScrollGrid[2] { physical, magical };
			foreach (UIScrollGrid uIScrollGrid in array)
			{
				uIScrollGrid.transform.localPosition += new Vector3(0f, -46f, 0f);
				UIPanel clippingPanel = uIScrollGrid.ClippingPanel;
				if (clippingPanel != null)
				{
					Vector4 baseClipRegion = clippingPanel.baseClipRegion;
					clippingPanel.baseClipRegion = new Vector4(baseClipRegion.x, baseClipRegion.y + 23f, baseClipRegion.z, baseClipRegion.w - 46f);
				}
				if (uIScrollGrid.DragScrollView != null)
				{
					BoxCollider component3 = uIScrollGrid.DragScrollView.GetComponent<BoxCollider>();
					UIWidget component4 = uIScrollGrid.DragScrollView.GetComponent<UIWidget>();
					if (component4 != null)
					{
						component4.height = Math.Max(40, component4.height - 46);
					}
					if (component3 != null)
					{
						component3.size = new Vector3(component3.size.x, Math.Max(40f, component3.size.y - 46f), component3.size.z);
					}
					uIScrollGrid.DragScrollView.transform.localPosition += new Vector3(0f, 23f, 0f);
				}
			}
		}
		SizeField(box, new Vector3(0f, num - 12f - 31f, 0f), (int)Math.Min(620f, num2 - 120f));
	}

	private static Transform FilterRow(HeroPortraitList window, Transform frame)
	{
		Transform row = frame.Find("SprkHeroFilterRow");
		if (row == null)
		{
			GameObject gameObject = new GameObject("SprkHeroFilterRow");
			gameObject.layer = frame.gameObject.layer;
			gameObject.transform.SetParent(frame, worldPositionStays: false);
			gameObject.AddComponent<UIPanel>();
			row = gameObject.transform;
		}
		int num = (from p in window.GetComponentsInChildren<UIPanel>(includeInactive: true)
			where !p.transform.IsChildOf(row)
			select p.depth).DefaultIfEmpty(0).Max() + 1;
		row.GetComponent<UIPanel>().depth = num;
		UIPopupRoot[] componentsInChildren = row.GetComponentsInChildren<UIPopupRoot>(includeInactive: true);
		for (int num2 = 0; num2 < componentsInChildren.Length; num2++)
		{
			UIPanel[] componentsInChildren2 = componentsInChildren[num2].GetComponentsInChildren<UIPanel>(includeInactive: true);
			for (int num3 = 0; num3 < componentsInChildren2.Length; num3++)
			{
				componentsInChildren2[num3].depth = num + 10;
			}
		}
		return row;
	}

	public static void BeforeHeroGrid(HeroPortraitList window)
	{
		SprkTicketSearchBox sprkTicketSearchBox = Box(window);
		if (sprkTicketSearchBox != null)
		{
			sprkTicketSearchBox.ResetSilently();
		}
		SprkHeroGridState component = window.GetComponent<SprkHeroGridState>();
		if (component != null)
		{
			component.heroes.Clear();
			if (component.classes != null)
			{
				component.classes.SelectItem(0, isNotify: false);
			}
		}
	}

	public static void EnsureInspectAllHeroes(InspectUserList window)
	{
		UIScrollGrid grid = window.HeroListGrid;
		UIPanel panel = grid.ClippingPanel;
		Transform parent = panel.transform.parent;
		if (parent.Find("SprkInspectAllHeroes") != null)
		{
			return;
		}
		UIButton source = VespaResource.Load<GameObject>(HeroList.PrefabPath)
			.GetComponentsInChildren<UIButton>(includeInactive: true)
			.First(b => b.onClick.Any(e => e.methodName == "OnClickAllHero"));
		Vector3 bottom = parent.InverseTransformPoint((panel.worldCorners[0] + panel.worldCorners[3]) * 0.5f);
		GameObject root = UnityEngine.Object.Instantiate(source.transform.parent.gameObject, parent, worldPositionStays: false);
		root.name = "SprkInspectAllHeroes";
		UIButton button = root.GetComponentInChildren<UIButton>(includeInactive: true);
		foreach (UIWidget widget in root.GetComponentsInChildren<UIWidget>(includeInactive: true))
		{
			widget.SetAnchor((Transform)null);
		}
		foreach (AtlasAssignerBase assigner in root.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true))
		{
			assigner.Assign();
		}
		root.transform.localPosition = new Vector3(bottom.x, bottom.y + 46f, 0f);
		button.onClick.Clear();
		EventDelegate.Add(button.onClick, window.OnClickAllHero);
		root.SetActive(true);
		Vector4 clip = panel.baseClipRegion;
		panel.baseClipRegion = new Vector4(clip.x, clip.y + 50f, clip.z, clip.w - 100f);
		if (grid.DragScrollView != null)
		{
			grid.DragScrollView.transform.localPosition += new Vector3(0f, 50f, 0f);
			UIWidget drag = grid.DragScrollView.GetComponent<UIWidget>();
			if (drag != null)
			{
				drag.height -= 100;
			}
			BoxCollider collider = grid.DragScrollView.GetComponent<BoxCollider>();
			if (collider != null)
			{
				collider.size -= new Vector3(0f, 100f, 0f);
			}
		}
	}

	public static void AfterHeroGrid(HeroPortraitList window)
	{
		try
		{
			SprkHeroGridState state = window.GetComponent<SprkHeroGridState>();
			if (state == null)
			{
				state = window.gameObject.AddComponent<SprkHeroGridState>();
			}
			state.heroes = Field<List<HeroPortraitComponent>>(window, "_heroPortraitList").ToList();
			UIScrollGrid heroListGrid = window.HeroListGrid;
			Transform parent = heroListGrid.transform.parent;
			Transform transform = FilterRow(window, parent);
			SprkTicketSearchBox box = Ensure(window, transform, SprkText.Get("Search hero"));
			if (state.classes == null)
			{
				UIComboBox uIComboBox = UnityEngine.Object.Instantiate(Field<UIComboBox>(VespaResource.Load<GameObject>("Prefab2/Window/Inventory/WeaponUniqueSelect").GetComponentInChildren<SelectFilter>(includeInactive: true), "ComboBoxSub"), transform, worldPositionStays: false);
				uIComboBox.name = "SprkHeroClass";
				uIComboBox.OnChanged = null;
				uIComboBox.ConditionGameObjects = new UIComboBox.ShowOrHideObjects[0];
				AtlasAssignerBase[] componentsInChildren = uIComboBox.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Assign();
				}
				UIWidget[] componentsInChildren2 = uIComboBox.GetComponentsInChildren<UIWidget>(includeInactive: true);
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].SetAnchor((Transform)null);
				}
				Geometry g = Capture(uIComboBox.transform);
				int num = (from p in g.childWidths
					where p.Key is UISprite
					select p.Value).DefaultIfEmpty(g.width).Max();
				int num2 = (int)Math.Round(414.6341552734375);
				Apply(uIComboBox.transform, g, 0f, (int)Math.Round((float)num2 * (float)g.width / (float)num));
				uIComboBox.transform.localScale = Vector3.one * 0.82f;
				uIComboBox.transform.localPosition = new Vector3(-170f, 392.9f, 0f);
				UISprite uISprite = (from s in uIComboBox.GetComponentsInChildren<UISprite>(includeInactive: true)
					where s.GetComponentInParent<UIPopupRoot>() == null
					orderby s.width descending
					select s).FirstOrDefault();
				BoxCollider component = uIComboBox.GetComponent<BoxCollider>();
				if (component != null && uISprite != null)
				{
					component.center = new Vector3(uISprite.transform.localPosition.x, uISprite.transform.localPosition.y, 0f);
					component.size = new Vector3(uISprite.width, uISprite.height, component.size.z);
				}
				uIComboBox.gameObject.SetActive(value: true);
				uIComboBox.Clear();
				UISprite uISprite2 = uIComboBox.GetComponentsInChildren<UISprite>(includeInactive: true).FirstOrDefault((UISprite s) => s.name == "Sprite_Dir");
				float num3 = ((uISprite2 != null) ? (uISprite2.transform.localPosition.x + (float)uISprite2.width * 0.5f + 8f) : ((float)(-num2) * 0.5f + 40f));
				float num4 = ((uISprite != null) ? uISprite.transform.localPosition.x : 0f);
				float num5 = Math.Min(num4 - num3, (float)num2 * 0.5f - 34f);
				UILabel selectedLabel = uIComboBox.SelectedLabel;
				selectedLabel.overflowMethod = UILabel.Overflow.ShrinkContent;
				selectedLabel.pivot = UIWidget.Pivot.Center;
				selectedLabel.alignment = NGUIText.Alignment.Center;
				selectedLabel.width = (int)(num5 * 2f);
				selectedLabel.height = 52;
				selectedLabel.transform.localPosition = new Vector3(num4, selectedLabel.transform.localPosition.y, 0f);
				UIPanel[] componentsInChildren3 = uIComboBox.RootWindow.GetComponentsInChildren<UIPanel>(includeInactive: true);
				for (int i = 0; i < componentsInChildren3.Length; i++)
				{
					componentsInChildren3[i].depth = transform.GetComponent<UIPanel>().depth + 10;
				}
				int num6 = num2 - 24;
				componentsInChildren2 = uIComboBox.RootWindow.GetComponentsInChildren<UIWidget>(includeInactive: true);
				foreach (UIWidget uIWidget in componentsInChildren2)
				{
					if (uIWidget.width > num6)
					{
						uIWidget.width = ((uIWidget is UILabel) ? (num6 - 36) : num6);
					}
				}
				BoxCollider[] componentsInChildren4 = uIComboBox.RootWindow.GetComponentsInChildren<BoxCollider>(includeInactive: true);
				foreach (BoxCollider boxCollider in componentsInChildren4)
				{
					boxCollider.size = new Vector3(Math.Min(num6, boxCollider.size.x), boxCollider.size.y, boxCollider.size.z);
				}
				uIComboBox.AddItem(0, SprkText.Get("All Classes"));
				foreach (CreatureTagType item in FilterHelper.GetListOrderCreatureTagType())
				{
					uIComboBox.AddItem((int)item, LocalizationManager.GetString(item));
				}
				uIComboBox.Reposition();
				uIComboBox.SelectFirstItem(isNotify: false);
				uIComboBox.OnChanged = delegate
				{
					box.Apply();
				};
				state.classes = uIComboBox;
				heroListGrid.transform.localPosition += new Vector3(0f, -96f, 0f);
				UIPanel clippingPanel = heroListGrid.ClippingPanel;
				Vector4 baseClipRegion = clippingPanel.baseClipRegion;
				clippingPanel.baseClipRegion = new Vector4(baseClipRegion.x, baseClipRegion.y + 48f, baseClipRegion.z, baseClipRegion.w - 96f);
				UIDragScrollView dragScrollView = heroListGrid.DragScrollView;
				if (dragScrollView != null)
				{
					dragScrollView.transform.localPosition += new Vector3(0f, 48f, 0f);
					UIWidget component2 = dragScrollView.GetComponent<UIWidget>();
					if (component2 != null)
					{
						component2.height -= 96;
					}
					BoxCollider component3 = dragScrollView.GetComponent<BoxCollider>();
					if (component3 != null)
					{
						component3.size -= new Vector3(0f, 96f, 0f);
					}
				}
			}
			box.styleSource = (from s in state.classes.GetComponentsInChildren<UISprite>(includeInactive: true)
				where s.GetComponentInParent<UIPopupRoot>() == null
				orderby s.width descending
				select s).FirstOrDefault();
			box.content.transform.localScale = Vector3.one * 0.82f;
			SizeField(box, new Vector3(170f, 397f, 0f), (int)Math.Round(414.6341552734375));
			box.pool = () => from h in state.heroes
				where h != null && (state.classes.CurrentKey <= 0 || h.CreatureData.TagType == (CreatureTagType)state.classes.CurrentKey)
				select HeroText(h.HeroInfo.HeroIndex);
			box.rebuild = delegate
			{
				ApplyHeroGrid(window, state, box);
			};
			heroListGrid.Grid.hideInactive = true;
			ApplyHeroGrid(window, state, box);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private static void ApplyHeroGrid(HeroPortraitList window, SprkHeroGridState state, SprkTicketSearchBox box)
	{
		foreach (HeroPortraitComponent hero in state.heroes)
		{
			if (hero != null)
			{
				hero.gameObject.SetActive((state.classes.CurrentKey <= 0 || hero.CreatureData.TagType == (CreatureTagType)state.classes.CurrentKey) && (!box.Active || box.Accepts(HeroText(hero.HeroInfo.HeroIndex))));
			}
		}
		UIHelper.RepositionGrid(window.HeroListGrid);
		ResetScroll(window.HeroListGrid);
	}

	public static bool MoveHeroGrid(HeroPortraitList window, int from, int to, bool bookmarked)
	{
		SprkHeroGridState component = window.GetComponent<SprkHeroGridState>();
		if (component == null)
		{
			return true;
		}
		if (from < 0 || to < 0 || from >= component.heroes.Count || to >= component.heroes.Count)
		{
			return false;
		}
		HeroPortraitComponent heroPortraitComponent = component.heroes[from];
		component.heroes.RemoveAt(from);
		component.heroes.Insert(to, heroPortraitComponent);
		heroPortraitComponent.SetBookmarkedSmall(bookmarked);
		for (int i = 0; i < component.heroes.Count; i++)
		{
			component.heroes[i].transform.SetSiblingIndex(i);
		}
		ApplyHeroGrid(window, component, Box(window));
		return false;
	}
}
