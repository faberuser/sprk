using System.Reflection;

namespace NGame2.NEditor.NViewer.NBase;

public class PropertyPage
{
	private NGame2.NEditor.NViewer.NBase.Page _page;

	protected IProvider _provider;

	public PropertyPage(IProvider provider)
	{
		if (provider != null)
		{
			_provider = provider;
			int paging = __Extension_Method_ICollection__.COUNT(_provider.GeCachedtPropertyInfos());
			SetPaging(paging);
		}
	}

	public void Refresh()
	{
		if (_provider != null)
		{
			int paging = __Extension_Method_ICollection__.COUNT(_provider.GeCachedtPropertyInfos());
			SetPaging(paging);
		}
	}

	protected void SetPaging(int totalCount)
	{
		if (_page == null)
		{
			_page = new NGame2.NEditor.NViewer.NBase.Page();
		}
		_page.Set(totalCount, 1);
	}

	protected int GetCurrenctPropertyInfoIndex()
	{
		if (_page == null)
		{
			return 0;
		}
		return _page.CurrenctPageIndex;
	}

	protected int GetCurrentPageNumber()
	{
		if (_page == null)
		{
			return 0;
		}
		return _page.CurPageNumber;
	}

	protected int GetTotalPageCount()
	{
		if (_page == null)
		{
			return 0;
		}
		return _page.TotalCount;
	}

	protected PropertyInfo GetCurrentPropertyInfo()
	{
		int currenctPropertyInfoIndex = GetCurrenctPropertyInfoIndex();
		return GetPropertyInfo(currenctPropertyInfoIndex);
	}

	protected PropertyInfo GetPropertyInfo(int index)
	{
		if (_provider == null)
		{
			return null;
		}
		return _provider.GetPropertyInfo(index);
	}

	protected void Next()
	{
		if (_page != null)
		{
			_page.Next();
		}
	}

	protected void Prev()
	{
		if (_page != null)
		{
			_page.Prev();
		}
	}

	protected void DrawPropertyPageGUI(string propertyDescFomat)
	{
		PropertyInfo currentPropertyInfo = GetCurrentPropertyInfo();
		if (!(currentPropertyInfo == null))
		{
			if (DrawHelper.ButtonAutoSize("<"))
			{
				Prev();
			}
			DrawHelper.LabelCustom(string.Format(propertyDescFomat, GetCurrentPageNumber(), currentPropertyInfo.Name), 250f);
			if (DrawHelper.ButtonAutoSize(">"))
			{
				Next();
			}
			DrawHelper.FlexibleSpace();
		}
	}
}
