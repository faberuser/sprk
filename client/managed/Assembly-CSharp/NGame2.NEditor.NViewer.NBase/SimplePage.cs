using NVespa.NGui;

namespace NGame2.NEditor.NViewer.NBase;

internal class SimplePage
{
	private NGame2.NEditor.NViewer.NBase.Page _page;

	private EditorVariable<int> _countPerPage;

	private IProvider _provider;

	public SimplePage(IProvider dataProvider)
	{
		_page = new NGame2.NEditor.NViewer.NBase.Page();
		_page.SetPagePerCount(20);
		_countPerPage = new EditorVariable<int>("페이지 당 표시개수");
		_provider = dataProvider;
	}

	public bool IsInner(int index)
	{
		if (_page.MinIndex <= index)
		{
			return index < _page.MaxIndex;
		}
		return false;
	}

	private int GetCount()
	{
		if (_provider == null)
		{
			return 0;
		}
		return _provider.GetListCount();
	}

	public void DrawGUI()
	{
		int count = GetCount();
		if (_page.TotalCount != count)
		{
			_page.SetTotalCount(count);
		}
		using (new BlockManager().Horizontal())
		{
			if (DrawHelper.ButtonAutoSize("<"))
			{
				_page.Prev();
			}
			DrawHelper.LabelCustom($"페이지 정보 : ({_page.CurPageNumber}/{_page.MaxPageIndex}) ", 250f);
			if (DrawHelper.ButtonAutoSize(">"))
			{
				_page.Next();
			}
			int num = _countPerPage.Edit(_page.PagePerCount);
			if (_page.PagePerCount != num)
			{
				_page.SetPagePerCount(num);
			}
		}
	}
}
