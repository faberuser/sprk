using System;

namespace NGame2.NEditor.NViewer.NBase;

internal class Page
{
	private int _curPageIndex;

	private int _curPageNumber;

	private int _maxPageIndex;

	private int _minIndex;

	private int _maxIndex;

	private int _totalCount;

	private int _pagePerCount;

	public int CurrenctPageIndex => _curPageIndex;

	public int CurPageNumber => _curPageNumber;

	public int MaxPageIndex => _maxPageIndex;

	public int MinIndex => _minIndex;

	public int MaxIndex => _maxIndex;

	public int TotalCount => _totalCount;

	public int PagePerCount => _pagePerCount;

	protected virtual void SetDefault()
	{
		_curPageIndex = 0;
		_curPageNumber = 0;
		_maxPageIndex = 0;
		_minIndex = 0;
		_maxIndex = 0;
		_totalCount = 0;
		_pagePerCount = 0;
	}

	public void Set(int totalCount, int pagePerCount = 0)
	{
		bool num = SetBasePagePerCount(pagePerCount);
		bool flag = SetBaseTotalCount(totalCount);
		if (num || flag)
		{
			SetPageValues(0);
		}
	}

	public void SetPagePerCount(int pagePerCount)
	{
		if (SetBasePagePerCount(pagePerCount))
		{
			SetPageValues(0);
		}
	}

	private bool SetBasePagePerCount(int pagePerCount)
	{
		if (_pagePerCount == pagePerCount)
		{
			return false;
		}
		_pagePerCount = pagePerCount;
		return true;
	}

	public void SetTotalCount(int totalCount)
	{
		if (SetBaseTotalCount(totalCount))
		{
			SetPageValues(0);
		}
	}

	private bool SetBaseTotalCount(int totalCount)
	{
		if (_totalCount == totalCount)
		{
			return false;
		}
		_totalCount = totalCount;
		return true;
	}

	public void Next()
	{
		int nextPageIndex = GetNextPageIndex();
		SetCurrentPageIndex(nextPageIndex);
	}

	public void Prev()
	{
		int prevPageIndex = GetPrevPageIndex();
		SetCurrentPageIndex(prevPageIndex);
	}

	private void SetPageValues(int pageIndex)
	{
		_curPageIndex = pageIndex;
		_maxPageIndex = GetMaxPageIndex();
		_minIndex = GetMinIndex();
		_maxIndex = GetMaxIndex();
		_curPageNumber = GetCurPageNubmer();
	}

	private void SetCurrentPageIndex(int pageIndex)
	{
		if (_curPageIndex != pageIndex)
		{
			SetPageValues(pageIndex);
		}
	}

	private int GetNextPageIndex()
	{
		if (_curPageIndex + 1 >= _maxPageIndex)
		{
			return _curPageIndex;
		}
		return _curPageIndex + 1;
	}

	private int GetPrevPageIndex()
	{
		if (_curPageIndex - 1 < 0)
		{
			_curPageIndex = 0;
			return _curPageIndex;
		}
		return _curPageIndex - 1;
	}

	private int GetMaxPageIndex()
	{
		if (_totalCount == 0)
		{
			return 0;
		}
		if (_pagePerCount <= 0)
		{
			return 1;
		}
		return (int)Math.Ceiling((float)_totalCount / (float)_pagePerCount);
	}

	private int GetMaxIndex()
	{
		if (_totalCount == 0)
		{
			return 0;
		}
		if (_pagePerCount == 0)
		{
			return _totalCount;
		}
		int num = _curPageIndex + 1;
		if (num != _maxPageIndex)
		{
			return _pagePerCount * num;
		}
		return _totalCount;
	}

	private int GetMinIndex()
	{
		if (_totalCount == 0 || _pagePerCount == 0)
		{
			return 0;
		}
		return _curPageIndex * _pagePerCount;
	}

	private int GetCurPageNubmer()
	{
		return Math.Min(_curPageIndex + 1, _maxPageIndex);
	}
}
