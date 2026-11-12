using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.NUI.NWindow;
using NShared;
using NShared.UsePotionItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserItemManager : NShared.UserItemManager
{
	public class ItemCountChangedInfo
	{
	}

	public const string KEY_NEW_ITEM_INDEX = "NewItemIndex_";

	public const int VALUE_INVALID = -1;

	private readonly UserObserverManager<ItemInfo> _observerManager;

	private readonly UserObserverManager<ItemCountChangedInfo> _countChangeObserverManager;

	private HashSet<int> _newItemIndiceSet = new HashSet<int>();

	private Dictionary<int, int> _newPetSoulInfoDic = new Dictionary<int, int>();

	private LocalAccountString _newPetSoulLocalInfos = new LocalAccountString("", "NewPetSoulLocalInfos");

	public IUserObserverManager ObserverManager => _observerManager;

	public IUserObserverManager ItemCountObserverManager => _countChangeObserverManager;

	public UserItemManager()
	{
		_observerManager = new UserObserverManager<ItemInfo>();
		_countChangeObserverManager = new UserObserverManager<ItemCountChangedInfo>();
		InitNewPetSoulInfoDic();
	}

	public void Apply(ItemResultInfo itemResultInfo)
	{
		if (itemResultInfo == null)
		{
			return;
		}
		ItemInfo itemInfo = Get(itemResultInfo.ItemIndex);
		if (itemInfo == null || itemInfo.Count != itemResultInfo.NewCount || itemInfo.Locked != itemResultInfo.Locked)
		{
			_ = itemInfo?.Count;
			if (itemInfo == null)
			{
				itemInfo = new ItemInfo
				{
					ItemIndex = itemResultInfo.ItemIndex,
					Count = itemResultInfo.NewCount,
					Locked = itemResultInfo.Locked
				};
				Add(itemInfo);
			}
			itemInfo.Count = itemResultInfo.NewCount;
			itemInfo.Locked = itemResultInfo.Locked;
			SetInfoByItemType(itemResultInfo, itemInfo);
			AnnounceToObservers(itemInfo);
		}
	}

	private void SetInfoByItemType(ItemResultInfo itemResultInfo, ItemInfo itemInfo)
	{
		if (itemResultInfo == null || itemInfo == null)
		{
			return;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
		if (data == null)
		{
			return;
		}
		switch (data.Type)
		{
		case ItemType.None:
			return;
		case ItemType.PetSoul:
			AddNewPetSoulInfo(itemInfo, itemResultInfo.AddCount);
			return;
		}
		AddNewItemIndice(itemInfo, itemResultInfo.AddCount);
		if (data.Type != ItemType.Potion)
		{
			return;
		}
		ItemData<PotionItemData> potionData = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(data.Code);
		if (potionData == null || potionData.SubData == null || !potionData.SubData.AutoUse || itemResultInfo.AddCount < 0)
		{
			return;
		}
		NShared.UsePotionItem.Request request = new NShared.UsePotionItem.Request();
		request.ItemIndex = data.Index;
		request.ItemCount = 1;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePotionItem.Response response)
		{
			if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				if (potionData.SubData.ActionType == ItemActionType.CompleteArchive)
				{
					GlobalAutoBehaviour<AccountManager>.instance.ArchiveManager.Apply(response.ReservedPlayerArchiveInfos);
				}
				else if (potionData.SubData.ActionType == ItemActionType.CompleteAchievement)
				{
					GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Apply(response.AchievementInfos);
				}
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
			}
		});
	}

	private void AddNewItemIndice(ItemInfo itemInfo, int addCount)
	{
		if (!_newItemIndiceSet.Contains(itemInfo.ItemIndex))
		{
			if (addCount > 0)
			{
				_newItemIndiceSet.Add(itemInfo.ItemIndex);
				LocalAccountInt localAccountInt = new LocalAccountInt(-1, "NewItemIndex_" + itemInfo.ItemIndex);
				int itemIndex = itemInfo.ItemIndex;
				localAccountInt.Value = itemIndex;
			}
		}
		else if (itemInfo.Count == 0)
		{
			_newItemIndiceSet.Remove(itemInfo.ItemIndex);
			new LocalAccountInt(-1, "NewItemIndex_" + itemInfo.ItemIndex).Remove();
		}
	}

	public void AnnounceToObservers(ItemInfo itemInfo)
	{
		_observerManager.AnnounceToObservers(itemInfo);
	}

	public void Apply(ItemResultInfo[] itemResultInfos)
	{
		if (itemResultInfos != null)
		{
			foreach (ItemResultInfo itemResultInfo in itemResultInfos)
			{
				Apply(itemResultInfo);
			}
		}
	}

	public void SetLocked(int ItemIndex, byte locked)
	{
		ItemInfo itemInfo = Get(ItemIndex);
		if (itemInfo != null)
		{
			itemInfo.Locked = locked;
			AnnounceToObservers(itemInfo);
		}
	}

	public List<ItemInfo> SelectItemList(Func<IItemInfo, bool> selector)
	{
		List<ItemInfo> list = new List<ItemInfo>();
		using IEnumerator<ItemInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			ItemInfo current = enumerator.Current;
			if (current != null && current.Count > 0 && selector(current))
			{
				list.Add(current);
			}
		}
		return list;
	}

	public List<ItemInfo> GetItemsByType(ItemType type)
	{
		return SelectItemList(delegate(IItemInfo e)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(e.ItemIndex);
			if (data == null)
			{
				VespaDebug.LogError("ItemData not found: " + e.ItemIndex);
				return false;
			}
			return data.Type == type;
		});
	}

	public List<ItemInfo> GetItemsByTypes(ItemType[] types)
	{
		return SelectItemList(delegate(IItemInfo e)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(e.ItemIndex);
			return types.Contains(data.Type);
		});
	}

	public bool AddedItemExists(ItemType type)
	{
		List<ItemInfo> itemsByType = GetItemsByType(type);
		for (int i = 0; i < itemsByType.Count; i++)
		{
			if (AddedItemExists(itemsByType[i].ItemIndex))
			{
				return true;
			}
		}
		return false;
	}

	public bool AddedItemExists(ItemType[] types)
	{
		List<ItemInfo> itemsByTypes = GetItemsByTypes(types);
		for (int i = 0; i < itemsByTypes.Count; i++)
		{
			if (AddedItemExists(itemsByTypes[i].ItemIndex))
			{
				return true;
			}
		}
		return false;
	}

	public int GetTotalAddedItemCount()
	{
		return _newItemIndiceSet.Count;
	}

	public bool AddedItemExists(int ItemIndex)
	{
		return _newItemIndiceSet.Contains(ItemIndex);
	}

	public void RemoveAddedItemIndex(ItemType[] types)
	{
		List<ItemInfo> itemsByTypes = GetItemsByTypes(types);
		bool changedPetSoulInfo = false;
		itemsByTypes.ForEach(delegate(ItemInfo info)
		{
			_newItemIndiceSet.Remove(info.ItemIndex);
			new LocalAccountInt(-1, "NewItemIndex_" + info.ItemIndex).Remove();
			if (RemoveNewPetSoulInfo(info.ItemIndex) && !changedPetSoulInfo)
			{
				changedPetSoulInfo = true;
			}
		});
		if (changedPetSoulInfo)
		{
			SaveNewPetSoulInfos();
		}
		_countChangeObserverManager.AnnounceToObservers(new ItemCountChangedInfo());
	}

	public void RemoveAddedItemIndex(int itemIndex)
	{
		_newItemIndiceSet.Remove(itemIndex);
		new LocalAccountInt(-1, "NewItemIndex_" + itemIndex).Remove();
		if (RemoveNewPetSoulInfo(itemIndex))
		{
			SaveNewPetSoulInfos();
		}
		_countChangeObserverManager.AnnounceToObservers(new ItemCountChangedInfo());
	}

	public void RemoveAddedItemIndex(ItemType type)
	{
		List<ItemInfo> itemsByType = GetItemsByType(type);
		bool changedPetSoulInfo = false;
		itemsByType.ForEach(delegate(ItemInfo info)
		{
			_newItemIndiceSet.Remove(info.ItemIndex);
			new LocalAccountInt(-1, "NewItemIndex_" + info.ItemIndex).Remove();
			if (RemoveNewPetSoulInfo(info.ItemIndex) && !changedPetSoulInfo)
			{
				changedPetSoulInfo = true;
			}
		});
		if (changedPetSoulInfo)
		{
			SaveNewPetSoulInfos();
		}
		_countChangeObserverManager.AnnounceToObservers(new ItemCountChangedInfo());
	}

	public bool IsNew(int itemIndex)
	{
		LocalAccountInt localAccountInt = new LocalAccountInt(-1, "NewItemIndex_" + itemIndex);
		if (localAccountInt.Value == -1)
		{
			return false;
		}
		if (itemIndex == localAccountInt.Value)
		{
			return true;
		}
		return false;
	}

	public override void Set(ItemInfo[] values)
	{
		if (values == null)
		{
			return;
		}
		foreach (ItemInfo itemInfo in values)
		{
			Add(itemInfo);
			if (itemInfo != null && IsNew(itemInfo.ItemIndex))
			{
				_newItemIndiceSet.Add(itemInfo.ItemIndex);
			}
		}
	}

	public void Apply(PetAddResultInfo[] petResultInfos)
	{
		if (petResultInfos != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petResultInfos); i++)
			{
				PetAddResultInfo petAddResultInfo = petResultInfos[i];
				Apply(petAddResultInfo);
			}
		}
	}

	public void Apply(PetAddResultInfo petAddResultInfo)
	{
		if (petAddResultInfo != null)
		{
			Apply(petAddResultInfo.PetSoulResults);
		}
	}

	public bool IsNewPetSoul(int itemIndex)
	{
		if (_newPetSoulInfoDic.ContainsKey(itemIndex))
		{
			return true;
		}
		return false;
	}

	public bool IsExistsNewPetSoul()
	{
		bool result = false;
		foreach (KeyValuePair<int, int> item in _newPetSoulInfoDic)
		{
			if (item.Value > 0)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public int GetNewPetSoulCount(int petSoulIndex)
	{
		if (!_newPetSoulInfoDic.ContainsKey(petSoulIndex))
		{
			return 0;
		}
		return _newPetSoulInfoDic[petSoulIndex];
	}

	private void AddNewPetSoulInfo(ItemInfo itemInfo, int addCount)
	{
		if (!_newPetSoulInfoDic.ContainsKey(itemInfo.ItemIndex))
		{
			if (addCount > 0)
			{
				_newPetSoulInfoDic.Add(itemInfo.ItemIndex, addCount);
				SaveNewPetSoulInfos();
			}
		}
		else if (itemInfo.Count == 0 && RemoveNewPetSoulInfo(itemInfo.ItemIndex))
		{
			SaveNewPetSoulInfos();
		}
	}

	public bool RemoveNewPetSoulInfo(int index)
	{
		if (_newPetSoulInfoDic.ContainsKey(index))
		{
			_newPetSoulInfoDic.Remove(index);
			return true;
		}
		return false;
	}

	public void RemoveAllNewPetSoulInfo()
	{
		_newPetSoulInfoDic.Clear();
		_newPetSoulLocalInfos.Value = string.Empty;
	}

	private void SaveNewPetSoulInfos()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<int, int> item in _newPetSoulInfoDic)
		{
			if (stringBuilder.Length == 0)
			{
				stringBuilder.AppendFormat("{0}_{1}", item.Key, item.Value);
			}
			else
			{
				stringBuilder.AppendFormat(",{0}_{1}", item.Key, item.Value);
			}
		}
		_newPetSoulLocalInfos.Value = stringBuilder.ToString();
	}

	private void InitNewPetSoulInfoDic()
	{
		_newPetSoulInfoDic.Clear();
		string value = _newPetSoulLocalInfos.Value;
		if (String_Extension_Method.IsNullOrEmpty(value))
		{
			return;
		}
		string[] array = value.Split(',');
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			string text = array[i];
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				continue;
			}
			string[] array2 = text.Split('_');
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(array2))
			{
				int valueInt = ArgParser.GetValueInt(array2, 0);
				int valueInt2 = ArgParser.GetValueInt(array2, 1);
				if (valueInt != 0)
				{
					_newPetSoulInfoDic.Add(valueInt, valueInt2);
				}
			}
		}
	}
}
