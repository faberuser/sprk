using System.Collections.Generic;

namespace NVespa.NNGUIExtension;

public class UIReuseScrollGridInfo
{
	private int _firstItemIndex;

	private int _lastItemIndex;

	private int _indexOfListFirstItem;

	private int _indexOfListLastItem;

	private int _createItemCount;

	private int _totaItemCount;

	private List<ReuseItemInfo> _listItemInfo = new List<ReuseItemInfo>();

	public void Setup(int firstItemIndex, int lastItemIndex, int createItemCount, int listCount)
	{
		_firstItemIndex = firstItemIndex;
		_lastItemIndex = lastItemIndex;
		_createItemCount = createItemCount;
		_indexOfListLastItem = listCount - 1;
		_indexOfListFirstItem = _indexOfListLastItem - (_createItemCount - 1);
		_totaItemCount = lastItemIndex - firstItemIndex + 1;
	}

	public void Repostion(List<ReuseItemInfo> list)
	{
		int num = _firstItemIndex;
		for (int i = _indexOfListFirstItem; i <= _indexOfListLastItem; i++)
		{
			ReuseItemInfo reuseItemInfo = list[i];
			if (reuseItemInfo != null && reuseItemInfo.IsValid())
			{
				reuseItemInfo.SetIndex(num);
				num++;
			}
		}
	}

	public void FitRepostion(List<ReuseItemInfo> list, int firstIndex, int lastIndex)
	{
		if (_createItemCount == 0)
		{
			return;
		}
		if (_createItemCount == _totaItemCount)
		{
			Repostion(list);
		}
		else if (_lastItemIndex < firstIndex || _firstItemIndex > lastIndex)
		{
			int num = _lastItemIndex;
			if (_lastItemIndex <= firstIndex)
			{
				for (int num2 = _indexOfListLastItem; num2 >= _indexOfListFirstItem; num2--)
				{
					ReuseItemInfo reuseItemInfo = list[num2];
					if (reuseItemInfo != null && reuseItemInfo.IsValid())
					{
						reuseItemInfo.SetIndex(num);
						num--;
					}
				}
			}
			else
			{
				Repostion(list);
			}
		}
		else
		{
			if (!CheckChangeFirstToLast(list, lastIndex) && !CheckChangeLastToFirst(list, firstIndex))
			{
				return;
			}
			int num3 = 0;
			if (firstIndex + _createItemCount < _lastItemIndex)
			{
				num3 = ((_firstItemIndex <= firstIndex) ? firstIndex : _firstItemIndex);
			}
			if (lastIndex - _createItemCount >= _firstItemIndex)
			{
				num3 = ((lastIndex <= _lastItemIndex) ? (lastIndex - _createItemCount) : (_lastItemIndex - _createItemCount));
			}
			for (int i = _indexOfListFirstItem; i <= _indexOfListLastItem; i++)
			{
				ReuseItemInfo reuseItemInfo2 = list[i];
				if (reuseItemInfo2 != null && reuseItemInfo2.IsValid())
				{
					reuseItemInfo2.SetIndex(num3);
					num3++;
				}
			}
		}
	}

	public bool IsContainItemIndex(int firstItemIndex, int lastItemIndex)
	{
		if (firstItemIndex > _firstItemIndex)
		{
			return true;
		}
		if (lastItemIndex < _lastItemIndex)
		{
			return true;
		}
		return false;
	}

	public bool CheckChangeLastToFirst(List<ReuseItemInfo> list, int firstItemIndex)
	{
		ReuseItemInfo reuseItemInfo = list[_indexOfListFirstItem];
		if (reuseItemInfo == null || !reuseItemInfo.IsValid())
		{
			return false;
		}
		int itemIndex = reuseItemInfo.GetItemIndex();
		if (itemIndex == _firstItemIndex)
		{
			return false;
		}
		if (itemIndex < firstItemIndex)
		{
			return false;
		}
		return true;
	}

	public bool CheckChangeFirstToLast(List<ReuseItemInfo> list, int lastItemIndex)
	{
		ReuseItemInfo reuseItemInfo = list[_indexOfListLastItem];
		if (reuseItemInfo == null || !reuseItemInfo.IsValid())
		{
			return false;
		}
		int itemIndex = reuseItemInfo.GetItemIndex();
		if (itemIndex == _lastItemIndex)
		{
			return false;
		}
		if (itemIndex > lastItemIndex)
		{
			return false;
		}
		return true;
	}

	public void ChangeLastToFirst(List<ReuseItemInfo> list, int count)
	{
		ReuseItemInfo reuseItemInfo = list[_indexOfListFirstItem];
		if (reuseItemInfo == null || !reuseItemInfo.IsValid())
		{
			VespaDebug.LogError("[UIReuseScrollGridInfo] ChangeLastToFirst FirstItem Is Null");
			return;
		}
		int num = reuseItemInfo.GetItemIndex();
		int num2 = num % count;
		if (num2 != 0)
		{
			count += num2;
		}
		if (_firstItemIndex > num - count)
		{
			count = num - _firstItemIndex;
		}
		for (int num3 = count - 1; num3 >= 0; num3--)
		{
			ReuseItemInfo reuseItemInfo2 = list[_indexOfListLastItem];
			if (reuseItemInfo2 == null || !reuseItemInfo2.IsValid())
			{
				VespaDebug.LogError("[UIReuseScrollGridInfo] ChangeLastToFirst lastItem Is Null");
			}
			num--;
			reuseItemInfo2.SetIndex(num);
			list.Remove(reuseItemInfo2);
			list.Insert(_indexOfListFirstItem, reuseItemInfo2);
		}
	}

	public void ChangeFirstToLast(List<ReuseItemInfo> list, int count)
	{
		ReuseItemInfo reuseItemInfo = list[_indexOfListLastItem];
		if (reuseItemInfo == null)
		{
			VespaDebug.LogError("[UIReuseScrollGridInfo] ChangeLastToFirst LastItem Is Null");
			return;
		}
		int num = reuseItemInfo.GetItemIndex();
		if (_lastItemIndex < num + count)
		{
			count = _lastItemIndex - num;
		}
		for (int i = 0; i < count; i++)
		{
			ReuseItemInfo reuseItemInfo2 = list[_indexOfListFirstItem];
			if (reuseItemInfo2 == null)
			{
				VespaDebug.LogError("[UIReuseScrollGridInfo] ChangeLastToFirst FirstItem Is Null");
			}
			num++;
			reuseItemInfo2.SetIndex(num);
			list.Remove(reuseItemInfo2);
			list.Insert(_indexOfListLastItem, reuseItemInfo2);
		}
	}
}
