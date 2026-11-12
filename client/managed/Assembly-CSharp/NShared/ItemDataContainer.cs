using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ItemDataContainer : IJitDataContainer, IGetData_for_GMS<int, ItemData>, IGetAllData_for_GMS<ItemData>
{
	private ItemBaseDataContainer _baseContainer;

	private Dictionary<ItemType, ISubItemDataContainer> _containerDicForSubItemData = new Dictionary<ItemType, ISubItemDataContainer>();

	private Dictionary<int, ItemData> _dicByIndex = new Dictionary<int, ItemData>();

	private Dictionary<string, ItemData> _dicByCode = new Dictionary<string, ItemData>();

	private Dictionary<ItemType, IList<ItemData>> _dicByItemType = new Dictionary<ItemType, IList<ItemData>>();

	private List<ItemData> _allDatas;

	public bool IsLoaded { get; private set; }

	public IList<int> GetMaxCapIndices()
	{
		return _baseContainer.GetMaxCapIndices();
	}

	void IJitDataContainer.Load()
	{
		_baseContainer = TableRoot.Instance.GetDataContainer<ItemBaseDataContainer>();
		RegisterSubDataContainer<AccessorySelectItemDataContainer>();
		RegisterSubDataContainer<AwakenStoneDataContainer>();
		RegisterSubDataContainer<BoosterItemDataContainer>();
		RegisterSubDataContainer<CinemaItemDataContainer>();
		RegisterSubDataContainer<CostumeSelectItemDataContainer>();
		RegisterSubDataContainer<EquipEnchantOptionItemDataContainer>();
		RegisterSubDataContainer<EquipItemDataContainer>();
		RegisterSubDataContainer<FlaskItemDataContainer>();
		RegisterSubDataContainer<HeroGrowthItemDataContainer>();
		RegisterSubDataContainer<HeroLimitBreakExpItemDataContainer>();
		RegisterSubDataContainer<HeroSelectItemDataContainer>();
		RegisterSubDataContainer<MultipleHeroSelectItemDataContainer>();
		RegisterSubDataContainer<NPCFriendlyPointItemDataContainer>();
		RegisterSubDataContainer<PackageItemDataContainer>();
		RegisterSubDataContainer<PetSelectItemDataContainer>();
		RegisterSubDataContainer<PotionItemDataContainer>();
		RegisterSubDataContainer<RuneItemDataContainer>();
		RegisterSubDataContainer<SelectItemDataContainer>();
		RegisterSubDataContainer<UpgradeBookItemDataContainer>();
		RegisterSubDataContainer<WeaponUniqueSelectItemDataContainer>();
		IsLoaded = true;
	}

	public void RegisterSubDataContainer<T>() where T : class, ISubItemDataContainer, IJitDataContainer, new()
	{
		T dataContainer = TableRoot.Instance.GetDataContainer<T>();
		ItemType[] subTypes = dataContainer.GetSubTypes();
		if (subTypes != null && subTypes.Length != 0)
		{
			for (int i = 0; i < subTypes.Length; i++)
			{
				_containerDicForSubItemData.Add(subTypes[i], dataContainer);
			}
		}
	}

	public ItemData GetData(int index)
	{
		lock (this)
		{
			if (_dicByIndex.TryGetValue(index, out var value))
			{
				return value;
			}
			return RegisterItemData(index);
		}
	}

	public IList<ItemData> GetAllData()
	{
		if (_allDatas != null)
		{
			return _allDatas;
		}
		lock (this)
		{
			if (_allDatas != null)
			{
				return _allDatas;
			}
			_allDatas = new List<ItemData>();
			foreach (ItemBaseData allDatum in _baseContainer.GetAllData())
			{
				_allDatas.Add(GetData(allDatum.Index));
			}
			return _allDatas;
		}
	}

	public void ForEachData(Action<ItemData> action)
	{
		if (action == null)
		{
			return;
		}
		foreach (ItemData allDatum in GetAllData())
		{
			action(allDatum);
		}
	}

	public ItemData GetDataByCode(string code)
	{
		lock (this)
		{
			if (_dicByCode.TryGetValue(code, out var value))
			{
				return value;
			}
			return RegisterItemData(code);
		}
	}

	public ItemData<T> GetData<T>(int itemIndex) where T : class, IItemSubData
	{
		return GetData(itemIndex) as ItemData<T>;
	}

	public ItemData<T> GetData<T>(string itemCode) where T : class, IItemSubData
	{
		return GetDataByCode(itemCode) as ItemData<T>;
	}

	public IList<ItemData> GetItemDataListByType(ItemType type)
	{
		lock (this)
		{
			if (_dicByItemType.TryGetValue(type, out var value))
			{
				return value;
			}
			IList<ItemBaseData> itemDataListByType = _baseContainer.GetItemDataListByType(type);
			if (itemDataListByType.Count == 0)
			{
				value = ReadonlyEmpty<ItemData>.IList;
			}
			else
			{
				value = new List<ItemData>();
				for (int i = 0; i < itemDataListByType.Count; i++)
				{
					ItemBaseData itemBaseData = itemDataListByType[i];
					ItemData data = GetData(itemBaseData.Index);
					if (data != null)
					{
						value.Add(data);
					}
				}
			}
			_dicByItemType.Add(type, value);
			return value;
		}
	}

	public ItemData GetItemDataByTypeAndValue(ItemType itemType, int valueIndex, int value)
	{
		IList<ItemData> itemDataListByType = GetItemDataListByType(itemType);
		for (int i = 0; i < itemDataListByType.Count; i++)
		{
			if (itemDataListByType[i].GetValueInt(valueIndex) == value)
			{
				return itemDataListByType[i];
			}
		}
		return null;
	}

	private ItemData RegisterItemData(int index)
	{
		ItemBaseData data = _baseContainer.GetData(index);
		if (data == null)
		{
			_dicByIndex.Add(index, null);
			return null;
		}
		return RegisterItemData(data);
	}

	private ItemData RegisterItemData(string code)
	{
		ItemBaseData dataByCode = _baseContainer.GetDataByCode(code);
		if (dataByCode == null)
		{
			_dicByCode.Add(code, null);
			return null;
		}
		return RegisterItemData(dataByCode);
	}

	private ItemData RegisterItemData(ItemBaseData itemBase)
	{
		_containerDicForSubItemData.TryGetValue(itemBase.Type, out var value);
		ItemData itemData = null;
		if (value != null)
		{
			itemData = value.CreateItemWithSubItem(itemBase);
		}
		if (itemData == null)
		{
			itemData = new ItemDataWithNullSub(itemBase);
		}
		if (_dicByCode.ContainsKey(itemData.Code))
		{
			_dicByCode.Remove(itemData.Code);
		}
		_dicByIndex.Add(itemData.Index, itemData);
		_dicByCode.Add(itemData.Code, itemData);
		return itemData;
	}
}
