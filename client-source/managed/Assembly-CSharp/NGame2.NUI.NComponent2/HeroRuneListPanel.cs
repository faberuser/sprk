using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroRuneListPanel : MonoBehaviour, EquipListManager.Listener, RuneCategoryButtonComponent.IListener, IUserObserver<ItemInfo>, IUserObserver
{
	public interface IListener
	{
		void OnClick(ItemIconComponent component);

		void UpdateItemIcon(ItemIconComponent component);
	}

	public interface IDataProvider
	{
		List<RuneSlotType> GetActivedRuneSlotTypes();
	}

	public EquipListManager _equipListManager;

	public UIScrollView ScrollView;

	public UIGrid CategoryButtonGrid;

	public UILabel Label_SelectedCategory;

	private EquipmentItemFilterManager _equipmentItemFilterManager = new EquipmentItemFilterManager();

	private List<RuneCategoryButtonComponent> _categoryButtonCompList = new List<RuneCategoryButtonComponent>();

	private bool _isDirty;

	private IListener _listener;

	private IDataProvider _dataProvider;

	private ItemType _selectRuneType = ItemType.Rune;

	public void Open(IListener listener, IDataProvider dataProvider)
	{
		SprkHeroRuneFilter.Open(this);
		_listener = listener;
		_dataProvider = dataProvider;
		_selectRuneType = ItemType.Rune;
		_equipListManager.Init(this);
		_equipListManager.InitEquipmentItemFilterManager(ref _equipmentItemFilterManager);
		_equipmentItemFilterManager.Init(EquipmentItemFilter.Mode.Inventory);
		GenerateCategoryButtonComponent();
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		ResetScroll();
	}

	public void GenerateCategoryButtonComponent()
	{
		if (!(CategoryButtonGrid == null))
		{
			RuneCategoryButtonComponent runeCategoryButtonComponent = RuneCategoryButtonComponent.PrefabAuto.Factory.Create(CategoryButtonGrid.transform);
			runeCategoryButtonComponent.Open(this, ItemType.Rune);
			_categoryButtonCompList.Add(runeCategoryButtonComponent);
			runeCategoryButtonComponent = RuneCategoryButtonComponent.PrefabAuto.Factory.Create(CategoryButtonGrid.transform);
			runeCategoryButtonComponent.Open(this, ItemType.PunishmentRune);
			_categoryButtonCompList.Add(runeCategoryButtonComponent);
			CategoryButtonGrid.Reposition();
		}
	}

	private int SortItem(ItemIconComponent component1, ItemIconComponent component2)
	{
		ItemData itemData = component1.ItemData;
		ItemData itemData2 = component2.ItemData;
		int num = 0;
		if (itemData.Type == ItemType.Rune && itemData2.Type == ItemType.Rune)
		{
			num = itemData2.Grade.CompareTo(itemData.Grade);
			if (num == 0)
			{
				num = component1.ItemIndex.CompareTo(component2.ItemIndex);
			}
		}
		else if (itemData.Type == ItemType.PunishmentRune && itemData2.Type == ItemType.PunishmentRune)
		{
			EquipItemInfo equipItemInfo = component1.EquipItemInfo;
			EquipItemInfo equipItemInfo2 = component2.EquipItemInfo;
			num = component1.ItemIndex.CompareTo(component2.ItemIndex);
			if (num == 0)
			{
				equipItemInfo.CreateDateTime.CompareTo(equipItemInfo2.CreateDateTime);
			}
			if (num == 0)
			{
				if (equipItemInfo == null || !equipItemInfo.IsEquipped)
				{
					num = 1;
				}
				if (equipItemInfo2 == null || !equipItemInfo2.IsEquipped)
				{
					num = -1;
				}
			}
		}
		return num;
	}

	public void ResetScroll()
	{
		if (ScrollView != null)
		{
			ScrollView.ResetPosition();
		}
	}

	public void Reset()
	{
		if (_equipListManager != null)
		{
			_equipListManager.ClearItemList();
		}
	}

	public void Close()
	{
		SprkHeroRuneFilter.Close(this);
		FactoryBase.ReleaseAndClearList(_categoryButtonCompList);
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
		Reset();
	}

	public void Refresh(bool immediate = true)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (!immediate)
		{
			_isDirty = true;
			return;
		}
		switch (_selectRuneType)
		{
		case ItemType.Rune:
			_equipListManager.InitItemList(Selector, null);
			_equipListManager.SetSortFunc(SortItem);
			break;
		case ItemType.PunishmentRune:
		{
			_equipListManager.InitEquipList(Selector, null, bInitMinSlot: false, InventoryType.PunishmentRune);
			ItemGridSort gridSort = new ItemGridSort(_equipListManager, _equipListManager._equipmentItemFilterManager);
			_equipListManager.SetGridSort(gridSort);
			break;
		}
		}
		Reposition();
		switch (_selectRuneType)
		{
		case ItemType.Rune:
			UIHelper.SetLabelText(Label_SelectedCategory, LocalizationManager.GetStringDirect("RUNE_PAGE_SORT_NORMAL"));
			break;
		case ItemType.PunishmentRune:
			UIHelper.SetLabelText(Label_SelectedCategory, LocalizationManager.GetStringDirect("RUNE_PAGE_SORT_PUNISHMENT"));
			break;
		}
		_isDirty = false;
	}

	private void Reposition()
	{
		if (base.gameObject.activeSelf)
		{
			_equipListManager.UpdateAllItem(_listener.UpdateItemIcon);
			UIHelper.RepositionGrid(_equipListManager.EquipIconGrid);
		}
	}

	public void Update()
	{
		if (_isDirty)
		{
			_isDirty = false;
			Refresh();
		}
	}

	private bool Selector(IItemInfo iItemInfo)
	{
		if (!SprkHeroRuneFilter.Matches(this, iItemInfo))
		{
			return false;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(iItemInfo.ItemIndex);
		bool flag = false;
		if (data != null)
		{
			if (data.Type == ItemType.PunishmentRune && iItemInfo is EquipItemInfo { IsEquipped: not false })
			{
				return false;
			}
			flag = data.Type == _selectRuneType;
			RuneSlotType[] runeSlotTypes = RuneHelper.GetRuneSlotTypes(data);
			if (__Extension_Method_ICollection__.IsNullOrEmpty(runeSlotTypes))
			{
				flag = false;
			}
			List<RuneSlotType> list = null;
			if (_dataProvider != null)
			{
				list = _dataProvider.GetActivedRuneSlotTypes();
			}
			if (flag && !__Extension_Method_ICollection__.IsNullOrEmpty(list))
			{
				flag = false;
				RuneSlotType[] array = runeSlotTypes;
				foreach (RuneSlotType item in array)
				{
					if (list.Contains(item))
					{
						flag = true;
						break;
					}
				}
			}
		}
		return flag;
	}

	void EquipListManager.Listener.OnClickItemIcon(ItemIconComponent This)
	{
		if (_listener != null)
		{
			_listener.OnClick(This);
		}
	}

	void RuneCategoryButtonComponent.IListener.OnSelectRuneCategory(ItemType type)
	{
		_selectRuneType = type;
		Refresh();
		ResetScroll();
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
