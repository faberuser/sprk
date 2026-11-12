using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow;

public class ShopItemFilterImpl : IShopItemFilterImpl
{
	public HashSet<EShopItemType> _hashItemType = new HashSet<EShopItemType>();

	public HashSet<EShopSortType> _hashSortType = new HashSet<EShopSortType>();

	public bool IsNoneSelectedFilter()
	{
		return __Extension_Method_ICollection__.IsNullOrEmpty(_hashItemType);
	}

	public bool IsNoneSelectedSort()
	{
		return __Extension_Method_ICollection__.IsNullOrEmpty(_hashSortType);
	}

	public void Clear()
	{
		_hashItemType.Clear();
		_hashSortType.Clear();
	}

	public bool Filter<T>(T info) where T : IShopItemFilterInfo
	{
		if (info == null)
		{
			return false;
		}
		if (FilterItemTypeContains(info.ShopItemType))
		{
			return true;
		}
		return false;
	}

	public bool Sort<T>(T info) where T : IShopItemFilterInfo
	{
		if (info == null)
		{
			return false;
		}
		if (FilterSortTypeContains(info.ShopSortType))
		{
			return true;
		}
		return false;
	}

	public bool FilterItemTypeContains(EShopItemType itemType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashItemType))
		{
			return true;
		}
		return _hashItemType.Contains(itemType);
	}

	public bool FilterSortTypeContains(EShopSortType sortType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashSortType))
		{
			return true;
		}
		return _hashSortType.Contains(sortType);
	}

	public EShopItemFilterSelected Select(IShopItemFilterInfo info)
	{
		if (info == null)
		{
			return EShopItemFilterSelected.UnSelect;
		}
		if (SelectShopItemType(info.ShopItemType))
		{
			return EShopItemFilterSelected.ShopItemType;
		}
		if (SelectShopSortType(info.ShopSortType))
		{
			return EShopItemFilterSelected.ShopSortType;
		}
		return EShopItemFilterSelected.UnSelect;
	}

	private bool SelectShopItemType(EShopItemType itemType)
	{
		if (itemType == EShopItemType.None)
		{
			return false;
		}
		return Select(_hashItemType, itemType);
	}

	private bool Select<T>(HashSet<T> hash, T value)
	{
		if (hash.Contains(value))
		{
			hash.Remove(value);
			return false;
		}
		hash.Add(value);
		return true;
	}

	private bool SelectShopSortType(EShopSortType sortType)
	{
		if (sortType == EShopSortType.None)
		{
			return false;
		}
		bool flag = false;
		if (_hashSortType.Contains(sortType))
		{
			_hashSortType.Remove(sortType);
			return false;
		}
		_hashSortType.Clear();
		_hashSortType.Add(sortType);
		return true;
	}
}
