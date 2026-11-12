using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;

namespace NGame2.NUI.NWindow;

[Serializable]
public class ShopItemSortFilter : BaseShopItemFilterPanel<ShopItemFilterImpl>
{
	public override void SetFilterGroupComponent(EShopItemFilterGroupType filterGroupType)
	{
		if (FilterScrollView == null || FilterTable == null)
		{
			return;
		}
		IList<ShopItemSortGroupData> dataListByGroupType = GameRoot.Instance.ShopItemSortGroupDataContainer.GetDataListByGroupType(filterGroupType);
		for (int i = 0; i < dataListByGroupType.Count; i++)
		{
			ShopItemSortGroupData shopItemSortGroupData = dataListByGroupType[i];
			if (shopItemSortGroupData != null)
			{
				ShopItemFilterGroupComponent shopItemFilterGroupComponent = ShopItemFilterGroupComponent.PrefabAuto.Factory.Create(FilterTable.transform);
				shopItemFilterGroupComponent.Open(shopItemSortGroupData, this);
				_listGroupComp.Add(shopItemFilterGroupComponent);
			}
		}
		FilterTable.Reposition();
		FilterScrollView.ResetPosition();
	}

	public override void OnClickFoldOut(bool isFoldOut)
	{
		if (FilterTable != null)
		{
			FilterTable.Reposition();
		}
		if (FilterScrollView != null)
		{
			FilterScrollView.ResetPosition();
		}
	}

	public override void OnClickShopItemFilter(ShopItemFilterComponent comp, IShopItemFilterInfo info)
	{
		if (comp == null || info == null)
		{
			return;
		}
		EShopItemFilterSelected num = _filterImpl.Select(info);
		bool selected = num != EShopItemFilterSelected.UnSelect;
		if (num == EShopItemFilterSelected.ShopSortType)
		{
			foreach (ShopItemFilterGroupComponent item in _listGroupComp)
			{
				item.ForFilterComponent(delegate(ShopItemFilterComponent component)
				{
					if (component.GetFilterInfo().ShopSortType != EShopSortType.None)
					{
						component.SetSelected(isSelect: false);
					}
				});
			}
		}
		comp.SetSelected(selected);
	}

	public bool IsNoneSelected()
	{
		if (_filterImpl.IsNoneSelectedFilter())
		{
			return _filterImpl.IsNoneSelectedSort();
		}
		return false;
	}

	public bool IsNoneSelectedFilter()
	{
		return _filterImpl.IsNoneSelectedFilter();
	}

	public bool Filter(IShopItemFilterInfo info)
	{
		if (_filterImpl.IsNoneSelectedFilter())
		{
			return true;
		}
		return _filterImpl.Filter(info);
	}

	public bool Filter(EShopItemType type)
	{
		if (_filterImpl.IsNoneSelectedFilter())
		{
			return true;
		}
		return _filterImpl.FilterItemTypeContains(type);
	}

	public bool IsNoneSelectedSort()
	{
		return _filterImpl.IsNoneSelectedSort();
	}

	public bool Sort(IShopItemFilterInfo info)
	{
		if (_filterImpl.IsNoneSelectedSort())
		{
			return true;
		}
		return _filterImpl.Sort(info);
	}

	public bool Sort(EShopSortType type)
	{
		if (_filterImpl.IsNoneSelectedSort())
		{
			return true;
		}
		return _filterImpl.FilterSortTypeContains(type);
	}
}
