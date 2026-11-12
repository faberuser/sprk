using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;

namespace NGame2.NUI.NWindow.NInventory;

public class ItemListListener : InvenList.IEquipListListener
{
	public enum EFunctionType
	{
		Sell,
		Break,
		Keep,
		Bring
	}

	public EFunctionType functionType;

	private InvenList.EToggle _toggle;

	private int _subCategoryIndex;

	public HashSet<ItemIconComponent> SelectedItemIconTable { get; private set; }

	public List<ItemIconComponent> SelectedItemIconList { get; private set; }

	public Func<ItemIconComponent, bool> SelectableFunction { get; set; }

	public Action<ItemIconComponent> OnClickItemIcon { get; set; }

	public Action<ItemIconComponent, bool> UpdateFunction { get; set; }

	public int MaxTotalSelectCount { get; set; }

	public bool isFiltering { get; set; }

	public ItemListListener(InvenList.EToggle toggle, int subCategoryIndex)
	{
		_toggle = toggle;
		_subCategoryIndex = subCategoryIndex;
		SelectedItemIconList = new List<ItemIconComponent>();
		SelectedItemIconTable = new HashSet<ItemIconComponent>();
		isFiltering = false;
	}

	public int GetTotalSelectedCount()
	{
		return SelectedItemIconList.Count;
	}

	private void OnClickYes(ItemIconComponent component)
	{
		SelectedItemIconList.Add(component);
		SelectedItemIconTable.Add(component);
		Vespa_System_Action_Extension_Method.fire(UpdateFunction, component, i_argB: true);
		Vespa_System_Action_Extension_Method.fire(OnClickItemIcon, component);
	}

	void InvenList.IEquipListListener.OnClick(ItemIconComponent component, Action onComplete = null)
	{
		if (component == null || component.ItemData == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		if (SelectableFunction != null && !Vespa_System_Action_Extension_Method.fire(SelectableFunction, component))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		if (SelectedItemIconList.Contains(component))
		{
			SelectedItemIconList.Remove(component);
			SelectedItemIconTable.Remove(component);
			Vespa_System_Action_Extension_Method.fire(UpdateFunction, component, i_argB: false);
		}
		else
		{
			if (MaxTotalSelectCount > 0 && GetTotalSelectedCount() >= MaxTotalSelectCount)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete);
				return;
			}
			EquipItemInfo equipItemInfo = component.EquipItemInfo;
			if (equipItemInfo != null)
			{
				if (equipItemInfo.IsEquipped)
				{
					Vespa_System_Action_Extension_Method.fire(onComplete);
					return;
				}
				if (!SprkSkipBreakSelectionWarning() && (equipItemInfo.IsUniqueWeapon || equipItemInfo.IsUniqueTreasure) && equipItemInfo.IsBreakable)
				{
					BreakConfirmPopup.Open(equipItemInfo, delegate(BreakConfirmPopup popup)
					{
						popup.SetOnClose(onComplete);
						OnClickYes(component);
					}, delegate(BreakConfirmPopup popup)
					{
						popup.SetOnClose(onComplete);
					});
					return;
				}
			}
			SelectedItemIconList.Add(component);
			SelectedItemIconTable.Add(component);
			Vespa_System_Action_Extension_Method.fire(UpdateFunction, component, i_argB: true);
		}
		Vespa_System_Action_Extension_Method.fire(OnClickItemIcon, component);
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	void InvenList.IEquipListListener.UpdateItemIcon(ItemIconComponent component)
	{
		if (!(component == null) && component.ItemData != null)
		{
			UpdateFunction(component, SelectedItemIconList.Contains(component));
		}
	}

	KeyValuePair<InvenList.EToggle, int> InvenList.IEquipListListener.GetAvailableTab()
	{
		return new KeyValuePair<InvenList.EToggle, int>(_toggle, _subCategoryIndex);
	}

	public bool CheckNotSellItem(ItemIconComponent component)
	{
		if (component != null && (component.ItemData.NotForSale || component.EquipItemInfo.Locked != 0))
		{
			return true;
		}
		return false;
	}

	public bool CheckPossibleSellItemBySelectedItemIcon()
	{
		foreach (ItemIconComponent selectedItemIcon in SelectedItemIconList)
		{
			if (!CheckNotSellItem(selectedItemIcon))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckNotBreakItem(ItemIconComponent component)
	{
		if (component != null && (!component.ItemData.Breakable || component.EquipItemInfo.Locked != 0))
		{
			return true;
		}
		return false;
	}

	public bool CheckPossibleBreakItemBySelectedItemIcon()
	{
		foreach (ItemIconComponent selectedItemIcon in SelectedItemIconList)
		{
			if (!CheckNotBreakItem(selectedItemIcon))
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveNotSellItem()
	{
		for (int num = SelectedItemIconList.Count - 1; num >= 0; num--)
		{
			if (CheckNotSellItem(SelectedItemIconList[num]))
			{
				SelectedItemIconTable.Remove(SelectedItemIconList[num]);
				SelectedItemIconList.RemoveAt(num);
			}
		}
	}

	public void RemoveNotBreakItem()
	{
		for (int num = SelectedItemIconList.Count - 1; num >= 0; num--)
		{
			if (CheckNotBreakItem(SelectedItemIconList[num]))
			{
				SelectedItemIconTable.Remove(SelectedItemIconList[num]);
				SelectedItemIconList.RemoveAt(num);
			}
		}
	}

	private bool SprkSkipBreakSelectionWarning()
	{
		if (functionType != EFunctionType.Break)
		{
			return isFiltering;
		}
		return true;
	}
}
