using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NValance;

[Serializable]
public class ValanceItemListPanel : ItemIconComponent.Listener, HeroEquipListComponent.Listener, UIScrollItemBehaviour.IListener
{
	public interface IListener
	{
		void OnClickItemIcon(EquipItemInfo equipItemInfo);
	}

	public Transform ItemListRoot;

	public UIReuseScrollGrid HeroEquipItemGrid;

	public UIToggle HeroEquipToggle;

	public UILabel Label_EmptyHero;

	public UIReuseScrollGrid InvenEquipItemGrid;

	public UIToggle InvenItemToggle;

	public UILabel Label_EmptyEquipItem;

	private bool DisableItemSelect;

	private IListener _listener;

	private EquipItemInfo _seletedEquipItemInfo;

	private List<CachedHeroInfo> _filteredHeroInfos;

	private List<EquipItemInfo> _filteredItemInfos;

	private Action<ItemIconComponent> _itemDisabledAction;

	private List<HeroEquipListComponent> _listHeroEquipComp = new List<HeroEquipListComponent>();

	private List<ItemIconComponent> _listItemIconComp = new List<ItemIconComponent>();

	public void Open(Action<ItemIconComponent> itemDisabler, IListener listener)
	{
		_listener = listener;
		_itemDisabledAction = itemDisabler;
		InitToggleButton();
	}

	private void InitToggleButton()
	{
		HeroEquipToggle.Set(state: true);
		InvenItemToggle.Set(state: false);
	}

