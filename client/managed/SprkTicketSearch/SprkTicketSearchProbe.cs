using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NClientData;
using NGame2.NAccount;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.NJit;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkTicketSearchProbe
{
	private static bool localized;

	private static readonly Dictionary<string, List<UIAtlas>> specAtlases = new Dictionary<string, List<UIAtlas>>();

	private static GameObject shown;

	private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static BaseWindow window;

	private static string family;

	private static string Output => Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR");

	private static void Prepare()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		if (!localized)
		{
			localized = true;
			LocalizationLoader instance = SingletonAuto<LocalizationLoader>.instance;
			instance.SetEnableLoadFromPatch(enableLoad: true);
			instance.ChangeLanguage(SystemLanguage.English);
		}
	}

	private static string Describe(Transform t, Transform root)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = t.name;
		Transform parent = t.parent;
		while (parent != null && parent != root.parent)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		stringBuilder.Append(text).Append(t.gameObject.activeSelf ? "" : " [off]");
		stringBuilder.Append(" pos=").Append(t.localPosition.x.ToString("0")).Append(",")
			.Append(t.localPosition.y.ToString("0"));
		if (t.localScale != Vector3.one)
		{
			stringBuilder.Append(" scale=").Append(t.localScale.x.ToString("0.##"));
		}
		Component[] components = t.GetComponents<Component>();
		foreach (Component component in components)
		{
			if (!(component == null) && !(component is Transform))
			{
				stringBuilder.Append(" | ").Append(component.GetType().Name);
				UIWidget uIWidget = component as UIWidget;
				if (uIWidget != null)
				{
					stringBuilder.Append("(").Append(uIWidget.width).Append("x")
						.Append(uIWidget.height)
						.Append(" d")
						.Append(uIWidget.depth)
						.Append(" pivot=")
						.Append(uIWidget.pivot)
						.Append(")");
				}
				UISprite uISprite = component as UISprite;
				UISpriteAtlasAssigner uISpriteAtlasAssigner = component as UISpriteAtlasAssigner;
				if (uISpriteAtlasAssigner != null)
				{
					stringBuilder.Append(" atlasName=").Append(uISpriteAtlasAssigner.AtlasName);
				}
				if (uISprite != null)
				{
					stringBuilder.Append(" sprite=").Append((uISprite.atlas != null) ? uISprite.atlas.name : "?").Append(":")
						.Append(uISprite.spriteName)
						.Append(" type=")
						.Append(uISprite.type)
						.Append(" color=")
						.Append(ColorUtility.ToHtmlStringRGBA(uISprite.color));
				}
				UILabel uILabel = component as UILabel;
				if (uILabel != null)
				{
					stringBuilder.Append(" font=").Append((uILabel.bitmapFont != null) ? uILabel.bitmapFont.name : ((uILabel.trueTypeFont != null) ? uILabel.trueTypeFont.name : "?")).Append(" size=")
						.Append(uILabel.fontSize)
						.Append(" color=")
						.Append(ColorUtility.ToHtmlStringRGBA(uILabel.color))
						.Append(" effect=")
						.Append(uILabel.effectStyle)
						.Append(" text=\"")
						.Append((uILabel.text ?? "").Replace("\n", "\\n"))
						.Append("\"");
				}
				UITexture uITexture = component as UITexture;
				if (uITexture != null)
				{
					stringBuilder.Append(" tex=").Append((uITexture.mainTexture != null) ? uITexture.mainTexture.name : "null");
				}
				UIPanel uIPanel = component as UIPanel;
				if (uIPanel != null)
				{
					stringBuilder.Append(" depth=").Append(uIPanel.depth).Append(" clip=")
						.Append(uIPanel.clipping)
						.Append(" region=")
						.Append(uIPanel.baseClipRegion);
				}
				UIInput uIInput = component as UIInput;
				if (uIInput != null)
				{
					stringBuilder.Append(" label=").Append((uIInput.label != null) ? uIInput.label.name : "null").Append(" default=\"")
						.Append(uIInput.defaultText)
						.Append("\" limit=")
						.Append(uIInput.characterLimit);
				}
				BoxCollider boxCollider = component as BoxCollider;
				if (boxCollider != null)
				{
					stringBuilder.Append(" size=").Append(boxCollider.size.x.ToString("0")).Append("x")
						.Append(boxCollider.size.y.ToString("0"));
				}
				UIButton uIButton = component as UIButton;
				if (uIButton != null)
				{
					stringBuilder.Append(" normal=").Append(uIButton.normalSprite).Append(" hover=")
						.Append(uIButton.hoverSprite)
						.Append(" pressed=")
						.Append(uIButton.pressedSprite);
				}
			}
		}
		return stringBuilder.ToString();
	}

	private static void DumpPrefab(string name, string path, StringBuilder summary)
	{
		GameObject gameObject = VespaResource.Load<GameObject>(path);
		if (gameObject == null)
		{
			summary.AppendLine(name + ": prefab missing " + path);
			return;
		}
		GameObject gameObject2 = new GameObject("TicketSearchDump");
		gameObject2.SetActive(value: false);
		try
		{
			GameObject go = UnityEngine.Object.Instantiate(gameObject, gameObject2.transform, worldPositionStays: false);
			string[] array = (from t in go.GetComponentsInChildren<Transform>(includeInactive: true)
				select Describe(t, go.transform)).ToArray();
			File.WriteAllLines(Path.Combine(Output, "dump-" + name + ".txt"), array);
			UIAtlas[] array2 = (from s in go.GetComponentsInChildren<UISprite>(includeInactive: true)
				where s.atlas != null
				select s.atlas).Distinct().ToArray();
			UIAtlas[] array3 = array2;
			foreach (UIAtlas uIAtlas in array3)
			{
				BetterList<string> listOfSprites = uIAtlas.GetListOfSprites();
				File.WriteAllText(Path.Combine(Output, "atlas-" + uIAtlas.name + ".txt"), string.Join("\n", listOfSprites.ToArray()));
			}
			summary.AppendLine(name + ": nodes=" + array.Length + " atlases=" + string.Join(",", array2.Select((UIAtlas a) => a.name).ToArray()));
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(gameObject2);
		}
	}

	private static void LoadSpecAtlases(GameObject root)
	{
		HashSet<string> hashSet = new HashSet<string>();
		UISpriteAtlasAssigner[] componentsInChildren = root.GetComponentsInChildren<UISpriteAtlasAssigner>(includeInactive: true);
		foreach (UISpriteAtlasAssigner uISpriteAtlasAssigner in componentsInChildren)
		{
			if (!string.IsNullOrEmpty(uISpriteAtlasAssigner.AtlasName) && (uISpriteAtlasAssigner.Target == null || uISpriteAtlasAssigner.Target.atlas == null))
			{
				hashSet.Add(uISpriteAtlasAssigner.AtlasName);
			}
		}
		foreach (string item2 in hashSet)
		{
			if (specAtlases.ContainsKey(item2))
			{
				continue;
			}
			List<UIAtlas> list = new List<UIAtlas>();
			specAtlases[item2] = list;
			string path = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle/HighAndMidSpec/NguiAtlas_UI_Prefab_" + item2 + ".unity3d");
			if (!File.Exists(path))
			{
				continue;
			}
			AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
			if (assetBundle == null)
			{
				continue;
			}
			GameObject[] array = assetBundle.LoadAllAssets<GameObject>();
			for (int i = 0; i < array.Length; i++)
			{
				UIAtlas component = array[i].GetComponent<UIAtlas>();
				if (component != null)
				{
					list.Add(component);
				}
			}
			UIAtlas[] array2 = assetBundle.LoadAllAssets<UIAtlas>();
			foreach (UIAtlas item in array2)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		List<UIAtlas> source = specAtlases.Values.SelectMany((List<UIAtlas> l) => l).ToList();
		UISprite[] componentsInChildren2 = root.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite sprite in componentsInChildren2)
		{
			if (!(sprite.atlas != null) && !string.IsNullOrEmpty(sprite.spriteName))
			{
				UIAtlas uIAtlas = source.FirstOrDefault((UIAtlas a) => a.GetSprite(sprite.spriteName) != null);
				if (uIAtlas != null)
				{
					string spriteName = sprite.spriteName;
					sprite.atlas = uIAtlas;
					sprite.spriteName = spriteName;
				}
			}
		}
	}

	private static string Show(string name)
	{
		if (shown != null)
		{
			UnityEngine.Object.DestroyImmediate(shown);
		}
		string text = null;
		Type[] types = typeof(BaseWindow).Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (type.Name == name && typeof(BaseWindow).IsAssignableFrom(type))
			{
				FieldInfo field = type.GetField("PrefabPath", BindingFlags.Static | BindingFlags.Public);
				if (field != null)
				{
					text = (string)field.GetValue(null);
				}
			}
		}
		if (text == null)
		{
			return "unknown window " + name;
		}
		UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		shown = UnityEngine.Object.Instantiate(VespaResource.Load<GameObject>(text), uIRoot.transform, worldPositionStays: false);
		shown.name = "TicketSearchShow_" + name;
		UIPanel[] componentsInChildren = shown.GetComponentsInChildren<UIPanel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].depth += 500;
		}
		shown.SetActive(value: true);
		bool patchDownloaded = Define.PatchDownloaded;
		Define.PatchDownloaded = true;
		try
		{
			AtlasAssignerBase[] componentsInChildren2 = shown.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].Assign();
			}
		}
		finally
		{
			Define.PatchDownloaded = patchDownloaded;
		}
		LoadSpecAtlases(shown);
		string[] value = (from s in shown.GetComponentsInChildren<UISprite>(includeInactive: true)
			where s.atlas == null && !string.IsNullOrEmpty(s.spriteName)
			select s.spriteName).Distinct().ToArray();
		return "shown " + name + " patchDownloaded=" + patchDownloaded + " unresolved=" + string.Join(",", value);
	}

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Ticket search: " + message);
		}
	}

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

	private static MethodInfo Method(Type type, string name, int count)
	{
		while (type != null)
		{
			MethodInfo methodInfo = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((MethodInfo x) => x.Name == name && x.GetParameters().Length == count);
			if (methodInfo != null)
			{
				return methodInfo;
			}
			type = type.BaseType;
		}
		return null;
	}

	private static ItemData Ticket(string family)
	{
		ItemData result = null;
		int num = -1;
		foreach (ItemData allDatum in GameRoot.Instance.ItemDataContainer.GetAllData())
		{
			int num2 = -1;
			try
			{
				switch (family)
				{
				case "hero":
				{
					HeroSelectItemData subData = allDatum.GetSubData<HeroSelectItemData>();
					if (subData != null && subData.HeroIndices != null)
					{
						num2 = subData.HeroIndices.Length;
					}
					goto IL_01aa;
				}
				case "costume":
				{
					CostumeSelectItemData subData2 = allDatum.GetSubData<CostumeSelectItemData>();
					if (subData2 != null && subData2.CostumeIndices != null)
					{
						num2 = subData2.CostumeIndices.Length;
					}
					goto IL_01aa;
				}
				case "soul":
				{
					WeaponUniqueSelectItemData weaponUniqueSelectItemData = ((allDatum.Code == null) ? null : GameRoot.Instance.WeaponUniqueSelectItemDataContainer.GetData(allDatum.Code));
					if (weaponUniqueSelectItemData != null && weaponUniqueSelectItemData.ItemIndices != null && allDatum.Code.StartsWith("SOULSTONE_SELECT"))
					{
						num2 = weaponUniqueSelectItemData.ItemIndices.Length;
					}
					goto IL_01aa;
				}
				}
				WeaponUniqueSelectItemData subData3 = allDatum.GetSubData<WeaponUniqueSelectItemData>();
				if (subData3 == null || subData3.ItemIndices == null || subData3.ItemCode == null)
				{
					continue;
				}
				string itemCode = subData3.ItemCode;
				bool num3;
				switch (family)
				{
				case "soul":
					num3 = itemCode.StartsWith("SOULSTONE_SELECT");
					break;
				case "artifact":
					num3 = itemCode.StartsWith("ARTIFACT_SELECT");
					break;
				case "treasure":
					num3 = itemCode.StartsWith("TREASURE_UNIQUE_SELECT");
					break;
				case "weapon":
					num3 = itemCode.StartsWith("WEAPON_UNIQUE_SELECT");
					break;
				default:
					goto IL_01aa;
				}
				if (num3)
				{
					num2 = subData3.ItemIndices.Length;
				}
				goto IL_01aa;
			}
			catch
			{
				goto IL_01aa;
			}
			IL_01aa:
			if (num2 > num)
			{
				num = num2;
				result = allDatum;
			}
		}
		return result;
	}

	private static string WindowFor(string family)
	{
		return family switch
		{
			"soul" => "SoulStoneSelect", 
			"hero" => "InvenUseHeroSelect", 
			"attr" => "InvenUseAttrTypeHeroSelect", 
			"costume" => "InvenUseHeroCostumeSelect", 
			_ => "WeaponUniqueSelect", 
		};
	}

	private static string EnsureAccount()
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		PropertyInfo property = instance.GetType().GetProperty("AccountManagers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		object obj = property.GetValue(instance, null);
		List<string> list = new List<string>();
		if (obj == null)
		{
			obj = Activator.CreateInstance(property.PropertyType, nonPublic: true);
			list.Add("created");
		}
		FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.GetValue(obj) == null && !fieldInfo.FieldType.IsAbstract && !fieldInfo.FieldType.IsValueType)
			{
				try
				{
					fieldInfo.SetValue(obj, Activator.CreateInstance(fieldInfo.FieldType, nonPublic: true));
					list.Add(fieldInfo.Name);
				}
				catch (Exception ex)
				{
					list.Add(fieldInfo.Name + "!" + (ex.InnerException ?? ex).GetType().Name);
				}
			}
		}
		property.SetValue(instance, obj, null);
		return string.Join(",", list.ToArray());
	}

	private static string Open(string kind)
	{
		string text = EnsureAccount();
		Show(WindowFor(kind));
		family = kind;
		window = shown.GetComponentInChildren<BaseWindow>(includeInactive: true);
		ItemData ticket = Ticket((kind == "attr") ? "hero" : kind);
		Check(ticket != null, "ticket for " + kind);
		MethodInfo methodInfo = window.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First((MethodInfo m) => m.Name == "Open");
		object[] parameters = (from p in methodInfo.GetParameters()
			select (!(p.ParameterType == typeof(ItemData))) ? ((!(p.ParameterType == typeof(int))) ? null : ((object)ticket.Index)) : ticket).ToArray();
		try
		{
			methodInfo.Invoke(window, parameters);
		}
		catch (Exception innerException)
		{
			throw new Exception("account=" + text, innerException);
		}
		Define.PatchDownloaded = true;
		try
		{
			AtlasAssignerBase[] componentsInChildren = shown.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true);
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				componentsInChildren[num].Assign();
			}
		}
		finally
		{
			Define.PatchDownloaded = false;
		}
		LoadSpecAtlases(shown);
		return "account=" + text + " opened " + kind + " ticket=" + ticket.Index + " " + ticket.Code + " " + State();
	}

	private static IEnumerable<string> Visible()
	{
		if (window is WeaponUniqueSelect)
		{
			IList list = Field<IList>(window, "_shopItemComponent");
			foreach (object item in list)
			{
				ItemData itemData = Field<ItemData>(item, "_itemData");
				yield return (itemData == null) ? "?" : itemData.Index.ToString();
			}
			yield break;
		}
		if (window is InvenUseAttrTypeHeroSelect)
		{
			string[] array = new string[2] { "_physicalHeroPortraitCompList", "_magicalHeroPortraitCompList" };
			foreach (string name in array)
			{
				foreach (Component item2 in Field<IList>(window, name))
				{
					HeroPortraitComponent componentInChildren = item2.GetComponentInChildren<HeroPortraitComponent>(includeInactive: true);
					if (componentInChildren != null && componentInChildren.CreatureData != null)
					{
						yield return "h" + componentInChildren.CreatureData.Index;
					}
				}
			}
			yield break;
		}
		foreach (HeroPortraitComponent item3 in Field<IList>(window, "_heroPortraitList"))
		{
			yield return (item3.CreatureData != null) ? ("h" + item3.CreatureData.Index) : "?";
		}
	}

	private static string State()
	{
		SprkTicketSearchBox component = window.GetComponent<SprkTicketSearchBox>();
		string[] array = Visible().ToArray();
		return "visible=" + array.Length + " box=" + (component != null && component.content != null) + " query=\"" + ((component == null) ? "" : component.text) + "\"";
	}

	private static string Type(string text)
	{
		SprkTicketSearchBox component = window.GetComponent<SprkTicketSearchBox>();
		Check(component != null && component.input != null, "search field");
		component.input.value = text;
		component.OnSubmitted();
		return State() + " first=" + string.Join(",", Visible().Take(8).ToArray());
	}

	private static string TextOf(string id)
	{
		if (id.StartsWith("h"))
		{
			return SprkTicketSearch.HeroText(int.Parse(id.Substring(1)));
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(int.Parse(id));
		if (window is SoulStoneSelect)
		{
			return SprkTicketSearch.SoulStoneText(data);
		}
		return SprkTicketSearch.EquipText(GameRoot.Instance.EquipItemDataContainer.GetData(data.Code));
	}

	private static string FirstName(string id)
	{
		if (id.StartsWith("h"))
		{
			return GameRoot.Instance.CreatureDataContainer.GetData(int.Parse(id.Substring(1))).Name;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(int.Parse(id));
		if (family == "artifact")
		{
			return data.Name;
		}
		if (window is SoulStoneSelect)
		{
			return GameRoot.Instance.CreatureDataContainer.GetData(data.GetValueInt(0)).Name;
		}
		EquipItemData data2 = GameRoot.Instance.EquipItemDataContainer.GetData(data.Code);
		return GameRoot.Instance.CreatureDataContainer.GetData(data2.CreatureIndex[0]).Name;
	}

	private static bool NoneLabelShown()
	{
		string[] array = new string[3] { "LbDescriptionNoneEquip", "_lbNoneHero", "_lbNoneCostume" };
		foreach (string name in array)
		{
			UILabel uILabel = Field<UILabel>(window, name);
			if (uILabel != null)
			{
				return uILabel.gameObject.activeSelf;
			}
		}
		return true;
	}

	private static Bounds ColliderBounds(Component c, Transform space)
	{
		BoxCollider component = c.GetComponent<BoxCollider>();
		Bounds result = new Bounds(space.InverseTransformPoint(component.transform.TransformPoint(component.center - component.size * 0.5f)), Vector3.zero);
		result.Encapsulate(space.InverseTransformPoint(component.transform.TransformPoint(component.center + component.size * 0.5f)));
		return result;
	}

	private static void CheckAcquire(WeaponUniqueSelect ticket, bool enabled, string message)
	{
		Check(ticket.Button_Buy.isEnabled == enabled, message + ": click state");
		UIWidget component = ticket.Button_Buy.GetComponent<UIWidget>();
		Check(component != null && component.isGrayscale == !enabled, message + ": grayscale matches click state");
	}

	private static string TestFamily(string kind, List<string> lines)
	{
		Open(kind);
		SprkTicketSearchBox component = window.GetComponent<SprkTicketSearchBox>();
		Check(component != null && component.content != null && component.content.activeInHierarchy, kind + ": search field visible");
		Check(component.input.GetComponent<UISprite>().atlas != null, kind + ": native box sprite resolved");
		string[] array = Visible().ToArray();
		Check(array.Length != 0, kind + ": native list populated");
		Check(array.Distinct().Count() == array.Length || kind == "costume", kind + ": no duplicate entries");
		Transform transform = window.transform;
		Bounds bounds = ColliderBounds(component.input, transform);
		foreach (UIComboBox item in from c in window.GetComponentsInChildren<UIComboBox>(includeInactive: true)
			where c.gameObject.activeInHierarchy && c.GetComponent<BoxCollider>() != null
			select c)
		{
			Bounds bounds2 = ColliderBounds(item, transform);
			Check(!(bounds.min.x < bounds2.max.x - 1f) || !(bounds.max.x > bounds2.min.x + 1f) || !(bounds.min.y < bounds2.max.y - 1f) || !(bounds.max.y > bounds2.min.y + 1f), kind + ": field overlaps dropdown " + item.name);
		}
		string text = FirstName(array[0]);
		Type(text.ToUpperInvariant());
		string[] array2 = Visible().ToArray();
		Check(array2.Length != 0 && array2.Contains(array[0]), kind + ": query for " + text + " keeps the first entry");
		string[] terms = SprkTicketSearch.Terms(text);
		if (kind != "costume")
		{
			string[] array3 = array.Where((string id) => SprkTicketSearch.Matches(terms, TextOf(id))).ToArray();
			Check(array2.SequenceEqual(array3), kind + ": exact native-order subset for " + text + " (" + array2.Length + " vs " + array3.Length + ")");
		}
		else
		{
			Check(array2.All((string id) => SprkTicketSearch.Matches(terms, TextOf(id))), kind + ": costume heroes match " + text);
		}
		Check(array2.Length < array.Length, kind + ": query narrows the list");
		Check(!component.fuzzy, kind + ": exact hits keep typo matching off");
		UISprite component2 = component.input.GetComponent<UISprite>();
		UISprite component3 = component.clear.GetComponent<UISprite>();
		float num = (float)component2.width * 0.5f;
		float x = component.clear.transform.localPosition.x;
		Check(num - x - (float)component3.width * 0.5f >= 50f, kind + ": clear icon stays inside the blue background");
		Check(num - ColliderBounds(component3, component.input.transform).max.x >= 40f, kind + ": clear hit area stays inside the right end cap");
		Check(component.label.transform.localPosition.x + (float)component.label.width <= x - (float)component3.width * 0.5f - 10f, kind + ": search text leaves space before clear icon");
		Check(Mathf.Abs(component.clear.transform.localPosition.y) < 0.01f, kind + ": clear icon remains vertically centered");
		string text2 = (from w in text.Split(' ').Select(SprkTicketSearch.Fold)
			orderby w.Length descending
			select w).First();
		if (text2.Length >= 5)
		{
			string typo = text2.Substring(0, 1) + text2[2] + text2[1] + text2.Substring(3);
			Type(typo);
			string[] array4 = Visible().ToArray();
			if (component.fuzzy)
			{
				Check(array4.Contains(array[0]), kind + ": typo \"" + typo + "\" still finds " + text);
				if (kind != "costume")
				{
					Check(array4.All((string id) => SprkTicketSearch.FuzzyMatches(new string[1] { typo }, TextOf(id))), kind + ": typo results all match within tolerance");
				}
			}
			else
			{
				Check(array4.All((string id) => kind == "costume" || SprkTicketSearch.Matches(new string[1] { typo }, TextOf(id))), kind + ": typo had exact hits elsewhere");
			}
			lines.Add(kind + " typo=\"" + typo + "\" fuzzy=" + component.fuzzy + " hits=" + array4.Length);
		}
		Type("zzqxv");
		Check(Visible().Count() == 0, kind + ": unmatched query empties the list");
		Check(NoneLabelShown(), kind + ": native empty-state label shown");
		Check(component.clear.activeSelf, kind + ": clear button shown for text");
		EventDelegate.Execute(component.clear.GetComponent<UIButton>().onClick);
		Check(Visible().SequenceEqual(array), kind + ": clear button restores the full native list and order");
		Check(!component.clear.activeSelf && component.input.value == "", kind + ": clear empties the field");
		if (window is WeaponUniqueSelect)
		{
			WeaponUniqueSelect weaponUniqueSelect = (WeaponUniqueSelect)window;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(int.Parse(array2[0]));
			weaponUniqueSelect.OnSelect(data);
			Check(Field<ItemData>(weaponUniqueSelect, "_buyItemData") == data && weaponUniqueSelect.Button_Buy.isEnabled, kind + ": native selection enables Acquire");
			CheckAcquire(weaponUniqueSelect, enabled: true, kind + ": selection restores Acquire color");
			Type(text);
			if (kind == "artifact")
			{
				Check(Field<ItemData>(weaponUniqueSelect, "_buyItemData") == null && !weaponUniqueSelect.Button_Buy.isEnabled, kind + ": artifact rebuild clears choice like its source filter");
			}
			else
			{
				Check(Field<ItemData>(weaponUniqueSelect, "_buyItemData") == data && weaponUniqueSelect.Button_Buy.isEnabled, kind + ": visible selection is kept");
			}
			Type("zzqxv");
			Check(Field<ItemData>(weaponUniqueSelect, "_buyItemData") == null && !weaponUniqueSelect.Button_Buy.isEnabled, kind + ": hidden selection cannot be acquired");
			CheckAcquire(weaponUniqueSelect, enabled: false, kind + ": hidden selection disables and greys Acquire");
			component.Clear();
			weaponUniqueSelect.OnSelect(data);
			CheckAcquire(weaponUniqueSelect, enabled: true, kind + ": reselection after clearing search restores Acquire color");
			weaponUniqueSelect.OnSelect(data);
			CheckAcquire(weaponUniqueSelect, enabled: true, kind + ": repeated selection keeps Acquire colored");
			lines.Add(kind + " Acquire click/grayscale selection, hidden, clear and reselection PASS");
		}
		Type(text);
		SelectFilter selectFilter = window.GetComponentsInChildren<SelectFilter>(includeInactive: true).FirstOrDefault();
		if (selectFilter != null)
		{
			Field<SelectFilter.Callback>(selectFilter, "_callback")?.Invoke();
			Check(Visible().All((string id) => kind == "costume" || SprkTicketSearch.Matches(terms, TextOf(id))) && Visible().Count() == array2.Length, kind + ": native filter rebuild keeps the query");
		}
		Open(kind);
		component = window.GetComponent<SprkTicketSearchBox>();
		Check(component.input.value == "" && !component.Active && Visible().SequenceEqual(array), kind + ": reopening starts unfiltered");
		Check(window.GetComponentsInChildren<Transform>(includeInactive: true).Count((Transform t) => t.name == "SprkTicketSearch") == 1, kind + ": one field after reopen");
		if (window is WeaponUniqueSelect)
		{
			WeaponUniqueSelect obj = (WeaponUniqueSelect)window;
			obj.OnSelect(GameRoot.Instance.ItemDataContainer.GetData(int.Parse(array[0])));
			CheckAcquire(obj, enabled: true, kind + ": selection after reopening restores Acquire color");
		}
		string text3 = kind + " all=" + array.Length + " query=\"" + text + "\" hits=" + array2.Length;
		lines.Add(text3);
		return text3;
	}

	private static string Test()
	{
		List<string> list = new List<string>();
		string[] array = new string[7] { "weapon", "treasure", "artifact", "soul", "hero", "attr", "costume" };
		for (int i = 0; i < array.Length; i++)
		{
			TestFamily(array[i], list);
		}
		Check(SprkTicketSearch.Fold("Dark Lord Kasel") == "darklordkasel", "fold spaces and case");
		Check(SprkTicketSearch.Fold("Lua's Tear") == "luastear", "fold punctuation");
		Check(SprkTicketSearch.Fold("Éclair") == "eclair", "fold accents");
		Check(SprkTicketSearch.Matches(SprkTicketSearch.Terms("lord kas"), "|darklordkasel|"), "multi-term AND");
		Check(!SprkTicketSearch.Matches(SprkTicketSearch.Terms("lord frey"), "|darklordkasel|"), "multi-term rejects");
		Check(SprkTicketSearch.Matches(SprkTicketSearch.Terms("  "), "|x|"), "blank query matches all");
		Check(SprkTicketSearch.Distance("kasle", "kasel") == 1, "transposition is one edit");
		Check(SprkTicketSearch.Distance("frey", "freya") == 0, "substring distance");
		Check(SprkTicketSearch.Distance("kasl", "darklordkasel") == 1, "deletion inside a longer name");
		Check(!SprkTicketSearch.Matches(SprkTicketSearch.Terms("kasle"), "|darklordkasel|#dlk#"), "exact tier rejects typo");
		Check(SprkTicketSearch.FuzzyMatches(SprkTicketSearch.Terms("kasle"), "|darklordkasel|#dlk#"), "fuzzy tier accepts typo");
		Check(SprkTicketSearch.FuzzyMatches(SprkTicketSearch.Terms("dlk"), "|darklordkasel|#dlk#"), "initials");
		Check(!SprkTicketSearch.Matches(SprkTicketSearch.Terms("dlk"), "|darklordkasel|#dlk#"), "initials only in fuzzy tier");
		Check(!SprkTicketSearch.FuzzyMatches(SprkTicketSearch.Terms("fry"), "|frey|#f#"), "no typo tolerance under 4 letters");
		Check(!SprkTicketSearch.FuzzyMatches(SprkTicketSearch.Terms("kxyzl"), "|kasel|#k#"), "tolerance is bounded");
		Check(SprkTicketSearch.FuzzyMatches(SprkTicketSearch.Terms("lord kasle"), "|darklordkasel|#dlk#"), "multi-word fuzzy");
		return "PASS: " + string.Join("; ", list.ToArray());
	}

	private static string HeroInfoTest()
	{
		EnsureAccount();
		Show("HeroPortraitList");
		HeroPortraitList component = shown.GetComponent<HeroPortraitList>();
		List<CreatureData> list = (from h in GameRoot.Instance.CreatureDataContainer.GetAllData()
			where h.OpenType == CreatureOpenType.Opened
			select h).ToList();
		Check(list.Count > 20, "hero fixture pool");
		typeof(HeroPortraitList).GetField("_SelectedHeroIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(component, list[0].Index);
		MethodInfo methodInfo = Method(typeof(HeroPortraitList), "CreateHeroItem", 1);
		foreach (CreatureData item in list)
		{
			CachedHeroInfo cachedHeroInfo = new CachedHeroInfo(item);
			cachedHeroInfo.Level = 100;
			cachedHeroInfo.Star = 5;
			cachedHeroInfo.Transcended = 5;
			methodInfo.Invoke(component, new object[1] { cachedHeroInfo });
		}
		Define.PatchDownloaded = true;
		try
		{
			SprkTicketSearch.AfterHeroGrid(component);
		}
		finally
		{
			Define.PatchDownloaded = false;
		}
		LoadSpecAtlases(shown);
		SprkHeroGridState state = component.GetComponent<SprkHeroGridState>();
		SprkTicketSearchBox component2 = component.GetComponent<SprkTicketSearchBox>();
		Func<int> func = () => state.heroes.Count((HeroPortraitComponent h) => h.gameObject.activeSelf);
		int depth = component.HeroListGrid.ClippingPanel.depth;
		Check(component2.input.GetComponentInParent<UIPanel>().depth > depth && state.classes.GetComponentInParent<UIPanel>().depth > depth, "filter row above grid panel");
		FieldInfo field = typeof(UIInput).GetField("mValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		Check(field == null || field.GetValue(component2.input) != null, "search input initialised before Start");
		Transform parent = component.HeroListGrid.transform.parent;
		Bounds bounds = ColliderBounds(component2.input, parent);
		Bounds bounds2 = ColliderBounds(state.classes, parent);
		Check(bounds2.max.x <= bounds.min.x + 1f, "class filter and search do not overlap");
		Check(Mathf.Abs(bounds2.size.x - bounds.size.x) < 2f, "class filter and search are equal width (" + bounds2.size.x.ToString("0") + " vs " + bounds.size.x.ToString("0") + ")");
		Check(bounds2.min.x >= -361f && bounds.max.x <= 361f, "row stays inside the frame (" + bounds2.min.x.ToString("0") + ".." + bounds.max.x.ToString("0") + ")");
		Vector3 vector = parent.InverseTransformPoint(component2.styleSource.transform.position);
		Check(component2.styleSource != null && Mathf.Abs(bounds.center.y - vector.y) < 2f && Mathf.Abs(bounds2.center.y - vector.y) < 2f, "row controls share a centre line");
		Check(bounds.max.y <= 453f && System.Math.Min(bounds.min.y, bounds2.min.y) >= component.HeroListGrid.ClippingPanel.baseClipRegion.y + component.HeroListGrid.ClippingPanel.baseClipRegion.w * 0.5f + component.HeroListGrid.ClippingPanel.transform.localPosition.y + component.HeroListGrid.transform.localPosition.y - 1f, "row stays between frame top and the grid");
		UISprite uISprite = state.classes.GetComponentsInChildren<UISprite>(includeInactive: true).First((UISprite s) => s.name == "Sprite_Dir");
		UILabel selectedLabel = state.classes.SelectedLabel;
		Check(selectedLabel.transform.localPosition.x - (float)selectedLabel.width * 0.5f >= uISprite.transform.localPosition.x + (float)uISprite.width * 0.5f, "class text clears the arrow");
		Check(Mathf.Abs(parent.InverseTransformPoint(selectedLabel.transform.position).x - vector.x) < 1f, "class text centred on its box");
		Check(func() == list.Count, "unfiltered Hero Info");
		int tag = (int)list[0].TagType;
		state.classes.SelectItem(tag);
		Check(func() == list.Count((CreatureData h) => h.TagType == (CreatureTagType)tag), "class filter");
		component2.input.value = list[0].Name;
		component2.OnSubmitted();
		Check(func() > 0 && state.heroes.Where((HeroPortraitComponent h) => h.gameObject.activeSelf).All((HeroPortraitComponent h) => h.CreatureData.TagType == (CreatureTagType)tag), "combined class and name");
		component2.input.value = "zzzzzzzzzzzzzzzzzzzz";
		component2.OnSubmitted();
		Check(func() == 0, "zero results");
		component2.Clear();
		Check(func() == list.Count((CreatureData h) => h.TagType == (CreatureTagType)tag), "clear preserves class");
		HeroPortraitComponent heroPortraitComponent = state.heroes[state.heroes.Count - 1];
		component.MoveGridItem(state.heroes.Count - 1, 0, bookmarked: true);
		Check(state.heroes[0] == heroPortraitComponent && heroPortraitComponent.transform.GetSiblingIndex() == 0, "bookmark uses full-list indices");
		state.classes.SelectItem(0);
		Check(func() == list.Count, "restore all classes");
		float y = component.HeroListGrid.transform.localPosition.y;
		Vector4 baseClipRegion = component.HeroListGrid.ClippingPanel.baseClipRegion;
		SprkTicketSearch.BeforeHeroGrid(component);
		SprkTicketSearch.AfterHeroGrid(component);
		Check(component.HeroListGrid.transform.localPosition.y == y && component.HeroListGrid.ClippingPanel.baseClipRegion == baseClipRegion, "layout idempotence");
		Check(component2.text == "" && state.classes.CurrentKey == 0 && func() == list.Count, "reopen reset");
		Check((int)typeof(HeroPortraitList).GetField("_SelectedHeroIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(component) == list[0].Index, "selection preserved");
		return "PASS: Hero Info class/name intersection, zero results, clear, bookmark indices, reopen, selection and layout; heroes=" + func();
	}

	private sealed class InspectSelectionListener : InspectUserList.Listener
	{
		public HeroPortraitComponent selected;
		public void OnClickHeroPortrait(HeroPortraitComponent portrait) { selected = portrait; }
		public void OnClickEquipIcon(ItemIconComponent caller) { }
		public void OnClickDetailView() { }
		public void OnClickChatting() { }
		public void OnClickRunehengeIcon() { }
		public void OnClose() { }
	}

	private static string InspectHeroTest()
	{
		EnsureAccount();
		Show("InspectUserList");
		InspectUserList inspect = shown.GetComponent<InspectUserList>();
		CachedHeroInfo[] heroes = GameRoot.Instance.CreatureDataContainer.GetAllData()
			.Where(h => h.OpenType == CreatureOpenType.Opened).Take(24)
			.Select(h => new CachedHeroInfo(h) { Level = 30, Star = 2 }).ToArray();
		InspectSelectionListener listener = new InspectSelectionListener();
		typeof(InspectUserList).GetField("_HeroInfos", Any).SetValue(inspect, heroes);
		typeof(InspectUserList).GetField("_SelectedHeroIndex", Any).SetValue(inspect, heroes[0].HeroIndex);
		typeof(InspectUserList).GetField("_listener", Any).SetValue(inspect, listener);
		Method(typeof(InspectUserList), "Setup", 0).Invoke(inspect, null);
		bool downloaded = Define.PatchDownloaded;
		Define.PatchDownloaded = true;
		try
		{
			SprkTicketSearch.EnsureInspectAllHeroes(inspect);
			Vector4 clip = inspect.HeroListGrid.ClippingPanel.baseClipRegion;
			SprkTicketSearch.EnsureInspectAllHeroes(inspect);
			Check(inspect.HeroListGrid.ClippingPanel.baseClipRegion == clip, "inspect layout idempotence");
			UIButton button = shown.GetComponentsInChildren<UIButton>().Single(b => b.transform.parent.name == "SprkInspectAllHeroes");
			Check(button.transform.parent.GetComponentInChildren<UILabel>() != null, "inspect button includes its localized label");
			EventDelegate.Execute(button.onClick);
			NGame2.NUI.WindowPrefab<HeroPortraitList> prefab = (NGame2.NUI.WindowPrefab<HeroPortraitList>)typeof(InspectUserList).GetField("_allHeroes", Any).GetValue(inspect);
			HeroPortraitList grid = prefab.instance;
			SprkHeroGridState state = grid.GetComponent<SprkHeroGridState>();
			SprkTicketSearchBox box = grid.GetComponent<SprkTicketSearchBox>();
			Check(state != null && state.heroes.Count == heroes.Length && state.heroes.All(h => heroes.Contains(h.HeroInfo)), "inspected roster only");
			int tag = (int)state.heroes[1].CreatureData.TagType;
			state.classes.SelectItem(tag);
			box.input.value = state.heroes[1].CreatureData.Name;
			box.OnSubmitted();
			Check(state.heroes.Any(h => h.gameObject.activeSelf) && state.heroes.Where(h => h.gameObject.activeSelf).All(h => (int)h.CreatureData.TagType == tag), "inspect class and search combine");
			box.input.value = "zzzzzzzzzzzzzzzzzz";
			box.OnSubmitted();
			Check(state.heroes.All(h => !h.gameObject.activeSelf), "inspect zero results");
			box.Clear();
			Check(state.heroes.Where(h => h.gameObject.activeSelf).All(h => (int)h.CreatureData.TagType == tag), "inspect clear preserves class");
			((HeroPortraitComponent.Listener)grid).OnClick(state.heroes[1]);
			Check(listener.selected != null && listener.selected.HeroInfo == heroes[1] && !grid.IsOpen(), "grid selection forwards inspected sidebar portrait and closes");
			inspect.OnClickAllHero();
			Check(box.text == "" && state.classes.CurrentKey == 0 && state.heroes.All(h => h.gameObject.activeSelf), "inspect reopen resets filters");
			Check((int)typeof(HeroPortraitList).GetField("_SelectedHeroIndex", Any).GetValue(grid) == heroes[1].HeroIndex, "inspect selected hero persists");
			LoadSpecAtlases(shown);
			LoadSpecAtlases(grid.gameObject);
			prefab.CloseWindow();
			foreach (UIPanel panel in shown.GetComponentsInChildren<UIPanel>(includeInactive: true))
			{
				panel.alpha = 1f;
			}
			return "PASS: inspected roster; class and search; no matches; clear; selection callback; reopen reset; selected hero; idempotent layout.";
		}
		finally { Define.PatchDownloaded = downloaded; }
	}

	public static string Run(string mode)
	{
		Prepare();
		if (mode == "inspect-hero-test")
		{
			return InspectHeroTest();
		}
		if (mode == "hero-info-test")
		{
			return HeroInfoTest();
		}
		if (mode != null && mode.StartsWith("show:"))
		{
			return Show(mode.Substring(5));
		}
		if (mode != null && mode.StartsWith("dumpshown:") && shown != null)
		{
			Transform under = shown.transform.Find(mode.Substring(10));
			if (under == null)
			{
				return "missing " + mode;
			}
			return string.Join("\n", (from t in under.GetComponentsInChildren<Transform>(includeInactive: true)
				select Describe(t, under)).ToArray());
		}
		if (mode != null && mode.StartsWith("open:"))
		{
			return Open(mode.Substring(5));
		}
		if (mode != null && mode.StartsWith("type:"))
		{
			return Type(mode.Substring(5));
		}
		switch (mode)
		{
		case "focus":
			window.GetComponent<SprkTicketSearchBox>().input.isSelected = true;
			return State();
		case "state":
			return State();
		case "test":
			return Test();
		case "hide":
			if (shown != null)
			{
				UnityEngine.Object.DestroyImmediate(shown);
			}
			return "hidden";
		default:
		{
			StringBuilder stringBuilder = new StringBuilder();
			KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[7]
			{
				new KeyValuePair<string, string>("WeaponUniqueSelect", "Prefab2/Window/Inventory/WeaponUniqueSelect"),
				new KeyValuePair<string, string>("SoulStoneSelect", "Prefab2/Window/Inventory/SoulStoneSelect"),
				new KeyValuePair<string, string>("InvenUseHeroSelect", "Prefab2/Window/Inventory/InvenUseHeroSelect"),
				new KeyValuePair<string, string>("InvenUseAttrTypeHeroSelect", "Prefab2/Window/Inventory/InvenUseAttrTypeHeroSelect"),
				new KeyValuePair<string, string>("InvenUseHeroCostumeSelect", "Prefab2/Window/Inventory/InvenUseHeroCostumeSelect"),
				new KeyValuePair<string, string>("FriendSearch", "Resources/Prefab2/Window/Friend/FriendSearch"),
				new KeyValuePair<string, string>("GuildList2", "Resources/Prefab2/Window/Guild/GuildList2")
			};
			for (int num = 0; num < array.Length; num++)
			{
				KeyValuePair<string, string> keyValuePair = array[num];
				DumpPrefab(keyValuePair.Key, keyValuePair.Value, stringBuilder);
			}
			return stringBuilder.ToString();
		}
		}
	}
}
