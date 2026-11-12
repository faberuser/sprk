using System.Collections.Generic;
using System.Reflection;
using NVespa.NGui;

namespace NGame2.NEditor.NViewer.NBase;

public class ViewerSearch<TValue> : PropertyPage
{
	private string _filter;

	private bool _checkIndexOf;

	protected new IProvider<TValue> _provider;

	public ViewerSearch(IProvider<TValue> dataProvider)
		: base(dataProvider)
	{
		_provider = dataProvider;
	}

	private string GetToggleLabel()
	{
		if (!_checkIndexOf)
		{
			return "검색 방법 : 일치";
		}
		return "검색 방볍 : 포함";
	}

	public void DrawGUI()
	{
		using (new BlockManager().Horizontal())
		{
			DrawPropertyPageGUI("{0}.{1} 찾기");
			_checkIndexOf = DrawHelper.ToggleAutoSize(GetToggleLabel(), _checkIndexOf);
			DrawHelper.LabelAutoSize("검색어");
			_filter = DrawHelper.TextFieldLarge(_filter);
			if (DrawHelper.ButtonNormal("적용하기"))
			{
				Search();
			}
		}
	}

	private List<TValue> GetList()
	{
		if (_provider == null)
		{
			return null;
		}
		return _provider.GetList();
	}

	public void Search()
	{
		List<TValue> list = GetList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		PropertyInfo currentPropertyInfo = GetCurrentPropertyInfo();
		if (currentPropertyInfo == null)
		{
			return;
		}
		_ = currentPropertyInfo.PropertyType;
		for (int i = 0; i < list.Count; i++)
		{
			TValue val = list[i];
			object value = currentPropertyInfo.GetValue(val, null);
			if (!Search(value, _filter))
			{
				list.RemoveAt(i);
				i--;
			}
		}
	}

	private bool Search(object value, string searchStr)
	{
		if (!_checkIndexOf)
		{
			return ViewerHelper.SearchEqual(value, searchStr);
		}
		return ViewerHelper.SearchIndexOf(value, searchStr);
	}
}