	public void SetHeroEquipItemList(List<CachedHeroInfo> heroInfos)
	{
		ClearHeroList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroInfos))
		{
			UIHelper.EnableGameObject(Label_EmptyHero, enable: true);
			return;
		}
		_filteredHeroInfos = heroInfos;
		if (HeroEquipItemGrid != null)
		{
			HeroEquipItemGrid.CreateReuseItem(HeroEquipListComponent.PrefabAuto, _listHeroEquipComp, __Extension_Method_ICollection__.COUNT(_filteredHeroInfos), this);
			HeroEquipItemGrid.RepositionGrid();
		}
		UIHelper.EnableGameObject(Label_EmptyHero, _filteredHeroInfos.Count == 0);
	}

	public void SetInvenItemList(List<EquipItemInfo> equipItemInfos)
	{
		ClearItemList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(equipItemInfos))
		{
			UIHelper.EnableGameObject(Label_EmptyEquipItem, enable: true);
			return;
		}
		_filteredItemInfos = equipItemInfos;
		if (InvenEquipItemGrid != null)
		{
			InvenEquipItemGrid.CreateReuseItem(ItemIconComponent.PrefabAuto, _listItemIconComp, __Extension_Method_ICollection__.COUNT(_filteredItemInfos), this);
			InvenEquipItemGrid.RepositionGrid();
		}
		UIHelper.EnableGameObject(Label_EmptyEquipItem, _filteredItemInfos.Count == 0);
	}

	public void ResetItemList()
	{
		ClearSeletedItem();
		RefreshItemList();
	}

	private void RefreshItemList()
	{
		RefreshHeroEquipItemList();
		RefreshInvenItemList();
	}

	private void RefreshHeroEquipItemList()
	{
		bool isSameItem = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listHeroEquipComp); i++)
		{
			HeroEquipListComponent heroEquipListComponent = _listHeroEquipComp[i];
			if (heroEquipListComponent == null)
			{
				continue;
			}
			heroEquipListComponent.ForEachItemComponent(delegate(ItemIconComponent itemComp)
			{
				isSameItem = itemComp.EquipItemInfo == _seletedEquipItemInfo;
				itemComp.SetSelected(isSameItem);
				if (_itemDisabledAction != null)
				{
					_itemDisabledAction(itemComp);
				}
				bool iconGrayScale = DisableItemSelect && !isSameItem;
				itemComp.SetIconGrayScale(iconGrayScale);
			});
		}
	}

	private void RefreshInvenItemList()
	{
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listItemIconComp); i++)
		{
			ItemIconComponent itemIconComponent = _listItemIconComp[i];
			if (!(itemIconComponent == null))
			{
				flag = itemIconComponent.EquipItemInfo == _seletedEquipItemInfo;
				itemIconComponent.SetSelected(flag);
				bool iconGrayScale = DisableItemSelect && !flag;
				itemIconComponent.SetIconGrayScale(iconGrayScale);
			}
		}
	}

	public void Close()
	{
		_listener = null;
		_itemDisabledAction = null;
		ClearSeletedItem();
		ClearItemList();
		ClearHeroList();
	}

	public void ClearSeletedItem()
	{
		_seletedEquipItemInfo = null;
	}

	private void ClearItemList()
	{
		if (_filteredItemInfos != null)
		{
			_filteredItemInfos.Clear();
			_filteredItemInfos = null;
		}
		FactoryBase.ReleaseAndClearList(_listItemIconComp);
		if (InvenEquipItemGrid != null)
		{
			InvenEquipItemGrid.ClearGridItem<ItemIconComponent>();
		}
	}

	private void ClearHeroList()
	{
		if (_filteredHeroInfos != null)
		{
			_filteredHeroInfos.Clear();
			_filteredHeroInfos = null;
		}
		FactoryBase.ReleaseAndClearList(_listHeroEquipComp);
		if (HeroEquipItemGrid != null)
		{
			HeroEquipItemGrid.ClearGridItem<HeroEquipListComponent>();
		}
	}

	public void OnToggleChanged()
	{
		bool flag = (bool)HeroEquipToggle && HeroEquipToggle.value;
		UIHelper.EnableGameObject(HeroEquipItemGrid, flag);
		UIHelper.EnableGameObject(InvenEquipItemGrid, !flag);
		if (flag)
		{
			HeroEquipItemGrid.ClippingPanel.SetDirty();
		}
		else
		{
			InvenEquipItemGrid.ClippingPanel.SetDirty();
		}
	}

	public void OnClickItemIcon(ItemIconComponent component)
	{
		if (DisableItemSelect || component == null || component.IsDisabledClick())
		{
			return;
		}
		EquipItemInfo equipItemInfo = component.EquipItemInfo;
		if (equipItemInfo != null && _listener != null)
		{
			if (_seletedEquipItemInfo == null)
			{
				_seletedEquipItemInfo = equipItemInfo;
				_listener.OnClickItemIcon(equipItemInfo);
				component.SetSelected(selected: true);
			}
			else if (equipItemInfo == _seletedEquipItemInfo)
			{
				ClearSeletedItem();
				_listener.OnClickItemIcon(null);
				component.SetSelected(selected: false);
			}
			else
			{
				_seletedEquipItemInfo = equipItemInfo;
				_listener.OnClickItemIcon(equipItemInfo);
				RefreshItemList();
			}
		}
	}

	public void ExternalClickItemIcon(EquipItemInfo equipItemInfo, bool isRefreshTab = false)
	{
		if (equipItemInfo == null || _listener == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = true;
		CachedHeroInfo equippedHeroInfo = equipItemInfo.EquippedHeroInfo;
		if (equippedHeroInfo != null && _filteredHeroInfos.Contains(equippedHeroInfo))
		{
			flag2 = true;
			flag = true;
		}
		if (!flag2 && _filteredItemInfos.Contains(equipItemInfo))
		{
			flag2 = true;
			flag = false;
		}
		if (flag2)
		{
			if (isRefreshTab)
			{
				HeroEquipToggle.Set(flag);
				InvenItemToggle.Set(!flag);
			}
			_seletedEquipItemInfo = equipItemInfo;
			_listener.OnClickItemIcon(_seletedEquipItemInfo);
			RefreshItemList();
		}
	}

	public void CheckIfItemIconDisabled()
	{
		DisableAllItemSelect(DisableItemSelect);
	}

	public void DisableAllItemSelect(bool disable, bool isForce = false)
	{
		if (DisableItemSelect != disable || isForce)
		{
			DisableItemSelect = disable;
			RefreshItemList();
		}
	}

	void UIScrollItemBehaviour.IListener.ChangedItemIndex(Component comp, int index)
	{
		HeroEquipListComponent heroEquipListComponent = comp as HeroEquipListComponent;
		bool isSameItem = false;
		if (heroEquipListComponent != null)
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_filteredHeroInfos, index))
			{
				return;
			}
			heroEquipListComponent.Open(_filteredHeroInfos[index], this);
			heroEquipListComponent.ForEachItemComponent(delegate(ItemIconComponent itemComp)
			{
				isSameItem = itemComp.EquipItemInfo == _seletedEquipItemInfo;
				itemComp.SetSelected(isSameItem);
				if (_itemDisabledAction != null)
				{
					_itemDisabledAction(itemComp);
				}
				bool iconGrayScale2 = DisableItemSelect && !isSameItem;
				itemComp.SetIconGrayScale(iconGrayScale2);
			});
		}
		ItemIconComponent itemIconComponent = comp as ItemIconComponent;
		if (itemIconComponent != null && Vespa_Generic_IList_Extension_Method.IsValidIndex(_filteredItemInfos, index))
		{
			EquipItemInfo equipItemInfo = _filteredItemInfos[index];
			isSameItem = equipItemInfo == _seletedEquipItemInfo;
			itemIconComponent.Open(equipItemInfo, this);
			itemIconComponent.SetSelected(isSameItem);
			itemIconComponent.Refresh();
			bool iconGrayScale = DisableItemSelect && !isSameItem;
			itemIconComponent.SetIconGrayScale(iconGrayScale);
		}
	}

	void HeroEquipListComponent.Listener.OnClick(HeroPortraitComponent component)
	{
	}

	void HeroEquipListComponent.Listener.OnClick(ItemIconComponent component)
	{
		OnClickItemIcon(component);
	}

	void HeroEquipListComponent.Listener.OnClickThis(int heroIndex)
	{
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		OnClickItemIcon(itemIconComponent);
	}
}
