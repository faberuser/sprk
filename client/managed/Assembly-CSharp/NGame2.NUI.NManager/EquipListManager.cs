using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager;

[Serializable]
public class EquipListManager : ItemIconComponent.Listener, ItemIconComponent.TooltipListener
{
	public interface Listener
	{
		void OnClickItemIcon(ItemIconComponent This);
	}

	private class ItemIconContainer
	{
		private List<ItemIconComponent> _list = new List<ItemIconComponent>();

		private Dictionary<Transform, ItemIconComponent> _transformDic = new Dictionary<Transform, ItemIconComponent>();

		public int Count => _list.Count;

		public void Add(ItemIconComponent itemIcon)
		{
			if (!(itemIcon == null))
			{
				_list.Add(itemIcon);
				_transformDic.Add(itemIcon.transform, itemIcon);
			}
		}

		public ItemIconComponent GetByIndex(int index)
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_list, index))
			{
				return null;
			}
			return _list[index];
		}

		public ItemIconComponent GetByEquipSlot(int equipSlotIndex)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
			{
				ItemIconComponent itemIconComponent = _list[i];
				if (itemIconComponent.EquipItemInfo != null && itemIconComponent.EquipItemInfo.SlotIndex == equipSlotIndex)
				{
					return itemIconComponent;
				}
			}
			return null;
		}

		public List<ItemIconComponent> GetItemCompList()
		{
			return _list;
		}

		public ItemIconComponent GetByTransform(Transform tm)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_transformDic, tm, null);
		}

		public ItemIconComponent GetByUid(string uid)
		{
			int i = 0;
			for (int num = __Extension_Method_ICollection__.COUNT(_list); i < num; i++)
			{
				ItemIconComponent itemIconComponent = _list[i];
				if (!(itemIconComponent == null) && itemIconComponent.EquipItemInfo != null && itemIconComponent.EquipItemInfo.Uid == uid)
				{
					return itemIconComponent;
				}
			}
			return null;
		}

		public void ReleaseAt(int index)
		{
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_list, index))
			{
				ItemIconComponent itemIconComponent = _list[index];
				_list.RemoveAt(index);
				_transformDic.Remove(itemIconComponent.transform);
				FactoryBase.ReleaseComponent(itemIconComponent);
			}
		}

		public void ReleaseAndClear()
		{
			FactoryBase.ReleaseAndClearList(_list);
			_transformDic.Clear();
		}
	}

	private Listener _listener;

	private ItemIconContainer _itemIconContainer = new ItemIconContainer();

	private List<EmptyItemIconComponent> _emptyItemIconComponentList = new List<EmptyItemIconComponent>();

	private string _selectedCompUid;

	private bool _useSelectedBorder;

	public int MinItemSlotCount;

	public UIScrollGrid EquipIconGrid;

	public EquipmentItemFilterManager _equipmentItemFilterManager;

	public EquipmentItemFilterManager _chestItemFilterManager;

	private int _totalItemCount;

	private int _totalItemSlotCount;

	private int _totalInventoryItemCount;

	private int _totalChestItemCount;

	private int _totalPunishmentRuneCount;

	public void Init(Listener listener)
	{
		_listener = listener;
	}

	public void InitEquipmentItemFilterManager(ref EquipmentItemFilterManager equipmentItemFilterManager)
	{
		_equipmentItemFilterManager = equipmentItemFilterManager;
	}

	public void InitChestItemFilterManager(ref EquipmentItemFilterManager chestItemFilterManager)
	{
		_chestItemFilterManager = chestItemFilterManager;
	}

	public bool UpdateUserChanged(NGame2.NAccount.UserSoulWeaponManager.SoulWeaponChangedInfo changeInfo)
	{
		if (changeInfo == null)
		{
			return false;
		}
		int slotIndex = changeInfo.SlotIndex;
		ItemIconComponent equipIconComponent = GetEquipIconComponent(slotIndex);
		if (equipIconComponent == null)
		{
			return false;
		}
		equipIconComponent.Refresh();
		return false;
	}

	public bool UpdateUserChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo equipStatInfo, Func<IItemInfo, bool> selector)
	{
		if (equipStatInfo == null)
		{
			return false;
		}
		int slotIndex = equipStatInfo.SlotIndex;
		EquipItemInfo equipItemInfo = equipStatInfo.EquipItemInfo;
		ItemIconComponent equipIconComponent = GetEquipIconComponent(slotIndex);
		if (equipItemInfo == null)
		{
			return true;
		}
		if (equipStatInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.EquippedHeroIndex)
		{
			return true;
		}
		if (equipIconComponent == null)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(equipStatInfo.SlotIndex) != null)
			{
				return false;
			}
			if (EquipIconCreatable(equipItemInfo, selector))
			{
				return true;
			}
			return false;
		}
		equipIconComponent.Refresh();
		return false;
	}

	public bool UpdateUserChanged(ItemInfo itemInfo, Func<IItemInfo, bool> selector)
	{
		if (itemInfo == null)
		{
			return false;
		}
		if (!selector(itemInfo))
		{
			return false;
		}
		return true;
	}

	public void SetSelected(string uid)
	{
		ItemIconComponent itemIconComponent = null;
		if (!String_Extension_Method.IsNullOrEmpty(_selectedCompUid))
		{
			itemIconComponent = GetItemIconComponent(_selectedCompUid);
			if (itemIconComponent != null)
			{
				itemIconComponent.SetSelected(selected: false, SelectSubComponent.ESelectType.Select_BorderEquip);
			}
		}
		_selectedCompUid = uid;
		if (!String_Extension_Method.IsNullOrEmpty(_selectedCompUid) && _useSelectedBorder)
		{
			itemIconComponent = GetItemIconComponent(_selectedCompUid);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.SetSelected(selected: true, SelectSubComponent.ESelectType.Select_BorderEquip);
			}
		}
	}

	public void SetUseSelectedBoarder(bool use)
	{
		_useSelectedBorder = use;
		if (!String_Extension_Method.IsNullOrEmpty(_selectedCompUid))
		{
			ItemIconComponent itemIconComponent = GetItemIconComponent(_selectedCompUid);
			if (itemIconComponent != null)
			{
				itemIconComponent.SetSelected(_useSelectedBorder, SelectSubComponent.ESelectType.Select_BorderEquip);
			}
		}
	}

	public int GetItemCount()
	{
		return _itemIconContainer.Count;
	}

	public ItemIconComponent GetItemIconComponent(Transform tm)
	{
		if (_itemIconContainer == null)
		{
			return null;
		}
		return _itemIconContainer.GetByTransform(tm);
	}

	public ItemIconComponent GetItemIconComponent(string uid)
	{
		if (_itemIconContainer == null)
		{
			return null;
		}
		return _itemIconContainer.GetByUid(uid);
	}

	public List<ItemIconComponent> GetitemIconContainer()
	{
		if (_itemIconContainer == null)
		{
			return null;
		}
		return _itemIconContainer.GetItemCompList();
	}

	public int GetAllItemCount()
	{
		return _totalItemCount;
	}

	public int GetAllSlotItemCount()
	{
		return _totalItemSlotCount;
	}

	public int GetInventoryItemCount()
	{
		return _totalInventoryItemCount;
	}

	public int GetChestItemCount()
	{
		return _totalChestItemCount;
	}

	public ItemIconComponent GetItemCompByTransform(Transform tr)
	{
		if (_itemIconContainer == null)
		{
			return null;
		}
		return _itemIconContainer.GetByTransform(tr);
	}

	public void UpdateAllItem(Action<ItemIconComponent> action)
	{
		if (action == null)
		{
			return;
		}
		for (int i = 0; i < _itemIconContainer.Count; i++)
		{
			ItemIconComponent byIndex = _itemIconContainer.GetByIndex(i);
			Vespa_System_Action_Extension_Method.fire(action, byIndex);
			if (!(byIndex == null) && byIndex.EquipItemInfo != null)
			{
				byIndex.SetSelected(_selectedCompUid == byIndex.EquipItemInfo.Uid, SelectSubComponent.ESelectType.Select_BorderEquip);
			}
		}
	}

	public void ClearItemList()
	{
		_itemIconContainer.ReleaseAndClear();
		FactoryBase.ReleaseAndClearList(_emptyItemIconComponentList);
	}

	public void InitEquipList(Func<IItemInfo, bool> selector, Action cleaner, bool bInitMinSlot = false, InventoryType inventoryType = InventoryType.Inventory)
	{
		ClearItemList();
		List<EquipItemInfo> list = null;
		list = GlobalAutoBehaviour<AccountManager>.instance.GetOwnedAllEquipItemList();
		_totalItemCount = 0;
		_totalInventoryItemCount = 0;
		_totalChestItemCount = 0;
		_totalPunishmentRuneCount = 0;
		foreach (EquipItemInfo item in list)
		{
			if (item == null || CheckExceptEquipItemType(item.ItemData))
			{
				continue;
			}
			if (item.GetInventoryType() == InventoryType.Inventory)
			{
				if (!item.IsEquipped)
				{
					_totalInventoryItemCount++;
				}
			}
			else if (item.GetInventoryType() == InventoryType.PunishmentRune)
			{
				if (!item.IsEquipped)
				{
					_totalPunishmentRuneCount++;
				}
			}
			else if (!item.IsEquipped)
			{
				_totalChestItemCount++;
			}
			_totalItemCount++;
		}
		list.RemoveAll((EquipItemInfo e) => !selector(e));
		int num = 0;
		foreach (EquipItemInfo item2 in list)
		{
			int itemIndex = item2.ItemIndex;
			if (GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemIndex) == null)
			{
				VespaDebug.LogError("Item data not found : index=" + itemIndex);
				continue;
			}
			if (item2.GetInventoryType() == InventoryType.Inventory && _equipmentItemFilterManager != null && _equipmentItemFilterManager.IsFiltered(item2))
			{
				CreateEquipIconComponent(item2);
				num++;
			}
			if (item2.GetInventoryType() == InventoryType.PunishmentRune)
			{
				CreateEquipIconComponent(item2);
				num++;
			}
			if (item2.GetInventoryType() == InventoryType.Chest && _chestItemFilterManager != null && _chestItemFilterManager.IsFiltered(item2))
			{
				CreateEquipIconComponent(item2);
				num++;
			}
		}
		_totalItemSlotCount = num;
		int num2 = 0;
		int num3 = 0;
		if (bInitMinSlot)
		{
			int num4 = ((EquipIconGrid != null && EquipIconGrid.Grid != null) ? EquipIconGrid.Grid.maxPerLine : 4);
			num2 = num / num4 * num4 + num4;
			num2 = Mathf.Max(num2, MinItemSlotCount);
			num3 = num2 - num;
		}
		else
		{
			if (inventoryType == InventoryType.Inventory)
			{
				num2 = GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount();
			}
			if (inventoryType == InventoryType.PunishmentRune)
			{
				num2 = GlobalAutoBehaviour<AccountManager>.instance.GetMaxPunishmentRuneItemCount();
			}
			if (inventoryType == InventoryType.Chest)
			{
				num2 = GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount();
			}
		}
		num3 = num2 - num;
		for (int num5 = 0; num5 < num3; num5++)
		{
			CreateEquipIconComponent(null);
		}
		Vespa_System_Action_Extension_Method.fire(cleaner);
	}

	public void InitItemList(Func<IItemInfo, bool> selector, Action cleaner)
	{
		ClearItemList();
		List<ItemInfo> list = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.SelectItemList(selector);
		int count = list.Count;
		foreach (ItemInfo item in list)
		{
			if (item != null)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
				CreateItemIconComponent(data, item.Count);
			}
			else
			{
				CreateItemIconComponent(null);
			}
		}
		AddEventItemInfoList(selector);
		int num = ((EquipIconGrid != null && EquipIconGrid.Grid != null) ? EquipIconGrid.Grid.maxPerLine : 4);
		int num2 = Mathf.Max(count / num * num + num, MinItemSlotCount) - count;
		for (int i = 0; i < num2; i++)
		{
			CreateItemIconComponent(null);
		}
		cleaner?.Invoke();
	}

	private void AddEventItemInfoList(Func<IItemInfo, bool> selector)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		NGame2.NAccount.UserEquipItemManager equipItemManager = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager;
		if (equipItemManager == null)
		{
			return;
		}
		foreach (EquipItemInfo item in equipItemManager)
		{
			if (item != null && item.ItemData != null && item.ItemData.Type == ItemType.EventEnchantWeapon && selector(item))
			{
				CreateItemIconComponent(item.ItemData);
			}
		}
	}

	public bool CheckHaveEventItem(Func<IItemInfo, bool> selector)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		NGame2.NAccount.UserEquipItemManager equipItemManager = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager;
		if (equipItemManager == null)
		{
			return false;
		}
		foreach (EquipItemInfo item in equipItemManager)
		{
			if (item != null && item.ItemData != null && CheckExceptEquipItemType(item.ItemData) && selector(item))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckExceptEquipItemType(ItemData itemData)
	{
		if (itemData == null)
		{
			return true;
		}
		if (itemData.Type == ItemType.EventEnchantWeapon)
		{
			return true;
		}
		return false;
	}

	private bool EquipIconCreatable(EquipItemInfo itemInfo, Func<IItemInfo, bool> selector = null)
	{
		if (itemInfo == null)
		{
			return false;
		}
		int itemIndex = itemInfo.ItemIndex;
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (selector != null && !selector(itemInfo))
		{
			return false;
		}
		if (GetEquipIconComponent(itemInfo.SlotIndex) != null)
		{
			return false;
		}
		if (data == null)
		{
			VespaDebug.LogError("Item data not found : index=" + itemIndex);
			return false;
		}
		return true;
	}

	private void CreateEquipIconComponent(EquipItemInfo itemInfo, Func<IItemInfo, bool> selector = null)
	{
		if (itemInfo != null)
		{
			int itemIndex = itemInfo.ItemIndex;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (!EquipIconCreatable(itemInfo, selector))
			{
				return;
			}
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(EquipIconGrid, ItemIconComponent.PrefabAuto);
			if (itemIconComponent == null)
			{
				VespaDebug.LogError("Failed to create equip icon : index=" + itemIndex);
				return;
			}
			itemIconComponent.Open(data, itemInfo, this, itemInfo.EquippedHeroInfo);
			itemIconComponent.SetNew(GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.IsNew(itemInfo.Uid));
			itemIconComponent.SetEquipped(isShow: true);
			if (_useSelectedBorder)
			{
				itemIconComponent.SetSelected(_selectedCompUid == itemInfo.Uid, SelectSubComponent.ESelectType.Select_BorderEquip);
			}
			_itemIconContainer.Add(itemIconComponent);
		}
		else
		{
			EmptyItemIconComponent emptyItemIconComponent = UIHelper.AddGridItem(EquipIconGrid, EmptyItemIconComponent.PrefabAuto);
			if (emptyItemIconComponent == null)
			{
				VespaDebug.LogError("Failed to create empty equip icon.");
				return;
			}
			emptyItemIconComponent.Open();
			_emptyItemIconComponentList.Add(emptyItemIconComponent);
		}
	}

	private void CreateItemIconComponent(ItemData itemData, int numItem = 1)
	{
		if (itemData != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(EquipIconGrid, ItemIconComponent.PrefabAuto);
			if (itemIconComponent == null)
			{
				VespaDebug.LogError("Failed to create item icon component");
				return;
			}
			itemIconComponent.Open(itemData, this);
			itemIconComponent.SetItemCount_with_MaxCap(numItem);
			itemIconComponent.SetNew(GlobalAutoBehaviour<AccountManager>.instance.ItemManager.IsNew(itemData.Index));
			if (itemData.AwakenStar > 0)
			{
				itemIconComponent.SetStar(itemData.AwakenStar);
			}
			_itemIconContainer.Add(itemIconComponent);
		}
		else
		{
			EmptyItemIconComponent emptyItemIconComponent = UIHelper.AddGridItem(EquipIconGrid, EmptyItemIconComponent.PrefabAuto);
			if (emptyItemIconComponent == null)
			{
				VespaDebug.LogError("Failed to create empty item icon component");
				return;
			}
			emptyItemIconComponent.Open();
			_emptyItemIconComponentList.Add(emptyItemIconComponent);
		}
	}

	private ItemIconComponent GetEquipIconComponent(int equipSlotIndex)
	{
		return _itemIconContainer.GetByEquipSlot(equipSlotIndex);
	}

	private bool RemoveEquipIconComponent(int equipSlotIndex)
	{
		for (int i = 0; i < _itemIconContainer.Count; i++)
		{
			ItemIconComponent byIndex = _itemIconContainer.GetByIndex(i);
			if (byIndex.EquipItemInfo != null && byIndex.EquipItemInfo.SlotIndex == equipSlotIndex)
			{
				_itemIconContainer.ReleaseAt(i);
				return true;
			}
		}
		return false;
	}

	private int SortItem(Transform lhs, Transform rhs, Comparison<ItemIconComponent> SortFunc)
	{
		ItemIconComponent byTransform = _itemIconContainer.GetByTransform(lhs);
		ItemIconComponent byTransform2 = _itemIconContainer.GetByTransform(rhs);
		if (byTransform == byTransform2)
		{
			return 0;
		}
		if (byTransform == null || byTransform2 == null)
		{
			if (byTransform != null)
			{
				return -1;
			}
			if (byTransform2 != null)
			{
				return 1;
			}
			return 0;
		}
		if (byTransform.ItemData != null && byTransform.ItemData.Tutorial)
		{
			return -1;
		}
		if (byTransform2.ItemData != null && byTransform2.ItemData.Tutorial)
		{
			return 1;
		}
		return SortFunc(byTransform, byTransform2);
	}

	public void SetSortFunc(Comparison<ItemIconComponent> SortFunc)
	{
		EquipIconGrid.Grid.sorting = UIGrid.Sorting.Custom;
		EquipIconGrid.Grid.onCustomSort = (Transform x, Transform y) => SortItem(x, y, SortFunc);
		UIHelper.RepositionGrid(EquipIconGrid);
	}

	public void SetGridSort(UIGrid.IGridSort gridSort)
	{
		EquipIconGrid.Grid.sorting = UIGrid.Sorting.GridSortInterface;
		EquipIconGrid.Grid.gridSort = gridSort;
		UIHelper.RepositionGrid(EquipIconGrid);
	}

	public void SetSortFunc(Comparison<Transform> SortFunc)
	{
		EquipIconGrid.Grid.sorting = UIGrid.Sorting.Custom;
		EquipIconGrid.Grid.onCustomSort = SortFunc;
		UIHelper.RepositionGrid(EquipIconGrid);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent This)
	{
		if (This.ItemData != null && (This.ItemData.Type != ItemType.Equip || This.EquipItemInfo != null) && _listener != null)
		{
			_listener.OnClickItemIcon(This);
		}
	}

	void ItemIconComponent.TooltipListener.OnShowTooltip(ItemIconComponent This)
	{
		if (!(This == null))
		{
			This.SetNew(isNew: false);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.NewIconRemove(This.EquipItemInfo);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.RemoveAddedItemIndex(This.ItemIndex);
		}
	}
}
