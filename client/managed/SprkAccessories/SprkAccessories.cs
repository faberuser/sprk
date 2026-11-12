using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using MiniJSON;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NCostume;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCustomizing;
using NGame2.NUtil.NCostume;
using NGameAsset;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkAccessories
{
	public const string WireKey = "SprkAccessories";

	private static readonly ConditionalWeakTable<object, List<SprkAccessoryEntry>> states = new ConditionalWeakTable<object, List<SprkAccessoryEntry>>();

	private static readonly FieldInfo backing = typeof(CachedHeroInfo).GetField("_heroInfo", BindingFlags.Instance | BindingFlags.NonPublic);

	private static object Key(object value)
	{
		while (value is CachedHeroInfo)
		{
			value = backing.GetValue(value);
		}
		return value;
	}

	public static bool Has(object value)
	{
		List<SprkAccessoryEntry> value2;
		if (value != null)
		{
			return states.TryGetValue(Key(value), out value2);
		}
		return false;
	}

	private static int Scalar(object value, int part)
	{
		PropertyInfo property = value.GetType().GetProperty("AccessoryCostumeIndex" + part);
		if (!(property == null))
		{
			return Convert.ToInt32(property.GetValue(value, null));
		}
		return 0;
	}

	public static List<SprkAccessoryEntry> Entries(object value)
	{
		List<SprkAccessoryEntry> list = new List<SprkAccessoryEntry>();
		if (value == null)
		{
			return list;
		}
		if (states.TryGetValue(Key(value), out var value2))
		{
			foreach (SprkAccessoryEntry item in value2)
			{
				list.Add(item.Clone());
			}
			return list;
		}
		for (int i = 1; i <= 6; i++)
		{
			int num = Scalar(value, i);
			if (num <= 0)
			{
				continue;
			}
			PositionInfo positionInfo = null;
			if (value is CachedHeroInfo cachedHeroInfo)
			{
				positionInfo = cachedHeroInfo.GetAccessoryPositionInfo((AccessoryPartType)i);
			}
			else
			{
				PropertyInfo property = value.GetType().GetProperty("PositionInfo" + i);
				if (property != null)
				{
					positionInfo = PositionInfo.Create().TryAssign(property.GetValue(value, null) as string);
				}
			}
			HeroInfo heroInfo = value as HeroInfo;
			if (positionInfo == null && heroInfo != null && GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				PlayerAccessoryCostumeInfo playerAccessoryCostumeInfo = GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(heroInfo.HeroIndex, num));
				if (playerAccessoryCostumeInfo != null)
				{
					positionInfo = PositionInfo.Create().TryAssign(playerAccessoryCostumeInfo.PositionInfo);
				}
			}
			list.Add(new SprkAccessoryEntry
			{
				Index = num,
				Part = (AccessoryPartType)i,
				Position = (positionInfo ?? PositionInfo.Create())
			});
		}
		return list;
	}

	public static void Store(object value, List<SprkAccessoryEntry> list)
	{
		if (value != null)
		{
			object key = Key(value);
			states.Remove(key);
			states.Add(key, list);
		}
	}

	public static void Copy(object target, object source)
	{
		if (target != null && source != null)
		{
			Store(target, Entries(source));
		}
	}

	public static void Read(object value, IDictionary json)
	{
		if (value == null || json == null)
		{
			return;
		}
		states.Remove(Key(value));
		if (!(json["SprkAccessories"] is IEnumerable enumerable))
		{
			return;
		}
		List<SprkAccessoryEntry> list = new List<SprkAccessoryEntry>();
		HashSet<int> hashSet = new HashSet<int>();
		foreach (object item in enumerable)
		{
			if (item is IDictionary dictionary)
			{
				int num = Convert.ToInt32(dictionary["Index"]);
				int num2 = Convert.ToInt32(dictionary["Part"]);
				if (num > 0 && num2 >= 1 && num2 <= 6 && hashSet.Add(num))
				{
					list.Add(new SprkAccessoryEntry
					{
						Index = num,
						Part = (AccessoryPartType)num2,
						Position = PositionInfo.Create().TryAssign(dictionary["PositionInfo"] as string)
					});
				}
			}
		}
		Store(value, list);
	}

	public static void Write(IDictionary json, object value)
	{
		if (json == null || !Has(value))
		{
			return;
		}
		List<object> list = new List<object>();
		foreach (SprkAccessoryEntry item in Entries(value))
		{
			list.Add(new Dictionary<string, object>
			{
				{ "Index", item.Index },
				{
					"Part",
					(int)item.Part
				},
				{
					"PositionInfo",
					JsonMarshaler.EncodeJsonString(item.Position ?? PositionInfo.Create())
				}
			});
		}
		json["SprkAccessories"] = list;
	}

	public static string Fingerprint(object value)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Write(dictionary, value);
		return Json.Serialize(dictionary);
	}

	public static bool Equal(bool equal, object a, object b)
	{
		if (equal)
		{
			return Fingerprint(a) == Fingerprint(b);
		}
		return false;
	}

	public static bool Contains(HeroInfo hero, int id)
	{
		foreach (SprkAccessoryEntry item in Entries(hero))
		{
			if (item.Index == id)
			{
				return true;
			}
		}
		return false;
	}

	public static PositionInfo Position(HeroInfo hero, int id)
	{
		foreach (SprkAccessoryEntry item in Entries(hero))
		{
			if (item.Index == id)
			{
				return item.Position;
			}
		}
		return PositionInfo.Create();
	}

	public static void Put(List<SprkAccessoryEntry> list, int id, AccessoryPartType part, PositionInfo position)
	{
		list.RemoveAll((SprkAccessoryEntry e) => e.Index == id || (part > AccessoryPartType.Body && e.Part == part));
		list.Add(new SprkAccessoryEntry
		{
			Index = id,
			Part = part,
			Position = ((position == null) ? PositionInfo.Create() : position.Clone())
		});
	}

	public static void SetStock(ref Dictionary<AccessoryPartType, CustomizeStock> map, CustomizeStock stock)
	{
		if (map == null)
		{
			map = new Dictionary<AccessoryPartType, CustomizeStock>();
		}
		if (stock.IsEmptyPart())
		{
			if (stock.Part < AccessoryPartType.None)
			{
				map.Remove(stock.Part);
				return;
			}
			List<AccessoryPartType> list = new List<AccessoryPartType>();
			foreach (KeyValuePair<AccessoryPartType, CustomizeStock> item in map)
			{
				if (item.Value.Part == stock.Part)
				{
					list.Add(item.Key);
				}
			}
			{
				foreach (AccessoryPartType item2 in list)
				{
					map.Remove(item2);
				}
				return;
			}
		}
		if (stock.Part > AccessoryPartType.Body)
		{
			List<AccessoryPartType> list2 = new List<AccessoryPartType>();
			foreach (KeyValuePair<AccessoryPartType, CustomizeStock> item3 in map)
			{
				if (item3.Value.Part == stock.Part)
				{
					list2.Add(item3.Key);
				}
			}
			foreach (AccessoryPartType item4 in list2)
			{
				map.Remove(item4);
			}
		}
		map[(AccessoryPartType)(-stock.AccessoryIndex)] = stock;
	}

	public static CustomizeStock[] Stocks(HeroInfo hero)
	{
		List<CustomizeStock> list = new List<CustomizeStock>();
		foreach (SprkAccessoryEntry item in Entries(hero))
		{
			AccessoryCostumeData data = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(item.Index);
			CustomizeStock stock = default(CustomizeStock);
			if (data != null && RenderFacadeBuilder.TryAssignCustomizeStock(ref stock, hero.HeroIndex, data, item.Position))
			{
				list.Add(stock);
			}
		}
		return list.ToArray();
	}

	public static void Build(HeroInfo hero, ref CreatureStock stock)
	{
		if (hero != null)
		{
			stock.ClearCustomizeStock();
			CustomizeStock[] array = Stocks(hero);
			foreach (CustomizeStock customizeStock in array)
			{
				stock.SetCustomizeStock(customizeStock);
			}
		}
	}

	public static bool Reconcile(ICreatureLoad load, CustomizeStock[] desired)
	{
		if (load == null || !load.IsDone || !load.IsValid || load.Value == null || desired == null)
		{
			return false;
		}
		ICreature value = load.Value;
		ICreatureOpenData creatureOpenData = value.GetCreatureOpenData();
		if (creatureOpenData == null)
		{
			return false;
		}
		Dictionary<AccessoryPartType, CustomizeStock>.ValueCollection valueCollection = creatureOpenData.GetStock().CustomizeStockValues();
		List<CustomizeStock> list = ((valueCollection == null) ? new List<CustomizeStock>() : new List<CustomizeStock>(valueCollection));
		CustomizeStock[] array;
		foreach (CustomizeStock item in list)
		{
			bool flag = false;
			array = desired;
			for (int i = 0; i < array.Length; i++)
			{
				CustomizeStock stock = array[i];
				if (!stock.IsEmptyPart() && item.IsEqualItem(stock))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				load.ChangeCustomizeStock(CustomizeStock.CreateEmptyPart((AccessoryPartType)(-item.AccessoryIndex)));
			}
		}
		array = desired;
		for (int i = 0; i < array.Length; i++)
		{
			CustomizeStock customizeStock = array[i];
			if (customizeStock.IsEmptyPart())
			{
				continue;
			}
			bool flag2 = false;
			foreach (CustomizeStock item2 in list)
			{
				if (item2.IsEqualItem(customizeStock) && item2.IsEqualTransform(customizeStock))
				{
					flag2 = true;
				}
			}
			if (!flag2 || value.GetCutomizeItemInstance(customizeStock) == null)
			{
				load.ChangeCustomizeStock(customizeStock);
			}
		}
		array = desired;
		for (int i = 0; i < array.Length; i++)
		{
			CustomizeStock stock2 = array[i];
			if (!stock2.IsEmptyPart() && value.GetCutomizeItemInstance(stock2) == null)
			{
				return false;
			}
		}
		return true;
	}

	public static void ApplyResult(HeroCostumeResultInfo result)
	{
		if (result != null && GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(result.HeroIndex);
			if (cachedHeroInfo != null)
			{
				Copy(cachedHeroInfo, result);
			}
		}
	}

	public static CostumeState State(HeroInfo hero, AccessoryCostumeData data, bool checkHeroOwn)
	{
		if (hero == null || data == null)
		{
			return CostumeState.None;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (checkHeroOwn && instance.HeroManager.Get(hero.HeroIndex) == null)
		{
			return CostumeState.NeedHero;
		}
		if (Contains(hero, data.Index))
		{
			return CostumeState.Equipped;
		}
		if (instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(hero.HeroIndex, data.Index)) != null)
		{
			return CostumeState.Owned;
		}
		if (!data.IsBuy)
		{
			return CostumeState.CannotBuy;
		}
		return CostumeState.Buyable;
	}

	public static CollectResult Collect(ref CustomizingCostumesStock stock, HeroCustomizingDataGetter getter)
	{
		CollectResult result = default(CollectResult);
		CachedHeroInfo cachedHeroInfo = getter.HeroInfoGetter();
		List<int> list = new List<int>();
		if (getter.AccessoryCostumeOpenDataGetterList != null)
		{
			foreach (Func<AccessoryCostumeComponent.OpenData> accessoryCostumeOpenDataGetter in getter.AccessoryCostumeOpenDataGetterList)
			{
				AccessoryCostumeComponent.OpenData openData = accessoryCostumeOpenDataGetter();
				if (!openData.IsValid())
				{
					continue;
				}
				if (openData.ComponentType == AccessoryCostumeComponent.ComponentType.SelectUnset)
				{
					foreach (SprkAccessoryEntry item in Entries(cachedHeroInfo))
					{
						if (item.Part == openData.AccessoryPartType && !list.Contains(item.Index))
						{
							list.Add(item.Index);
						}
					}
					continue;
				}
				AccessoryCostumeData accessoryCostumeData = openData.AccessoryCostumeData;
				if (accessoryCostumeData == null)
				{
					continue;
				}
				CostumeState costumeState = State(cachedHeroInfo, accessoryCostumeData, checkHeroOwn: true);
				if (costumeState == CostumeState.Owned || costumeState == CostumeState.Equipped || costumeState == CostumeState.Buyable)
				{
					PositionInfo positionInfo = ((getter.AccessoryPositionInfoGetter == null) ? Position(cachedHeroInfo, accessoryCostumeData.Index) : getter.AccessoryPositionInfoGetter(accessoryCostumeData));
					if (stock.AccessoryPositionPairs == null)
					{
						stock.AccessoryPositionPairs = new Dictionary<int, PositionInfo>();
					}
					if (!stock.AccessoryPositionPairs.ContainsKey(accessoryCostumeData.Index) && costumeState == CostumeState.Buyable)
					{
						Tuple<int, int> tuple = CustomizingHelper.AccessoryCostumeCalculrater(accessoryCostumeData);
						stock.BuyReqGem += tuple.Item1;
						stock.Mileage += tuple.Item2;
					}
					stock.AccessoryPositionPairs[accessoryCostumeData.Index] = positionInfo ?? PositionInfo.Create();
				}
			}
		}
		if (stock.AccessoryPositionPairs != null && stock.AccessoryPositionPairs.Count > 0)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (KeyValuePair<int, PositionInfo> accessoryPositionPair in stock.AccessoryPositionPairs)
			{
				dictionary[accessoryPositionPair.Key.ToString()] = JsonMarshaler.EncodeJsonObject(accessoryPositionPair.Value);
			}
			stock.AccessoryCostumePositionInfo = Json.Serialize(dictionary);
			result.CostumeNeedUpdate = true;
		}
		if (list.Count > 0)
		{
			stock.UnsetAccessoryCostumeIndices = list.ToArray();
			result.CostumeNeedUpdate = true;
		}
		return result;
	}

	public static void Preview(AccessoryCostumeViewWidget widget)
	{
		IRenderFacadeController renderFacadeController = (IRenderFacadeController)typeof(WidgetBase).GetField("_renderFacadeController", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(widget);
		if (renderFacadeController == null || widget.HeroInfo == null)
		{
			return;
		}
		CachedHeroInfo localHeroInfo = renderFacadeController.GetLocalHeroInfo();
		List<SprkAccessoryEntry> list = Entries(localHeroInfo);
		AccessoryCostumeData currentComponentAccesoryCostumeData = widget.CurrentComponentAccesoryCostumeData;
		AccessoryPartType part = widget.GetUnsetOpenData().AccessoryPartType;
		list.RemoveAll((SprkAccessoryEntry e) => e.Part == part);
		if (currentComponentAccesoryCostumeData != null)
		{
			foreach (SprkAccessoryEntry item in Entries(widget.HeroInfo))
			{
				if (item.Part == part)
				{
					list.Add(item);
				}
			}
		}
		if (currentComponentAccesoryCostumeData != null)
		{
			Put(list, currentComponentAccesoryCostumeData.Index, currentComponentAccesoryCostumeData.PartType, localHeroInfo.GetAccessoryPositionInfo(currentComponentAccesoryCostumeData.PartType));
		}
		else
		{
			list.RemoveAll((SprkAccessoryEntry e) => e.Part == widget.GetUnsetOpenData().AccessoryPartType);
		}
		Store(localHeroInfo, list);
		renderFacadeController.ChangeRenderFacadeOnly(localHeroInfo);
	}

	public static void EditPosition(AccessoryCostumeViewWidget widget, PositionInfo position)
	{
		IRenderFacadeController renderFacadeController = (IRenderFacadeController)typeof(WidgetBase).GetField("_renderFacadeController", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(widget);
		AccessoryCostumeData currentComponentAccesoryCostumeData = widget.CurrentComponentAccesoryCostumeData;
		if (renderFacadeController != null && currentComponentAccesoryCostumeData != null)
		{
			CachedHeroInfo localHeroInfo = renderFacadeController.GetLocalHeroInfo();
			List<SprkAccessoryEntry> list = Entries(localHeroInfo);
			Put(list, currentComponentAccesoryCostumeData.Index, currentComponentAccesoryCostumeData.PartType, position);
			Store(localHeroInfo, list);
		}
	}

	public static void ShowEquipped(AccessoryCostumeViewWidget widget)
	{
		IRenderFacadeController renderFacadeController = (IRenderFacadeController)typeof(WidgetBase).GetField("_renderFacadeController", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(widget);
		if (renderFacadeController == null || widget.HeroInfo == null)
		{
			return;
		}
		CachedHeroInfo localHeroInfo = renderFacadeController.GetLocalHeroInfo();
		List<SprkAccessoryEntry> list = Entries(localHeroInfo);
		AccessoryPartType part = widget.GetUnsetOpenData().AccessoryPartType;
		list.RemoveAll((SprkAccessoryEntry e) => e.Part == part);
		foreach (SprkAccessoryEntry item in Entries(widget.HeroInfo))
		{
			if (item.Part == part)
			{
				list.Add(item);
			}
		}
		Store(localHeroInfo, list);
		renderFacadeController.ChangeRenderFacadeOnly(localHeroInfo);
	}

	public static HeroCustomizingDataGetter Getter(HeroCustomizingDataGetter getter, AccessoryCostumeViewWidget widget)
	{
		Func<AccessoryCostumeData, PositionInfo> original = getter.AccessoryPositionInfoGetter;
		getter.AccessoryPositionInfoGetter = delegate(AccessoryCostumeData data)
		{
			if (((Dictionary<int, PositionInfo>)typeof(AccessoryCostumeViewWidget).GetField("_cachedAccesssoryIndexPositionInfoPair", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(widget)).TryGetValue(data.Index, out var value))
			{
				return value.Clone();
			}
			if (Contains(widget.HeroInfo, data.Index))
			{
				return Position(widget.HeroInfo, data.Index);
			}
			return (original != null) ? original(data) : PositionInfo.Create();
		};
		return getter;
	}

	public static void UpdateButtons(AccessoryCostumeViewWidget widget)
	{
		HeroCostumeView componentInParent = widget.GetComponentInParent<HeroCostumeView>();
		if (componentInParent == null)
		{
			return;
		}
		SprkAccessoryControls sprkAccessoryControls = componentInParent.GetComponent<SprkAccessoryControls>();
		if (sprkAccessoryControls == null)
		{
			sprkAccessoryControls = componentInParent.gameObject.AddComponent<SprkAccessoryControls>();
		}
		sprkAccessoryControls.View = componentInParent;
		sprkAccessoryControls.Current = widget;
		HeroCostumeView.CostumeButtonInfo[] array = (HeroCostumeView.CostumeButtonInfo[])typeof(WidgetBase).GetProperty("ButtonInfos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(widget, null);
		foreach (HeroCostumeView.CostumeButtonInfo costumeButtonInfo in array)
		{
			if (costumeButtonInfo.Button != null && costumeButtonInfo.Button.gameObject.activeSelf && (costumeButtonInfo.State == CostumeState.Equipped || costumeButtonInfo.State == CostumeState.Owned))
			{
				sprkAccessoryControls.ApplyLabels(costumeButtonInfo.Button);
				costumeButtonInfo.Button.isEnabled = true;
			}
		}
		sprkAccessoryControls.Refresh();
	}

	public static void AfterResponse()
	{
		AccessoryCostumeViewWidget[] array = UnityEngine.Object.FindObjectsOfType<AccessoryCostumeViewWidget>();
		foreach (AccessoryCostumeViewWidget accessoryCostumeViewWidget in array)
		{
			if (accessoryCostumeViewWidget.gameObject.activeInHierarchy)
			{
				accessoryCostumeViewWidget.UpdateCostumeListState();
				accessoryCostumeViewWidget.UpdateButtonState();
				Preview(accessoryCostumeViewWidget);
			}
		}
	}
}
