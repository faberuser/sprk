using System;
using System.Collections.Generic;
using System.Reflection;
using NVespa.NGui;

namespace NGame2.NEditor.NViewer.NBase;

internal class ViewerSort<TValue> : PropertyPage
{
	private bool _isAsc = true;

	public ViewerSort(IProvider<TValue> dataProvider)
		: base(dataProvider)
	{
	}

	private string GetDescLabel()
	{
		if (!_isAsc)
		{
			return "내림차순";
		}
		return "오름차순";
	}

	public void DrawGUI()
	{
		using (new BlockManager().Horizontal())
		{
			DrawPropertyPageGUI("{0}.{1} 정렬");
			_isAsc = DrawHelper.ToggleAutoSize(GetDescLabel(), _isAsc);
			if (DrawHelper.ButtonNormal("정렬하기"))
			{
				Sort();
			}
		}
	}

	private List<TValue> GetList()
	{
		if (_provider == null)
		{
			return null;
		}
		if (!(_provider is IProvider<TValue> provider))
		{
			return null;
		}
		return provider.GetList();
	}

	public void Sort()
	{
		List<TValue> list = GetList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		PropertyInfo curPropertyInfo = GetCurrentPropertyInfo();
		if (!(curPropertyInfo == null))
		{
			Type propertyType = curPropertyInfo.PropertyType;
			list.Sort(delegate(TValue lhs, TValue rhs)
			{
				object value = curPropertyInfo.GetValue(lhs, null);
				object value2 = curPropertyInfo.GetValue(rhs, null);
				int num = ViewerHelper.GenericCompareTo(value, value2, propertyType);
				return (!_isAsc) ? (-num) : num;
			});
		}
	}
}
